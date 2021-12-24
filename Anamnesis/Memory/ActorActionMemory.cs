// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;

	public class ActorActionMemory : MemoryBase
	{
		public static readonly int ActionMemoryLength = 0xA0;
		public static readonly int EntriesInActionTable = 0x40;

		public enum ActionTypes : byte
		{
			Emote = 0x1,
			Action = 0x2,
		}

		[Bind(0x00)] public IntPtr ActorPtr { get; set; }
		[Bind(0x08)] public uint ActorObjectId { get; set; }
		[Bind(0x10)] public ActionTypes ActionType { get; set; }
		[Bind(0x14)] public ushort ActionId { get; set; }
	}
}
