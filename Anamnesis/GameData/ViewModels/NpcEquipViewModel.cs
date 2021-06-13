// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.GameData.ViewModels
{
	using Anamnesis.Character.Utilities;
	using Anamnesis.Services;
	using Lumina;
	using Lumina.Excel.GeneratedSheets;

	public class NpcEquipViewModel : INpcEquip
	{
		public readonly ENpcBase Value;
		public readonly NpcEquip? Equip;

		public NpcEquipViewModel(ENpcBase value)
		{
			this.Value = value;

			this.Equip = this.Value.NpcEquip.Value;
		}

		public int Key => 0;
		public IItem MainHand => LuminaExtensions.GetWeaponItem(ItemSlots.MainHand, this.Value.ModelMainHand);
		public IDye DyeMainHand => this.GetDye(this.Value.DyeMainHand.Value?.RowId);
		public IItem OffHand => LuminaExtensions.GetWeaponItem(ItemSlots.OffHand, this.Value.ModelOffHand);
		public IDye DyeOffHand => this.GetDye(this.Value.DyeOffHand.Value?.RowId);
		public IItem Head => this.GetItem(ItemSlots.Head, this.Value.ModelHead, this.Equip?.ModelHead);
		public IDye DyeHead => this.GetDye(this.Value.DyeHead.Value?.RowId);
		public IItem Body => this.GetItem(ItemSlots.Body, this.Value.ModelBody, this.Equip?.ModelBody);
		public IDye DyeBody => this.GetDye(this.Value.DyeBody.Value?.RowId);
		public IItem Legs => this.GetItem(ItemSlots.Legs, this.Value.ModelLegs, this.Equip?.ModelLegs);
		public IDye DyeLegs => this.GetDye(this.Value.DyeLegs.Value?.RowId);
		public IItem Feet => this.GetItem(ItemSlots.Feet, this.Value.ModelFeet, this.Equip?.ModelFeet);
		public IDye DyeFeet => this.GetDye(this.Value.DyeFeet.Value?.RowId);
		public IItem Hands => this.GetItem(ItemSlots.Hands, this.Value.ModelHands, this.Equip?.ModelHands);
		public IDye DyeHands => this.GetDye(this.Value.DyeHands.Value?.RowId);
		public IItem Wrists => this.GetItem(ItemSlots.Wrists, this.Value.ModelWrists, this.Equip?.ModelWrists);
		public IDye DyeWrists => this.GetDye(this.Value.DyeWrists.Value?.RowId);
		public IItem Neck => this.GetItem(ItemSlots.Neck, this.Value.ModelNeck, this.Equip?.ModelNeck);
		public IDye DyeNeck => this.GetDye(this.Value.DyeNeck.Value?.RowId);
		public IItem Ears => this.GetItem(ItemSlots.Ears, this.Value.ModelEars, this.Equip?.ModelEars);
		public IDye DyeEars => this.GetDye(this.Value.DyeEars.Value?.RowId);
		public IItem LeftRing => this.GetItem(ItemSlots.LeftRing, this.Value.ModelLeftRing, this.Equip?.ModelLeftRing);
		public IDye DyeLeftRing => this.GetDye(this.Value.DyeLeftRing.Value?.RowId);
		public IItem RightRing => this.GetItem(ItemSlots.RightRing, this.Value.ModelRightRing, this.Equip?.ModelRightRing);
		public IDye DyeRightRing => this.GetDye(this.Value.DyeRightRing.Value?.RowId);

		private IItem GetItem(ItemSlots slot, uint baseVal, uint? equipVal)
		{
			if (equipVal != null && equipVal != 0 && equipVal != uint.MaxValue && equipVal != long.MaxValue)
				return LuminaExtensions.GetGearItem(slot, (uint)equipVal);

			return LuminaExtensions.GetGearItem(slot, baseVal);
		}

		private IDye GetDye(uint? id)
		{
			if (GameDataService.Dyes == null)
				return ItemUtility.NoneDye;

			if (id == null)
				return ItemUtility.NoneDye;

			return GameDataService.Dyes.Get((uint)id);
		}
	}
}
