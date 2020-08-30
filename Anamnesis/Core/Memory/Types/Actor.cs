// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Explicit)]
	public struct Actor
	{
		[FieldOffset(0x30)]
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
		public string Name;

		[FieldOffset(116)]
		public int ActorId;

		[FieldOffset(128)]
		public int DataId;

		[FieldOffset(132)]
		public int OwnerId;

		[FieldOffset(140)]
		public ActorTypes ObjectKind;

		[FieldOffset(141)]
		public byte SubKind;

		[FieldOffset(142)]
		public bool IsFriendly;

		// This is some kind of enum
		[FieldOffset(145)]
		public byte PlayerTargetStatus;

		[FieldOffset(160)]
		public Vector Position;

		[FieldOffset(176)]
		public float Rotation;

		[FieldOffset(0x17B8)]
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
		public byte[] Customize;

		[FieldOffset(0x1F0)]
		public int PlayerCharacterTargetActorId;

		[FieldOffset(0x17F8)]
		public int BattleNpcTargetActorId;

		[FieldOffset(0x1868)]
		public int NameId;

		public static bool IsSame(Actor? lhs, Actor? rhs)
		{
			return lhs?.Name == rhs?.Name && lhs?.ActorId == rhs?.ActorId && lhs?.NameId == rhs?.NameId;
		}
	}
}
