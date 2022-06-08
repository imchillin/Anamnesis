// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using Anamnesis.Windows;
using System.Windows.Controls;

public class PanelService
{
	public static UserControl Show<T>()
		where T : PanelBase, new()
	{
		IPanelGroupHost groupHost = CreateHost();

		PanelBase panel = new T();
		panel.Host = groupHost;
		groupHost.PanelGroupArea.Content = panel;
		groupHost.Show();

		return panel;
	}

	private static IPanelGroupHost CreateHost()
	{
		// TODO: if OverlayMode!
		return new OverlayWindow();
	}
}
