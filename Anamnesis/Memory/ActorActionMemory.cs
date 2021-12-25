// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;

	public class ActorActionMemory : MemoryBase
	{
		public static readonly int ActionMemoryLength = 0xA0;
		public static readonly int EntriesInActionTable = 0x40;

		public enum ActionTypes : uint
		{
			None = 0x0,
			Emote = 0x1,
			Action = 0x2,
		}

		[Bind(0x00)] public IntPtr ActorPtr { get; set; }
		[Bind(0x08)] public uint ActorObjectId { get; set; }
		[Bind(0x10)] public ActionTypes ActionType { get; set; }
		[Bind(0x14)] public uint ActionId { get; set; }
		[Bind(0x40)] public uint SubActionType { get; set; }
		[Bind(0x44)] public uint SubActionId { get; set; }
		[Bind(0x70)] public IntPtr GPoseActorPtr { get; set; }
	}
}
