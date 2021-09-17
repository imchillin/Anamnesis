// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Explicit)]
	public struct Model
	{
		[FieldOffset(0x030)] public IntPtr Weapons;
		[FieldOffset(0x050)] public Transform Transform;
		[FieldOffset(0x0A0)] public IntPtr Skeleton;
		[FieldOffset(0x148)] public IntPtr Bust;
		[FieldOffset(0x240)] public IntPtr ExtendedAppearance;
		[FieldOffset(0x26C)] public float Height;
		[FieldOffset(0x2B0)] public float Wetness;
		[FieldOffset(0x2BC)] public float Drenched;
		[FieldOffset(0x938)] public short DataPath;
		[FieldOffset(0x93C)] public byte DataHead;

		/// <summary>
		/// Known data paths.
		/// </summary>
		public enum DataPaths : short
		{
			MidlanderMasculine = 101,
			MidlanderMasculineChild = 104,
			MidlanderFeminine = 201,
			MidlanderFeminineChild = 204,
			HighlanderMasculine = 301,
			HighlanderFeminine = 401,
			ElezenMasculine = 501,
			ElezenMasculineChild = 504,
			ElezenFeminine = 601,
			ElezenFeminineChild = 604,
			MiqoteMasculine = 701,
			MiqoteMasculineChild = 704,
			MiqoteFeminine = 801,
			MiqoteFeminineChild = 804,
			RoegadynMasculine = 901,
			RoegadynFeminine = 1001,
			LalafellMasculine = 1101,
			LalafellFeminine = 1201,
			AuRaMasculine = 1301,
			AuRaFeminine = 1401,
			Hrothgar = 1501,
			Viera = 1801,
			PadjalMasculine = 9104,
			PadjalFeminine = 9204,
		}
	}
}
