// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using Anamnesis.Windows;
using System;

public class PanelService
{
	public static T Show<T>()
		where T : PanelBase, new()
	{
		T? panel = Show(typeof(T)) as T;

		if (panel == null)
			throw new Exception($"Failed to create instance of panel: {typeof(T)}");

		return panel;
	}

	public static PanelBase Show(Type type)
	{
		IPanelGroupHost groupHost = CreateHost();

		PanelBase? panel = Activator.CreateInstance(type, groupHost) as PanelBase;

		if (panel == null)
			throw new Exception($"Failed to create instance of panel: {type}");

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
