// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Core;

using Anamnesis.Actor.Pages;
using Anamnesis.Memory;
using Anamnesis.Posing;
using Anamnesis.Services;
using Microsoft.Extensions.ObjectPool;
using PropertyChanged;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;

// TODO: Add fallback to the old method of adding bones if the new method fails.

/// <summary>
/// Represents a skeleton of hierarchically-parented bones of an actor that can be posed.
/// </summary>
[AddINotifyPropertyChangedInterface]
public class Skeleton : INotifyPropertyChanged
{
	/// <summary>
	/// A dictionary containing all bones in the skeleton, indexed by their names.
	/// </summary>
	public readonly ConcurrentDictionary<string, Bone> Bones = new();

	/// <summary>A mapping of hair bone names to their suffixes and default names.</summary>
	/// <remarks>
	/// This is used to automatically pick a hair bone when multiple options are available.
	/// </remarks>
	protected readonly Dictionary<string, Tuple<string, string>> HairNameToSuffixMap = new()
	{
		{ "HairAutoFrontLeft", new("l", "j_kami_f_l") },    // Hair, Front Left
		{ "HairAutoFrontRight", new("r", "j_kami_f_r") },   // Hair, Front Right
		{ "HairAutoA", new("a", "j_kami_a") },              // Hair, Back Up
		{ "HairAutoB", new("b", "j_kami_b") },              // Hair, Back Down
		{ "HairFront", new("f", string.Empty) },            // Hair, Front (Custom Bone Name)
	};

	/// <summary>
	/// Maximum number of attempts to retry accessing bone array memory.
	/// </summary>
	private const uint MAX_READ_RETRY_ATTEMPTS = 15;

	/// <summary>
	/// A shared pool of bone stack structures to reduce memory alloc.
	/// </summary>
	private static readonly ObjectPool<Stack<Bone>> s_boneStackPool = ObjectPool.Create<Stack<Bone>>();

	/// <summary>
	/// A snapshot of the transforms of all bones in the skeleton.
	/// </summary>
	/// <remarks>
	/// The object is defined as a field to avoid the memory (de)allocation every time a snapshot is taken.
	/// </remarks>
	private readonly Dictionary<string, Transform> snapshot = [];

	/// <summary>
	/// A lock object used to synchronize access to the snapshot dictionary.
	/// </summary>
	private readonly Lock snapshotLock = new();

	/// <summary>Initializes a new instance of the <see cref="Skeleton"/> class.</summary>
	/// <param name="actor">The actor memory associated with this skeleton.</param>
	public Skeleton(ObjectHandle<ActorMemory> actor)
	{
		this.Actor = actor;
		this.SetActor(actor);
	}

	/// <inheritdoc/>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <summary>Gets the actor memory associated with this skeleton.</summary>
	public ObjectHandle<ActorMemory> Actor { get; private set; }

	/// <summary>Gets the model space root rotation of the skeleton.</summary>
	public Quaternion RootRotation => this.Actor?.DoRef(a => a.ModelObject)?.Transform?.Rotation ?? Quaternion.Identity;

	/// <summary> Gets a value indicating whether the actor has a tail.</summary>
	public bool HasTail => (this.Actor?.Do(a => a.Customize?.Race == ActorCustomizeMemory.Races.Miqote
		|| a.Customize?.Race == ActorCustomizeMemory.Races.AuRa
		|| a.Customize?.Race == ActorCustomizeMemory.Races.Hrothgar) == true) || this.IsIVCS;

	/// <summary>Gets a value indicating whether the actor has a standard face.</summary>
	public bool IsStandardFace => this.Actor == null || (!this.IsMiqote && !this.IsHrothgar && !this.IsViera);

	/// <summary>Gets a value indicating whether the actor is a Miqote.</summary>
	public bool IsMiqote => this.Actor?.Do(a => a.Customize?.Race == ActorCustomizeMemory.Races.Miqote) == true;

	/// <summary>Gets a value indicating whether the actor is a Viera.</summary>
	public bool IsViera => this.Actor?.Do(a => a.Customize?.Race == ActorCustomizeMemory.Races.Viera) == true;

	/// <summary>Gets a value indicating whether the actor is an Elezen.</summary>
	public bool IsElezen => this.Actor?.Do(a => a.Customize?.Race == ActorCustomizeMemory.Races.Elezen) == true;

	/// <summary>Gets a value indicating whether the actor is a Hrothgar.</summary>
	public bool IsHrothgar => this.Actor?.Do(a => a.Customize?.Race == ActorCustomizeMemory.Races.Hrothgar) == true;

	/// <summary>Gets a value indicating whether the actor has a tail or ears.</summary>
	public bool HasTailOrEars => this.IsViera || this.HasTail;

	/// <summary>Gets a value indicating whether the actor is a Viera and has ears type 01.</summary>
	public bool IsEars01 => this.IsViera && this.Actor?.Do(a => a.Customize?.TailEarsType <= 1) == true;

	/// <summary>Gets a value indicating whether the actor is a Viera and has ears type 02.</summary>
	public bool IsEars02 => this.IsViera && this.Actor?.Do(a => a.Customize?.TailEarsType == 2) == true;

	/// <summary>Gets a value indicating whether the actor is a Viera and has ears type 03.</summary>
	public bool IsEars03 => this.IsViera && this.Actor?.Do(a => a.Customize?.TailEarsType == 3) == true;

	/// <summary>Gets a value indicating whether the actor is a Viera and has ears type 04.</summary>
	public bool IsEars04 => this.IsViera && this.Actor?.Do(a => a.Customize?.TailEarsType == 4) == true;

	/// <summary>Gets a value indicating whether the skeleton has IVCS bones.</summary>
	public bool IsIVCS { get; private set; }

	/// <summary>Gets a value indicating whether the actor is a Viera and has floppy ears.</summary>
	public bool IsVieraEarsFlop
	{
		get
		{
			if (!this.IsViera)
				return false;

			ActorCustomizeMemory? customize = this.Actor?.DoRef(a => a.Customize);

			if (customize == null)
				return false;

			if (customize.Gender == ActorCustomizeMemory.Genders.Feminine && customize.TailEarsType == 3)
				return true;

			if (customize.Gender == ActorCustomizeMemory.Genders.Masculine && customize.TailEarsType == 2)
				return true;

			return false;
		}
	}

	/// <summary>
	/// Gets a value indicating whether the actor has a legacy face with bones from before Dawntrail.
	/// </summary>
	public bool HasPreDTFace
	{
		get
		{
			// If the skeleton is not initialized, we can't determine if it's a pre-DT face.
			if (this.Bones.IsEmpty)
				return false;

			// We can determine if we have a DT-updated face if we have a tongue bone.
			// EW faces don't have this bone, whereas all updated faces in DT have it.
			// It would be better to enumerate all of the faces and be more specific.
			// Note: This only applies to humanoid skeletons.
			return this.GetBone("j_f_bero_01") == null;
		}
	}

	/// <summary>Gets the logger instance for the <see cref="Skeleton"/> class.</summary>
	private static ILogger Log => Serilog.Log.ForContext<Skeleton>();

	/// <summary>Clears all bones from the skeleton.</summary>
	public virtual void Clear()
	{
		this.Bones.Clear();
	}

	/// <summary>Gets a bone from the skeleton by its name.</summary>
	/// <param name="name">The name of the bone.</param>
	/// <returns>The bone if found; otherwise, null.</returns>
	public virtual Bone? GetBone(string name)
	{
		// Only process valid skeletons that have atleast one partial skeleton
		if (this.Actor?.Do(a => a.ModelObject?.Skeleton == null || a.ModelObject!.Skeleton!.Length <= 0) ?? true)
			return null;

		string? modernName = LegacyBoneNameConverter.GetModernName(name);
		if (modernName != null)
			name = modernName;

		// Attempt to find hairstyle-specific bones. If not found, default to the standard hair bones.
		if (this.HairNameToSuffixMap.TryGetValue(name, out Tuple<string, string>? suffixAndDefault))
		{
			Bone? bone = this.FindHairBoneByPattern(suffixAndDefault.Item1);
			if (bone != null)
				return bone;

			name = suffixAndDefault.Item2; // If not found, default to the standard hair bones.
		}

		this.Bones.TryGetValue(name, out var result);
		return result;
	}

	/// <summary>Reads the transforms of all bones in the skeleton.</summary>
	public void ReadTransforms()
	{
		if (this.Bones == null || (this.Actor?.Do(a => a.ModelObject?.Skeleton == null) ?? true) || !GposeService.IsInGpose())
			return;

		// If history is restoring, wait until it's done.
		lock (HistoryService.Instance.LockObject)
		{
			// Take a snapshot of the current transforms and update bone transforms.
			var snapshot = this.TakeSnapshot();
			var rootBones = new List<Bone>();
			foreach (var bone in this.Bones.Values)
			{
				if (bone.Parent == null)
					rootBones.Add(bone);
			}

			foreach (var rootBone in rootBones)
			{
				if (IsBoneOrDescendantDirty(rootBone))
				{
					rootBone.ReadTransform(true, snapshot);
				}
			}
		}
	}

	/// <summary>
	/// Prepend a prefix to the bone name and return the converted bone name.
	/// </summary>
	/// <param name="prefix">The prefix to add to the bone name.</param>
	/// <param name="name">The original bone name.</param>
	/// <returns>The converted bone name.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static string ConvertBoneName(string? prefix, string name) => prefix != null ? prefix + name : name;

	/// <summary>
	/// Checks if the given bone or any of its descendants are dirty
	/// (i.e., have been modified) but not yet written back to game memory.
	/// </summary>
	/// <param name="root">The root bone to check.</param>
	/// <returns>
	/// True if the bone or any of its descendants are dirty; otherwise, false.
	/// </returns>
	protected static bool IsBoneOrDescendantDirty(Bone root)
	{
		var stack = s_boneStackPool.Get();
		try
		{
			stack.Push(root);

			while (stack.Count > 0)
			{
				var current = stack.Pop();
				if (current.IsDirty)
					return true;

				foreach (var child in current.Children)
					stack.Push(child);
			}

			return false;
		}
		finally
		{
			stack.Clear();
			s_boneStackPool.Return(stack);
		}
	}

	/// <summary>Takes a snapshot of the current transforms of all bones.</summary>
	/// <remarks>
	/// The intended use of this method is to speed up memory reads by reading all bone transforms at once.
	/// </remarks>
	/// <returns>A dictionary containing the transforms of all bones.</returns>
	protected Dictionary<string, Transform> TakeSnapshot()
	{
		lock (this.snapshotLock)
		{
			this.snapshot.Clear();

			return this.Actor?.DoRef(a =>
			{
				if (a.ModelObject == null || a.ModelObject?.Skeleton == null)
					return this.snapshot;

				a.ModelObject.Skeleton.EnableReading = false;

				foreach (var (name, bone) in this.Bones)
				{
					var transform = bone.TransformMemory;
					if (transform == null)
						continue;

					this.snapshot[name] = new Transform
					{
						Position = transform.Position,
						Rotation = transform.Rotation,
						Scale = transform.Scale,
					};
				}

				a.ModelObject.Skeleton.EnableReading = true;
				return this.snapshot;
			}) ?? [];
		}
	}

	/// <summary>Sets the actor memory for the skeleton and initializes all bones.</summary>
	/// <param name="actor">The actor memory to set.</param>
	/// <param name="onlyAddBones">
	/// If true, only adds bones without setting up links or reading transforms.
	/// </param>
	protected virtual void SetActor(ObjectHandle<ActorMemory> actor, bool onlyAddBones = false)
	{
		this.Actor = actor;

		this.Clear();

		this.Actor.Do(a =>
		{
			if (!GposeService.Instance.IsGpose || a.ModelObject?.Skeleton == null)
				return;

			// Get all bones
			this.AddBones(a.ModelObject.Skeleton);

			if (a.MainHand?.Model?.Skeleton != null)
				this.AddBones(a.MainHand.Model.Skeleton, "mh_");

			if (a.OffHand?.Model?.Skeleton != null)
				this.AddBones(a.OffHand.Model.Skeleton, "oh_");

			// Create Bone links from the link database
			foreach ((string name, Bone bone) in this.Bones)
			{
				foreach (LinkedBones.LinkSet links in LinkedBones.Links)
				{
					if (links.Tribe != null && a.Customize?.Tribe != links.Tribe)
						continue;

					if (links.Gender != null && a.Customize?.Gender != links.Gender)
						continue;

					if (!links.Contains(name))
						continue;

					foreach (string linkedBoneName in links.Bones)
					{
						if (linkedBoneName == name)
							continue;

						Bone? linkedBone = this.GetBone(linkedBoneName);

						if (linkedBone == null)
							continue;

						bone.LinkedBones.Add(linkedBone);
					}
				}
			}
		});

		var snapshot = this.TakeSnapshot();
		var rootBones = new List<Bone>();
		foreach (var bone in this.Bones.Values)
		{
			if (bone.Parent == null)
				rootBones.Add(bone);
		}

		foreach (var rootBone in rootBones)
		{
			rootBone.ReadTransform(true, snapshot);
		}

		// Check for IVCS bones
		this.IsIVCS = this.Bones.Keys.Any(name => name.StartsWith("iv_"));

		// Notify that the skeleton has changed.
		// All properties that depend on the skeleton are prompted to update.
		this.RaisePropertyChanged(string.Empty);
	}

	/// <summary>
	/// Adds bones from a skeleton memory to the current skeleton.
	/// </summary>
	/// <param name="skeleton">The skeleton memory to add bones from.</param>
	/// <param name="namePrefix">An optional prefix to add to the bone names.</param>
	protected virtual void AddBones(SkeletonMemory skeleton, string? namePrefix = null)
	{
		for (int partialSkeletonIndex = 0; partialSkeletonIndex < skeleton.Length; partialSkeletonIndex++)
		{
			int index = partialSkeletonIndex;
			PartialSkeletonMemory partialSkeleton = skeleton[index];
			HkaPoseMemory? bestHkaPose = partialSkeleton.Pose1;

			if (bestHkaPose == null ||
				bestHkaPose.Skeleton?.Bones == null ||
				bestHkaPose.Skeleton?.ParentIndices == null ||
				bestHkaPose.Transforms == null)
			{
				Log.Verbose("Unable to find best pose for partial skeleton.");
				continue;
			}

			int count = bestHkaPose.Transforms.Length;

			// Load all bones first
			for (int boneIndex = 0; boneIndex < count; boneIndex++)
			{
				string originalName = bestHkaPose.Skeleton.Bones[boneIndex].Name.ToString();
				string name = ConvertBoneName(namePrefix, originalName);
				TransformMemory? transform = bestHkaPose.Transforms[boneIndex];

				if (!this.Bones.TryGetValue(name, out var currentBone))
				{
					currentBone = this.CreateBone(this, [transform], name, index);
					if (currentBone == null)
						throw new Exception($"Failed to create bone: {name}");

					this.Bones[name] = currentBone;
				}
				else
				{
					lock (currentBone.TransformMemories)
					{
						currentBone.TransformMemories.Add(transform);
					}
				}

				if (originalName == "n_root")
					currentBone.IsTransformLocked = true;
			}
		}

		// Set parents now that all bones are loaded
		for (int partialSkeletonIndex = 0; partialSkeletonIndex < skeleton.Length; partialSkeletonIndex++)
		{
			PartialSkeletonMemory partialSkeleton = skeleton[partialSkeletonIndex];
			HkaPoseMemory? bestHkaPose = partialSkeleton.Pose1;

			if (bestHkaPose == null
				|| bestHkaPose.Skeleton?.Bones == null
				|| bestHkaPose.Skeleton?.ParentIndices == null
				|| bestHkaPose.Transforms == null)
				continue;

			int count = bestHkaPose.Transforms.Length;
			for (int boneIndex = 0; boneIndex < count; boneIndex++)
			{
				int parentIndex = bestHkaPose.Skeleton.ParentIndices[boneIndex];
				string boneName = ConvertBoneName(namePrefix, bestHkaPose.Skeleton.Bones[boneIndex].Name.ToString());
				if (!this.Bones.TryGetValue(boneName, out var bone))
					continue;

				if (bone.Parent != null)
					continue;

				if (parentIndex >= 0)
				{
					string parentBoneName = ConvertBoneName(namePrefix, bestHkaPose.Skeleton.Bones[parentIndex].Name.ToString());
					if (this.Bones.TryGetValue(parentBoneName, out var parentBone) && parentBone is Bone typedParentBone)
					{
						bone.SetParent(typedParentBone);
					}
					else
					{
						Log.Warning($"Parent bone '{parentBoneName}' not found for bone '{bone.Name}'");
					}
				}
			}
		}
	}

	/// <summary>Raises the property changed event.</summary>
	/// <param name="propertyName">The name of the property that changed.</param>
	protected void RaisePropertyChanged(string propertyName)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	/// <summary> Attempts to find a hair bone by its pattern.</summary>
	/// <param name="suffix">The suffix of the hair bone.</param>
	/// <returns>The bone if found; otherwise, null.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected virtual Bone? FindHairBoneByPattern(string suffix)
	{
		var regex = new Regex($@"j_ex_h\d{{4}}_ke_{suffix}");
		return this.Bones.FirstOrDefault(b => regex.IsMatch(b.Key)).Value;
	}

	/// <summary>Creates a new bone instance.</summary>
	/// <param name="skeleton">The skeleton to which the bone belongs.</param>
	/// <param name="transformMemories">The list of transform memories for the bone.</param>
	/// <param name="name">The name of the bone.</param>
	/// <param name="partialSkeletonIndex">The index of the partial skeleton.</param>
	/// <returns>The created bone.</returns>
	protected virtual Bone CreateBone(Skeleton skeleton, List<TransformMemory> transformMemories, string name, int partialSkeletonIndex)
	{
		return new Bone(skeleton, transformMemories, name, partialSkeletonIndex);
	}
}