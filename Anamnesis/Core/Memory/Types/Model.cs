// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Reflection;
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

	public class ModelViewModel : MemoryViewModelBase<Model>
	{
		public ModelViewModel(IntPtr pointer, IMemoryViewModel? parent)
			: base(pointer, parent)
		{
		}

		[ModelField] public WeaponExtendedViewModel? Weapons { get; set; }
		[ModelField] public TransformViewModel? Transform { get; set; }
		[ModelField] public SkeletonWrapperViewModel? Skeleton { get; set; }
		[ModelField] public BustViewModel? Bust { get; set; }
		[ModelField] public float Height { get; set; }
		[ModelField] public float Wetness { get; set; }
		[ModelField] public float Drenched { get; set; }
		[ModelField] public short DataPath { get; set; }
		[ModelField] public byte DataHead { get; set; }
		[ModelField(0x28, 0x20)] public ExtendedAppearanceViewModel? ExtendedAppearance { get; set; }

		public bool LockWetness
		{
			get => this.IsValueFrozen(nameof(ModelViewModel.Wetness));
			set => this.FreezeValue(nameof(ModelViewModel.Wetness), value);
		}

		public bool ForceDrenched
		{
			get => this.IsValueFrozen(nameof(ModelViewModel.Drenched));
			set => this.FreezeValue(nameof(ModelViewModel.Drenched), value, value ? 5 : 0);
		}

		protected override bool HandleModelToViewUpdate(PropertyInfo viewModelProperty, FieldInfo modelField)
		{
			if (viewModelProperty.Name == nameof(ModelViewModel.ExtendedAppearance))
			{
				// No extended appearance for anything that isn't a player!
				if (this.DataPath > (short)Anamnesis.Memory.Model.DataPaths.Viera)
				{
					if (this.ExtendedAppearance != null)
					{
						this.ExtendedAppearance.Dispose();
						this.ExtendedAppearance = null;
					}

					return false;
				}
			}

			return base.HandleModelToViewUpdate(viewModelProperty, modelField);
		}
	}
}
