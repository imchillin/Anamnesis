// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule.Controls
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Media;
	using System.Windows.Shapes;
	using ConceptMatrix.WpfStyles.DependencyProperties;
	using MaterialDesignThemes.Wpf;

	public partial class BoneView : UserControl
	{
		public static readonly IBind<string> LabelDp = Binder.Register<string, BoneView>(nameof(Label));
		public static readonly IBind<string> NameDp = Binder.Register<string, BoneView>(nameof(BoneName));

		private static Dictionary<Bone, List<BoneView>> boneViews = new Dictionary<Bone, List<BoneView>>();

		private SkeletonViewModel viewModel;
		private Bone bone;

		private List<Line> linesToChildren = new List<Line>();

		public BoneView()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;
			this.BindDataContext();
		}

		public string Label
		{
			get => LabelDp.Get(this);
			set => LabelDp.Set(this, value);
		}

		public string BoneName
		{
			get => NameDp.Get(this);
			set => NameDp.Set(this, value);
		}

		public new bool IsEnabled
		{
			get
			{
				if (this.viewModel != null && !this.viewModel.IsEnabled)
					return false;

				if (this.bone == null || !this.bone.IsEnabled)
					return false;

				return base.IsEnabled;
			}
			set
			{
				base.IsEnabled = value;
			}
		}

		public static bool HasView(Bone bone)
		{
			return boneViews.ContainsKey(bone);
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			this.BindDataContext();
		}

		private void BindDataContext()
		{
			if (!this.IsVisible)
				return;

			try
			{
				if (this.DataContext is SkeletonViewModel viewModel)
				{
					this.viewModel = viewModel;
					this.viewModel.PropertyChanged += this.OnViewModelPropertyChanged;

					this.SetBone(this.BoneName);
				}
				else
				{
					this.IsEnabled = false;
				}
			}
			catch (Exception ex)
			{
				this.IsEnabled = false;
				this.ToolTip = ex.Message;
				Console.WriteLine(ex.Message);
			}
		}

		private void DrawSkeleton()
		{
			foreach (Line line in this.linesToChildren)
			{
				if (line.Parent is Panel parentPanel)
				{
					parentPanel.Children.Remove(line);
				}
			}

			this.linesToChildren.Clear();

			foreach (Bone bone in this.bone.Children)
			{
				if (!boneViews.ContainsKey(bone))
					continue;

				foreach (BoneView childView in boneViews[bone])
				{
					if (this.Parent is Canvas c1 && childView.Parent is Canvas c2 && c1 == c2)
					{
						Line line = new Line();
						line.SnapsToDevicePixels = true;
						line.StrokeThickness = 1;
						line.Stroke = Brushes.Gray;
						line.IsHitTestVisible = false;

						line.X1 = Canvas.GetLeft(this) + (this.Width / 2);
						line.Y1 = Canvas.GetTop(this) + (this.Height / 2);
						line.X2 = Canvas.GetLeft(childView) + (childView.Width / 2);
						line.Y2 = Canvas.GetTop(childView) + (childView.Height / 2);

						c1.Children.Insert(0, line);
						this.linesToChildren.Add(line);
					}
				}
			}
		}

		private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(this.viewModel.CameraRotation))
				return;

			this.UpdateState();

			if (e.PropertyName == nameof(this.viewModel.FlipSides))
			{
				this.SetBone(SkeletonViewModel.GetBoneName(this.BoneName, this.viewModel.FlipSides));
			}
			else if (e.PropertyName == nameof(this.viewModel.Bones))
			{
				this.SetBone(this.BoneName);
			}
		}

		private void SetBone(string name)
		{
			if (this.bone != null)
			{
				if (boneViews.ContainsKey(this.bone))
				{
					boneViews[this.bone].Remove(this);
				}
			}

			this.bone = this.viewModel.GetBone(name);

			if (this.bone != null)
			{
				this.ToolTip = this.bone.Tooltip;

				if (!boneViews.ContainsKey(this.bone))
					boneViews.Add(this.bone, new List<BoneView>());

				boneViews[this.bone].Add(this);

				this.ToolTip = this.bone.Tooltip;
				this.IsEnabled = true;

				// Wait for all bone views to load, then draw the skeleton
				Application.Current.Dispatcher.InvokeAsync(async () =>
				{
					await Task.Delay(1);
					this.DrawSkeleton();
					this.UpdateState();
				});
			}
			else
			{
				Log.Write("Bone not found: " + name, "Posing", Log.Severity.Warning);
				this.IsEnabled = false;
				this.UpdateState();
			}
		}

		private void OnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			if (!this.IsEnabled)
				return;

			this.viewModel.MouseOverBone = this.bone;
		}

		private void OnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			if (!this.IsEnabled)
				return;

			if (this.viewModel.MouseOverBone == this.bone)
			{
				this.viewModel.MouseOverBone = null;
			}
		}

		private void OnMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (!this.IsEnabled)
				return;

			if (this.viewModel == null || this.bone == null)
				return;

			this.viewModel.CurrentBone = this.bone;
		}

		private void UpdateState()
		{
			if (this.bone == null)
			{
				this.ErrorEllipse.Visibility = Visibility.Visible;
				this.BackgroundElipse.Visibility = Visibility.Collapsed;
				return;
			}

			this.ErrorEllipse.Visibility = Visibility.Collapsed;
			this.BackgroundElipse.Visibility = Visibility.Visible;

			PaletteHelper ph = new PaletteHelper();
			ITheme theme = ph.GetTheme();

			if (!this.IsEnabled)
			{
				this.SetState(new SolidColorBrush(Colors.Transparent), 1);
				this.BackgroundElipse.Opacity = 0.5;
				this.BackgroundElipse.StrokeThickness = 0;
				return;
			}

			this.BackgroundElipse.Opacity = 1;
			this.BackgroundElipse.StrokeThickness = 1;

			bool hovered = this.viewModel.GetIsBoneHovered(this.bone);
			bool selected = this.viewModel.GetIsBoneSelected(this.bone);
			bool parentSelected = this.viewModel.GetIsBoneParentsSelected(this.bone);
			bool parentHovered = this.viewModel.GetIsBoneParentsHovered(this.bone);

			Color color = parentHovered ? theme.PrimaryMid.Color : theme.BodyLight;
			int thickenss = parentSelected || selected || parentHovered ? 2 : 1;

			this.ForegroundElipse.Visibility = (selected || hovered) ? Visibility.Visible : Visibility.Hidden;
			this.BackgroundElipse.Stroke = new SolidColorBrush(theme.PrimaryMid.Color);
			this.SetState(new SolidColorBrush(color), thickenss);
		}

		private void SetState(Brush stroke, int thickness)
		{
			this.BackgroundElipse.StrokeThickness = thickness;

			foreach (Line line in this.linesToChildren)
			{
				line.Stroke = stroke;
				line.StrokeThickness = thickness;
			}
		}

		private void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			this.BindDataContext();
		}
	}
}
