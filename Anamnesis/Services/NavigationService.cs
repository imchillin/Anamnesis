// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using System;

public class NavigationService : ServiceBase<NavigationService>
{
	public static void Navigate(Uri uri)
	{
		try
		{
			throw new NotImplementedException();
		}
		catch (Exception ex)
		{
			Log.Error(ex, $"Failed to handle navigation request to Uri: {uri}");
		}
	}
}
