// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using FontAwesome.Sharp;
using System.Windows;
using System.Windows.Controls;

public abstract class PanelBase : UserControl, IPanel
{
	public PanelBase(IPanelGroupHost host)
	{
		this.Host = host;
	}

	public IPanelGroupHost Host { get; init; }

	public string Title
	{
		get => this.Host.Title;
		set => this.Host.Title = value;
	}

	public IconChar Icon
	{
		get => this.Host.Icon;
		set => this.Host.Icon = value;
	}

	public Rect Rect
	{
		get => this.Host.Rect;
		set => this.Host.Rect = value;
	}

	public bool ShowBackground
	{
		get => this.Host.ShowBackground;
		set => this.Host.ShowBackground = value;
	}

	public bool AllowAutoClose
	{
		get => this.Host.AllowAutoClose;
		set => this.Host.AllowAutoClose = value;
	}

	public void DragMove() => this.Host.DragMove();
}
