// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using System.Windows.Controls;

public abstract class PanelBase : UserControl
{
	public IPanelGroupHost? Host { get; set; }

	protected void DragMove() => this.Host?.DragMove();
}
