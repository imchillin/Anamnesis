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
		private IEnumerable<ActorActionMemory>? actionTable = null;

		public override async Task Start()
		{
			await base.Start();
		}

		public bool SetAction(ActorBasicMemory actor, ActorActionMemory.ActionTypes actionType, uint actionId)
		{
			ActorActionMemory? targetAction = null;

			// Make sure we have the latest version
			this.RefreshTable();

			// First search for an existing entry in both overworld and gpose
			targetAction = this.GetAction(actor);

			// If no match and not in gpose we search for a blank entry (you can't add a new entry during gpose as we don't have a handle to the overworld actor anymore)
			if (targetAction == null && !GposeService.Instance.IsGpose)
			{
				foreach (var entry in this.actionTable!)
				{
					if (entry.ActorPtr == IntPtr.Zero && entry.ActorObjectId == 0xE0000000)
					{
						targetAction = entry;
						targetAction.ActorPtr = actor.Address;
						targetAction.ActorObjectId = actor.ObjectId;
						break;
					}
				}
			}

			// If there is no space maybe we should write a random one?
			if (targetAction == null)
				return false;

			// Update table entry
			targetAction.ActionType = actionType;
			targetAction.ActionId = actionId;

			if(actionType == ActorActionMemory.ActionTypes.Action)
			{
				targetAction.SubActionType = 1;
				targetAction.SubActionId = actionId;
			}

			return true;
		}

		public ActorActionMemory? GetAction(ActorBasicMemory actor, bool searchOverworld = true, bool searchGPose = true, bool refresh = false)
		{
			if (refresh || this.actionTable == null)
				this.RefreshTable();

			foreach (var entry in this.actionTable!)
			{
				if ((searchOverworld && (entry.ActorPtr == actor.Address && entry.ActorObjectId == actor.ObjectId))
					|| (searchGPose && (entry.GPoseActorPtr == actor.Address)))
				{
					return entry;
				}
			}

			return null;
		}

		public IEnumerable<ActorActionMemory> GetTable(bool refresh = false)
		{
			if (refresh || this.actionTable == null)
				this.RefreshTable();

			return this.actionTable!;
		}

		public void RefreshTable()
		{
			if (this.actionTable == null)
			{
				this.actionTable = this.ReadTable();
				return;
			}

			foreach (var action in this.actionTable)
			{
				action.Tick();
			}
		}

		private IEnumerable<ActorActionMemory> ReadTable()
		{
			IntPtr tableStart = MemoryService.ReadPtr(AddressService.ActorActionTable) + 0x5B0;

			List<ActorActionMemory> result = new();
			for (int i = 0; i < ActorActionMemory.EntriesInActionTable; ++i)
			{
				IntPtr address = tableStart + (i * ActorActionMemory.ActionMemoryLength);
				ActorActionMemory actorActionMemory = new();
				actorActionMemory.SetAddress(address);

				result.Add(actorActionMemory);
			}

			return result;
		}
	}
}
