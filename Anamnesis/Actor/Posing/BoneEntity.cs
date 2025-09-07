// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Posing;

using Anamnesis.Core;
using Anamnesis.Memory;
using Anamnesis.Services;
using PropertyChanged;
using System;
using System.Collections.Generic;

public enum BoneCategory
{
	Uncategorized,
	Body,
	Head,
	Hair,
	Met,
	Top,
	MainHand,
	OffHand,
}

/// <summary>
/// Derived class of <see cref="Bone"/> that adds additional functionality for UI-based operations.
/// </summary>
[AddINotifyPropertyChangedInterface]
public class BoneEntity : Bone
{
	public BoneEntity(SkeletonEntity skeleton, List<TransformMemory> transformMemories, string name, int partialSkeletonIndex, BoneEntity? parent = null)
		: base(skeleton, transformMemories, name, partialSkeletonIndex, parent)
	{
		this.Skeleton = skeleton;
	}

	/// <summary> Gets the skeleton that the bone belongs to.</summary>
	public new SkeletonEntity Skeleton { get; private set; }

	/// <summary>Gets the bone's parent.</summary>
	public new BoneEntity? Parent => base.Parent as BoneEntity;

	/// <summary> Gets the bone's category.</summary>
	/// <remarks>
	/// This is used to categorize bones for display in the user interface.
	/// </remarks>
	public BoneCategory Category => this.Name switch
	{
		_ when this.Name.StartsWith("mh_", StringComparison.Ordinal) => BoneCategory.MainHand,
		_ when this.Name.StartsWith("oh_", StringComparison.Ordinal) => BoneCategory.OffHand,
		_ when this.Name.Equals("j_ago", StringComparison.Ordinal) || this.Name.Equals("j_kao", StringComparison.Ordinal) => BoneCategory.Body,
		_ => this.PartialSkeletonIndex switch
		{
			0 => BoneCategory.Body,
			1 => BoneCategory.Head,
			2 => BoneCategory.Hair,
			3 => BoneCategory.Met,
			4 => BoneCategory.Top,
			_ => BoneCategory.Uncategorized,
		},
	};

	/// <summary>
	/// Gets or sets a value indicating whether the bone is enabled.
	/// </summary>
	public bool IsEnabled { get; set; } = true;

	/// <summary>
	/// Gets a value indicating whether the bone is selected.
	/// </summary>
	public bool IsSelected { get; internal set; }

	/// <summary>
	/// Gets a value indicating whether the bone is hovered.
	/// </summary>
	public bool IsHovered { get; internal set; }

	/// <summary>Gets the tooltip key for the bone.</summary>
	public virtual string TooltipKey => "Pose_" + this.Name;

	/// <summary>Gets or sets the tooltip for the bone.</summary>
	public string Tooltip
	{
		get
		{
			string? customName = CustomBoneNameService.GetBoneName(this.Name);
			if (!string.IsNullOrEmpty(customName))
				return customName;

			string str = LocalizationService.GetString(this.TooltipKey, true);
			return string.IsNullOrEmpty(str) ? this.Name : str;
		}
		set
		{
			if (string.IsNullOrEmpty(value) || LocalizationService.GetString(this.TooltipKey, true) == value)
			{
				CustomBoneNameService.SetBoneName(this.Name, null);
			}
			else
			{
				CustomBoneNameService.SetBoneName(this.Name, value);
			}
		}
	}

	/// <summary>Determines whether any ancestor bone is selected.</summary>
	/// <returns>True if any ancestor bone is selected; otherwise, false.</returns>
	public bool IsAncestorSelected()
	{
		for (BoneEntity? current = this; current != null; current = current.Parent)
		{
			if (current.IsSelected)
				return true;
		}

		return false;
	}

	/// <summary>Determines whether any ancestor bone is hovered.</summary>
	/// <returns>True if any ancestor bone is hovered; otherwise, false.</returns>
	public bool IsAncestorHovered()
	{
		for (BoneEntity? current = this; current != null; current = current.Parent)
		{
			if (current.IsHovered)
				return true;
		}

		return false;
	}
}
