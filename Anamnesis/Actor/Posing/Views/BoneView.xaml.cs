// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using Anamnesis.Actor.Pages;
using Anamnesis.Actor.Posing;
using MaterialDesignThemes.Wpf;
using Serilog;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using XivToolsWpf.DependencyProperties;

public partial class BoneView : UserControl
{
	public static readonly IBind<string> LabelDp = Binder.Register<string, BoneView>(nameof(Label));
	public static readonly IBind<string> NameDp = Binder.Register<string, BoneView>(nameof(BoneName));
	public static readonly IBind<string> FlippedNameDp = Binder.Register<string, BoneView>(nameof(FlippedBoneName));

	private readonly List<Line> linesToChildren = new();
	private readonly List<Line> mouseLinesToChildren = new();

	private SkeletonEntity? skeleton;

	public BoneView()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
		this.BindDataContext();

		this.IsEnabledChanged += this.OnIsEnabledChanged;
	}

	public BoneEntity? Bone { get; private set; }

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

			if (this.DataContext is SkeletonEntity viewModel)
			{
				this.skeleton = viewModel;
				this.SetBone(this.CurrentBoneName);
				this.skeleton.PropertyChanged += this.OnSkeletonPropertyChanged;
			}
			else if (this.DataContext is BoneEntity bone)
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
		Application.Current.Dispatcher.Invoke(() =>
		{
			bool refreshBone = this.Bone == null || e.PropertyName == nameof(SkeletonEntity.FlipSides) || e.PropertyName == nameof(SkeletonEntity.Bones);
			if (refreshBone && this.DataContext is SkeletonEntity)
				this.SetBone(this.CurrentBoneName);

			this.UpdateState();
		});
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
		if (page == null || this.Bone?.Parent == null)
			return;

		foreach (BoneView childView in page.GetBoneViews(this.Bone.Parent))
		{
			if (childView.Visibility != Visibility.Visible)
				continue;

			var scale = 1.0;

			// Determine line stroke thickness (if placed inside a Viewbox element)
			if (this.Parent is Canvas cvs && cvs.Parent is Viewbox vbox)
			{
				scale = vbox.ActualWidth / cvs.ActualWidth;
			}

			if (this.Parent is Canvas c1 && childView.Parent is Canvas c2 && c1 == c2)
			{
				Line line = new()
				{
					SnapsToDevicePixels = true,
					StrokeThickness = 1 / scale,
					Stroke = Brushes.Gray,
					IsHitTestVisible = false,
					X1 = this.GetCenterX(),
					Y1 = this.GetCenterY(),
					X2 = childView.GetCenterX(),
					Y2 = childView.GetCenterY(),
				};

				c1.Children.Insert(0, line);
				this.linesToChildren.Add(line);

				// A transparent line to make mouse operations easier
				var line2 = new Line
				{
					StrokeThickness = 25,
					Stroke = Brushes.Transparent,
					X1 = line.X1,
					Y1 = line.Y1,
					X2 = line.X2,
					Y2 = line.Y2,
				};

				line2.MouseEnter += childView.OnMouseEnter;
				line2.MouseLeave += childView.OnMouseLeave;
				line2.MouseUp += childView.OnMouseUp;

				c1.Children.Insert(0, line2);
				this.mouseLinesToChildren.Add(line2);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private double GetCenterX() => Canvas.GetLeft(this) + ((this.ActualWidth + this.Margin.Left + this.Margin.Right + this.Padding.Left + this.Padding.Right + this.BorderThickness.Right) / 2);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private double GetCenterY() => Canvas.GetTop(this) + ((this.ActualHeight + this.Margin.Top + this.Margin.Bottom + this.Padding.Top + this.Padding.Bottom + this.BorderThickness.Bottom) / 2);

	private void SetBone(string name)
	{
		this.SetBone(this.skeleton?.GetBone(name));
	}

	private void SetBone(BoneEntity? bone)
	{
		PosePage? page = this.FindParent<PosePage>();
		page?.BoneViews.Add(this);

		this.Bone = bone;
		this.IsEnabled = bone != null;

		if (this.Bone != null)
		{
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

		this.skeleton.Select(this.Bone);
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

		PaletteHelper ph = new();
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

		bool hovered = this.Bone.IsHovered;
		bool selected = this.Bone.IsSelected;
		bool parentSelected = this.Bone.IsAncestorSelected();
		bool parentHovered = this.Bone.IsAncestorHovered();

		Color color = parentHovered ? theme.PrimaryMid.Color : theme.BodyLight;
		double thickness = parentSelected || selected || parentHovered ? 2 : 1;

		// Scale thickness based on viewbox scale (if applicable)
		if (this.Parent is Canvas cvs && cvs.Parent is Viewbox vbox)
		{
			thickness /= vbox.ActualWidth / cvs.ActualWidth;
		}

		this.ForegroundElipse.Visibility = (selected || hovered) ? Visibility.Visible : Visibility.Hidden;
		this.BackgroundElipse.Stroke = new SolidColorBrush(theme.PrimaryMid.Color);
		this.SetState(new SolidColorBrush(color), thickness);
	}

	private void SetState(Brush stroke, double thickness)
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
