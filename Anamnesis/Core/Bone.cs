// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Core;

using Anamnesis.Memory;
using Anamnesis.Services;
using PropertyChanged;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using XivToolsWpf.Math3D.Extensions;

/// <summary>
/// Represents a bone in a skeleton, providing mechanisms for reading and writing transforms,
/// synchronizing with memory, and handling linked bones.
/// </summary>
[AddINotifyPropertyChangedInterface]
public class Bone : ITransform
{
	public const float EQUALITY_TOLERANCE = 0.00001f;
	protected readonly ReaderWriterLockSlim TransformLock = new();
	private static readonly HashSet<string> s_attachmentBoneNames = new() { "n_buki_r", "n_buki_l", "j_buki_sebo_r", "j_buki_sebo_l" };
	private static bool s_scaleLinked = true;
	private bool hasInitialReading = false;

	/// <summary>
	/// Initializes a new instance of the <see cref="Bone"/> class.
	/// </summary>
	/// <param name="skeleton">The skeleton to which this bone belongs.</param>
	/// <param name="transformMemories">The list of transform memory objects linked to this bone.</param>
	/// <param name="name">The bone's internal name.</param>
	/// <param name="partialSkeletonIndex">The index of the bone in the partial skeleton.</param>
	/// <param name="parent">The parent bone, if any.</param>
	public Bone(Skeleton skeleton, List<TransformMemory> transformMemories, string name, int partialSkeletonIndex, Bone? parent = null)
	{
		this.Skeleton = skeleton;
		this.Name = name;
		this.PartialSkeletonIndex = partialSkeletonIndex;
		this.Parent = parent;
		this.TransformMemories = transformMemories;

		this.Position = this.TransformMemory?.Position ?? Vector3.Zero;
		this.Rotation = this.TransformMemory?.Rotation ?? Quaternion.Identity;
		this.Scale = this.TransformMemory?.Scale ?? Vector3.Zero;
	}

	/// <summary>Gets or sets the skeleton to which this bone belongs.</summary>
	public Skeleton Skeleton { get; protected set; }

	/// <summary>Gets all transform memory objects linked to this bone.</summary>
	public List<TransformMemory> TransformMemories { get; }

	/// <summary>Gets the primary transform memory object for this bone.</summary>
	public TransformMemory? TransformMemory => this.TransformMemories.FirstOrDefault();

	/// <summary> Gets or sets the parent bone of this bone.</summary>
	public Bone? Parent { get; protected set; }

	/// <summary>Gets or sets the index of the bone in the partial skeleton.</summary>
	public int PartialSkeletonIndex { get; protected set; }

	/// <summary>Gets or sets the list of child bones of this bone.</summary>
	public List<Bone> Children { get; protected set; } = new();

	/// <summary>Gets or sets the bone's internal name.</summary>
	public string Name { get; protected set; }

	/// <summary>Gets or sets the list of bones linked to this bone.</summary>
	public List<Bone> LinkedBones { get; set; } = new();

	/// <summary> Gets or sets a value indicating whether the transform of this bone is locked.</summary>
	public bool IsTransformLocked { get; set; } = false;

	/// <inheritdoc/>
	public bool CanTranslate => PoseService.Instance.FreezePositions && !this.IsTransformLocked;

	/// <summary>
	/// Gets or sets the parent-relative position of the bone.
	/// If the bone has no parent, this value will be relative to the root of the skeleton.
	/// </summary>
	/// <remarks>
	/// If you want to get character-relative position, use the position in <see cref="TransformMemory"/> property instead.
	/// </remarks>
	public Vector3 Position { get; set; }

	/// <inheritdoc/>
	public bool CanRotate => PoseService.Instance.FreezeRotation && !this.IsTransformLocked;

	/// <summary>
	/// Gets or sets the parent-relative rotation of the bone.
	/// If the bone has no parent, this value will be relative to the root of the skeleton.
	/// </summary>
	/// <remarks>
	/// If you want to get character-relative rotation, use the rotation in <see cref="TransformMemory"/> property instead.
	/// </remarks>
	public Quaternion Rotation { get; set; }

	/// <summary>Gets the root rotation of the bone.</summary>
	public Quaternion RootRotation => this.Parent == null
		? this.Skeleton.RootRotation
		: Quaternion.Normalize(this.Skeleton.RootRotation * this.Parent.TransformMemory!.Rotation);

	/// <inheritdoc/>
	public bool CanScale => PoseService.Instance.FreezeScale && !this.IsTransformLocked;

	/// <summary>Gets or sets the scale of the bone.</summary>
	public Vector3 Scale { get; set; }

	/// <summary>Gets a value indicating whether this bone is an attachment bone.</summary>
	/// <remarks>
	/// Attachment bones are bones that are used to attach items to a character, such as weapons or shields.
	/// </remarks>
	public bool IsAttachmentBone => s_attachmentBoneNames.Contains(this.Name);

	/// <inheritdoc/>
	public bool CanLinkScale => !this.IsAttachmentBone;

	/// <inheritdoc/>
	public bool ScaleLinked
	{
		get => this.IsAttachmentBone || s_scaleLinked;
		set => s_scaleLinked = value;
	}

	/// <summary>Gets or sets a value indicating whether linked bones are enabled.</summary>
	public bool EnableLinkedBones
	{
		get => this.LinkedBones.Count > 0 && SettingsService.Current.PosingBoneLinks.Get(this.Name, true);
		set
		{
			SettingsService.Current.PosingBoneLinks.Set(this.Name, value);
			foreach (var link in this.LinkedBones)
			{
				SettingsService.Current.PosingBoneLinks.Set(link.Name, value);
			}
		}
	}

	/// <summary>Sorts the specified bones by their depth in the skeleton hierarchy.</summary>
	/// <param name="bones">The enumerable collection of bones to sort.</param>
	/// <returns>The sorted list of bones.</returns>
	public static List<Bone> SortBonesByHierarchy(IEnumerable<Bone> bones)
		=> bones.OrderBy(bone => Bone.GetBoneDepth(bone)).ToList();

	/// <summary>
	/// Converts a local (parent-relative) transform to model (character-relative) space.
	/// </summary>
	/// <param name="localTransform">The local transform.</param>
	/// <param name="parentTransform">The parent's character-relative transform.</param>
	/// <returns>The model space transform.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Transform LocalToModelSpace(Transform localTransform, Transform parentTransform)
	{
		// Apply the parent's character-relative rotation to the local position
		Vector3 modelPosition = Vector3.Transform(localTransform.Position, parentTransform.Rotation);
		modelPosition += parentTransform.Position;

		// Apply the parent's character-relative rotation to the local rotation
		Quaternion modelRotation = Quaternion.Normalize(parentTransform.Rotation * localTransform.Rotation);

		return new Transform
		{
			Position = modelPosition,
			Rotation = modelRotation,
			Scale = localTransform.Scale, // Scale is not affected by parent transform
		};
	}

	/// <summary>
	/// Converts a model (character-relative) transform to local (parent-relative) space.
	/// </summary>
	/// <param name="modelTransform">The model transform.</param>
	/// <param name="parentTransform">The parent's character-relative transform.</param>
	/// <returns>The local space transform.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Transform ModelToLocalSpace(Transform modelTransform, Transform parentTransform)
	{
		// Subtract the parent's character-relative position from the model position
		Vector3 localPosition = modelTransform.Position - parentTransform.Position;

		// Apply the inverse of the parent's character-relative rotation to the local position
		Quaternion parentRotInverse = Quaternion.Conjugate(parentTransform.Rotation);
		localPosition = Vector3.Transform(localPosition, parentRotInverse);

		// Apply the inverse of the parent's character-relative rotation to the model rotation
		Quaternion localRotation = Quaternion.Normalize(parentRotInverse * modelTransform.Rotation);

		return new Transform
		{
			Position = localPosition,
			Rotation = localRotation,
			Scale = modelTransform.Scale, // Scale is not affected by parent transform
		};
	}

	/// <summary>
	/// Gets the depth of the specified bone in the skeleton hierarchy.
	/// </summary>
	/// <param name="bone">The bone whose depth is to be determined.</param>
	/// <returns>The depth of the bone in the hierarchy.</returns>
	public static int GetBoneDepth(Bone bone)
	{
		int depth = 0;
		while (bone.Parent != null)
		{
			depth++;
			bone = bone.Parent;
		}

		return depth;
	}

	/// <summary>Synchronizes the bone with its transform memories.</summary>
	public virtual void Synchronize()
	{
		foreach (TransformMemory transformMemory in this.TransformMemories)
			transformMemory.Synchronize();

		this.ReadTransform();
	}

	/// <summary>Reads the transform of the bone from game memory or a snapshot.</summary>
	/// <remarks>
	/// Snapshots are primarily used by the skeleton object to optimize memory reads.
	/// </remarks>
	/// <param name="readChildren">Whether to read the transforms of child bones.</param>
	/// <param name="snapshot">An optional snapshot of transforms to use instead of memory.</param>
	public virtual void ReadTransform(bool readChildren = false, Dictionary<string, Transform>? snapshot = null)
	{
		if (this.TransformMemories.Count == 0)
			return;

		Stack<Bone> bonesToProcess = new();
		bonesToProcess.Push(this);

		while (bonesToProcess.Count > 0)
		{
			Bone currentBone = bonesToProcess.Pop();

			// Use snapshot if available, otherwise use values from memory
			// Note: Values are expected to be in model space
			Transform localTransform;
			if (snapshot != null && snapshot.TryGetValue(currentBone.Name, out var transform))
			{
				localTransform = transform;
			}
			else
			{
				var transformMemory = currentBone.TransformMemories[0];
				localTransform = new Transform
				{
					Position = transformMemory.Position,
					Rotation = transformMemory.Rotation,
					Scale = transformMemory.Scale,
				};
			}

			// Convert the character-relative transform into a parent-relative transform
			if (currentBone.Parent != null)
			{
				Transform parentTransform;
				if (snapshot != null && snapshot.TryGetValue(currentBone.Parent.Name, out var parentSnapshot))
				{
					parentTransform = parentSnapshot;
				}
				else
				{
					var parentTransformMemory = currentBone.Parent.TransformMemories[0];
					parentTransform = new Transform
					{
						Position = parentTransformMemory.Position,
						Rotation = parentTransformMemory.Rotation,
						Scale = parentTransformMemory.Scale,
					};
				}

				localTransform = ModelToLocalSpace(localTransform, parentTransform);
			}

			currentBone.TransformLock.EnterReadLock();
			try
			{
				currentBone.Position = localTransform.Position;
				currentBone.Rotation = localTransform.Rotation;
				currentBone.Scale = localTransform.Scale;
				currentBone.hasInitialReading = true;
			}
			finally
			{
				currentBone.TransformLock.ExitReadLock();
			}

			if (readChildren)
			{
				foreach (var child in currentBone.Children)
				{
					bonesToProcess.Push(child);
				}
			}
		}
	}

	/// <summary>Writes the transform of the bone to game memory.</summary>
	/// <param name="writeChildren">Whether to write the transforms of child bones.</param>
	/// <param name="writeLinked">Whether to write the transforms of linked bones.</param>
	public virtual void WriteTransform(bool writeChildren = true, bool writeLinked = true)
	{
		Stack<(Bone Bone, bool WriteLinked, Transform? PrecalcParent)> bonesToProcess = new();
		bonesToProcess.Push((this, writeLinked, null));

		while (bonesToProcess.Count > 0)
		{
			var (currentBone, currentWriteLinked, precalcParent) = bonesToProcess.Pop();
			var transformMemories = currentBone.TransformMemories;

			if (transformMemories.Count == 0)
				throw new System.InvalidOperationException($"Bone \"{currentBone.Name}\" does not have any transform memories.");

			// Carry out initial transform if it hasn't been done yet
			if (!currentBone.hasInitialReading)
			{
				currentBone.ReadTransform();
			}

			Transform modelTransform = new()
			{
				Position = currentBone.Position,
				Rotation = currentBone.Rotation,
				Scale = currentBone.Scale,
			};

			if (currentBone.Parent != null)
			{
				// Workaround: Instead of retrieving the parent transform from game memory, use the local transform and convert it
				// manually. This prevents bone drifting as the skeleton is unstable due to ocassional mid-frame updates.
				Transform parentTransform = precalcParent ?? ComputeModelSpaceTransform(currentBone.Parent);
				modelTransform = LocalToModelSpace(modelTransform, parentTransform);
			}

			currentBone.TransformLock.EnterWriteLock();
			try
			{
				foreach (TransformMemory transformMemory in transformMemories)
				{
					transformMemory.EnableReading = false;
				}

				bool changed = false;

				foreach (TransformMemory transformMemory in transformMemories)
				{
					if (currentBone.CanTranslate && !transformMemory.Position.IsApproximately(modelTransform.Position, EQUALITY_TOLERANCE))
					{
						transformMemory.Position = modelTransform.Position;
						changed = true;
					}

					if (currentBone.CanScale && !transformMemory.Scale.IsApproximately(modelTransform.Scale, EQUALITY_TOLERANCE))
					{
						transformMemory.Scale = modelTransform.Scale;
						changed = true;
					}

					if (currentBone.CanRotate && !transformMemory.Rotation.IsApproximately(modelTransform.Rotation, EQUALITY_TOLERANCE))
					{
						transformMemory.Rotation = modelTransform.Rotation;
						changed = true;
					}
				}

				if (changed)
				{
					if (currentWriteLinked && currentBone.EnableLinkedBones)
					{
						foreach (var link in currentBone.LinkedBones)
						{
							// TODO: Figure out how to sync linked bone positions
							link.Rotation = currentBone.Rotation;
							link.Scale = currentBone.Scale;
							bonesToProcess.Push((link, false, null));
						}
					}

					if (writeChildren)
					{
						foreach (var child in currentBone.Children)
						{
							if (PoseService.Instance.EnableParenting)
							{
								bonesToProcess.Push((child, currentWriteLinked, modelTransform));
							}
							else
							{
								child.ReadTransform(true);
							}
						}
					}
				}

				foreach (TransformMemory transformMemory in transformMemories)
				{
					transformMemory.EnableReading = true;
				}
			}
			finally
			{
				currentBone.TransformLock.ExitWriteLock();
			}
		}
	}

	/// <summary>Gets the descendants of this bone.</summary>
	/// <returns>The list of descendant bones.</returns>
	public List<Bone> GetDescendants()
	{
		List<Bone> descendants = new();
		Stack<Bone> stack = new(this.Children);

		while (stack.Count > 0)
		{
			Bone current = stack.Pop();
			descendants.Add(current);
			foreach (Bone child in current.Children)
			{
				stack.Push(child);
			}
		}

		return descendants;
	}

	/// <summary>Determines whether the bone has the specified target bone as an ancestor.</summary>
	/// <param name="target">The target bone to check.</param>
	/// <returns>True if the target bone is an ancestor of this bone; otherwise, false.</returns>
	public bool HasAncestor(Bone target)
	{
		Bone? current = this.Parent;
		while (current != null)
		{
			if (current == target)
				return true;
			current = current.Parent;
		}

		return false;
	}

	/// <summary>Returns a string that represents the current object.</summary>
	/// <returns>A string that represents the current object.</returns>
	public override string ToString() => base.ToString() + "(" + this.Name + ")";

	/// <summary>Sets the parent of this bone.</summary>
	/// <param name="newParent">The new parent bone.</param>
	internal virtual void SetParent<TBone>(TBone newParent)
		where TBone : Bone?
	{
		this.Parent?.Children.Remove(this);
		this.Parent = newParent;
		newParent?.Children.Add(this);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Transform ComputeModelSpaceTransform(Bone bone)
	{
		int depth = 0;
		Bone? current = bone;
		while (current != null)
		{
			depth++;
			current = current.Parent;
		}

		Bone[] ancestors = System.Buffers.ArrayPool<Bone>.Shared.Rent(depth);
		try
		{
			current = bone;
			int index = 0;
			while (current != null && index < depth)
			{
				ancestors[index++] = current;
				current = current.Parent;
			}

			Transform modelTransform = new()
			{
				Position = Vector3.Zero,
				Rotation = Quaternion.Identity,
				Scale = Vector3.One,
			};

			for (int i = depth - 1; i >= 0; i--)
			{
				Bone ancestor = ancestors[i];
				Transform localTransform = new()
				{
					Position = ancestor.Position,
					Rotation = ancestor.Rotation,
					Scale = ancestor.Scale,
				};

				modelTransform = LocalToModelSpace(localTransform, modelTransform);
			}

			return modelTransform;
		}
		finally
		{
			System.Buffers.ArrayPool<Bone>.Shared.Return(ancestors);
		}
	}
}
