// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character.Items
{
	using Anamnesis.Character.Utilities;
	using Anamnesis.GameData;

	public class DummyNoneNpcEquip : INpcEquip
	{
		public virtual IItem MainHand => ItemUtility.NoneItem;
		public virtual IDye DyeMainHand => DyeUtility.NoneDye;
		public virtual IItem OffHand => ItemUtility.NoneItem;
		public virtual IDye DyeOffHand => DyeUtility.NoneDye;
		public virtual IItem Head => ItemUtility.NoneItem;
		public virtual IDye DyeHead => DyeUtility.NoneDye;
		public virtual IItem Body => ItemUtility.NoneItem;
		public virtual IDye DyeBody => DyeUtility.NoneDye;
		public virtual IItem Legs => ItemUtility.NoneItem;
		public virtual IDye DyeLegs => DyeUtility.NoneDye;
		public virtual IItem Feet => ItemUtility.NoneItem;
		public virtual IDye DyeFeet => DyeUtility.NoneDye;
		public virtual IItem Hands => ItemUtility.NoneItem;
		public virtual IDye DyeHands => DyeUtility.NoneDye;
		public virtual IItem Wrists => ItemUtility.NoneItem;
		public virtual IDye DyeWrists => DyeUtility.NoneDye;
		public virtual IItem Neck => ItemUtility.NoneItem;
		public virtual IDye DyeNeck => DyeUtility.NoneDye;
		public virtual IItem Ears => ItemUtility.NoneItem;
		public virtual IDye DyeEars => DyeUtility.NoneDye;
		public virtual IItem LeftRing => ItemUtility.NoneItem;
		public virtual IDye DyeLeftRing => DyeUtility.NoneDye;
		public virtual IItem RightRing => ItemUtility.NoneItem;
		public virtual IDye DyeRightRing => DyeUtility.NoneDye;
	}
}
