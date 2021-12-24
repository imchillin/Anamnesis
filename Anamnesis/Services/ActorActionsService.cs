// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Anamnesis.Core.Memory;
    using Anamnesis.Memory;

    public class ActorActionsService : ServiceBase<ActorActionsService>
	{
		public override Task Start()
		{
			return base.Start();
		}

		public IEnumerable<ActorActionMemory> GetTable()
		{
			IntPtr tableStart = MemoryService.ReadPtr(AddressService.ActorActionTable) + 0x5B0;

			List<ActorActionMemory> result = new();
			for(int i = 0; i < ActorActionMemory.EntriesInActionTable; ++i)
			{
				IntPtr address = tableStart + (i * ActorActionMemory.ActionMemoryLength);
				ActorActionMemory actorActionMemory = new();
				actorActionMemory.SetAddress(address);

				result.Add(actorActionMemory);
			}

			return result;
		}

		public void SetActorAction(ActorBasicMemory actor, ActorActionMemory.ActionTypes actionType, ushort actionId)
		{
			ActorActionMemory? toSet = null;
			var table = this.GetTable();

			// First search for an existing entry
			foreach (var entry in table)
			{
				if (entry.ActorPtr == actor.Address && entry.ActorObjectId == actor.ObjectId)
				{
					toSet = entry;
					break;
				}
			}

			if (toSet == null)
			{
				// Next we search for an empty entry
				foreach (var entry in table)
				{
					if (entry.ActorPtr == IntPtr.Zero && entry.ActorObjectId == 0xE0000000)
					{
						toSet = entry;
						break;
					}
				}
			}

			// If there is no space maybe we should write a random one?
			if (toSet == null)
				return;

			toSet.ActorPtr = actor.Address;
			toSet.ActorObjectId = actor.ObjectId;
			toSet.ActionType = actionType;
			toSet.ActionId = actionId;
		}
	}
}
