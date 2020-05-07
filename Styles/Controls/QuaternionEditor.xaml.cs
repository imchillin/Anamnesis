// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.WpfStyles.Controls
{
	using System;
	using System.ComponentModel;
	using System.Threading;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Media.Media3D;
	using ConceptMatrix.Three3D;
	using ConceptMatrix.ThreeD;
	using ConceptMatrix.ThreeD.Lines;
	using ConceptMatrix.WpfStyles.DependencyProperties;
	using PropertyChanged;

	using CmQuaternion = ConceptMatrix.Quaternion;
	using Vector = ConceptMatrix.Vector;

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

		public static readonly IBind<double> EulerXDp = Binder.Register<double, QuaternionEditor>(nameof(EulerX), OnEulerChanged);
		public static readonly IBind<double> EulerYDp = Binder.Register<double, QuaternionEditor>(nameof(EulerY), OnEulerChanged);
		public static readonly IBind<double> EulerZDp = Binder.Register<double, QuaternionEditor>(nameof(EulerZ), OnEulerChanged);

		////private Vector3D euler;
		private readonly RotationGizmo rotationGizmo;
		private bool lockdp = false;
		private bool mouseDown = false;

		private Quaternion worldSpaceDelta;
		private bool worldSpace;

		public QuaternionEditor()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;

			this.TickFrequency = 0.5;

			this.rotationGizmo = new RotationGizmo(this);
			this.Viewport.Children.Add(this.rotationGizmo);

			this.Viewport.Camera = new PerspectiveCamera(new Point3D(0, 0, -2.5), new Vector3D(0, 0, 1), new Vector3D(0, 1, 0), 45);

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

		public double EulerX
		{
			get => EulerXDp.Get(this);
			set => EulerXDp.Set(this, value);
		}

		public double EulerY
		{
			get => EulerYDp.Get(this);
			set => EulerYDp.Set(this, value);
		}

		public double EulerZ
		{
			get => EulerZDp.Get(this);
			set => EulerZDp.Set(this, value);
		}

		public Quaternion Root
		{
			get
			{
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

		[SuppressPropertyChangedWarnings]
		private static void OnValueChanged(QuaternionEditor sender, CmQuaternion value)
		{
			sender.ValueQuat = new Quaternion(value.X, value.Y, value.Z, value.W);

			if (sender.RootRotation != null)
			{
				sender.ValueQuat = sender.Root * sender.ValueQuat;
			}

			sender.worldSpaceDelta = sender.ValueQuat;

			if (sender.WorldSpace)
				sender.ValueQuat = Quaternion.Identity;

			sender.rotationGizmo.Transform = new RotateTransform3D(new QuaternionRotation3D(sender.ValueQuat));

			if (sender.lockdp)
				return;

			sender.lockdp = true;

			Vector euler = sender.Value.ToEuler();
			sender.EulerX = euler.X;
			sender.EulerY = euler.Y;
			sender.EulerZ = euler.Z;

			sender.lockdp = false;
		}

		[SuppressPropertyChangedWarnings]
		private static void OnRootRotationChanged(QuaternionEditor sender, CmQuaternion? value)
		{
			OnValueChanged(sender, sender.Value);
		}

		[SuppressPropertyChangedWarnings]
		private static void OnValueQuatChanged(QuaternionEditor sender, Quaternion value)
		{
			Quaternion newrot = value;

			if (sender.WorldSpace)
				newrot *= sender.worldSpaceDelta;

			sender.rotationGizmo.Transform = new RotateTransform3D(new QuaternionRotation3D(newrot));

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

			Vector euler = sender.Value.ToEuler();
			sender.EulerX = euler.X;
			sender.EulerY = euler.Y;
			sender.EulerZ = euler.Z;

			sender.lockdp = false;
		}

		[SuppressPropertyChangedWarnings]
		private static void OnEulerChanged(QuaternionEditor sender, double val)
		{
			if (sender.lockdp)
				return;

			sender.lockdp = true;
			Vector euler = new Vector((float)sender.EulerX, (float)sender.EulerY, (float)sender.EulerZ);
			sender.Value = CmQuaternion.FromEuler(euler);
			sender.lockdp = false;
		}

		private void OnViewportMouseDown(object sender, MouseButtonEventArgs e)
		{
			this.mouseDown = true;
			Mouse.Capture(this.Viewport);
		}

		private void OnViewportMouseUp(object sender, MouseButtonEventArgs e)
		{
			this.mouseDown = false;
			Mouse.Capture(null);
			this.rotationGizmo.Hover(null);
		}

		private void OnViewportMouseMove(object sender, MouseEventArgs e)
		{
			Point mousePosition = e.GetPosition(this.Viewport);

			if (!this.mouseDown)
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
			if (this.mouseDown)
				return;

			this.rotationGizmo.Hover(null);
			this.mouseDown = false;
		}

		private void OnViewportMouseWheel(object sender, MouseWheelEventArgs e)
		{
			double delta = e.Delta > 0 ? this.TickFrequency : -this.TickFrequency;

			if (Keyboard.IsKeyDown(Key.LeftShift))
				delta *= 10;

			this.rotationGizmo.Scroll(delta);
		}

		private void WatchCamera()
		{
			IMemory<float> camX = Offsets.Main.CameraOffset.GetMemory(Offsets.Main.CameraAngleX);
			IMemory<float> camY = Offsets.Main.CameraOffset.GetMemory(Offsets.Main.CameraAngleY);
			IMemory<float> camZ = Offsets.Main.CameraOffset.GetMemory(Offsets.Main.CameraRotation);

			Vector3D camEuler = default;

			bool vis = true;
			while (vis && Application.Current != null)
			{
				camEuler.Y = (float)MathUtils.RadiansToDegrees((double)camX.Value) - 180;
				camEuler.Z = (float)-MathUtils.RadiansToDegrees((double)camY.Value);
				camEuler.X = (float)MathUtils.RadiansToDegrees((double)camZ.Value);
				Quaternion q = camEuler.ToQuaternion();

				try
				{
					Application.Current.Dispatcher.Invoke(() =>
					{
						vis = this.IsVisible; ////&& this.IsEnabled;
						this.Viewport.Camera.Transform = new RotateTransform3D(new QuaternionRotation3D(q));
					});
				}
				catch (Exception)
				{
				}

				Thread.Sleep(16);
			}
		}

		[SuppressPropertyChangedWarnings]
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
			private AxisGizmo hoveredGizmo;

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

			public bool Hover(DependencyObject visual)
			{
				AxisGizmo gizmo = null;
				if (visual is Circle r)
				{
					gizmo = (AxisGizmo)VisualTreeHelper.GetParent(r);
				}
				else if (visual is Cylinder c)
				{
					gizmo = (AxisGizmo)VisualTreeHelper.GetParent(c);
				}

				if (this.hoveredGizmo != null)
					this.hoveredGizmo.Hovered = false;

				this.hoveredGizmo = gizmo;

				if (this.hoveredGizmo != null)
				{
					this.hoveredGizmo.Hovered = true;
					return true;
				}

				return false;
			}

			public void Drag(Point3D mousePosition)
			{
				if (this.hoveredGizmo == null)
					return;

				Vector3D angleDelta = this.hoveredGizmo.Drag(mousePosition);
				this.ApplyDelta(angleDelta);
			}

			public void Scroll(double delta)
			{
				if (this.hoveredGizmo == null)
					return;

				Vector3D angleDelta = this.hoveredGizmo.Axis * delta;
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
			private bool hovered;
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
				this.cylinder.Length = 0.15;
				this.cylinder.Transform = new RotateTransform3D(new AxisAngleRotation3D(axis, 90));
				this.cylinder.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Transparent));
				this.Children.Add(this.cylinder);
			}

			public bool Hovered
			{
				get
				{
					return this.hovered;
				}

				set
				{
					this.hovered = value;

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

			public void StartDrag()
			{
				this.lastPoint = null;
			}

			public Vector3D Drag(Point3D mousePosition)
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

					return this.Axis * (angle * 2);
				}
			}
		}
	}
}
