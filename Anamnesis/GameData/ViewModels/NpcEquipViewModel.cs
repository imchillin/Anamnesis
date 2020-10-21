// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GameData.ViewModels
{
	using Anamnesis.Services;
	using Lumina;
	using Lumina.Excel.GeneratedSheets;

	public class NpcEquipViewModel : INpcEquip
	{
		public readonly ENpcBase Value;

		public NpcEquipViewModel(ENpcBase value)
		{
			this.Value = value;
		}

		public int Key => 0;
		public IItem MainHand => LuminaExtensions.GetItem(ItemSlots.MainHand, this.Value.ModelMainHand);
		public IDye DyeMainHand => GameDataService.Dyes!.Get((int)this.Value.DyeMainHand.Value.RowId);
		public IItem OffHand => LuminaExtensions.GetItem(ItemSlots.OffHand, this.Value.ModelOffHand);
		public IDye DyeOffHand => GameDataService.Dyes!.Get((int)this.Value.DyeOffHand.Value.RowId);
		public IItem Head => LuminaExtensions.GetItem(ItemSlots.Head, this.Value.ModelHead);
		public IDye DyeHead => GameDataService.Dyes!.Get((int)this.Value.DyeHead.Value.RowId);
		public IItem Body => LuminaExtensions.GetItem(ItemSlots.Body, this.Value.ModelBody);
		public IDye DyeBody => GameDataService.Dyes!.Get((int)this.Value.DyeBody.Value.RowId);
		public IItem Legs => LuminaExtensions.GetItem(ItemSlots.Legs, this.Value.ModelLegs);
		public IDye DyeLegs => GameDataService.Dyes!.Get((int)this.Value.DyeLegs.Value.RowId);
		public IItem Feet => LuminaExtensions.GetItem(ItemSlots.Feet, this.Value.ModelFeet);
		public IDye DyeFeet => GameDataService.Dyes!.Get((int)this.Value.DyeFeet.Value.RowId);
		public IItem Hands => LuminaExtensions.GetItem(ItemSlots.Hands, this.Value.ModelHands);
		public IDye DyeHands => GameDataService.Dyes!.Get((int)this.Value.DyeHands.Value.RowId);
		public IItem Wrists => LuminaExtensions.GetItem(ItemSlots.Wrists, this.Value.ModelWrists);
		public IDye DyeWrists => GameDataService.Dyes!.Get((int)this.Value.DyeWrists.Value.RowId);
		public IItem Neck => LuminaExtensions.GetItem(ItemSlots.Neck, this.Value.ModelNeck);
		public IDye DyeNeck => GameDataService.Dyes!.Get((int)this.Value.DyeNeck.Value.RowId);
		public IItem Ears => LuminaExtensions.GetItem(ItemSlots.Ears, this.Value.ModelEars);
		public IDye DyeEars => GameDataService.Dyes!.Get((int)this.Value.DyeEars.Value.RowId);
		public IItem LeftRing => LuminaExtensions.GetItem(ItemSlots.LeftRing, this.Value.ModelLeftRing);
		public IDye DyeLeftRing => GameDataService.Dyes!.Get((int)this.Value.DyeLeftRing.Value.RowId);
		public IItem RightRing => LuminaExtensions.GetItem(ItemSlots.RightRing, this.Value.ModelRightRing);
		public IDye DyeRightRing => GameDataService.Dyes!.Get((int)this.Value.DyeRightRing.Value.RowId);
	}
}
