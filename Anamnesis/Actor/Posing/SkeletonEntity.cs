// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Posing;

using Anamnesis.Core;
using Anamnesis.Memory;
using Anamnesis.Services;
using PropertyChanged;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

/// <summary>
/// Derived class of <see cref="Skeleton"/> that adds additional functionality for UI-based operations.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SkeletonEntity"/> class.
/// </remarks>
/// <param name="actor">The actor memory associated with this skeleton.</param>
[AddINotifyPropertyChangedInterface]
public class SkeletonEntity(ObjectHandle<ActorMemory> actor) : Skeleton(actor)
{
	private List<BoneEntity>? selectedBonesCache;
	private List<BoneEntity>? hoveredBonesCache;

	/// <summary>Specifies the bone selection mode.</summary>
	public enum SelectMode
	{
		/// <summary>Override the current selection.</summary>
		Override,

		/// <summary>Add to the current selection.</summary>
		Add,

		/// <summary>
		/// Toggle the current selection.
		/// If a bone was selected, it will be unselected and vice versa.
		/// </summary>
		Toggle,
	}

	/// <summary>Gets a value indicating whether the skeleton has equipment bones.</summary>
	public bool HasEquipmentBones => this.Bones.Values.OfType<BoneEntity>().Any(b => b.Category == BoneCategory.Met || b.Category == BoneCategory.Top);

	/// <summary>Gets a value indicating whether the skeleton has weapon bones.</summary>
	public bool HasWeaponBones => this.Bones.Values.OfType<BoneEntity>().Any(b => b.Category == BoneCategory.MainHand || b.Category == BoneCategory.OffHand);

	/// <summary>Gets a value indicating whether the skeleton has any selected bones.</summary>
	public bool HasSelection => this.Bones.Values.OfType<BoneEntity>().Any(b => b.IsSelected);

	/// <summary>Gets a value indicating whether the skeleton has any hovered bones.</summary>
	public bool HasHover => this.Bones.Values.OfType<BoneEntity>().Any(b => b.IsHovered);

	/// <summary>Gets the selected bones.</summary>
	public IEnumerable<BoneEntity> SelectedBones => this.selectedBonesCache ??= this.Bones.Values.OfType<BoneEntity>().Where(b => b.IsSelected).ToList();

	/// <summary>Gets the hovered bones.</summary>
	public IEnumerable<BoneEntity> HoveredBones => this.hoveredBonesCache ??= this.Bones.Values.OfType<BoneEntity>().Where(b => b.IsHovered).ToList();

	/// <summary>Gets the count of selected linked bones.</summary>
	[DependsOn(nameof(SelectedBones), nameof(SelectedEnableLinkedBones))]
	public int SelectedLinkedCount => this.SelectedBones.SelectMany(bone => bone.LinkedBones).Distinct().Count();

	/// <summary>Gets or sets a value indicating whether to enable linked bones for all selected bones.</summary>
	public bool SelectedEnableLinkedBones
	{
		get => this.SelectedBones.Any() && this.SelectedBones.All(b => b.EnableLinkedBones);
		set
		{
			var selectedBones = this.SelectedBones.Where(b => b.LinkedBones.Count != 0).ToList();
			if (selectedBones.Any(b => b.EnableLinkedBones != value))
			{
				foreach (var bone in selectedBones)
					bone.EnableLinkedBones = value;

				this.RaisePropertyChanged(nameof(this.SelectedEnableLinkedBones));
			}
		}
	}

	/// <summary>Gets the selected linked bones.</summary>
	[DependsOn(nameof(SelectedBones), nameof(SelectedEnableLinkedBones))]
	public IEnumerable<BoneEntity> SelectedLinkedBones => this.SelectedBones.SelectMany(bone => bone.LinkedBones.OfType<BoneEntity>()).Distinct();

	/// <summary>Gets the parents of the selected bones.</summary>
	public IEnumerable<BoneEntity> SelectedBonesParents => this.SelectedBones.Select(bone => bone.Parent).Where(parent => parent != null).Cast<BoneEntity>().Distinct();

	/// <summary>
	/// Gets or sets a value indicating whether to flip the sides of the pose GUI.
	/// </summary>
	public bool FlipSides
	{
		get => SettingsService.Current.FlipPoseGuiSides;
		set
		{
			SettingsService.Current.FlipPoseGuiSides = value;
			this.RaisePropertyChanged(nameof(this.FlipSides));
		}
	}

	/// <summary>
	/// Returns a list of bones in the skeleton in a depth-first order.
	/// </summary>
	/// <param name="skeleton">The skeleton to traverse.</param>
	/// <returns>A list of bones in the skeleton in a depth-first order.</returns>
	public static IEnumerable<BoneEntity> TraverseSkeleton(SkeletonEntity skeleton)
	{
		if (skeleton.Bones == null || skeleton.Bones.IsEmpty)
			return [];

		Stack<BoneEntity> stack = new(skeleton.Bones.Values.OfType<BoneEntity>().Where(b => b.Parent == null).OrderBy(b => b.Name));
		List<BoneEntity> result = new(stack.Count);

		while (stack.Count > 0)
		{
			BoneEntity current = stack.Pop();
			result.Add(current);

			foreach (var child in current.Children.OfType<BoneEntity>().OrderBy(b => b.Name))
			{
				stack.Push(child);
			}
		}

		return result;
	}

	/// <inheritdoc/>
	public override BoneEntity? GetBone(string name) => base.GetBone(name) as BoneEntity;

	/// <summary>Writes the transforms of the selected bones to the skeleton.</summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void WriteSkeleton()
	{
		this.Actor?.Do(actor =>
		{
			if (actor.ModelObject?.Skeleton == null)
				return;

			if (this.HasSelection && PoseService.Instance.IsEnabled)
			{
				lock (HistoryService.Instance.LockObject)
				{
					try
					{
						actor.PauseSynchronization = true;

						// Get all selected bones
						var selectedBones = this.selectedBonesCache ??= this.Bones.Values.OfType<BoneEntity>().Where(b => b.IsSelected).ToList();

						// Filter out bones that are descendants of other selected bones
						HashSet<BoneEntity> ancestorBones = new();
						bool isAncestor;

						foreach (var bone in selectedBones)
						{
							isAncestor = false;
							foreach (var otherBone in selectedBones)
							{
								if (bone != otherBone && bone.HasAncestor(otherBone))
								{
									isAncestor = true;
									break;
								}
							}

							if (!isAncestor)
							{
								ancestorBones.Add(bone);
							}
						}

						// Write transforms for all ancestor bones
						foreach (var bone in ancestorBones)
							bone.WriteTransform();
					}
					catch (Exception ex)
					{
						Log.Error(ex, "Failed to write bone transforms");
						this.ClearSelection();
					}
					finally
					{
						actor.PauseSynchronization = false;
					}
				}
			}
		});
	}

	/// <summary>Selects a bone.</summary>
	/// <param name="bone">The bone to select.</param>
	public void Select(BoneEntity bone)
	{
		if (Application.Current?.Dispatcher == null)
			return;

		// Ensure input-related operations are done on the main thread
		Application.Current.Dispatcher.Invoke(() =>
		{
			SelectMode mode = SelectMode.Override;

			if (Keyboard.IsKeyDown(Key.LeftCtrl))
				mode = SelectMode.Toggle;

			if (Keyboard.IsKeyDown(Key.LeftShift))
				mode = SelectMode.Add;

			this.Select(new List<BoneEntity> { bone }, mode);
		});
	}

	/// <summary>Selects multiple bones.</summary>
	/// <param name="bones">The bones to select.</param>
	public void Select(IEnumerable<BoneEntity> bones)
	{
		// Ensure input-related operations are done on the main thread
		if (Application.Current?.Dispatcher == null)
			return;

		Application.Current.Dispatcher.Invoke(() =>
		{
			SelectMode mode = SelectMode.Override;

			if (Keyboard.IsKeyDown(Key.LeftCtrl))
				mode = SelectMode.Toggle;

			if (Keyboard.IsKeyDown(Key.LeftShift))
				mode = SelectMode.Add;

			this.Select(bones, mode);
		});
	}

	/// <summary>Selects multiple bones with a specified selection mode.</summary>
	/// <param name="bones">The bones to select.</param>
	/// <param name="mode">The selection mode.</param>
	public void Select(IEnumerable<BoneEntity> bones, SelectMode mode)
	{
		if (mode == SelectMode.Override)
			this.ClearSelection();

		foreach (var bone in bones)
			bone.IsSelected = mode != SelectMode.Toggle || !bone.IsSelected;

		this.InvalidateSelectedBonesCache();
	}

	/// <summary>Selects all head bones, including "j_kao".</summary>
	public void SelectHead()
	{
		this.ClearSelection();

		if (this.GetBone("j_kao") is not BoneEntity headBone)
			return;

		List<BoneEntity> headBones = new() { headBone };
		headBones.AddRange(headBone.GetDescendants().Cast<BoneEntity>());

		this.Select(headBones, SelectMode.Add);
	}

	/// <summary>Selects all body bones.</summary>
	public void SelectBody()
	{
		this.SelectHead();
		this.InvertSelection();

		List<BoneEntity> additionalBones = new();
		if (this.GetBone("j_kao") is BoneEntity headBone)
			additionalBones.Add(headBone);

		this.Select(additionalBones, SelectMode.Add);
	}

	/// <summary>Selects all weapon bones.</summary>
	public void SelectWeapons()
	{
		this.ClearSelection();
		var bonesToSelect = this.Bones.Values.OfType<BoneEntity>()
			.Where(b => b.Category == BoneCategory.MainHand || b.Category == BoneCategory.OffHand)
			.ToList();

		if (this.GetBone("n_buki_l") is BoneEntity boneLeft)
			bonesToSelect.Add(boneLeft);

		if (this.GetBone("n_buki_r") is BoneEntity boneRight)
			bonesToSelect.Add(boneRight);

		this.Select(bonesToSelect, SelectMode.Add);
	}

	/// <summary>Inverts the current bone selection.</summary>
	public void InvertSelection()
	{
		foreach (var bone in this.Bones.Values.OfType<BoneEntity>())
			bone.IsSelected = !bone.IsSelected;

		this.InvalidateSelectedBonesCache();
	}

	/// <summary>Clears the current bone selection.</summary>
	public void ClearSelection()
	{
		var selectedBones = this.SelectedBones.ToList();
		if (selectedBones.Count == 0)
			return;

		// Unselect all previously selected bones.
		foreach (var bone in selectedBones)
			bone.IsSelected = false;

		this.InvalidateSelectedBonesCache();
	}

	/// <summary>Sets the hover state of the target bone.</summary>
	/// <param name="bone">The bone to hover.</param>
	/// <param name="isHovered">The hover state.</param>
	public void Hover(BoneEntity bone, bool isHovered = true)
	{
		if (bone.IsHovered == isHovered)
			return;

		bone.IsHovered = isHovered;

		this.InvalidateHoveredBonesCache();
	}

	/// <summary>Clears the skeleton, including the selection.</summary>
	public override void Clear()
	{
		this.ClearSelection();
		base.Clear();
	}

	/// <summary>Reselects the previously selected bones.</summary>
	public void Reselect()
	{
		var selection = new List<BoneEntity>(this.SelectedBones);
		this.ClearSelection();
		this.Select(selection);
	}

	/// <inheritdoc/>
	protected override Bone CreateBone(Skeleton skeleton, List<TransformMemory> transformMemories, string name, int partialSkeletonIndex)
	{
		if (skeleton is SkeletonEntity skeletonEntity)
		{
			return new BoneEntity(skeletonEntity, transformMemories, name, partialSkeletonIndex);
		}
		else
		{
			throw new InvalidOperationException("Expected skeleton to be of type SkeletonEntity.");
		}
	}

	/// <summary>Invalidates the selected bones cache.</summary>
	private void InvalidateSelectedBonesCache()
	{
		this.selectedBonesCache = null;
		this.RaisePropertyChanged(nameof(this.HasSelection));
		this.RaisePropertyChanged(nameof(this.SelectedBones));
		this.RaisePropertyChanged(nameof(this.SelectedLinkedCount));
		this.RaisePropertyChanged(nameof(this.SelectedEnableLinkedBones));
		this.RaisePropertyChanged(nameof(this.SelectedLinkedBones));
		this.RaisePropertyChanged(nameof(this.SelectedBonesParents));
	}

	/// <summary>Invalidates the hovered bones cache.</summary>
	private void InvalidateHoveredBonesCache()
	{
		this.hoveredBonesCache = null;
		this.RaisePropertyChanged(nameof(this.HasHover));
		this.RaisePropertyChanged(nameof(this.HoveredBones));
	}
}
