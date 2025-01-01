// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using Anamnesis.Actor.Posing;
using Anamnesis.Actor.Posing.Visuals;
using Anamnesis.Services;
using PropertyChanged;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Enumeration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using XivToolsWpf;
using XivToolsWpf.Math3D.Extensions;

/// <summary>
/// Interaction logic for CharacterPoseView.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class Pose3DView : UserControl
{
	public readonly PerspectiveCamera Camera;
	public readonly RotateTransform3D CameraRotaion;
	public readonly TranslateTransform3D CameraPosition;

	private CancellationTokenSource? camUpdateCancelTokenSrc;
	private bool cameraIsTicking = false;

	public Pose3DView()
	{
		this.InitializeComponent();

		this.Camera = new PerspectiveCamera(new Point3D(0, 0.75, -4), new Vector3D(0, 0, 1), new Vector3D(0, 1, 0), 45);
		this.Viewport.Camera = this.Camera;

		this.CameraRotaion = new RotateTransform3D();
		QuaternionRotation3D camRot = new QuaternionRotation3D();
		camRot.Quaternion = CameraService.Instance.Camera?.Rotation3d.ToMedia3DQuaternion() ?? Quaternion.Identity;
		this.CameraRotaion.Rotation = camRot;
		this.CameraPosition = new TranslateTransform3D();
		Transform3DGroup transformGroup = new Transform3DGroup();
		transformGroup.Children.Add(this.CameraRotaion);
		transformGroup.Children.Add(this.CameraPosition);
		this.Camera.Transform = transformGroup;

		this.ContentArea.DataContext = this;

		if (CameraService.Instance.Camera != null)
		{
			CameraService.Instance.Camera.PropertyChanged += this.OnCameraChanged;
		}
	}

	public SkeletonEntity? Skeleton { get; set; }
	public SkeletonVisual3D? Visual { get; set; }

	public double CameraDistance { get; set; }
	public Quaternion CameraRotation { get; set; }

	public string BoneSearch { get; set; } = string.Empty;
	public IEnumerable<BoneEntity> BoneSearchResult
	{
		get
		{
			if (this.Skeleton == null)
				return Array.Empty<BoneEntity>();

			return string.IsNullOrWhiteSpace(this.BoneSearch)
				? this.Skeleton.Bones.Values.OfType<BoneEntity>()
				: this.Skeleton.Bones.Values.OfType<BoneEntity>().Where(b => FileSystemName.MatchesSimpleExpression($"*{this.BoneSearch}*", b.Name) || FileSystemName.MatchesSimpleExpression($"*{this.BoneSearch}*", b.Tooltip));
		}
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		this.OnDataContextChanged(null, default);
		Task.Run(this.UpdateCamera);
	}

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		this.camUpdateCancelTokenSrc?.Cancel();
		this.SkeletonRoot.Children.Clear();
	}

	private void OnDataContextChanged(object? sender, DependencyPropertyChangedEventArgs e)
	{
		this.Skeleton = this.DataContext as SkeletonEntity;

		if (this.Skeleton == null)
			return;

		this.SkeletonRoot.Children.Clear();

		this.Visual = new SkeletonVisual3D(this.Skeleton);

		if (!this.SkeletonRoot.Children.Contains(this.Visual))
			this.SkeletonRoot.Children.Add(this.Visual);

		this.SkeletonRoot.Children.Add(new ModelVisual3D() { Content = new AmbientLight(Colors.White) });

		this.FrameSkeleton();
	}

	private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (this.DataContext is not SkeletonEntity vm)
			return;

		if (e.AddedItems == null || e.AddedItems.Count <= 0)
			return;

		if (e.AddedItems[0] is not BoneEntity selected)
			return;

		this.Skeleton?.Hover(selected, true);
		vm.Select(selected);
	}

	private void OnCameraChanged(object? sender, PropertyChangedEventArgs? e)
	{
		if (CameraService.Instance == null || CameraService.Instance.Camera == null)
			return;
	}

	private void OnFrameClicked(object sender, RoutedEventArgs e)
	{
		this.FrameSkeleton();
	}

	private void FrameSkeleton()
	{
		// position camera at average center position of skeleton
		if (this.Skeleton == null || this.Skeleton.Bones == null || this.Skeleton.Bones.IsEmpty)
			return;

		Rect3D bounds = default;
		Vector3D? pos = null;

		foreach (var bone in this.Skeleton.Bones.Values.OfType<BoneEntity>())
		{
			var bonePos = new Vector3D(bone.Position.X, bone.Position.Y, bone.Position.Z);
			pos = pos == null ? bonePos : pos + bonePos;

			var point = new Point3D(bone.Position.X, bone.Position.Y, bone.Position.Z);
			bounds.Union(point);
		}

		if (pos == null)
			return;

		pos /= this.Skeleton.Bones.Count;

		this.CameraDistance = Math.Max(Math.Max(bounds.SizeX, bounds.SizeY), bounds.SizeZ);
	}

	private void Viewport_MouseMove(object sender, MouseEventArgs e)
	{
	}

	private void Viewport_MouseWheel(object sender, MouseWheelEventArgs e)
	{
		this.CameraDistance -= e.Delta / 120;
		this.CameraDistance = Math.Clamp(this.CameraDistance, 0, 300);
	}

	private async Task UpdateCamera()
	{
		if (this.cameraIsTicking)
			return;

		this.cameraIsTicking = true;
		this.camUpdateCancelTokenSrc = new CancellationTokenSource();
		var token = this.camUpdateCancelTokenSrc.Token;

		await Dispatch.MainThread();

		while (this.IsLoaded)
		{
			if (!this.IsVisible)
			{
				await Task.Delay(100, token);
				continue;
			}

			await Task.Delay(33, token);
			await Dispatch.MainThread();

			try
			{
				// If we're not in GPose, skip the update
				if (!GposeService.GetIsGPose())
					continue;

				if (this.Skeleton == null || this.Skeleton.Actor == null || CameraService.Instance.Camera == null)
					continue;

				// Update visual skeleton
				this.Visual?.Update();

				// TODO: allow the user to rotate camera with the mouse instead
				this.CameraRotation = CameraService.Instance.Camera.Rotation3d.ToMedia3DQuaternion();

				// Apply camera rotation
				QuaternionRotation3D rot = (QuaternionRotation3D)this.CameraRotaion.Rotation;
				rot.Quaternion = this.CameraRotation;
				this.CameraRotaion.Rotation = rot;

				// Apply camera position
				Point3D pos = this.Camera.Position;
				pos.Z = -this.CameraDistance;
				this.Camera.Position = pos;
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to update pose camera");
			}
		}

		this.cameraIsTicking = false;
	}
}
