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

public enum CameraInteractionMode
{
	None,
	Panning,
	Rotating,
	Selection,
}

/// <summary>
/// Interaction logic for Pose3DView.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class Pose3DView : UserControl
{
	public readonly PerspectiveCamera Camera;
	public readonly RotateTransform3D CameraRotation;
	public readonly TranslateTransform3D CameraPosition;

	private CancellationTokenSource? camUpdateCancelTokenSrc;
	private bool cameraIsTicking = false;

	private Point lastMousePosition;

	private CameraInteractionMode interactionMode = CameraInteractionMode.None;

	public Pose3DView()
	{
		this.InitializeComponent();

		this.Camera = new PerspectiveCamera(new Point3D(0, 0.75, -4), new Vector3D(0, 0, 1), new Vector3D(0, 1, 0), 45);
		this.Viewport.Camera = this.Camera;

		this.CameraRotation = new RotateTransform3D();
		QuaternionRotation3D camRot = new()
		{
			Quaternion = CameraService.Instance.Camera?.Rotation3d.ToMedia3DQuaternion() ?? Quaternion.Identity,
		};
		this.CameraRotation.Rotation = camRot;
		this.CameraPosition = new TranslateTransform3D();

		Transform3DGroup transformGroup = new();
		transformGroup.Children.Add(this.CameraRotation);
		transformGroup.Children.Add(this.CameraPosition);
		this.Camera.Transform = transformGroup;

		this.ContentArea.DataContext = this;
	}

	public SkeletonEntity? Skeleton { get; set; }
	public SkeletonVisual3D? Visual { get; set; }

	public double CameraDistance { get; set; }

	public string BoneSearch { get; set; } = string.Empty;
	public IEnumerable<BoneEntity> BoneSearchResult
	{
		get
		{
			if (this.Skeleton == null)
				return [];

			var bones = SkeletonEntity.TraverseSkeleton(this.Skeleton);

			if (string.IsNullOrWhiteSpace(this.BoneSearch))
				return bones;

			string searchPattern = $"*{this.BoneSearch}*";
			return bones.Where(b => FileSystemName.MatchesSimpleExpression(searchPattern, b.Name) || FileSystemName.MatchesSimpleExpression(searchPattern, b.Tooltip));
		}
	}

	public bool SyncWithGameCamera { get; set; } = true;

	private static BoneVisual3D? FindBoneVisual(DependencyObject visual)
	{
		while (visual != null)
		{
			if (visual is BoneVisual3D boneVisual)
				return boneVisual;

			visual = VisualTreeHelper.GetParent(visual);
		}

		return null;
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		Task.Run(this.UpdateCamera);
	}

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		this.camUpdateCancelTokenSrc?.Cancel();
		this.SkeletonRoot.Children.Clear();
	}

	private void OnDataContextChanged(object? sender, DependencyPropertyChangedEventArgs e)
	{
		if (this.DataContext is not SkeletonEntity skeleton)
			return;

		// Clear the existing children and dispose of the current visual
		this.SkeletonRoot.Children.Clear();
		if (this.Visual != null)
		{
			this.Visual.Dispose();
			this.Visual = null;
		}

		// Set the new skeleton and create a new visual
		this.Skeleton = skeleton;
		this.Visual = new SkeletonVisual3D(this.Skeleton);

		// Add the new visual to the SkeletonRoot
		this.SkeletonRoot.Children.Add(this.Visual);
		this.SkeletonRoot.Children.Add(new ModelVisual3D() { Content = new AmbientLight(Colors.White) });

		// Frame the skeleton into view
		this.FrameSkeleton();
	}

	private void OnResetCameraButtonClicked(object sender, RoutedEventArgs e)
	{
		this.FrameSkeleton();
	}

	private void FrameSkeleton()
	{
		// Position camera at average center position of skeleton
		if (this.Skeleton == null || this.Skeleton.Bones == null || this.Skeleton.Bones.IsEmpty)
			return;

		Rect3D bounds = default;
		foreach (var bone in this.Skeleton.Bones.Values.OfType<BoneEntity>())
		{
			bounds.Union(new Point3D(bone.Position.X, bone.Position.Y, bone.Position.Z));
		}

		this.CameraDistance = Math.Max(Math.Max(bounds.SizeX, bounds.SizeY), bounds.SizeZ);

		if (this.Visual != null)
		{
			foreach (BoneVisual3D bone in this.Visual.Children.OfType<BoneVisual3D>())
			{
				bone.OnCameraUpdated(this);
			}
		}
	}

	private void OnViewportMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Left)
		{
			Point mousePosition = e.GetPosition(this.Viewport);
			HitTestResult hitResult = VisualTreeHelper.HitTest(this.Viewport, mousePosition);

			if (hitResult is RayHitTestResult rayHitResult)
			{
				BoneVisual3D? boneVisual = FindBoneVisual(rayHitResult.VisualHit);
				if (boneVisual != null)
				{
					this.Skeleton?.Select(boneVisual.Bone);
					e.Handled = true;
				}
			}
		}
		else if (e.ChangedButton == MouseButton.Middle)
		{
			if (Keyboard.IsKeyDown(Key.LeftShift))
			{
				this.interactionMode = CameraInteractionMode.Panning;
			}
			else
			{
				this.interactionMode = CameraInteractionMode.Rotating;
			}

			this.lastMousePosition = e.GetPosition(this.Viewport);
			this.Viewport.CaptureMouse();
			e.Handled = true;
		}
	}

	private void OnViewportMouseMove(object sender, MouseEventArgs e)
	{
		if (this.interactionMode == CameraInteractionMode.Panning)
		{
			Point currentMousePosition = e.GetPosition(this.Viewport);
			Vector delta = Point.Subtract(currentMousePosition, this.lastMousePosition);

			double panSpeedFactor = 0.005;
			double effectivePanSpeed = SettingsService.Current.ViewportPanSpeed * panSpeedFactor;

			// Transform the delta vector by the camera's rotation
			Vector3D panVector = new(delta.X * effectivePanSpeed, delta.Y * effectivePanSpeed, 0);
			Matrix3D rotationMatrix = this.CameraRotation.Value;
			Vector3D transformedPanVector = rotationMatrix.Transform(panVector);

			this.CameraPosition.OffsetX += transformedPanVector.X;
			this.CameraPosition.OffsetY += transformedPanVector.Y;
			this.CameraPosition.OffsetZ += transformedPanVector.Z;

			this.lastMousePosition = currentMousePosition;
			e.Handled = true;
		}
		else if (this.interactionMode == CameraInteractionMode.Rotating && !this.SyncWithGameCamera)
		{
			Point currentMousePosition = e.GetPosition(this.Viewport);
			Vector delta = Point.Subtract(currentMousePosition, this.lastMousePosition);

			double rotationSpeedFactor = 0.5;
			double effectiveRotationSpeed = SettingsService.Current.ViewportRotationSpeed * rotationSpeedFactor;
			QuaternionRotation3D rot = (QuaternionRotation3D)this.CameraRotation.Rotation;
			Quaternion q = rot.Quaternion;

			q *= new Quaternion(new Vector3D(0, 1, 0), -delta.X * effectiveRotationSpeed);
			q *= new Quaternion(new Vector3D(1, 0, 0), delta.Y * effectiveRotationSpeed);

			rot.Quaternion = q;
			this.CameraRotation.Rotation = rot;

			this.lastMousePosition = currentMousePosition;
			e.Handled = true;
		}
	}

	private void OnViewportMouseUp(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Middle)
		{
			this.interactionMode = CameraInteractionMode.None;
			this.Viewport.ReleaseMouseCapture();
			e.Handled = true;
		}
	}

	private void OnViewportMouseWheel(object sender, MouseWheelEventArgs e)
	{
		double zoomSpeedFactor = 0.2;
		double effectiveZoomSpeed = SettingsService.Current.ViewportZoomSpeed * zoomSpeedFactor;

		this.CameraDistance -= e.Delta / 120 * effectiveZoomSpeed;
		this.CameraDistance = Math.Clamp(this.CameraDistance, 0, 300);

		if (this.Visual != null)
		{
			foreach (BoneVisual3D bone in this.Visual.Children.OfType<BoneVisual3D>())
			{
				bone.OnCameraUpdated(this);
			}
		}

		e.Handled = true;
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
			// If we're not in GPose or the view is not visible, skip the update
			if (!this.IsVisible || !GposeService.Instance.IsGpose)
			{
				await Task.Delay(100, token);
				continue;
			}

			await Task.Delay(33, token);
			await Dispatch.MainThread();

			try
			{
				// Validate that all objects are valid and we're in GPose
				if (!GposeService.Instance.IsGpose || this.Skeleton == null || this.Skeleton.Actor == null || CameraService.Instance.Camera == null)
					continue;

				// Update visual skeleton
				this.Visual?.Update();

				// Apply camera rotation
				if (this.SyncWithGameCamera)
				{
					QuaternionRotation3D rot = (QuaternionRotation3D)this.CameraRotation.Rotation;
					rot.Quaternion = CameraService.Instance.Camera.Rotation3d.ToMedia3DQuaternion();
					this.CameraRotation.Rotation = rot;
				}

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
