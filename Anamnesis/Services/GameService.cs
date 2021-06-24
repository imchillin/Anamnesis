// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
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
			try
			{
				if (TerritoryService.Instance.CurrentTerritory == null)
					return false;

				IntPtr startAddress;
				if (GposeService.Instance.IsGpose)
				{
					startAddress = AddressService.GPoseActorTable + 8;
				}
				else
				{
					startAddress = AddressService.ActorTable;
				}

				IntPtr ptr = MemoryService.ReadPtr(AddressService.ActorTable);

				if (ptr == IntPtr.Zero)
					return false;

				Actor actor = MemoryService.Read<Actor>(ptr);
				if (string.IsNullOrEmpty(actor.Name))
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

				if (!this.IsSignedIn)
				{
					TargetService.Instance.ClearSelection();
				}
				else
				{
					await Task.Delay(1000);
					TargetService.Instance.EnsureSelection();
				}

				await Task.Delay(16);
			}
		}
	}
}
