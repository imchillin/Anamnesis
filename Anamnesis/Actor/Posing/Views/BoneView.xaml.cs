// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using Anamnesis.Actor.Posing;
using MaterialDesignThemes.Wpf;
using Serilog;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using XivToolsWpf;
using XivToolsWpf.DependencyProperties;

/// <summary>
/// Interaction logic for BoneView.xaml.
/// Represents a 2D bone selector for a <see cref="Core.Bone"/> in the actor skeleton,
/// providing mechanisms for displaying and interacting with it.
/// </summary>
public partial class BoneView : UserControl
{
	/// <summary>Dependency property for the label of the bone view.</summary>
	public static readonly IBind<string> LabelDp = Binder.Register<string, BoneView>(nameof(Label));

	/// <summary>Dependency property for the name of the bone.</summary>
	public static readonly IBind<string> NameDp = Binder.Register<string, BoneView>(nameof(BoneName));

	/// <summary>Dependency property for the flipped name of the bone.</summary>
	public static readonly IBind<string> FlippedNameDp = Binder.Register<string, BoneView>(nameof(FlippedBoneName));

	private readonly List<Line> linesToChildren = new();
	private readonly List<Line> mouseLinesToChildren = new();

	private SkeletonEntity? skeleton;

	/// <summary>
	/// Initializes a new instance of the <see cref="BoneView"/> class.
	/// </summary>
	public BoneView()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		BoneViewManager.Instance.AddBoneView(this);

		this.IsEnabledChanged += this.OnIsEnabledChanged;
	}

	/// <summary>Gets the bone associated with this view.</summary>
	public BoneEntity? Bone { get; private set; }

	/// <summary>Gets or sets the label of the bone view.</summary>
	public string Label
	{
		get => LabelDp.Get(this);
		set => LabelDp.Set(this, value);
	}

	/// <summary>Gets or sets the name of the bone.</summary>
	public string BoneName
	{
		get => NameDp.Get(this);
		set => NameDp.Set(this, value);
	}

	/// <summary>Gets or sets the flipped name of the bone.</summary>
	public string FlippedBoneName
	{
		get => FlippedNameDp.Get(this);
		set => FlippedNameDp.Set(this, value);
	}

	/// <summary>
	/// Gets the current name of the bone, considering whether the skeleton is flipped.
	/// </summary>
	public string CurrentBoneName
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			if (this.skeleton == null)
				return this.BoneName;

			if (string.IsNullOrEmpty(this.FlippedBoneName) || !this.skeleton.FlipSides)
				return this.BoneName;

			return this.FlippedBoneName;
		}
	}

	/// <summary>Sets the bone by name.</summary>
	/// <param name="name">The name of the bone to set.</param>
	public void SetBone(string name) => this.SetBone(this.skeleton?.GetBone(name));

	/// <summary>Sets the bone.</summary>
	/// <param name="bone">The bone to set.</param>
	public void SetBone(BoneEntity? bone)
	{
		this.Bone = bone;
		this.IsEnabled = bone != null;
		BoneViewManager.Instance.AddBoneView(this);
	}

	/// <summary>
	/// Redraws the skeleton, updating the lines connecting <see cref="BoneView"/> instances.
	/// </summary>
	public void RedrawSkeleton()
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

		if (!this.IsEnabled || this.Bone?.Parent == null)
			return;

		foreach (BoneView childView in BoneViewManager.Instance.GetBoneViews(this.Bone.Parent))
		{
			if (childView.Visibility != Visibility.Visible)
				continue;

			var scale = 1.0;

			// Determine line stroke thickness (if placed inside a Viewbox element)
			if (this.Parent is Canvas cvs && cvs.Parent is Viewbox vbox)
			{
				scale = vbox.ActualWidth / cvs.ActualWidth;
			}

			double x1 = this.GetCenterX();
			double y1 = this.GetCenterY();
			double x2 = childView.GetCenterX();
			double y2 = childView.GetCenterY();

			if (double.IsNaN(x1) || double.IsNaN(y1) || double.IsNaN(x2) || double.IsNaN(y2))
				continue;

			if (this.Parent is Canvas c1 && childView.Parent is Canvas c2 && c1 == c2)
			{
				Line line = new()
				{
					SnapsToDevicePixels = true,
					StrokeThickness = 1 / scale,
					Stroke = Brushes.Gray,
					IsHitTestVisible = false,
					X1 = x1,
					Y1 = y1,
					X2 = x2,
					Y2 = y2,
				};

				c1.Children.Insert(0, line);
				this.linesToChildren.Add(line);

				// A transparent line to make mouse operations easier
				var line2 = new Line
				{
					StrokeThickness = 25,
					Stroke = Brushes.Transparent,
					X1 = x1,
					Y1 = y1,
					X2 = x2,
					Y2 = y2,
				};

				line2.MouseEnter += childView.OnMouseEnter;
				line2.MouseLeave += childView.OnMouseLeave;
				line2.MouseUp += childView.OnMouseUp;

				c1.Children.Insert(0, line2);
				this.mouseLinesToChildren.Add(line2);
			}
		}
	}

	/// <summary>
	/// Updates the state of the bone view, including visual appearance based on
	/// selection and hover states.
	/// </summary>
	public void UpdateState()
	{
		if (this.Bone == null)
		{
			this.ErrorEllipse.Visibility = Visibility.Visible;
			this.ForegroundElipse.Visibility = Visibility.Hidden;
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
			this.ForegroundElipse.Visibility = Visibility.Hidden;
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

	/// <summary>Handles the data context changed event.</summary>
	/// <param name="sender">The sender of the event.</param>
	/// <param name="e">The event data.</param>
	private async void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		await Dispatch.MainThread();

		try
		{
			if (this.DataContext is SkeletonEntity viewModel)
			{
				this.skeleton = viewModel;
				this.SetBone(this.CurrentBoneName);
			}
			else if (this.DataContext is BoneEntity bone)
			{
				this.skeleton = bone.Skeleton;
				this.SetBone(bone);
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

	/// <summary>Handles the IsEnabledChanged event.</summary>
	/// <param name="sender">The sender of the event.</param>
	/// <param name="e">The event data.</param>
	private async void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		await Dispatch.MainThread();

		this.UpdateState();
	}

	/// <summary>Gets the center X coordinate of the bone view.</summary>
	/// <returns>The center X coordinate.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private double GetCenterX() => Canvas.GetLeft(this) + ((this.ActualWidth + this.Margin.Left + this.Margin.Right + this.Padding.Left + this.Padding.Right + this.BorderThickness.Right) / 2);

	/// <summary>Gets the center Y coordinate of the bone view.</summary>
	/// <returns>The center Y coordinate.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private double GetCenterY() => Canvas.GetTop(this) + ((this.ActualHeight + this.Margin.Top + this.Margin.Bottom + this.Padding.Top + this.Padding.Bottom + this.BorderThickness.Bottom) / 2);

	/// <summary>Handles the MouseEnter event.</summary>
	/// <param name="sender">The sender of the event.</param>
	/// <param name="e">The event data.</param>
	private void OnMouseEnter(object sender, MouseEventArgs e)
	{
		if (!this.IsEnabled || this.skeleton == null || this.Bone == null)
			return;

		this.skeleton.Hover(this.Bone, true);
	}

	/// <summary>Handles the MouseLeave event.</summary>
	/// <param name="sender">The sender of the event.</param>
	/// <param name="e">The event data.</param>
	private void OnMouseLeave(object sender, MouseEventArgs e)
	{
		if (!this.IsEnabled || this.skeleton == null || this.Bone == null)
			return;

		this.skeleton.Hover(this.Bone, false);
	}

	/// <summary>Handles the MouseUp event.</summary>
	/// <param name="sender">The sender of the event.</param>
	/// <param name="e">The event data.</param>
	private void OnMouseUp(object sender, MouseButtonEventArgs e)
	{
		if (!this.IsEnabled || this.skeleton == null || this.Bone == null)
			return;

		this.skeleton.Select(this.Bone);
	}

	/// <summary>Sets the state of the bone view.</summary>
	/// <param name="stroke">The stroke brush.</param>
	/// <param name="thickness">The stroke thickness.</param>
	private void SetState(Brush stroke, double thickness)
	{
		this.BackgroundElipse.StrokeThickness = thickness;

		foreach (Line line in this.linesToChildren)
		{
			line.Stroke = stroke;
			line.StrokeThickness = thickness;
		}
	}

	/// <summary>Handles the Unloaded event.</summary>
	/// <param name="sender">The sender of the event.</param>
	/// <param name="e">The event data.</param>
	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		foreach (Line line in this.mouseLinesToChildren)
		{
			if (line.Parent is Panel parentPanel)
			{
				parentPanel.Children.Remove(line);
			}

			line.MouseEnter -= this.OnMouseEnter;
			line.MouseLeave -= this.OnMouseLeave;
			line.MouseUp -= this.OnMouseUp;
		}

		foreach (Line line in this.linesToChildren)
		{
			if (line.Parent is Panel parentPanel)
			{
				parentPanel.Children.Remove(line);
			}
		}

		this.mouseLinesToChildren.Clear();
		this.linesToChildren.Clear();

		this.IsEnabledChanged -= this.OnIsEnabledChanged;
	}
}
