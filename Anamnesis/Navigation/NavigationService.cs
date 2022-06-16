// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Navigation;

using System;

public class NavigationService : ServiceBase<NavigationService>
{
	public static void Navigate(NavigationService.Request request)
	{
		try
		{
			throw new NotImplementedException();
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
