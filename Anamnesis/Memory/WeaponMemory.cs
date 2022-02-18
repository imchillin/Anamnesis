// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using PropertyChanged;

	public class WeaponMemory : MemoryBase, IEquipmentItemMemory
	{
		[Flags]
		public enum WeaponFlagDefs : byte
		{
			WeaponHidden = 1 << 1,
		}

		[Bind(0x000, BindFlags.ActorRefresh)] public ushort Set { get; set; }
		[Bind(0x002, BindFlags.ActorRefresh)] public ushort Base { get; set; }
		[Bind(0x004, BindFlags.ActorRefresh)] public ushort Variant { get; set; }
		[Bind(0x006, BindFlags.ActorRefresh)] public byte Dye { get; set; }
		[Bind(0x008, BindFlags.Pointer)] public WeaponModelMemory? Model { get; set; }
		[Bind(0x05C)] public WeaponFlagDefs WeaponFlags { get; set; }

		[DependsOn(nameof(WeaponFlags))]
		public bool WeaponHidden
		{
			get => this.WeaponFlags.HasFlag(WeaponFlagDefs.WeaponHidden);
			set
			{
				if (value)
				{
					this.WeaponFlags |= WeaponFlagDefs.WeaponHidden;
				}
				else
				{
					this.WeaponFlags &= ~WeaponFlagDefs.WeaponHidden;
				}
			}
		}
	}
}
