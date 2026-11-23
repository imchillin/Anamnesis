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
using XivToolsWpf.Math3D;
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

	private const double MENU_BUFFER_PX = 45.0;

	private readonly List<BoneEntity> currentOverlappingBones = [];

	private CancellationTokenSource? camUpdateCancelTokenSrc;
	private bool cameraIsTicking = false;

	private Point lastMousePosition;
	private Point menuOpenPosition;
	private Point wheelMousePosition;
	private CameraInteractionMode interactionMode = CameraInteractionMode.None;

	private BoneEntity? hoveredBone;
	private BoneEntity? selectedMenuBone;
	private int selectedMenuIndex = -1;
	private bool isMenuOpen = false;
	private bool isWheelSelection = false;
	private bool isCursorHiddenForWheel = false;

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

	private static BoneVisual3D? FindBoneVisual(DependencyObject? visual, bool sphereOnly = true)
	{
		bool hasSphere = !sphereOnly;
		while (visual != null)
		{
			if (visual is Sphere)
				hasSphere = true;

			if (visual is BoneVisual3D boneVisual)
				return hasSphere ? boneVisual : null;

			visual = VisualTreeHelper.GetParent(visual);
		}

		return null;
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		if (this.SkeletonRoot.Children.Count == 0 && this.DataContext is SkeletonEntity)
		{
			this.OnDataContextChanged(null, default);
		}

		Task.Run(this.UpdateCamera);
	}

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		this.RestoreCursor();
		this.CloseBoneMenu(applySelection: false);
		this.UpdateHover3D(null);
		this.camUpdateCancelTokenSrc?.Cancel();
		this.SkeletonRoot.Children.Clear();
	}

	private void OnDataContextChanged(object? sender, DependencyPropertyChangedEventArgs e)
	{
		this.CloseBoneMenu(applySelection: false);
		this.UpdateHover3D(null);

		// Clear the existing children and dispose of the current visual
		this.SkeletonRoot.Children.Clear();
		if (this.Visual != null)
		{
			this.Visual.Dispose();
			this.Visual = null;
		}

		if (this.DataContext is not SkeletonEntity skeleton)
		{
			this.Skeleton = null;
			return;
		}

		// Set the new skeleton and create a new visual
		this.Skeleton = skeleton;
		this.Visual = new SkeletonVisual3D(this.Skeleton);
		this.Skeleton.Actor?.Do(a =>
		{
			this.Visual.Attach("mh_n_root", a.DrawData.MainHand?.Model?.Attach);
			this.Visual.Attach("oh_n_root", a.DrawData.OffHand?.Model?.Attach);
			this.Visual.Attach("orn_n_root", a.Ornament?.ModelObject?.Attach);
			this.Visual.Attach("pr1_n_root", a.DrawData.Prop1?.Model?.Attach);
			this.Visual.Attach("pr2_n_root", a.DrawData.Prop2?.Model?.Attach);
		});

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
		if (this.isMenuOpen)
		{
			bool isLeft = e.ChangedButton == MouseButton.Left;
			this.CloseBoneMenu(applySelection: isLeft);
			if (isLeft)
			{
				e.Handled = true;
				return;
			}
		}

		if (e.ChangedButton == MouseButton.Left)
		{
			Point mousePosition = e.GetPosition(this.Viewport);
			HitTestResult hitResult = VisualTreeHelper.HitTest(this.Viewport, mousePosition);

			if (hitResult is RayHitTestResult rayHitResult)
			{
				BoneVisual3D? boneVisual = FindBoneVisual(rayHitResult.VisualHit, sphereOnly: true);
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
			this.CloseBoneMenu(applySelection: false);

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
			this.CloseBoneMenu(applySelection: false);

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
		else if (this.interactionMode == CameraInteractionMode.None)
		{
			if (this.isCursorHiddenForWheel)
			{
				Point currentMousePosition = e.GetPosition(this.Viewport);
				if ((currentMousePosition - this.wheelMousePosition).LengthSquared > 16)
				{
					this.RestoreCursor();
					this.isWheelSelection = false;
				}
			}

			this.HandleViewportHover(e);
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
		if (this.isMenuOpen)
		{
			if (this.BoneMenuBorder.IsMouseOver)
			{
				this.HideCursorForWheel(e.GetPosition(this.Viewport));
			}

			this.CycleMenuSelection(e.Delta < 0 ? 1 : -1);
			e.Handled = true;
			return;
		}

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

	private List<BoneEntity> GetBonesUnderMouse(Point mousePosition)
	{
		if (this.Skeleton == null || this.Visual == null)
			return [];

		List<(BoneEntity Bone, double DistanceSq)> hits = [];
		Point3D cameraPos = this.Camera.Transform?.Transform(this.Camera.Position) ?? this.Camera.Position;

		VisualTreeHelper.HitTest(
			this.Viewport,
			null,
			result =>
			{
				if (result is RayHitTestResult rayHit)
				{
					BoneVisual3D? boneVisual = FindBoneVisual(rayHit.VisualHit, sphereOnly: true);
					if (boneVisual?.Bone != null)
					{
						hits.Add((boneVisual.Bone, (rayHit.PointHit - cameraPos).LengthSquared));
					}
				}

				return HitTestResultBehavior.Continue;
			},
			new PointHitTestParameters(mousePosition));

		return hits
			.OrderBy(h => h.DistanceSq)
			.Select(h => h.Bone)
			.Distinct()
			.ToList();
	}

	private void HandleViewportHover(MouseEventArgs e)
	{
		if (this.Skeleton == null || this.Visual == null || this.BoneMenuBorder.IsMouseOver)
			return;

		Point mousePos = e.GetPosition(this.Viewport);
		var bones = this.GetBonesUnderMouse(mousePos);

		if (bones.Count > 1)
		{
			if (!this.isMenuOpen || !this.currentOverlappingBones.SequenceEqual(bones))
			{
				this.ShowOrUpdateBoneMenu(bones, mousePos);
			}

			return;
		}

		if (this.isMenuOpen)
		{
			if (this.IsNearMenuOrTrigger(mousePos))
				return;

			this.CloseBoneMenu(applySelection: this.selectedMenuBone != null);
			this.UpdateHover3D(bones.FirstOrDefault());
			return;
		}

		this.UpdateHover3D(bones.FirstOrDefault());
	}

	private bool IsNearMenuOrTrigger(Point point)
	{
		const double bufferSq = MENU_BUFFER_PX * MENU_BUFFER_PX;

		if ((point - this.menuOpenPosition).LengthSquared <= bufferSq)
			return true;

		double left = Canvas.GetLeft(this.BoneMenuBorder);
		double top = Canvas.GetTop(this.BoneMenuBorder);
		double width = !double.IsNaN(this.BoneMenuBorder.Width) && this.BoneMenuBorder.Width > 0
			? this.BoneMenuBorder.Width
			: this.BoneMenuBorder.ActualWidth;
		double height = this.BoneMenuBorder.ActualHeight > 0 ? this.BoneMenuBorder.ActualHeight : 100;

		double right = left + width;
		double bottom = top + height;

		double dx = Math.Max(0, Math.Max(left - point.X, point.X - right));
		double dy = Math.Max(0, Math.Max(top - point.Y, point.Y - bottom));

		return (dx * dx) + (dy * dy) <= bufferSq;
	}

	private void ShowOrUpdateBoneMenu(List<BoneEntity> bones, Point position)
	{
		this.isWheelSelection = false;
		this.RestoreCursor();

		this.currentOverlappingBones.Clear();
		this.currentOverlappingBones.AddRange(bones);
		this.menuOpenPosition = position;
		this.isMenuOpen = true;

		if (this.selectedMenuBone != null && this.currentOverlappingBones.Contains(this.selectedMenuBone))
		{
			this.selectedMenuIndex = this.currentOverlappingBones.IndexOf(this.selectedMenuBone);
		}
		else
		{
			this.selectedMenuIndex = -1;
			this.selectedMenuBone = null;
			this.UpdateHover3D(null);
		}

		this.BoneMenuList.ItemsSource = null;
		this.BoneMenuList.ItemsSource = this.currentOverlappingBones;
		this.BoneMenuList.SelectedIndex = this.selectedMenuIndex;

		this.BoneMenuBorder.Visibility = Visibility.Visible;
		this.PositionMenu(position);
	}

	private void PositionMenu(Point position)
	{
		this.BoneMenuBorder.ClearValue(FrameworkElement.WidthProperty);
		this.BoneMenuBorder.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

		double menuWidth = Math.Max(180, this.BoneMenuBorder.DesiredSize.Width + 12);
		double menuHeight = this.BoneMenuBorder.DesiredSize.Height > 0 ? this.BoneMenuBorder.DesiredSize.Height : 100;
		this.BoneMenuBorder.Width = menuWidth;

		double x = position.X + 8;
		double y = position.Y + 8;

		if (x + menuWidth > this.Viewport.ActualWidth)
			x = Math.Max(0, position.X - menuWidth - 4);

		if (y + menuHeight > this.Viewport.ActualHeight)
			y = Math.Max(0, position.Y - menuHeight - 4);

		Canvas.SetLeft(this.BoneMenuBorder, x);
		Canvas.SetTop(this.BoneMenuBorder, y);
	}

	private void CloseBoneMenu(bool applySelection)
	{
		if (!this.isMenuOpen)
			return;

		this.isWheelSelection = false;
		this.RestoreCursor();

		var boneToSelect = this.selectedMenuBone;

		this.isMenuOpen = false;
		this.selectedMenuIndex = -1;
		this.selectedMenuBone = null;
		this.currentOverlappingBones.Clear();
		this.BoneMenuList.ItemsSource = null;
		this.BoneMenuBorder.Visibility = Visibility.Collapsed;

		this.UpdateHover3D(null);

		if (applySelection && boneToSelect != null)
		{
			this.Skeleton?.Select(boneToSelect);
		}
	}

	private void SelectMenuItem(int index)
	{
		if (index < 0 || index >= this.currentOverlappingBones.Count)
			return;

		this.selectedMenuIndex = index;
		this.selectedMenuBone = this.currentOverlappingBones[index];
		this.BoneMenuList.SelectedIndex = index;
		this.BoneMenuList.ScrollIntoView(this.selectedMenuBone);
		this.UpdateHover3D(this.selectedMenuBone);
	}

	private void CycleMenuSelection(int direction)
	{
		if (this.currentOverlappingBones.Count == 0)
			return;

		this.isWheelSelection = true;

		int count = this.currentOverlappingBones.Count;
		int nextIndex = this.selectedMenuIndex == -1
			? (direction > 0 ? 0 : count - 1)
			: (this.selectedMenuIndex + direction + count) % count;

		this.SelectMenuItem(nextIndex);
	}

	private void HideCursorForWheel(Point position)
	{
		this.wheelMousePosition = position;
		if (!this.isCursorHiddenForWheel)
		{
			Mouse.OverrideCursor = Cursors.None;
			this.isCursorHiddenForWheel = true;
		}
	}

	private void RestoreCursor()
	{
		if (this.isCursorHiddenForWheel)
		{
			Mouse.OverrideCursor = null;
			this.isCursorHiddenForWheel = false;
		}
	}

	private void UpdateHover3D(BoneEntity? bone)
	{
		if (this.hoveredBone == bone)
			return;

		if (this.hoveredBone != null)
		{
			this.Skeleton?.Hover(this.hoveredBone, false);
		}

		this.hoveredBone = bone;

		if (this.hoveredBone != null)
		{
			this.Skeleton?.Hover(this.hoveredBone, true);
		}
	}

	private void UpdateMenuSelectionFromMouse(DependencyObject? source)
	{
		while (source != null && source is not ListBoxItem)
		{
			source = source is Visual || source is Visual3D
				? VisualTreeHelper.GetParent(source)
				: LogicalTreeHelper.GetParent(source);
		}

		if (source is ListBoxItem { DataContext: BoneEntity bone })
		{
			this.SelectMenuItem(this.currentOverlappingBones.IndexOf(bone));
		}
	}

	private void OnMenuPreviewMouseMove(object sender, MouseEventArgs e)
	{
		if (this.isCursorHiddenForWheel)
		{
			Point currentPos = e.GetPosition(this.Viewport);
			if ((currentPos - this.wheelMousePosition).LengthSquared > 16)
			{
				this.RestoreCursor();
				this.isWheelSelection = false;
				this.UpdateMenuSelectionFromMouse(e.OriginalSource as DependencyObject);
			}
		}
	}

	private void OnMenuPreviewMouseWheel(object sender, MouseWheelEventArgs e)
	{
		if (this.isMenuOpen)
		{
			this.HideCursorForWheel(e.GetPosition(this.Viewport));
			this.CycleMenuSelection(e.Delta < 0 ? 1 : -1);
			e.Handled = true;
		}
	}

	private void OnViewportMouseLeave(object sender, MouseEventArgs e)
	{
		this.CloseBoneMenu(applySelection: true);
		this.UpdateHover3D(null);
	}

	private void OnBoneMenuItemMouseEnter(object sender, MouseEventArgs e)
	{
		if (this.isCursorHiddenForWheel)
			return;

		this.isWheelSelection = false;

		if (sender is FrameworkElement { DataContext: BoneEntity bone })
		{
			this.SelectMenuItem(this.currentOverlappingBones.IndexOf(bone));
		}
	}

	private void OnBoneMenuItemClick(object sender, MouseButtonEventArgs e)
	{
		if (!this.isWheelSelection && sender is FrameworkElement { DataContext: BoneEntity bone })
		{
			this.selectedMenuBone = bone;
		}

		this.CloseBoneMenu(applySelection: true);
		e.Handled = true;
	}

	private void OnPreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Escape && this.isMenuOpen)
		{
			this.CloseBoneMenu(applySelection: false);
			e.Handled = true;
		}
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
