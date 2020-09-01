// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Runtime.InteropServices;
	using PropertyChanged;

	public enum RenderModes : int
	{
		Draw = 0,
		Unload = 2,
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct Actor
	{
		[FieldOffset(0x0030)]
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
		public string Name;

		[FieldOffset(0x0074)]
		public int ActorId;

		[FieldOffset(0x0080)]
		public int DataId;

		[FieldOffset(0x0084)]
		public int OwnerId;

		[FieldOffset(0x008c)]
		public ActorTypes ObjectKind;

		[FieldOffset(0x008D)]
		public byte SubKind;

		[FieldOffset(0x008E)]
		public bool IsFriendly;

		// This is some kind of enum
		[FieldOffset(0x0091)]
		public byte PlayerTargetStatus;

		[FieldOffset(0x00A0)]
		public Vector Position;

		[FieldOffset(0x00B0)]
		public float Rotation;

		[FieldOffset(0x00F0)]
		public IntPtr Transform;

		[FieldOffset(0x0104)]
		public RenderModes RenderMode;

		[FieldOffset(0x01F0)]
		public int PlayerCharacterTargetActorId;

		[FieldOffset(0x17B8)]
		public Appearance Customize;

		[FieldOffset(0x17F8)]
		public int BattleNpcTargetActorId;

		[FieldOffset(0x1868)]
		public int NameId;

		[FieldOffset(0x1888)]
		public int ModelType;

		public static bool IsSame(Actor? lhs, Actor? rhs)
		{
			return lhs?.Name == rhs?.Name && lhs?.ActorId == rhs?.ActorId && lhs?.NameId == rhs?.NameId;
		}
	}

	[AddINotifyPropertyChangedInterface]
	public class ActorViewModel : MemoryViewModelBase<Actor>
	{
		public ActorViewModel(IntPtr pointer)
			: base(pointer)
		{
		}

		public string Name { get; set; } = string.Empty;
		public int ActorId { get; set; }
		public int DataId { get; set; }
		public int OwnerId { get; set; }
		public ActorTypes ObjectKind { get; set; }
		public byte SubKind { get; set; }
		public bool IsFriendly { get; set; }
		public byte PlayerTargetStatus { get; set; }
		public Vector Position { get; set; }
		public float Rotation { get; set; }
		public AppearanceViewModel? Customize { get; set; }
		public int PlayerCharacterTargetActorId { get; set; }
		public int BattleNpcTargetActorId { get; set; }
		public int NameId { get; set; }
		public int ModelType { get; set; }
		public RenderModes RenderMode { get; set; }

		[ViewModelOffset(0x50)]
		public TransformViewModel? Transform { get; set; }
	}
}
