// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Navigation;

using Anamnesis.Panels;
using System;
using System.Collections.Generic;
using System.Windows;

public class NavigationService : ServiceBase<NavigationService>
{
	private static readonly Dictionary<string, Type> Panels = new()
	{
		{ "Weather", typeof(WeatherPanel) },
	};

	public static void Navigate(NavigationService.Request request)
	{
		try
		{
			Type? panelType;
			if (!Panels.TryGetValue(request.Destination, out panelType))
				throw new Exception($"No panel type found for navigation: {request.Destination}");

			PanelBase panel = PanelService.Show(panelType);

			if (NavigationPanel.Instance != null)
			{
				panel.Host.Align(NavigationPanel.Instance.Host, HorizontalAlignment.Right, VerticalAlignment.Top);
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex, $"Failed to handle navigation request to Uri: \"{request}\"");
		}
	}

	public struct Request
	{
		public string Destination;
		public object? Context;

		public Request(string destination, object? context = null)
		{
			this.Destination = destination;
			this.Context = context;
		}

		public readonly override string? ToString()
		{
			if (this.Context != null)
				return $"{this.Destination} - {this.Context}";

			return this.Destination;
		}
	}
}
