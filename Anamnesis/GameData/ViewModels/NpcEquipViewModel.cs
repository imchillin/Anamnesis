// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.ViewModels
{
	using Anamnesis.Character.Utilities;
	using Anamnesis.Services;
	using Lumina;
	using Lumina.Excel.GeneratedSheets;

	public class NpcEquipViewModel : INpcEquip
	{
		public readonly NpcEquip Value;

		public NpcEquipViewModel(NpcEquip value)
		{
			this.Value = value;
		}

		public int Key => 0;
		public virtual IItem MainHand => LuminaExtensions.GetWeaponItem(ItemSlots.MainHand, this.Value.ModelMainHand);
		public virtual IDye DyeMainHand => this.GetDye(this.Value.DyeMainHand.Value?.RowId);
		public virtual IItem OffHand => LuminaExtensions.GetWeaponItem(ItemSlots.OffHand, this.Value.ModelOffHand);
		public virtual IDye DyeOffHand => this.GetDye(this.Value.DyeOffHand.Value?.RowId);
		public virtual IItem Head => this.GetItem(ItemSlots.Head, this.Value.ModelHead, this.Value?.ModelHead);
		public virtual IDye DyeHead => this.GetDye(this.Value.DyeHead.Value?.RowId);
		public virtual IItem Body => this.GetItem(ItemSlots.Body, this.Value.ModelBody, this.Value?.ModelBody);
		public virtual IDye DyeBody => this.GetDye(this.Value.DyeBody.Value?.RowId);
		public virtual IItem Legs => this.GetItem(ItemSlots.Legs, this.Value.ModelLegs, this.Value?.ModelLegs);
		public virtual IDye DyeLegs => this.GetDye(this.Value.DyeLegs.Value?.RowId);
		public virtual IItem Feet => this.GetItem(ItemSlots.Feet, this.Value.ModelFeet, this.Value?.ModelFeet);
		public virtual IDye DyeFeet => this.GetDye(this.Value.DyeFeet.Value?.RowId);
		public virtual IItem Hands => this.GetItem(ItemSlots.Hands, this.Value.ModelHands, this.Value?.ModelHands);
		public virtual IDye DyeHands => this.GetDye(this.Value.DyeHands.Value?.RowId);
		public virtual IItem Wrists => this.GetItem(ItemSlots.Wrists, this.Value.ModelWrists, this.Value?.ModelWrists);
		public virtual IDye DyeWrists => this.GetDye(this.Value.DyeWrists.Value?.RowId);
		public virtual IItem Neck => this.GetItem(ItemSlots.Neck, this.Value.ModelNeck, this.Value?.ModelNeck);
		public virtual IDye DyeNeck => this.GetDye(this.Value.DyeNeck.Value?.RowId);
		public virtual IItem Ears => this.GetItem(ItemSlots.Ears, this.Value.ModelEars, this.Value?.ModelEars);
		public virtual IDye DyeEars => this.GetDye(this.Value.DyeEars.Value?.RowId);
		public virtual IItem LeftRing => this.GetItem(ItemSlots.LeftRing, this.Value.ModelLeftRing, this.Value?.ModelLeftRing);
		public virtual IDye DyeLeftRing => this.GetDye(this.Value.DyeLeftRing.Value?.RowId);
		public virtual IItem RightRing => this.GetItem(ItemSlots.RightRing, this.Value.ModelRightRing, this.Value?.ModelRightRing);
		public virtual IDye DyeRightRing => this.GetDye(this.Value.DyeRightRing.Value?.RowId);

		protected IItem GetItem(ItemSlots slot, uint baseVal, uint? equipVal)
		{
			if (equipVal != null && equipVal != 0 && equipVal != uint.MaxValue && equipVal != long.MaxValue)
				return LuminaExtensions.GetGearItem(slot, (uint)equipVal);

			return LuminaExtensions.GetGearItem(slot, baseVal);
		}

		protected IDye GetDye(uint? id)
		{
			if (GameDataService.Dyes == null)
				return ItemUtility.NoneDye;

			if (id == null)
				return ItemUtility.NoneDye;

			return GameDataService.Dyes.Get((uint)id);
		}
	}
}
