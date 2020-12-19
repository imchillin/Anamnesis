// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.PoseModule.Views
{
	using System;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Media;
	using System.Windows.Media.Media3D;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for CharacterPoseView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class Pose3DView : UserControl
	{
		private PerspectiveCamera camera;

		public Pose3DView()
		{
			this.InitializeComponent();

			this.camera = new PerspectiveCamera(new Point3D(0, 0.75, -4), new Vector3D(0, 0, 1), new Vector3D(0, 1, 0), 45);
			this.Viewport.Camera = this.camera;

			this.Viewport.Children.Add(new ModelVisual3D() { Content = new AmbientLight(Colors.White) });

			this.ContentArea.DataContext = this;

			////Anamnesis.Quaternion rootrot = Module.SkeletonViewModel.GetBone("Root").RootRotation;
			////this.root.Transform = new RotateTransform3D(new QuaternionRotation3D(new Quaternion(rootrot.X, rootrot.Y, rootrot.Z, rootrot.W)));
		}

		public SkeletonVisual3d? Skeleton { get; set; }

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.OnDataContextChanged(null, default);
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			SkeletonVisual3d? vm = this.DataContext as SkeletonVisual3d;

			if (vm == null)
				return;

			this.Viewport.Children.Remove(vm);
		}

		private void OnDataContextChanged(object? sender, DependencyPropertyChangedEventArgs e)
		{
			this.Skeleton = this.DataContext as SkeletonVisual3d;

			if (this.Skeleton == null)
				return;

			this.Viewport.Children.Clear();

			if (!this.Viewport.Children.Contains(this.Skeleton))
				this.Viewport.Children.Add(this.Skeleton);

			// position camera at average center position of skeleton
			if (this.Skeleton.Bones != null && this.Skeleton.Bones.Count > 0)
			{
				Rect3D bounds = default;

				Vector3D pos = this.Skeleton.Bones.First().ViewModel.Position.ToMedia3DVector();
				foreach (BoneVisual3d visual in this.Skeleton.Bones)
				{
					pos += visual.ViewModel.Position.ToMedia3DVector();

					Point3D point = visual.ViewModel.Position.ToMedia3DPoint();
					bounds.Union(point);
				}

				pos /= this.Skeleton.Bones.Count;
				double d = Math.Max(Math.Max(bounds.SizeX, bounds.SizeY), bounds.SizeZ);
				Point3D center = new Point3D(pos.X, pos.Y, pos.Z - (d + 3));
				this.camera.Position = center;
			}
		}

		private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SkeletonVisual3d? vm = this.DataContext as SkeletonVisual3d;

			if (vm == null)
				return;

			if (e.AddedItems == null || e.AddedItems.Count <= 0)
				return;

			BoneVisual3d? selected = e.AddedItems[0] as BoneVisual3d;

			if (selected == null)
				return;

			vm.Hover(selected, true);
			vm.Select(selected);
		}
	}
}
