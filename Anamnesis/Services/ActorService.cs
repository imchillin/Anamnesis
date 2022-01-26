// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Threading.Tasks;
	using Anamnesis.Core.Memory;
	using Anamnesis.Memory;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class ActorService : ServiceBase<ActorService>
	{
		private const int TickDelay = 10;
		private const int ActorTableSize = 424;

		private readonly IntPtr[] actorTable = new IntPtr[ActorTableSize];

		public ReadOnlyCollection<IntPtr> ActorTable => Array.AsReadOnly(this.actorTable);

		public int GetActorTableIndex(IntPtr pointer, bool refresh = false)
		{
			if (pointer == IntPtr.Zero)
				return -1;

			if (refresh)
				this.UpdateActorTable();

			return Array.IndexOf(this.actorTable, pointer);
		}

		public bool IsActorInTable(IntPtr ptr, bool refresh = false)
		{
			return this.GetActorTableIndex(ptr, refresh) != -1;
		}

		public bool IsActorInTable(MemoryBase memory, bool refresh = false) => this.IsActorInTable(memory.Address, refresh);

		public List<ActorBasicMemory> GetAllActors(bool refresh = false)
		{
			if (refresh)
				this.UpdateActorTable();

			List<ActorBasicMemory> results = new();

			foreach(var ptr in this.actorTable)
			{
				if (ptr == IntPtr.Zero)
					continue;

				try
				{
					ActorBasicMemory actor = new();
					actor.SetAddress(ptr);
					results.Add(actor);
				}
				catch (Exception ex)
				{
					Log.Warning(ex, $"Failed to create Actor Basic View Model for address: {ptr}");
				}
			}

			return results;
		}

		public void ForceRefresh()
		{
			this.UpdateActorTable();
		}

		public override async Task Initialize()
		{
			await base.Initialize();
		}

		public override Task Start()
		{
			this.UpdateActorTable();

			_ = Task.Run(this.TickTask);
			return base.Start();
		}

		public override async Task Shutdown()
		{
			await base.Shutdown();
		}

		private async Task TickTask()
		{
			while (this.IsAlive)
			{
				await Task.Delay(TickDelay);

				this.ForceRefresh();
			}
		}

		private void UpdateActorTable()
		{
			lock(this.actorTable)
			{
				for (int i = 0; i < ActorTableSize; i++)
				{
					IntPtr ptr = MemoryService.ReadPtr(AddressService.ActorTable + (i * 8));
					this.actorTable[i] = ptr;
				}
			}

			this.RaisePropertyChanged(nameof(this.ActorTable));
		}
	}
}
