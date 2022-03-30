// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Anamnesis.Memory;

	/// <summary>
	/// The NPC Face hack requires us to turn all pinned overworld non-npc actors into NPC's just as they enter gpose, then change
	/// the original overworld actors and the new gpose actors back to their original type.
	/// This means that during the gpose loading screen, all actors should be npcs, allowing them to all load npc options.
	/// </summary>
	public class NpcFaceHackService : ServiceBase<NpcFaceHackService>
	{
		private readonly List<Actor> actors = new List<Actor>();

		public override async Task Start()
		{
			await base.Start();

			GposeService.GposeStateChanging += this.OnGposeStateChanging;
			GposeService.GposeStateChanged += this.OnGposeStateChanged;
		}

		private void OnGposeStateChanging(bool isGPose)
		{
			if (isGPose)
			{
				// When entering GPose, get every pinned actor, set them to npc.
				// record everyone we changed and what their original type was
				foreach (var pinnedActor in TargetService.Instance.PinnedActors)
				{
					if (pinnedActor.Memory == null)
						continue;

					// Only consider player actors for the npc face hack.
					if (pinnedActor.Memory.ObjectKind != ActorTypes.Player)
						continue;

					var actor = new Actor(pinnedActor);
					this.actors.Add(actor);
					actor.ChangeToNpc();
				}
			}
			else
			{
				this.actors.Clear();
			}
		}

		private void OnGposeStateChanged(bool isGPose)
		{
			// Restore the original types to all the pinned actors we changed previously.
			foreach (var actor in this.actors)
			{
				actor.RestoreFromNpc();
			}

			// Once we're fully back out of gpose, clear the tracked actors
			if (!isGPose)
			{
				this.actors.Clear();
			}
		}

		private class Actor
		{
			public readonly TargetService.PinnedActor Pinned;
			public readonly ActorBasicMemory Memory;
			public readonly ActorTypes OriginalType;
			public readonly IntPtr OriginalTypeAddress;

			public Actor(TargetService.PinnedActor pinned)
			{
				var memory = pinned.GetMemory();

				if (memory == null)
					throw new Exception("Unable to get memory from piunned actor");

				this.Pinned = pinned;
				this.Memory = memory;
				this.OriginalType = this.Memory.ObjectKind;
				this.OriginalTypeAddress = this.Memory.GetAddressOfProperty(nameof(ActorBasicMemory.ObjectKind));
			}

			public void ChangeToNpc()
			{
				MemoryService.Write(this.OriginalTypeAddress, ActorTypes.BattleNpc, "NPC face hack change");
			}

			public void RestoreFromNpc()
			{
				IntPtr newTypeAddress = this.Memory.GetAddressOfProperty(nameof(ActorBasicMemory.ObjectKind));

				MemoryService.Write(newTypeAddress, this.OriginalType, "NPC face hack restore (new)");
				MemoryService.Write(this.OriginalTypeAddress, this.OriginalType, "NPC face hack restore (original)");
			}
		}
	}
}
