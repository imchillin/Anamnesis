// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule.Controls
{
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Media.Media3D;
	using ConceptMatrix.Three3D;
	using ConceptMatrix.ThreeD;
	using ConceptMatrix.ThreeD.Lines;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for QuaternionEditor.xaml.
	/// </summary>
	public partial class QuaternionEditor : UserControl, INotifyPropertyChanged
	{
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(Quaternion), typeof(QuaternionEditor), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnValueChangedStatic)));
		public static readonly DependencyProperty TickFrequencyProperty = DependencyProperty.Register(nameof(TickFrequency), typeof(double), typeof(QuaternionEditor));
		public static readonly DependencyProperty CameraRotationProperty = DependencyProperty.Register(nameof(CameraRotation), typeof(Quaternion), typeof(QuaternionEditor), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnCameraRotationChangedStatic)));

		private Vector3D euler;
		private bool eulerLock = false;
		private RotationGizmo rotationGizmo;
		private bool mouseDown = false;

		public QuaternionEditor()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;

			this.TickFrequency = 0.5;

			this.rotationGizmo = new RotationGizmo(this);
			this.Viewport.Children.Add(this.rotationGizmo);

			this.Viewport.Camera = new PerspectiveCamera(new Point3D(0, 0, -2.5), new Vector3D(0, 0, 1), new Vector3D(0, 1, 0), 45);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public double TickFrequency
		{
			get
			{
				return (double)this.GetValue(TickFrequencyProperty);
			}
			set
			{
				this.SetValue(TickFrequencyProperty, value);
			}
		}

		[AlsoNotifyFor(nameof(EulerX), nameof(EulerY), nameof(EulerZ))]
		public Quaternion Value
		{
			get
			{
				return (Quaternion)this.GetValue(ValueProperty);
			}

			set
			{
				if (!this.eulerLock)
					this.euler = value.ToEulerAngles();

				this.SetValue(ValueProperty, value);
				this.rotationGizmo.Transform = new RotateTransform3D(new QuaternionRotation3D(value));
			}
		}

		public Quaternion CameraRotation
		{
			get
			{
				return (Quaternion)this.GetValue(CameraRotationProperty);
			}

			set
			{
				this.SetValue(CameraRotationProperty, value);
				this.Viewport.Camera.Transform = new RotateTransform3D(new QuaternionRotation3D(value));
			}
		}

		public double EulerX
		{
			get
			{
				return this.euler.X;
			}
			set
			{
				this.eulerLock = true;
				this.euler.X = value;
				this.Value = this.euler.ToQuaternion();
				this.eulerLock = false;
			}
		}

		public double EulerY
		{
			get
			{
				return this.euler.Y;
			}
			set
			{
				this.eulerLock = true;
				this.euler.Y = value;
				this.Value = this.euler.ToQuaternion();
				this.eulerLock = false;
			}
		}

		public double EulerZ
		{
			get
			{
				return this.euler.Z;
			}
			set
			{
				this.eulerLock = true;
				this.euler.Z = value;
				this.Value = this.euler.ToQuaternion();
				this.eulerLock = false;
			}
		}

		private static void OnValueChangedStatic(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			if (sender is QuaternionEditor quaternionEditor)
			{
				if (quaternionEditor.eulerLock)
					return;

				quaternionEditor.euler = quaternionEditor.Value.ToEulerAngles();
				quaternionEditor.rotationGizmo.Transform = new RotateTransform3D(new QuaternionRotation3D(quaternionEditor.Value));

				quaternionEditor.PropertyChanged.Invoke(sender, new PropertyChangedEventArgs(nameof(Value)));
				quaternionEditor.PropertyChanged.Invoke(sender, new PropertyChangedEventArgs(nameof(EulerX)));
				quaternionEditor.PropertyChanged.Invoke(sender, new PropertyChangedEventArgs(nameof(EulerY)));
				quaternionEditor.PropertyChanged.Invoke(sender, new PropertyChangedEventArgs(nameof(EulerZ)));
			}
		}

		private static void OnCameraRotationChangedStatic(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			if (sender is QuaternionEditor quaternionEditor)
			{
				quaternionEditor.Viewport.Camera.Transform = new RotateTransform3D(new QuaternionRotation3D(quaternionEditor.CameraRotation));

				quaternionEditor.PropertyChanged.Invoke(sender, new PropertyChangedEventArgs(nameof(CameraRotation)));
			}
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

		private class RotationGizmo : ModelVisual3D
		{
			private AxisGizmo hoveredGizmo;
			private QuaternionEditor target;

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
				// special case for LHS to RHS conversion, I think.
				angleEuler.Z *= -1;

				Quaternion angle = angleEuler.ToQuaternion();
				this.target.Value *= angle;
			}
		}

		private class AxisGizmo : ModelVisual3D
		{
			public readonly Vector3D Axis;

			private Circle circle;
			private Cylinder cylinder;
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

			public void StartDrag(Point3D mousePosition)
			{
				this.lastPoint = null;
			}

			public Vector3D Drag(Point3D mousePosition)
			{
				Point3D? point = this.circle.NearestPoint2D(mousePosition);

				if (point == null)
					return default(Vector3D);

				point = this.circle.TransformToAncestor(this).Transform((Point3D)point);

				if (this.lastPoint == null)
				{
					this.lastPoint = point;
					return default(Vector3D);
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

					return this.Axis * (angle * 2);
				}
			}
		}
	}
}
