// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using Anamnesis.Actor.Posing;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

/// <summary>
/// Interaction logic for PoseMatrixPage.xaml.
/// </summary>
public partial class PoseMatrixView : UserControl
{
	public PoseMatrixView()
	{
		this.InitializeComponent();

		this.DataContextChanged += this.OnDataContextChanged;
	}

	public SkeletonEntity? Skeleton { get; private set; }
	public IEnumerable<BoneEntity> HairBones => this.Skeleton?.Bones.Values.OfType<BoneEntity>().Where(b => b.Category == BoneCategory.Hair) ?? Enumerable.Empty<BoneEntity>();
	public IEnumerable<BoneEntity> MetBones => this.Skeleton?.Bones.Values.OfType<BoneEntity>().Where(b => b.Category == BoneCategory.Met) ?? Enumerable.Empty<BoneEntity>();
	public IEnumerable<BoneEntity> TopBones => this.Skeleton?.Bones.Values.OfType<BoneEntity>().Where(b => b.Category == BoneCategory.Top) ?? Enumerable.Empty<BoneEntity>();
	public IEnumerable<BoneEntity> MainHandBones => this.Skeleton?.Bones.Values.OfType<BoneEntity>().Where(b => b.Category == BoneCategory.MainHand) ?? Enumerable.Empty<BoneEntity>();
	public IEnumerable<BoneEntity> OffHandBones => this.Skeleton?.Bones.Values.OfType<BoneEntity>().Where(b => b.Category == BoneCategory.OffHand) ?? Enumerable.Empty<BoneEntity>();

	public void OnDataContextChanged(object? sender, DependencyPropertyChangedEventArgs e)
	{
		if (this.DataContext is not SkeletonEntity skeleton)
			return;

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
}
