// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Runtime.InteropServices;
	using PropertyChanged;

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
		////[MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
		public Appearance Customize;

		[FieldOffset(0x1F0)]
		public int PlayerCharacterTargetActorId;

		[FieldOffset(0x17F8)]
		public int BattleNpcTargetActorId;

		[FieldOffset(0x1868)]
		public int NameId;

		[FieldOffset(0x0F0)]
		public IntPtr TransformPointer;

		public static bool IsSame(Actor? lhs, Actor? rhs)
		{
			return lhs?.Name == rhs?.Name && lhs?.ActorId == rhs?.ActorId && lhs?.NameId == rhs?.NameId;
		}
	}

	[AddINotifyPropertyChangedInterface]
	public class ActorViewModel : MemoryViewModelBase<Actor>
	{
		private IntPtr transformPointer;

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
		public Appearance Customize { get; set; }
		public int PlayerCharacterTargetActorId { get; set; }
		public int BattleNpcTargetActorId { get; set; }
		public int NameId { get; set; }

		public TransformViewModel? Transform { get; set; }

		public IntPtr TransformPointer
		{
			get
			{
				return this.transformPointer;
			}

			set
			{
				this.transformPointer = value;

				IntPtr transformPointer = value + 0x50;
				this.Transform = new TransformViewModel(transformPointer);
			}
		}
	}
}
