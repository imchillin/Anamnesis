// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;
	using System.Diagnostics;
	using Serilog;

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
	}
}
