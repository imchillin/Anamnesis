// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using FontAwesome.Sharp;
using System.Windows.Controls;

public interface IPanelGroupHost
{
	ContentPresenter PanelGroupArea { get; }
	string Title { get; set; }
	IconChar Icon { get; set; }
	void Show();
	void DragMove();
}
