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
		{ "WeatherSelector", typeof(WeatherSelectorPanel) },
	};

	/// <summary>
	/// Navigate to a panel, and recieve a callback of type T from the panel.
	/// </summary>
	public static void Navigate<T>(Request request, Action<T?> resultCallback)
	{
		PanelBase? panel = Navigate(request);

		if (panel == null)
			return;

		panel.NavigationResultCallback = (result) =>
		{
			if (result is not T tResult)
				throw new Exception($"navigation result: {result} was not expected type: {typeof(T)}");

			resultCallback.Invoke(tResult);
		};
	}

	/// <summary>
	/// Navigate to a panel.
	/// </summary>
	public static PanelBase? Navigate(Request request)
	{
		try
		{
			Type? panelType;
			if (!Panels.TryGetValue(request.Destination, out panelType))
				throw new Exception($"No panel type found for navigation: {request.Destination}");

			IPanelGroupHost groupHost = PanelService.CreateHost();
			PanelBase? panel = Activator.CreateInstance(panelType, groupHost) as PanelBase;

			if (panel == null)
				throw new Exception($"Failed to create instance of panel: {panelType}");

			groupHost.PanelGroupArea.Content = panel;

			// Move the panel to the target position next to the navigation menu
			PanelBase? originPanel = request.GetOriginPanel();
			if (originPanel != null)
			{
				Rect panelRect = panel.Rect;
				Rect navRect = originPanel.Rect;
				Point pos = originPanel.GetSubPanelDockOffset();

				panelRect.X = navRect.X + pos.X + 6;
				panelRect.Y = navRect.Y + pos.Y + 3;

				panel.Rect = panelRect;
				panel.SetParent(originPanel);
			}

			groupHost.Show();

			return panel;
		}
		catch (Exception ex)
		{
			Log.Error(ex, $"Failed to handle navigation request to Uri: \"{request}\"");
			return null;
		}
	}

	public struct Request
	{
		public object? Origin;
		public string Destination;
		public object? Context;

		public Request(object origin, string destination, object? context = null)
		{
			this.Origin = origin;
			this.Destination = destination;
			this.Context = context;
		}

		public Request(string destination, object? context = null)
		{
			this.Origin = null;
			this.Destination = destination;
			this.Context = context;
		}

		/// <summary>
		/// Navigate to a panel, and recieve a callback of type T from the panel.
		/// </summary>
		public void Navigate<T>(Action<T?> resultCallback) => NavigationService.Navigate<T>(this, resultCallback);

		/// <summary>
		/// Navigate to a panel.
		/// </summary>
		public PanelBase? Navigate() => NavigationService.Navigate(this);

		public PanelBase? GetOriginPanel()
		{
			if (this.Origin is PanelBase panel)
				return panel;

			if (this.Origin is FrameworkElement fe)
				return fe.FindParent<PanelBase>();

			return null;
		}

		public readonly override string? ToString()
		{
			if (this.Context != null)
				return $"{this.Destination} - {this.Context}";

			return this.Destination;
		}
	}
}
