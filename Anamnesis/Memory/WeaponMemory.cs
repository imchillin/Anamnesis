// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using Anamnesis.Character.Utilities;
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
		[Bind(0x040)] public bool IsSheathed { get; set; }
		[Bind(0x05C)] public WeaponFlagDefs WeaponFlags { get; set; }

		[DependsOn(nameof(WeaponFlags), nameof(IsSheathed))]
		public bool WeaponHidden
		{
			get => (this.IsSheathed && this.WeaponFlags.HasFlag(WeaponFlagDefs.WeaponHidden)) || (!this.IsSheathed && this.Model?.Transform?.Scale == Vector.Zero);
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

				if (this.Model?.Transform == null)
					return;

				// If the weapon is unsheathed (in hands) the visibility flag won't work,
				// so fall back to setting the weapons scale to 0.
				if (!this.IsSheathed)
				{
					this.Model.Transform.Scale = value ? Vector.Zero : Vector.One;
				}

				// Special handling for a weapon with 0 scale that has been sheathed attempting to un-hide
				else if (!value && this.Model.Transform.Scale == Vector.Zero)
				{
					this.Model.Transform.Scale = Vector.One;
				}
			}
		}

		public void Clear(bool isPlayer)
		{
			// Clear the weapon to the Emperor's New Fists
			// Since having actually no weapon is bad.
			this.Set = ItemUtility.EmperorsNewFists.ModelSet;
			this.Base = ItemUtility.EmperorsNewFists.ModelBase;
			this.Variant = ItemUtility.EmperorsNewFists.ModelVariant;
			this.Dye = 0;
		}
	}
}
