// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using System;
using System.Threading.Tasks;
using Anamnesis.Core.Memory;
using Anamnesis.Memory;
using PropertyChanged;

[AddINotifyPropertyChangedInterface]
public class GameService : ServiceBase<GameService>
{
	public static bool Ready => Exists && Instance.IsSignedIn;

	public bool IsSignedIn { get; private set; }

	public static bool GetIsSignedIn()
	{
#if DEBUG
		if (MemoryService.Process == null)
			return true;
#endif

		try
		{
			if (GameDataService.Territories == null)
				return false;

			int territoryID = MemoryService.Read<int>(AddressService.Territory);

			if (territoryID == -1)
				return false;

			if (GameDataService.Territories.GetOrDefault((uint)territoryID) == null)
				return false;

			return true;
		}
		catch (Exception ex)
		{
			Log.Warning(ex, "Failed to safely check for sign in");
			return false;
		}
	}

	public override Task Initialize()
	{
		Task.Run(this.CheckSignedIn);
		return base.Initialize();
	}

	public async Task CheckSignedIn()
	{
		while (this.IsAlive)
		{
			this.IsSignedIn = GetIsSignedIn();

			/*if (!this.IsSignedIn)
			{
				TargetService.Instance.ClearSelection();
			}*/

			/*else
			{
				await Task.Delay(1000);
				TargetService.Instance.EnsureSelection();
			}*/

			await Task.Delay(16);
		}
	}
}
