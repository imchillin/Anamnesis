// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Controls;

using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Anamnesis.Keyboard;
using Anamnesis.Memory;
using Anamnesis.Services;
using PropertyChanged;
using XivToolsWpf.DependencyProperties;
using XivToolsWpf.Meida3D;
using XivToolsWpf.Meida3D.Lines;

using CmQuaternion = Anamnesis.Memory.Quaternion;
using Color = System.Windows.Media.Color;
using Quaternion = System.Windows.Media.Media3D.Quaternion;
using Vector = Anamnesis.Memory.Vector;

/// <summary>
/// Interaction logic for QuaternionEditor.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class QuaternionEditor : UserControl
{
	public static readonly IBind<CmQuaternion> ValueDp = Binder.Register<CmQuaternion, QuaternionEditor>(nameof(Value), OnValueChanged);
	public static readonly IBind<CmQuaternion?> RootRotationdp = Binder.Register<CmQuaternion?, QuaternionEditor>(nameof(RootRotation), OnRootRotationChanged);
	public static readonly IBind<double> TickDp = Binder.Register<double, QuaternionEditor>(nameof(TickFrequency));

	public static readonly IBind<Quaternion> ValueQuatDp = Binder.Register<Quaternion, QuaternionEditor>(nameof(ValueQuat), OnValueQuatChanged);
	public static readonly IBind<Vector> EulerDp = Binder.Register<Vector, QuaternionEditor>(nameof(Euler), OnEulerChanged);

	////private Vector3D euler;
	private readonly RotationGizmo rotationGizmo;
	private bool lockdp = false;

	private Quaternion worldSpaceDelta;
	private bool worldSpace;

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

		this.TickFrequency = 0.5;

		this.rotationGizmo = new RotationGizmo(this);
		this.Viewport.Children.Add(this.rotationGizmo);

		this.Viewport.Camera = new PerspectiveCamera(new Point3D(0, 0, -2.0), new Vector3D(0, 0, 1), new Vector3D(0, 1, 0), 45);

		this.worldSpace = false;
	}

	public double TickFrequency
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
		get => RootRotationdp.Get(this);
		set => RootRotationdp.Set(this, value);
	}

	public Quaternion ValueQuat
	{
		get => ValueQuatDp.Get(this);
		set => ValueQuatDp.Set(this, value);
	}

	public Vector Euler
	{
		get => EulerDp.Get(this);
		set => EulerDp.Set(this, value);
	}

	public Settings Settings => SettingsService.Current;

	public Quaternion Root
	{
		get
		{
			if (this.RootRotation == null)
				return Quaternion.Identity;

			CmQuaternion root = (CmQuaternion)this.RootRotation;
			return new Quaternion(root.X, root.Y, root.Z, root.W);
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
				this.ValueQuat = Quaternion.Identity;
				this.rotationGizmo.Transform = new RotateTransform3D(new QuaternionRotation3D(Quaternion.Identity));
			}
		}
	}

	private static void OnValueChanged(QuaternionEditor sender, CmQuaternion value)
	{
		Quaternion valueQuat = new Quaternion(value.X, value.Y, value.Z, value.W);

		if (sender.RootRotation != null)
			valueQuat = sender.Root * valueQuat;

		sender.worldSpaceDelta = valueQuat;

		if (sender.WorldSpace)
			valueQuat = Quaternion.Identity;

		sender.rotationGizmo.Transform = new RotateTransform3D(new QuaternionRotation3D(valueQuat));
		sender.ValueQuat = valueQuat;

		if (sender.lockdp)
			return;

		sender.lockdp = true;

		sender.Euler = sender.Value.ToEuler();

		sender.lockdp = false;
	}

	private static void OnRootRotationChanged(QuaternionEditor sender, CmQuaternion? value)
	{
		OnValueChanged(sender, sender.Value);
	}

	private static void OnValueQuatChanged(QuaternionEditor sender, Quaternion value)
	{
		Quaternion newrot = value;
		sender.rotationGizmo.Transform = new RotateTransform3D(new QuaternionRotation3D(newrot));

		if (sender.WorldSpace)
		{
			newrot *= sender.worldSpaceDelta;
			sender.rotationGizmo.Transform = new RotateTransform3D(new QuaternionRotation3D(Quaternion.Identity));
		}

		if (sender.lockdp)
			return;

		sender.lockdp = true;

		if (sender.RootRotation != null)
		{
			Quaternion rootInv = sender.Root;
			rootInv.Invert();
			newrot = rootInv * newrot;
		}

		sender.Value = new CmQuaternion((float)newrot.X, (float)newrot.Y, (float)newrot.Z, (float)newrot.W);

		sender.Euler = sender.Value.ToEuler();

		sender.lockdp = false;
	}

	private static void OnEulerChanged(QuaternionEditor sender, Vector val)
	{
		if (sender.lockdp)
			return;

		sender.lockdp = true;
		sender.Value = CmQuaternion.FromEuler(sender.Euler);
		sender.lockdp = false;
	}

	private static string? GetAxisName(Vector3D? axis)
	{
		if (axis == null)
			return null;

		Vector3D v = (Vector3D)axis;

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
		Mouse.Capture(this.Viewport);
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
			Point3D mousePos3D = new Point3D(mousePosition.X, mousePosition.Y, 0);
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
		double delta = e.Delta > 0 ? this.TickFrequency : -this.TickFrequency;

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
		// only roate on Press or Down events
		if (state == KeyboardKeyStates.Released)
			return false;

		if (!this.IsVisible)
			return false;

		Vector euler = this.Euler;
		euler.X = Float.Wrap(euler.X + (float)x, 0, 360);
		euler.Y = Float.Wrap(euler.Y + (float)y, 0, 360);
		euler.Z = Float.Wrap(euler.Z + (float)z, 0, 360);
		this.Euler = euler;

		return true;
	}

	private void WatchCamera()
	{
		bool vis = true;
		while (vis && Application.Current != null)
		{
			try
			{
				Application.Current.Dispatcher.Invoke(() =>
				{
					vis = this.IsVisible; ////&& this.IsEnabled;

					if (CameraService.Instance.Camera != null)
					{
						this.Viewport.Camera.Transform = new RotateTransform3D(new QuaternionRotation3D(CameraService.Instance.Camera.Rotation3d));
					}
				});
			}
			catch (Exception)
			{
			}

			Thread.Sleep(16);
		}
	}

	private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (this.IsVisible)
		{
			// Watch camera thread
			new Thread(new ThreadStart(this.WatchCamera)).Start();
		}
	}

	private class RotationGizmo : ModelVisual3D
	{
		private readonly QuaternionEditor target;

		public RotationGizmo(QuaternionEditor target)
		{
			this.target = target;

			Sphere sphere = new Sphere();
			sphere.Radius = 0.48;
			Color c = Colors.Black;
			c.A = 128;
			sphere.Material = new DiffuseMaterial(new SolidColorBrush(c));
			this.Children.Add(sphere);

			this.Children.Add(new AxisGizmo(Colors.Blue, new Vector3D(1, 0, 0)));
			this.Children.Add(new AxisGizmo(Colors.Green, new Vector3D(0, 1, 0)));
			this.Children.Add(new AxisGizmo(Colors.Red, new Vector3D(0, 0, 1)));
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

			Vector3D angleDelta = this.Active.Drag(mousePosition);
			this.ApplyDelta(angleDelta);
		}

		public void Scroll(double delta)
		{
			if (this.Active == null)
				return;

			Vector3D angleDelta = this.Active.Axis * delta;
			this.ApplyDelta(angleDelta);
		}

		private void ApplyDelta(Vector3D angleEuler)
		{
			Quaternion angle = angleEuler.ToQuaternion();
			this.target.ValueQuat *= angle;
		}
	}

	private class AxisGizmo : ModelVisual3D
	{
		public readonly Vector3D Axis;
		private readonly Circle circle;
		private readonly Cylinder cylinder;
		private Color color;

		private Point3D? lastPoint;

		public AxisGizmo(Color color, Vector3D axis)
		{
			this.Axis = axis;
			this.color = color;

			Vector3D rotationAxis = new Vector3D(axis.Z, 0, axis.X);

			this.circle = new Circle();
			this.circle.Thickness = 1;
			this.circle.Color = color;
			this.circle.Radius = 0.5;
			this.circle.Transform = new RotateTransform3D(new AxisAngleRotation3D(axis, 90));
			this.Children.Add(this.circle);

			this.cylinder = new Cylinder();
			this.cylinder.Radius = 0.49;
			this.cylinder.Length = 0.20;
			this.cylinder.Transform = new RotateTransform3D(new AxisAngleRotation3D(axis, 90));
			this.cylinder.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Transparent));
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
			}
		}

		public void StartDrag()
		{
			this.lastPoint = null;
		}

		public Vector3D Drag(Point3D mousePosition)
		{
			bool useCircularDrag = true;

			if (useCircularDrag)
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
				else
				{
					Vector3D axis = new Vector3D(0, 1, 0);

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

					if (Keyboard.IsKeyDown(Key.LeftShift))
						speed = 4;

					if (Keyboard.IsKeyDown(Key.RightShift))
						speed = 4;

					if (Keyboard.IsKeyDown(Key.LeftCtrl))
						speed = 0.5f;

					if (Keyboard.IsKeyDown(Key.RightCtrl))
						speed = 0.5f;

					return this.Axis * (angle * speed);
				}
			}
			else
			{
				if (this.lastPoint == null)
				{
					this.lastPoint = mousePosition;
					return default;
				}
				else
				{
					Vector3D delta = (Point3D)this.lastPoint - mousePosition;
					this.lastPoint = mousePosition;

					float speed = 0.5f;

					if (Keyboard.IsKeyDown(Key.LeftShift))
						speed = 2;

					if (Keyboard.IsKeyDown(Key.LeftCtrl))
						speed = 0.25f;

					double distPos = Math.Max(delta.X, delta.Y);
					double distNeg = Math.Min(delta.X, delta.Y);

					double dist = distNeg;
					if (Math.Abs(distPos) > Math.Abs(distNeg))
						dist = distPos;

					return this.Axis * (-dist * speed);
				}
			}
		}
	}
}
