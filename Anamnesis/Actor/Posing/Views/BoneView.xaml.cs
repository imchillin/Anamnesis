// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Anamnesis.Actor.Pages;
using MaterialDesignThemes.Wpf;
using Serilog;
using XivToolsWpf.DependencyProperties;

public partial class BoneView : UserControl, IBone
{
	public static readonly IBind<string> LabelDp = Binder.Register<string, BoneView>(nameof(Label));
	public static readonly IBind<string> NameDp = Binder.Register<string, BoneView>(nameof(BoneName));
	public static readonly IBind<string> FlippedNameDp = Binder.Register<string, BoneView>(nameof(FlippedBoneName));

	private readonly List<Line> linesToChildren = new List<Line>();
	private readonly List<Line> mouseLinesToChildren = new List<Line>();

	private SkeletonVisual3d? skeleton;

	public BoneView()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
		this.BindDataContext();

		this.IsEnabledChanged += this.OnIsEnabledChanged;
	}

	public BoneVisual3d? Bone { get; private set; }

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

	public string FlippedBoneName
	{
		get => FlippedNameDp.Get(this);
		set => FlippedNameDp.Set(this, value);
	}

	public BoneVisual3d? Visual => this.Bone;

	public string CurrentBoneName
	{
		get
		{
			if (this.skeleton == null)
				return this.BoneName;

			if (string.IsNullOrEmpty(this.FlippedBoneName) || !this.skeleton.FlipSides)
				return this.BoneName;

			return this.FlippedBoneName;
		}
	}

	private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		this.BindDataContext();
	}

	private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		this.UpdateState();
	}

	private void BindDataContext()
	{
		try
		{
			if (this.skeleton != null)
				this.skeleton.PropertyChanged -= this.OnSkeletonPropertyChanged;

			if (this.DataContext is SkeletonVisual3d viewModel)
			{
				this.skeleton = viewModel;
				this.SetBone(this.CurrentBoneName);
				this.skeleton.PropertyChanged += this.OnSkeletonPropertyChanged;
			}
			else if (this.DataContext is BoneVisual3d bone)
			{
				this.skeleton = bone.Skeleton;
				this.SetBone(bone);
				this.skeleton.PropertyChanged += this.OnSkeletonPropertyChanged;
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
			Log.Error(ex, "Failed to bind bone view");
		}
	}

	private void OnSkeletonPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		bool refreshBone = this.Bone == null || e.PropertyName == nameof(SkeletonVisual3d.FlipSides);

		if (refreshBone && this.DataContext is SkeletonVisual3d)
			this.SetBone(this.CurrentBoneName);

		this.UpdateState();
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

		foreach (Line line in this.mouseLinesToChildren)
		{
			if (line.Parent is Panel parentPanel)
				parentPanel.Children.Remove(line);

			line.MouseEnter -= this.OnMouseEnter;
			line.MouseLeave -= this.OnMouseLeave;
			line.MouseUp -= this.OnMouseUp;
		}

		this.linesToChildren.Clear();

		PosePage? page = this.FindParent<PosePage>();
		if (page == null)
			return;

		BoneVisual3d? parent = this.Bone?.Parent;
		if (parent != null)
		{
			List<BoneView> parentViews = page.GetBoneViews(parent);

			foreach (BoneView childView in parentViews)
			{
				if (childView.Visibility != Visibility.Visible)
					continue;

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

					// A transparent line to make mouse operations easier
					Line line2 = new Line();
					line2.StrokeThickness = 25;
					line2.Stroke = Brushes.Transparent;
					////line2.Opacity = 0.1f;

					line2.MouseEnter += childView.OnMouseEnter;
					line2.MouseLeave += childView.OnMouseLeave;
					line2.MouseUp += childView.OnMouseUp;

					line2.X1 = line.X1;
					line2.Y1 = line.Y1;
					line2.X2 = line.X2;
					line2.Y2 = line.Y2;

					c1.Children.Insert(0, line2);
					this.mouseLinesToChildren.Add(line2);
				}
			}
		}
	}

	private void SetBone(string name)
	{
		this.SetBone(this.skeleton?.GetBone(name));
	}

	private void SetBone(BoneVisual3d? bone)
	{
		PosePage? page = this.FindParent<PosePage>();
		if (page != null)
			page.BoneViews.Add(this);

		this.Bone = bone;

		if (this.Bone != null)
		{
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
			this.IsEnabled = false;
			this.UpdateState();
		}
	}

	private void OnMouseEnter(object sender, MouseEventArgs e)
	{
		if (!this.IsEnabled || this.skeleton == null || this.Bone == null)
			return;

		this.skeleton.Hover(this.Bone, true);
	}

	private void OnMouseLeave(object sender, MouseEventArgs e)
	{
		if (!this.IsEnabled || this.skeleton == null || this.Bone == null)
			return;

		this.skeleton.Hover(this.Bone, false);
	}

	private void OnMouseUp(object sender, MouseButtonEventArgs e)
	{
		if (!this.IsEnabled)
			return;

		if (this.skeleton == null || this.Bone == null)
			return;

		this.skeleton.Select(this);
	}

	private void UpdateState()
	{
		if (this.Bone == null)
		{
			this.ErrorEllipse.Visibility = Visibility.Visible;
			this.BackgroundElipse.Visibility = Visibility.Collapsed;
			return;
		}

		this.ErrorEllipse.Visibility = Visibility.Collapsed;
		this.BackgroundElipse.Visibility = Visibility.Visible;

		PaletteHelper ph = new PaletteHelper();
		ITheme theme = ph.GetTheme();

		if (!this.IsEnabled || this.skeleton == null)
		{
			this.SetState(new SolidColorBrush(Colors.Transparent), 1);
			this.BackgroundElipse.Opacity = 0.5;
			this.BackgroundElipse.StrokeThickness = 0;
			return;
		}

		this.BackgroundElipse.Opacity = 1;
		this.BackgroundElipse.StrokeThickness = 1;

		bool hovered = this.skeleton.GetIsBoneHovered(this.Bone);
		bool selected = this.skeleton.GetIsBoneSelected(this.Bone);
		bool parentSelected = this.skeleton.GetIsBoneParentsSelected(this.Bone);
		bool parentHovered = this.skeleton.GetIsBoneParentsHovered(this.Bone);

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

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		PosePage? page = this.FindParent<PosePage>();
		if (page == null)
			return;

		page.BoneViews.Add(this);
	}

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		PosePage? page = this.FindParent<PosePage>();
		if (page == null)
			return;

		page.BoneViews.Remove(this);
	}
}
