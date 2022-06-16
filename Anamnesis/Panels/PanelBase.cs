// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using System.Windows.Controls;

public abstract class PanelBase : UserControl
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

	protected void DragMove() => this.Host.DragMove();
}
