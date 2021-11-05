// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.ViewModels
{
	using Anamnesis.Character.Utilities;
	using Anamnesis.Services;
	using Lumina;
	using Lumina.Excel.GeneratedSheets;

	public class ENpcEquipViewModel : NpcEquipViewModel
	{
		public readonly ENpcBase Npc;

		public ENpcEquipViewModel(ENpcBase value)
			: base(value.NpcEquip.Value!)
		{
			this.Npc = value;
		}

		public override IItem MainHand => LuminaExtensions.GetWeaponItem(ItemSlots.MainHand, this.Npc.ModelMainHand);
		public override IDye DyeMainHand => this.GetDye(this.Npc.DyeMainHand.Value?.RowId);
		public override IItem OffHand => LuminaExtensions.GetWeaponItem(ItemSlots.OffHand, this.Npc.ModelOffHand);
		public override IDye DyeOffHand => this.GetDye(this.Npc.DyeOffHand.Value?.RowId);
		public override IItem Head => this.GetItem(ItemSlots.Head, this.Npc.ModelHead, this.Value?.ModelHead);
		public override IDye DyeHead => this.GetDye(this.Npc.DyeHead.Value?.RowId);
		public override IItem Body => this.GetItem(ItemSlots.Body, this.Npc.ModelBody, this.Value?.ModelBody);
		public override IDye DyeBody => this.GetDye(this.Npc.DyeBody.Value?.RowId);
		public override IItem Legs => this.GetItem(ItemSlots.Legs, this.Npc.ModelLegs, this.Value?.ModelLegs);
		public override IDye DyeLegs => this.GetDye(this.Npc.DyeLegs.Value?.RowId);
		public override IItem Feet => this.GetItem(ItemSlots.Feet, this.Npc.ModelFeet, this.Value?.ModelFeet);
		public override IDye DyeFeet => this.GetDye(this.Npc.DyeFeet.Value?.RowId);
		public override IItem Hands => this.GetItem(ItemSlots.Hands, this.Npc.ModelHands, this.Value?.ModelHands);
		public override IDye DyeHands => this.GetDye(this.Npc.DyeHands.Value?.RowId);
		public override IItem Wrists => this.GetItem(ItemSlots.Wrists, this.Npc.ModelWrists, this.Value?.ModelWrists);
		public override IDye DyeWrists => this.GetDye(this.Npc.DyeWrists.Value?.RowId);
		public override IItem Neck => this.GetItem(ItemSlots.Neck, this.Npc.ModelNeck, this.Value?.ModelNeck);
		public override IDye DyeNeck => this.GetDye(this.Npc.DyeNeck.Value?.RowId);
		public override IItem Ears => this.GetItem(ItemSlots.Ears, this.Npc.ModelEars, this.Value?.ModelEars);
		public override IDye DyeEars => this.GetDye(this.Npc.DyeEars.Value?.RowId);
		public override IItem LeftRing => this.GetItem(ItemSlots.LeftRing, this.Npc.ModelLeftRing, this.Value?.ModelLeftRing);
		public override IDye DyeLeftRing => this.GetDye(this.Npc.DyeLeftRing.Value?.RowId);
		public override IItem RightRing => this.GetItem(ItemSlots.RightRing, this.Npc.ModelRightRing, this.Value?.ModelRightRing);
		public override IDye DyeRightRing => this.GetDye(this.Npc.DyeRightRing.Value?.RowId);
	}
}
