// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Runtime.InteropServices;
	using Anamnesis.Core.Memory;

	public enum RenderModes : int
	{
		Draw = 0,
		Unload = 2,
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct Actor
	{
		public const int ObjectKindOffset = 0x008c;
		public const int RenderModeOffset = 0x0104;

		[FieldOffset(0x0030)]
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
		public byte[] NameBytes;

		[FieldOffset(0x0080)] public int DataId;
		[FieldOffset(ObjectKindOffset)] public ActorTypes ObjectKind;
		[FieldOffset(0x008D)] public byte SubKind;
		[FieldOffset(0x0090)] public byte DistanceFromPlayerX;
		[FieldOffset(0x0092)] public byte DistanceFromPlayerY;
		[FieldOffset(0x00F0)] public IntPtr ModelObject;
		[FieldOffset(RenderModeOffset)] public RenderModes RenderMode;
		[FieldOffset(0x01B4)] public int ModelType;
		[FieldOffset(0x01E2)] public byte ClassJob;
		[FieldOffset(0x07C4)] [MarshalAs(UnmanagedType.I1)]public bool IsAnimating;
		[FieldOffset(0x0F08)] public Weapon MainHand;
		[FieldOffset(0x0F70)] public Weapon OffHand;
		[FieldOffset(0x1040)] public Equipment Equipment;
		[FieldOffset(0x182C)] public float Transparency;
		[FieldOffset(0x1898)] public Customize Customize;

		public string Id => this.Name + this.DataId;

		public string Name
		{
			get => SeString.FromSeStringBytes(this.NameBytes);
			set => this.NameBytes = SeString.ToSeStringBytes(value);
		}
	}
}
