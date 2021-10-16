// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	public class ActorModelMemory : MemoryBase
	{
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

		[Bind(0x030, BindFlags.Pointer)] public ExtendedWeaponMemory? Weapons { get; set; }
		[Bind(0x050)] public TransformMemory? Transform { get; set; }
		[Bind(0x0A0, BindFlags.Pointer)] public SkeletonWrapperMemory? Skeleton { get; set; }
		[Bind(0x148, BindFlags.Pointer)] public BustMemory? Bust { get; set; }
		[Bind(0x240, BindFlags.Pointer)] public ExtendedAppearanceMemory? ExtendedAppearance { get; set; }
		[Bind(0x26C)] public float Height { get; set; }
		[Bind(0x2B0)] public float Wetness { get; set; }
		[Bind(0x2BC)] public float Drenched { get; set; }
		[Bind(0x938)] public short DataPath { get; set; }
		[Bind(0x93C)] public byte DataHead { get; set; }
	}
}