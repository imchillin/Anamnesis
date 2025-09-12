// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using Anamnesis.Actor.Posing;
using PropertyChanged;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

/// <summary>
/// Interaction logic for PoseMatrixPage.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class PoseMatrixView : UserControl
{
	private IEnumerable<BoneEntity>? hairBonesCache;
	private IEnumerable<BoneEntity>? metBonesCache;
	private IEnumerable<BoneEntity>? topBonesCache;
	private IEnumerable<BoneEntity>? mainHandBonesCache;
	private IEnumerable<BoneEntity>? offHandBonesCache;

	public PoseMatrixView()
	{
		this.InitializeComponent();
		this.DataContextChanged += this.OnDataContextChanged;
	}

	public SkeletonEntity? Skeleton { get; private set; }

	[DependsOn(nameof(this.Skeleton))]
	public IEnumerable<BoneEntity> HairBones => this.hairBonesCache ??= this.GetBonesByCategory(BoneCategory.Hair);

	[DependsOn(nameof(this.Skeleton))]
	public IEnumerable<BoneEntity> MetBones => this.metBonesCache ??= this.GetBonesByCategory(BoneCategory.Met);

	[DependsOn(nameof(this.Skeleton))]
	public IEnumerable<BoneEntity> TopBones => this.topBonesCache ??= this.GetBonesByCategory(BoneCategory.Top);

	[DependsOn(nameof(this.Skeleton))]
	public IEnumerable<BoneEntity> MainHandBones => this.mainHandBonesCache ??= this.GetBonesByCategory(BoneCategory.MainHand);

	[DependsOn(nameof(this.Skeleton))]
	public IEnumerable<BoneEntity> OffHandBones => this.offHandBonesCache ??= this.GetBonesByCategory(BoneCategory.OffHand);

	public void OnDataContextChanged(object? sender, DependencyPropertyChangedEventArgs e)
	{
		if (this.DataContext is not SkeletonEntity skeleton)
			return;

		this.InvalidateCaches();
		this.Skeleton = skeleton;
	}

	private void OnGroupClicked(object sender, RoutedEventArgs e)
	{
		if (sender is DependencyObject ob)
		{
			GroupBox? groupBox = ob.FindParent<GroupBox>();
			if (groupBox == null || this.Skeleton == null)
				return;

			if (!Keyboard.IsKeyDown(Key.LeftCtrl))
				this.Skeleton.ClearSelection();

			List<BoneEntity> bones = groupBox.FindChildren<BoneView>()
				.Where(b => b.Bone != null)
				.Select(b => b.Bone!)
				.ToList();
			this.Skeleton.Select(bones);
		}
	}

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		this.DataContextChanged -= this.OnDataContextChanged;
	}

	private IEnumerable<BoneEntity> GetBonesByCategory(BoneCategory category)
	{
		return this.Skeleton != null
			? SkeletonEntity.TraverseSkeleton(this.Skeleton).Where(b => b.Category == category)
			: [];
	}

	private void InvalidateCaches()
	{
		this.hairBonesCache = null;
		this.metBonesCache = null;
		this.topBonesCache = null;
		this.mainHandBonesCache = null;
		this.offHandBonesCache = null;
	}
}
