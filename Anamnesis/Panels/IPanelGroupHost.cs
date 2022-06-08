// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using System.Windows.Controls;

public interface IPanelGroupHost
{
	ContentPresenter PanelGroupArea { get; }
	void Show();
	void DragMove();
}
