// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using Anamnesis.Actor.Posing;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

/// <summary>
/// Interaction logic for PoseFaceGuiView.xaml.
/// </summary>
public partial class PoseFaceGuiView : UserControl
{
	private readonly Dictionary<string, Point> mouthGuideLineCoords = new()
	{
		{ "StandardFaceTemplate", new Point(224, 346) },
		{ "HrothgarFaceTemplate", new Point(210, 345) },
		{ "MiqoteFaceTemplate", new Point(220, 355) },
		{ "VieraFloppyFaceTemplate", new Point(214, 370) },
		{ "VieraStraightFaceTemplate", new Point(214, 372) },
	};

	private readonly Border? mouthSelectorBorder;
	private readonly Line? mouthGuideLine;
	private readonly Ellipse? mouthGuideEllipse;
	private readonly FaceTemplateSelector? templateSelector;

	public PoseFaceGuiView()
	{
		this.InitializeComponent();

		if (this.Resources["FaceTemplateSelector"] is FaceTemplateSelector selector)
		{
			this.templateSelector = selector;
			this.templateSelector.TemplateChanged += this.OnTemplateChanged;
		}

		// Load components.
		this.mouthGuideEllipse = this.FindName("MouthGuideEllipse") as Ellipse;
		Debug.Assert(this.mouthGuideEllipse != null, "Failed to find MouthGuideEllipse.");

		this.mouthGuideLine = this.FindName("MouthGuideLine") as Line;
		Debug.Assert(this.mouthGuideLine != null, "Failed to find MouthGuideLine.");

		this.mouthSelectorBorder = this.FindName("MouthSelectorBorder") as Border;
		Debug.Assert(this.mouthSelectorBorder != null, "Failed to find MouthSelectorBorder.");
	}

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		if (this.templateSelector != null)
		{
			this.templateSelector.TemplateChanged -= this.OnTemplateChanged;
		}
	}

	private void OnTemplateChanged(object? sender, string templateName)
	{
		if (this.mouthGuideLine == null || this.mouthGuideEllipse == null)
		{
			Log.Error("Failed to change properties of uninitialized component.");
			return;
		}

		if (!this.mouthGuideLineCoords.TryGetValue(templateName, out Point coords))
			return;

		// Update Ellipse and Line positions
		Canvas.SetLeft(this.mouthGuideEllipse, coords.X);
		Canvas.SetTop(this.mouthGuideEllipse, coords.Y);

		this.mouthGuideLine.X1 = coords.X + this.mouthGuideEllipse.Width;
		this.mouthGuideLine.Y1 = coords.Y + (this.mouthGuideEllipse.Height / 2);
	}

	private void SetOpacity(string[] elementNames, double opacity)
	{
		foreach (var name in elementNames)
		{
			if (this.FindName(name) is UIElement element)
				element.Opacity = opacity;
		}
	}

	private void MouthSelector_MouseEnter(object sender, MouseEventArgs e)
	{
		if (this.mouthSelectorBorder == null)
		{
			Log.Error("Failed to change properties of uninitialized component.");
			return;
		}

		this.mouthSelectorBorder.BorderBrush = new SolidColorBrush(Colors.Gray) { Opacity = 1.0 };
		this.SetOpacity(["MouthGuideLine", "MouthGuideEllipse"], 0.75);
	}

	private void MouthSelector_MouseLeave(object sender, MouseEventArgs e)
	{
		if (this.mouthSelectorBorder == null)
		{
			Log.Error("Failed to change properties of uninitialized component.");
			return;
		}

		this.MouthSelectorBorder.BorderBrush = new SolidColorBrush(Colors.Gray) { Opacity = 0.5 };
		this.SetOpacity(["MouthGuideLine", "MouthGuideEllipse"], 0);
	}
}

public class FaceTemplateSelector : DataTemplateSelector
{
	public event EventHandler<string>? TemplateChanged;

	public DataTemplate? StandardFaceTemplate { get; set; }
	public DataTemplate? HrothgarFaceTemplate { get; set; }
	public DataTemplate? MiqoteFaceTemplate { get; set; }
	public DataTemplate? VieraFloppyFaceTemplate { get; set; }
	public DataTemplate? VieraStraightFaceTemplate { get; set; }

	public override DataTemplate? SelectTemplate(object item, DependencyObject container)
	{
		if (item is not SkeletonEntity skeleton)
			return null;

		string templateName = "StandardFaceTemplate";
		if (skeleton.IsHrothgar)
		{
			templateName = "HrothgarFaceTemplate";
		}
		else if (skeleton.IsMiqote)
		{
			templateName = "MiqoteFaceTemplate";
		}
		else if (skeleton.IsViera)
		{
			if (skeleton.IsVieraEarsFlop)
				templateName = "VieraFloppyFaceTemplate";
			else
				templateName = "VieraStraightFaceTemplate";
		}

		this.TemplateChanged?.Invoke(this, templateName);

		Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () => BoneViewManager.Instance.Refresh());

		return templateName switch
		{
			"HrothgarFaceTemplate" => this.HrothgarFaceTemplate,
			"MiqoteFaceTemplate" => this.MiqoteFaceTemplate,
			"VieraFloppyFaceTemplate" => this.VieraFloppyFaceTemplate,
			"VieraStraightFaceTemplate" => this.VieraStraightFaceTemplate,
			_ => this.StandardFaceTemplate,
		};
	}
}
