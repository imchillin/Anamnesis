// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Posing;

using Anamnesis.Actor.Views;
using Anamnesis.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using XivToolsWpf;

/// <summary>Manages all <see cref="BoneView"/> in the application.</summary>
/// <remarks>
/// This is necessary as the UI elements are initialized out of order.
/// </remarks>
public class BoneViewManager : IDisposable
{
	private static readonly Lazy<BoneViewManager> s_lazy = new(() => new BoneViewManager());

	private readonly HashSet<BoneView> boneViews = new();
	private SkeletonEntity? skeleton;

	private BoneViewManager()
	{
	}

	/// <summary>
	/// Gets the singleton instance of the <see cref="BoneViewManager"/>.
	/// </summary>
	public static BoneViewManager Instance => s_lazy.Value;

	/// <summary>
	/// Gets the collection of bone views managed by this instance.
	/// </summary>
	public HashSet<BoneView> BoneViews => this.boneViews;

	/// <summary>
	/// Disposes the resources used by the <see cref="BoneViewManager"/>.
	/// </summary>
	public void Dispose()
	{
		if (this.skeleton != null)
			this.skeleton.PropertyChanged -= this.OnSkeletonPropertyChanged;

		this.boneViews.Clear();

		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Gets a list of all <see cref="BoneView"/> associated with a specific bone.
	/// </summary>
	/// <param name="bone">The bone to get views for.</param>
	/// <returns>A list of bone views associated with the specified bone.</returns>
	public List<BoneView> GetBoneViews(Bone bone) => this.BoneViews.Where(bv => bv.Bone == bone).ToList();

	/// <summary>
	/// Sets the skeleton entity to be managed by this instance.
	/// </summary>
	/// <param name="skeleton">The skeleton entity to set.</param>
	public void SetSkeleton(SkeletonEntity? skeleton)
	{
		if (this.skeleton == skeleton)
			return;

		if (this.skeleton != null)
			this.skeleton.PropertyChanged -= this.OnSkeletonPropertyChanged;

		this.skeleton = skeleton;

		if (this.skeleton != null)
			this.skeleton.PropertyChanged += this.OnSkeletonPropertyChanged;
	}

	/// <summary>
	/// Adds a bone view to the manager.
	/// </summary>
	/// <param name="boneView">The bone view to add.</param>
	/// <returns>True if the bone view was added; otherwise, false.</returns>
	public bool AddBoneView(BoneView boneView) => this.boneViews.Add(boneView);

	/// <summary>
	/// Removes a bone view from the manager.
	/// </summary>
	/// <param name="boneView">The bone view to remove.</param>
	/// <returns>True if the bone view was removed; otherwise, false.</returns>
	public bool RemoveBoneView(BoneView boneView) => this.boneViews.Remove(boneView);

	/// <summary>
	/// Refreshes all bone views managed by this instance.
	/// </summary>
	public async void Refresh()
	{
		await Dispatch.MainThread();

		foreach (var boneView in this.boneViews)
		{
			boneView.RedrawSkeleton();
			boneView.UpdateState();
		}
	}

	/// <summary>
	/// Handles property changes in the skeleton entity.
	/// </summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">The event data.</param>
	private async void OnSkeletonPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		Debug.Assert(this.skeleton != null, "Skeleton should not be null. Possible event handler leak");

		if (e.PropertyName == nameof(SkeletonEntity.FlipSides) || e.PropertyName == nameof(SkeletonEntity.Bones))
		{
			await Dispatch.MainThread();

			foreach (var boneView in this.boneViews)
			{
				// Bones that haven't loaded in yet won't have a name. Skip them.
				if (boneView.CurrentBoneName == null)
					continue;

				boneView.SetBone(boneView.CurrentBoneName);
				boneView.UpdateState();
			}

			return;
		}

		if (e.PropertyName == nameof(SkeletonEntity.SelectedBones) || e.PropertyName == nameof(SkeletonEntity.HoveredBones))
		{
			await Dispatch.MainThread();

			foreach (var boneView in this.boneViews)
			{
				boneView.UpdateState();
			}
		}
	}
}
