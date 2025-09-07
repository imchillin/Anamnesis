// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis;

using Serilog;
using System;
using System.Diagnostics;

public static class UrlUtility
{
	public static void Open(string url)
	{
		try
		{
			Process.Start(new ProcessStartInfo(@"cmd", $"/c start \"\" \"{url}\"") { CreateNoWindow = true });
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to navigate to url");
		}
	}

	/// <summary>
	/// Checks if a path is a URL.
	/// </summary>
	/// <param name="path">Path to check.</param>
	/// <returns>true if its a URL matching UriSchemeHttp or UriSchemeHttps, false otherwise.</returns>
	public static bool IsUrl(string path)
	{
		if (Uri.TryCreate(path, UriKind.Absolute, out Uri? uriResult))
		{
			return uriResult?.Scheme == Uri.UriSchemeHttp || uriResult?.Scheme == Uri.UriSchemeHttps;
		}

		return false;
	}
}
