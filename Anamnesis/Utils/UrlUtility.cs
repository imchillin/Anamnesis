// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;
	using System.Diagnostics;

	public static class UrlUtility
	{
		public static void Open(string url)
		{
			try
			{
				Process.Start(new ProcessStartInfo(@"cmd", $"/c start {url}") { CreateNoWindow = true });
			}
			catch (Exception ex)
			{
				Log.Write(ex, "URL", Log.Severity.Error);
			}
		}
	}
}
