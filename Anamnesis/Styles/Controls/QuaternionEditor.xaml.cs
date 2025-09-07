// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Controls;

using Anamnesis.Keyboard;
using Anamnesis.Memory;
using Anamnesis.Services;
using PropertyChanged;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using XivToolsWpf.DependencyProperties;
using XivToolsWpf.Math3D;
using XivToolsWpf.Math3D.Extensions;
using static Anamnesis.Styles.Controls.SliderInputBox;
using CmQuaternion = System.Numerics.Quaternion;
using CmVector = System.Numerics.Vector3;
using Color = System.Windows.Media.Color;
using Quaternion = System.Windows.Media.Media3D.Quaternion;

/// <summary>
/// Interaction logic for QuaternionEditor.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class QuaternionEditor : UserControl, INotifyPropertyChanged
{
	public static readonly IBind<CmQuaternion> ValueDp = Binder.Register<CmQuaternion, QuaternionEditor>(nameof(Value), OnValueChanged);
	public static readonly IBind<CmQuaternion?> RootRotationDp = Binder.Register<CmQuaternion?, QuaternionEditor>(nameof(RootRotation), OnRootRotationChanged);
	public static readonly IBind<decimal> TickDp = Binder.Register<decimal, QuaternionEditor>(nameof(TickFrequency));

	public static readonly IBind<CmQuaternion> ValueQuatDp = Binder.Register<CmQuaternion, QuaternionEditor>(nameof(ValueQuat), OnValueQuatChanged);
	public static readonly IBind<CmVector> EulerDp = Binder.Register<CmVector, QuaternionEditor>(nameof(Euler), OnEulerChanged);

	private readonly RotationGizmo rotationGizmo;
	private readonly bool isInitialized = false;
	private readonly Sphere pivotSphere;
	private bool lockdp = false;

	private CmQuaternion worldSpaceDelta;
	private bool worldSpace;

	// [FOR DEBUGGING]
	//// Represents the tangent along which the mouse is moving for the linear drag
	// private readonly Line axisProjectionLine;

	//// Represents the plane normal
	// private readonly Line planeNormalLine;

	//// Represents the tangent plane, created from the pivot point and the plane normal
	// private readonly PlaneVisual3D tangentPlaneVisual;

	//// Represents the intersection point of the raycast mouse position onto the tangent plane
	// private readonly Sphere intersectionSphere;

	//// Represents the near point of the unprojected camera ray
	// private readonly Sphere nearSphere;

	//// Represents the far point of the unprojected camera ray
	// private readonly Sphere farSphere;

	//// Represents the axis plane (i.e., the axis circle's plane)
	// private readonly PlaneVisual3D axisPlane;

	public QuaternionEditor()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		HotkeyService.RegisterHotkeyHandler("QuaternionEditor.RotateXPlus", (k) => this.Rotate(k, 1, 0, 0));
		HotkeyService.RegisterHotkeyHandler("QuaternionEditor.RotateXMinus", (k) => this.Rotate(k, -1, 0, 0));
		HotkeyService.RegisterHotkeyHandler("QuaternionEditor.RotateYPlus", (k) => this.Rotate(k, 0, 1, 0));
		HotkeyService.RegisterHotkeyHandler("QuaternionEditor.RotateYMinus", (k) => this.Rotate(k, 0, -1, 0));
		HotkeyService.RegisterHotkeyHandler("QuaternionEditor.RotateZPlus", (k) => this.Rotate(k, 0, 0, 1));
		HotkeyService.RegisterHotkeyHandler("QuaternionEditor.RotateZMinus", (k) => this.Rotate(k, 0, 0, -1));
		HotkeyService.RegisterHotkeyHandler("QuaternionEditor.RotateXPlusFast", (k) => this.Rotate(k, 10, 0, 0));
		HotkeyService.RegisterHotkeyHandler("QuaternionEditor.RotateXMinusFast", (k) => this.Rotate(k, -10, 0, 0));
		HotkeyService.RegisterHotkeyHandler("QuaternionEditor.RotateYPlusFast", (k) => this.Rotate(k, 0, 10, 0));
		HotkeyService.RegisterHotkeyHandler("QuaternionEditor.RotateYMinusFast", (k) => this.Rotate(k, 0, -10, 0));
		HotkeyService.RegisterHotkeyHandler("QuaternionEditor.RotateZPlusFast", (k) => this.Rotate(k, 0, 0, 10));
		HotkeyService.RegisterHotkeyHandler("QuaternionEditor.RotateZMinusFast", (k) => this.Rotate(k, 0, 0, -10));
		HotkeyService.RegisterHotkeyHandler("QuaternionEditor.RotateXPlusSlow", (k) => this.Rotate(k, 0.1, 0, 0));
		HotkeyService.RegisterHotkeyHandler("QuaternionEditor.RotateXMinusSlow", (k) => this.Rotate(k, -0.1, 0, 0));
		HotkeyService.RegisterHotkeyHandler("QuaternionEditor.RotateYPlusSlow", (k) => this.Rotate(k, 0, 0.1, 0));
		HotkeyService.RegisterHotkeyHandler("QuaternionEditor.RotateYMinusSlow", (k) => this.Rotate(k, 0, -0.1, 0));
		HotkeyService.RegisterHotkeyHandler("QuaternionEditor.RotateZPlusSlow", (k) => this.Rotate(k, 0, 0, 0.1));
		HotkeyService.RegisterHotkeyHandler("QuaternionEditor.RotateZMinusSlow", (k) => this.Rotate(k, 0, 0, -0.1));

		this.TickFrequency = 0.5m;

		this.rotationGizmo = new RotationGizmo(this);
		this.Viewport.Children.Add(this.rotationGizmo);

		// Create and add the pivot sphere directly to the viewport
		this.pivotSphere = new Sphere
		{
			Radius = 0.03,
			Material = new DiffuseMaterial(new SolidColorBrush(Colors.Yellow)),
		};

		// [FOR DEBUGGING]
		// this.intersectionSphere = new Sphere
		// {
		//   Radius = 0.03,
		//   Material = new DiffuseMaterial(new SolidColorBrush(Colors.Red)),
		// };
		// this.Viewport.Children.Add(this.intersectionSphere);

		// this.nearSphere = new Sphere
		// {
		//   Radius = 0.02,
		//   Material = new DiffuseMaterial(new SolidColorBrush(Colors.Magenta)),
		// };
		// this.Viewport.Children.Add(this.nearSphere);

		// this.farSphere = new Sphere
		// {
		//   Radius = 0.02,
		//   Material = new DiffuseMaterial(new SolidColorBrush(Colors.Cyan)),
		// };
		// this.Viewport.Children.Add(this.farSphere);

		// this.axisProjectionLine = new Line
		// {
		//   Thickness = 2,
		//   Color = Colors.Red,
		// };
		// this.Viewport.Children.Add(this.axisProjectionLine);

		// this.planeNormalLine = new Line
		// {
		//   Thickness = 2,
		//   Color = Colors.Green,
		// };
		// this.Viewport.Children.Add(this.planeNormalLine);

		// this.tangentPlaneVisual = new PlaneVisual3D();
		// this.Viewport.Children.Add(this.tangentPlaneVisual);

		// this.axisPlane = new PlaneVisual3D();
		// this.Viewport.Children.Add(this.axisPlane);

		// Add a light source to the scene
		this.Viewport.Children.Add(new ModelVisual3D() { Content = new AmbientLight(Colors.White) });

		var camera = new PerspectiveCamera(new Point3D(0, 0, -2.0), new Vector3D(0, 0, 1), new Vector3D(0, 1, 0), 45)
		{
			FarPlaneDistance = 10000,
		};

		this.Viewport.Camera = camera;

		this.worldSpace = false;

		this.isInitialized = true;

		SettingsService.SettingsChanged += this.OnSettingsChanged;
	}

	/// <inheritdoc/>
	public event PropertyChangedEventHandler? PropertyChanged;

	public static Settings Settings => SettingsService.Current;

	public static OverflowModes RotationOverflowBehavior => Settings.WrapRotationSliders ? OverflowModes.Loop : OverflowModes.Clamp;

	public decimal TickFrequency
	{
		get => TickDp.Get(this);
		set => TickDp.Set(this, value);
	}

	public CmQuaternion Value
	{
		get => ValueDp.Get(this);
		set => ValueDp.Set(this, value);
	}

	public CmQuaternion? RootRotation
	{
		get => RootRotationDp.Get(this);
		set => RootRotationDp.Set(this, value);
	}

	public CmQuaternion ValueQuat
	{
		get => ValueQuatDp.Get(this);
		set => ValueQuatDp.Set(this, value);
	}

	public CmVector Euler
	{
		get => EulerDp.Get(this);
		set => EulerDp.Set(this, value);
	}

	public CmQuaternion Root
	{
		get
		{
			if (this.RootRotation == null)
				return CmQuaternion.Identity;

			CmQuaternion root = (CmQuaternion)this.RootRotation;
			return new CmQuaternion(root.X, root.Y, root.Z, root.W);
		}
	}

	public bool WorldSpace
	{
		get
		{
			return this.worldSpace;
		}
		set
		{
			bool old = this.worldSpace;
			this.worldSpace = value;

			if (old && !value)
			{
				OnValueChanged(this, this.Value);
			}
			else
			{
				this.ValueQuat = CmQuaternion.Identity;
				this.rotationGizmo.Transform = new RotateTransform3D(new QuaternionRotation3D(Quaternion.Identity));
			}
		}
	}

	public static Matrix3D GetWorldMatrixFor(Visual3D? visual)
	{
		Matrix3D worldMatrix = Matrix3D.Identity;

		// Traverse up the visual tree to accumulate transformations
		while (visual != null)
		{
			if (visual.Transform != null)
				worldMatrix.Append(visual.Transform.Value);

			visual = VisualTreeHelper.GetParent(visual) as Visual3D;
		}

		return worldMatrix;
	}

	private static void OnValueChanged(QuaternionEditor sender, CmQuaternion value)
	{
		if (!sender.isInitialized || sender.lockdp)
			return;

		sender.lockdp = true;

		var valueQuat = new CmQuaternion(value.X, value.Y, value.Z, value.W);

		if (sender.RootRotation != null)
			valueQuat = sender.Root * valueQuat;

		sender.worldSpaceDelta = valueQuat;

		if (sender.WorldSpace)
			valueQuat = CmQuaternion.Identity;

		sender.rotationGizmo.Transform = new RotateTransform3D(new QuaternionRotation3D(valueQuat.ToMedia3DQuaternion()));
		sender.ValueQuat = valueQuat;

		sender.Euler = sender.Value.ToEuler();

		sender.lockdp = false;
	}

	private static void OnRootRotationChanged(QuaternionEditor sender, CmQuaternion? value)
	{
		if (!sender.isInitialized)
			return;

		OnValueChanged(sender, sender.Value);
	}

	private static void OnValueQuatChanged(QuaternionEditor sender, CmQuaternion value)
	{
		if (!sender.isInitialized)
			return;

		Quaternion newrot = value.ToMedia3DQuaternion();
		sender.rotationGizmo.Transform = new RotateTransform3D(new QuaternionRotation3D(newrot));

		if (sender.WorldSpace)
		{
			newrot *= sender.worldSpaceDelta.ToMedia3DQuaternion();
			sender.rotationGizmo.Transform = new RotateTransform3D(new QuaternionRotation3D(Quaternion.Identity));
		}

		if (sender.lockdp)
			return;

		sender.lockdp = true;

		if (sender.RootRotation != null)
		{
			Quaternion rootInv = sender.Root.ToMedia3DQuaternion();
			rootInv.Invert();
			newrot = rootInv * newrot;
		}

		sender.Value = new CmQuaternion((float)newrot.X, (float)newrot.Y, (float)newrot.Z, (float)newrot.W);

		sender.Euler = sender.Value.ToEuler();

		sender.lockdp = false;
	}

	private static void OnEulerChanged(QuaternionEditor sender, CmVector val)
	{
		if (!sender.isInitialized || sender.lockdp)
			return;

		sender.lockdp = true;

		var value = QuaternionExtensions.FromEuler(sender.Euler);
		sender.Value = value;

		if (sender.RootRotation != null)
			value = sender.Root * value;

		sender.worldSpaceDelta = value;

		if (sender.WorldSpace)
			value = CmQuaternion.Identity;

		sender.rotationGizmo.Transform = new RotateTransform3D(new QuaternionRotation3D(value.ToMedia3DQuaternion()));
		sender.ValueQuat = value;

		sender.lockdp = false;
	}

	private static string? GetAxisName(CmVector? axis)
	{
		if (axis == null)
			return null;

		CmVector v = (CmVector)axis;

		if (v.X > v.Y && v.X > v.Z)
			return "X";

		if (v.Y > v.X && v.Y > v.Z)
			return "Y";

		if (v.Z > v.X && v.Z > v.Y)
			return "Z";

		return null;
	}

	private void OnViewportMouseDown(object sender, MouseButtonEventArgs e)
	{
		Mouse.Capture(this.Viewport, CaptureMode.SubTree);

		if (e.LeftButton == MouseButtonState.Pressed)
		{
			Point mousePos = e.GetPosition(this.Viewport);
			var activeAxis = this.rotationGizmo.Active;
			if (activeAxis == null)
				return;

			// Find the nearest point on the active Axis gizmo circle
			Point3D? nearestPoint = activeAxis.NearestPoint2D(mousePos);
			if (nearestPoint == null)
				return;

			// Transform the nearest point from the Axis gizmo's space to the quaternion editor's transform space
			Point3D transformedPoint = GetWorldMatrixFor(activeAxis).Transform((Point3D)nearestPoint);

			// Set the pivot point for the active axis gizmo
			activeAxis.SetPivotPoint(transformedPoint);

			// Set the pivot sphere's position and color
			this.pivotSphere.Transform = new TranslateTransform3D(transformedPoint.X, transformedPoint.Y, transformedPoint.Z);
			this.pivotSphere.Material = new DiffuseMaterial(new SolidColorBrush(!activeAxis.Locked ? Colors.Yellow : Colors.White));

			if (this.Viewport.Children.Contains(this.pivotSphere))
				this.Viewport.Children.Remove(this.pivotSphere);

			this.Viewport.Children.Add(this.pivotSphere);

			// [FOR DEBUGGING]
			// if (this.Viewport.Children.Contains(this.tangentPlaneVisual))
			//   this.Viewport.Children.Remove(this.tangentPlaneVisual);

			// this.Viewport.Children.Add(this.tangentPlaneVisual);
		}
	}

	private void OnViewportMouseUp(object sender, MouseButtonEventArgs e)
	{
		Mouse.Capture(null);

		if (e.ChangedButton == MouseButton.Right)
		{
			this.LockedIndicator.IsChecked = this.rotationGizmo.LockHoveredGizmo();
			this.LockedIndicator.IsEnabled = (bool)this.LockedIndicator.IsChecked;
			this.LockedAxisDisplay.Text = GetAxisName(this.rotationGizmo.Locked?.Axis);
		}

		this.Viewport.Children.Remove(this.pivotSphere);
		// this.Viewport.Children.Remove(this.tangentPlaneVisual); [FOR DEBUGGING]
		this.rotationGizmo.Hover(null);
	}

	private void OnViewportMouseMove(object sender, MouseEventArgs e)
	{
		Point mousePosition = e.GetPosition(this.Viewport);

		if (e.LeftButton != MouseButtonState.Pressed)
		{
			HitTestResult result = VisualTreeHelper.HitTest(this.Viewport, mousePosition);
			this.rotationGizmo.Hover(result?.VisualHit);
		}
		else
		{
			var mousePos3D = new Point3D(mousePosition.X, mousePosition.Y, 0);
			this.rotationGizmo.Drag(mousePos3D);
		}
	}

	private void OnViewportMouseLeave(object sender, MouseEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed)
			return;

		this.rotationGizmo.Hover(null);
	}

	private void OnViewportMouseWheel(object sender, MouseWheelEventArgs e)
	{
		double delta = e.Delta > 0 ? (double)this.TickFrequency : -(double)this.TickFrequency;

		if (Keyboard.IsKeyDown(Key.LeftShift))
			delta *= 10;

		this.rotationGizmo.Scroll(delta);
	}

	private void LockedIndicator_Unchecked(object sender, RoutedEventArgs e)
	{
		this.rotationGizmo.UnlockGizmo();
		this.LockedIndicator.IsEnabled = false;
		this.LockedAxisDisplay.Text = GetAxisName(this.rotationGizmo.Locked?.Axis);
	}

	private bool Rotate(KeyboardKeyStates state, double x, double y, double z)
	{
		if (!this.IsEnabled)
			return false;

		// only rotate on Press or Down events
		if (state == KeyboardKeyStates.Released)
			return false;

		if (!this.IsVisible)
			return false;

		CmVector euler = this.Euler;
		euler.X = Float.Wrap(euler.X + (float)x, 0, 360);
		euler.Y = Float.Wrap(euler.Y + (float)y, 0, 360);
		euler.Z = Float.Wrap(euler.Z + (float)z, 0, 360);
		this.Euler = euler;

		return true;
	}

	private async Task WatchCamera()
	{
		bool vis = true;
		while (vis && Application.Current != null)
		{
			if (Application.Current.Dispatcher.CheckAccess())
			{
				vis = this.IsVisible;

				if (CameraService.Instance.Camera != null)
				{
					this.Viewport.Camera.Transform = new RotateTransform3D(new QuaternionRotation3D(CameraService.Instance.Camera.Rotation3d.ToMedia3DQuaternion()));
				}
			}
			else
			{
				Application.Current.Dispatcher.Invoke(() =>
				{
					vis = this.IsVisible;

					if (CameraService.Instance.Camera != null)
					{
						this.Viewport.Camera.Transform = new RotateTransform3D(new QuaternionRotation3D(CameraService.Instance.Camera.Rotation3d.ToMedia3DQuaternion()));
					}
				});
			}

			await Task.Delay(16);
		}
	}

	private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (!this.IsVisible)
			return;

		// Watch camera thread
		Task.Run(this.WatchCamera);
	}

	/// <summary>Handles changes to the settings.</summary>
	/// <param name="sender">The sender.</param>
	/// <param name="e">The event arguments.</param>
	private void OnSettingsChanged(object? sender, EventArgs e)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RotationOverflowBehavior)));
	}

	private class RotationGizmo : ModelVisual3D
	{
		private readonly QuaternionEditor target;

		public RotationGizmo(QuaternionEditor target)
		{
			this.target = target;

			var sphere = new Sphere
			{
				Radius = 0.48,
				Material = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(128, 0, 0, 0))),
			};
			this.Children.Add(sphere);

			this.Children.Add(new AxisGizmo(this, target, (Color)ColorConverter.ConvertFromString("#1c59ff"), new CmVector(1, 0, 0)));
			this.Children.Add(new AxisGizmo(this, target, (Color)ColorConverter.ConvertFromString("#94e800"), new CmVector(0, 1, 0)));
			this.Children.Add(new AxisGizmo(this, target, (Color)ColorConverter.ConvertFromString("#ff0d3e"), new CmVector(0, 0, 1)));
		}

		public AxisGizmo? Locked
		{
			get;
			private set;
		}

		public AxisGizmo? Hovered
		{
			get;
			private set;
		}

		public AxisGizmo? Active
		{
			get
			{
				if (this.Locked != null)
					return this.Locked;

				return this.Hovered;
			}
		}

		public bool LockHoveredGizmo()
		{
			if (this.Locked != null)
				this.Locked.Locked = false;

			this.Locked = this.Hovered;

			if (this.Locked != null)
				this.Locked.Locked = true;

			return this.Locked != null;
		}

		public void UnlockGizmo()
		{
			if (this.Locked != null)
				this.Locked.Locked = false;

			this.Locked = null;
		}

		public bool Hover(DependencyObject? visual)
		{
			if (this.Locked != null)
			{
				this.Hovered = null;
				return true;
			}

			AxisGizmo? gizmo = null;
			if (visual is Circle r)
			{
				gizmo = (AxisGizmo)VisualTreeHelper.GetParent(r);
			}
			else if (visual is Cylinder c)
			{
				gizmo = (AxisGizmo)VisualTreeHelper.GetParent(c);
			}

			if (this.Hovered != null)
				this.Hovered.Hovered = false;

			this.Hovered = gizmo;

			if (this.Hovered != null)
			{
				this.Hovered.Hovered = true;
				return true;
			}

			return false;
		}

		public void Drag(Point3D mousePosition)
		{
			if (this.Active == null)
				return;

			CmVector angleDelta = this.Active.Drag(mousePosition);
			this.ApplyDelta(angleDelta);
		}

		public void Scroll(double delta)
		{
			if (this.Active == null)
				return;

			CmVector angleDelta = CmVector.Multiply(this.Active.Axis, (float)delta);
			this.ApplyDelta(angleDelta);
		}

		private void ApplyDelta(CmVector angleEuler)
		{
			CmQuaternion angle = QuaternionExtensions.FromEuler(angleEuler);
			CmQuaternion targetQuat = this.target.ValueQuat * angle;
			this.target.ValueQuat = CmQuaternion.Slerp(this.target.ValueQuat, targetQuat, 0.6f);
		}
	}

	private class AxisGizmo : ModelVisual3D
	{
		public readonly CmVector Axis;
		private readonly Circle circle;
		private readonly Cylinder cylinder;
		private readonly RotationGizmo rotationGizmo;
		private readonly QuaternionEditor target;
		private Color color;
		private Point3D? lastPoint;
		private Point3D pivotPoint;
		private Vector3D planeNormal;
		private Vector3D dragTangent;
		private bool locked = false;

		public AxisGizmo(RotationGizmo rotationGizmo, QuaternionEditor target, Color color, CmVector axis)
		{
			this.rotationGizmo = rotationGizmo;
			this.target = target;
			this.Axis = axis;
			this.color = color;

			this.circle = new Circle
			{
				Thickness = 1,
				Color = color,
				Radius = 0.5,

				Transform = new RotateTransform3D(new AxisAngleRotation3D(axis.ToMedia3DVector(), 90)),
			};
			this.Children.Add(this.circle);

			this.cylinder = new Cylinder
			{
				Radius = 0.48,
				Length = 0.20,
				Transform = new RotateTransform3D(new AxisAngleRotation3D(axis.ToMedia3DVector(), 90)),
				Material = new DiffuseMaterial(new SolidColorBrush(Colors.Transparent)),
			};
			this.Children.Add(this.cylinder);
		}

		public bool Hovered
		{
			set
			{
				if (!value)
				{
					this.circle.Color = this.color;
					this.circle.Thickness = 1;
					this.lastPoint = null;
				}
				else
				{
					this.circle.Color = Colors.Yellow;
					this.circle.Thickness = 3;
				}
			}
		}

		public bool Locked
		{
			get => this.locked;
			set
			{
				if (!value)
				{
					this.circle.Color = this.color;
					this.circle.Thickness = 1;
					this.lastPoint = null;
				}
				else
				{
					this.circle.Color = Colors.White;
					this.circle.Thickness = 3;
				}

				this.locked = value;
			}
		}

		public Point3D? NearestPoint2D(Point mousePosition)
		{
			Point3D? nearestPoint = this.circle.NearestPoint2D(new Point3D(mousePosition.X, mousePosition.Y, 0));
			if (nearestPoint == null)
				return null;

			return this.circle.TransformToAncestor(this).Transform((Point3D)nearestPoint);
		}

		public void SetPivotPoint(Point3D point)
		{
			// Pivot point
			this.pivotPoint = point;

			// Plane normal (From the center of the gizmo pointing outwards, passing through the pivot point)
			Matrix3D worldMatrix = GetWorldMatrixFor(this.rotationGizmo);
			Point3D gizmoCenter = worldMatrix.Transform(new Point3D(0, 0, 0));
			Vector3D centerToPivot = this.pivotPoint - gizmoCenter;

			// Get camera position in world space
			var camera = (PerspectiveCamera)this.target.Viewport.Camera;
			Point3D camOrigin = camera.Position;
			if (camera.Transform != null)
				camOrigin = camera.Transform.Value.Transform(camOrigin);

			// Calculate plane normal
			Vector3D normal = camOrigin - this.pivotPoint;
			if (normal.LengthSquared < 1e-6)
				normal = camera.LookDirection; // Fallback to camera look direction

			normal.Normalize();
			this.planeNormal = normal;

			// Tangent direction
			Vector3D axis = this.Axis.ToMedia3DVector();
			if (axis.X >= 1)
				axis = new Vector3D(0, 0, 1);
			else if (axis.Z >= 1)
				axis = new Vector3D(1, 0, 0);

			Vector3D axisInWorld = worldMatrix.Transform(axis);
			axisInWorld.Normalize();

			Vector3D axisProjected = axisInWorld - (Vector3D.DotProduct(axisInWorld, centerToPivot) * centerToPivot);
			if (axisProjected.LengthSquared < 1e-6)
			{
				axisProjected = new Vector3D(1, 0, 0); // Fallback if projection breaks down
				if (axisProjected.LengthSquared < 1e-6)
					axisProjected = new Vector3D(0, 1, 0);
			}

			axisProjected.Normalize();

			this.dragTangent = Vector3D.CrossProduct(centerToPivot, axisProjected);
			this.dragTangent.Normalize();

			// [FOR DEBUGGING]
			// this.target.axisProjectionLine.Points = [this.pivotPoint, this.pivotPoint + this.dragTangent];
			// this.target.planeNormalLine.Points = [this.pivotPoint, this.pivotPoint + this.planeNormal];
			// this.target.tangentPlaneVisual.UpdatePlane(this.pivotPoint, this.planeNormal);
			// this.target.axisPlane.UpdatePlane(new Point3D(0, 0, 0), axisInWorld);
		}

		public CmVector Drag(Point3D mousePosition)
		{
			if (SettingsService.Current.GizmoDragMode == Settings.GizmoDragModes.Circular)
			{
				Point3D? point = this.circle.NearestPoint2D(mousePosition);

				if (point == null)
					return default;

				point = this.circle.TransformToAncestor(this).Transform((Point3D)point);

				if (this.lastPoint == null)
				{
					this.lastPoint = point;
					return default;
				}

				Vector3D axis = new(0, 1, 0);

				Vector3D from = (Vector3D)this.lastPoint;
				Vector3D to = (Vector3D)point;

				this.lastPoint = null;

				double angle = Vector3D.AngleBetween(from, to);

				Vector3D cross = Vector3D.CrossProduct(from, to);
				if (Vector3D.DotProduct(axis, cross) < 0)
					angle = -angle;

				// X rotation gizmo is always backwards...
				if (this.Axis.X >= 1)
					angle = -angle;

				float speed = 2;

				if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
					speed = 4;

				if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
					speed = 0.5f;

				return CmVector.Multiply(this.Axis, (float)(angle * speed));
			}
			else // Linear drag
			{
				if (this.lastPoint == null)
				{
					this.lastPoint = mousePosition;
					return default;
				}

				Point3D? prevIntersection = this.RaycastOnPlane(
					new Point(this.lastPoint.Value.X, this.lastPoint.Value.Y),
					this.pivotPoint,
					this.planeNormal);

				Point3D? currentIntersection = this.RaycastOnPlane(
					new Point(mousePosition.X, mousePosition.Y),
					this.pivotPoint,
					this.planeNormal);

				if (prevIntersection == null || currentIntersection == null)
					return default;

				// [FOR DEBUGGING]
				// Visualize the intersection point of the mouse ray on the plane
				// this.target.intersectionSphere.Transform = new TranslateTransform3D(
				//   currentIntersection.Value.X,
				//   currentIntersection.Value.Y,
				//   currentIntersection.Value.Z);

				Vector3D movement = (Vector3D)(currentIntersection - prevIntersection);
				this.lastPoint = mousePosition;

				double distanceAlongTangent = Vector3D.DotProduct(movement, this.dragTangent);

				// Apply modifiers
				float baseSensitivity = 100.0f;
				float keyModifier = 1.0f;
				if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
					keyModifier = 4.0f;
				else if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
					keyModifier = 0.25f;

				float rotationDegrees = (float)(distanceAlongTangent * baseSensitivity * keyModifier);
				return CmVector.Multiply(this.Axis, -rotationDegrees);
			}
		}

		private static Point3D Unproject(Point3D point, PerspectiveCamera camera, double viewportWidth, double viewportHeight)
		{
			// Get the view matrix and include the camera's transform
			Matrix3D viewMatrix = MathUtils.GetViewMatrix(camera);

			if (camera.Transform != null)
			{
				Matrix3D transformMatrix = camera.Transform.Value;
				transformMatrix.Invert();
				viewMatrix = transformMatrix * viewMatrix;
			}

			// Get the projection matrix
			Matrix3D projectionMatrix = MathUtils.GetProjectionMatrix(camera, viewportWidth / viewportHeight);

			// Combine view and projection matrices
			Matrix3D viewProjectionMatrix = viewMatrix * projectionMatrix;

			if (!viewProjectionMatrix.HasInverse)
				return new Point3D(double.NaN, double.NaN, double.NaN);

			// Invert the combined matrix
			viewProjectionMatrix.Invert();

			// Transform the point using the inverted matrix
			Point3D unprojectedPoint = viewProjectionMatrix.Transform(point);
			return unprojectedPoint;
		}

		private Point3D? RaycastOnPlane(Point screenPos, Point3D pivotPoint, Vector3D planeNormal)
		{
			// Get camera and viewport dimensions
			var camera = (PerspectiveCamera)this.target.Viewport.Camera;
			double w = this.target.Viewport.ActualWidth;
			double h = this.target.Viewport.ActualHeight;

			// Convert screen position to normalized device coords
			double vx = (screenPos.X / w * 2.0) - 1.0;
			double vy = 1.0 - (screenPos.Y / h * 2.0);

			// Unproject the 2D viewport coordinates to 3D world coordinates
			Point3D nearPoint = Unproject(new Point3D(vx, vy, 0), camera, w, h);
			Point3D farPoint = Unproject(new Point3D(vx, vy, 1), camera, w, h);

			// [FOR DEBUGGING]
			// this.target.nearSphere.Transform = new TranslateTransform3D(nearPoint.X, nearPoint.Y, nearPoint.Z);
			// this.target.farSphere.Transform = new TranslateTransform3D(farPoint.X, farPoint.Y, farPoint.Z);

			// Create a ray from the camera position through the unprojected world point
			Vector3D rayDirection = farPoint - nearPoint;
			rayDirection.Normalize();

			// Plane intersection
			double denom = Vector3D.DotProduct(planeNormal, rayDirection);
			if (Math.Abs(denom) < 1e-6)
				return null; // Ray is parallel

			double t = Vector3D.DotProduct(planeNormal, pivotPoint - nearPoint) / denom;
			if (t < 0)
				return null; // Intersection behind camera

			return nearPoint + (rayDirection * t);
		}
	}
}

public class PlaneVisual3D : ModelVisual3D
{
	private readonly MeshGeometry3D mesh;
	private readonly GeometryModel3D model;

	public PlaneVisual3D()
	{
		this.mesh = new MeshGeometry3D();
		this.model = new GeometryModel3D
		{
			Geometry = this.mesh,
			Material = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(128, 255, 255, 255))),
			BackMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(128, 255, 255, 255))),
		};

		this.Content = this.model;
	}

	public void UpdatePlane(Point3D point, Vector3D normal)
	{
		// Create a plane with a size of 1x1 units
		double size = 1.0;

		// Calculate the plane's basis vectors
		Vector3D u = Vector3D.CrossProduct(new Vector3D(0, 1, 0), normal);
		if (u.LengthSquared < 1e-6)
			u = Vector3D.CrossProduct(new Vector3D(1, 0, 0), normal);

		u.Normalize();
		Vector3D v = Vector3D.CrossProduct(normal, u);
		v.Normalize();

		// Calculate the plane's corners
		Point3D p0 = point + ((-u - v) * size);
		Point3D p1 = point + ((u - v) * size);
		Point3D p2 = point + ((u + v) * size);
		Point3D p3 = point + ((-u + v) * size);

		// Update the mesh
		this.mesh.Positions = [p0, p1, p2, p3];
		this.mesh.TriangleIndices = [0, 1, 2, 0, 2, 3];
		this.mesh.Normals = [normal, normal, normal, normal];
	}
}
