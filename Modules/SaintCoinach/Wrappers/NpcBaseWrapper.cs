// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.SaintCoinachModule
{
	using ConceptMatrix;
	using ConceptMatrix.GameData;
	using SaintCoinach.Xiv;

	internal class NpcBaseWrapper : ObjectWrapper<ENpcBase>, INpcBase
	{
		private INpcEquip equip;

		public NpcBaseWrapper(ENpcBase row)
			: base(row)
		{
		}

		public IRace Race => GameDataService.Instance.Races.Get(this.Value.Race.Key);
		public ITribe Tribe => GameDataService.Instance.Tribes.Get(this.Value.Tribe.Key);

		public int FacePaintColor => this.Value.FacePaintColor;
		public int FacePaint => this.Value.FacePaint;
		public int ExtraFeature2OrBust => this.Value.ExtraFeature2OrBust;
		public int ExtraFeature1 => this.Value.ExtraFeature1;
		public int ModelType => this.Value.ModelChara.Key;
		public int Gender => this.Value.Gender;
		public int BodyType => this.Value.BodyType;
		public int Height => this.Value.Height;
		public int Face => this.Value.Face;
		public int HairStyle => this.Value.HairStyle;
		public int HairHighlight => this.Value.HairHighlight;
		public int SkinColor => this.Value.SkinColor;
		public int EyeHeterochromia => this.Value.EyeHeterochromia;
		public int HairHighlightColor => this.Value.HairHighlight;
		public int FacialFeature => this.Value.FacialFeature;
		public int FacialFeatureColor => this.Value.FacialFeatureColor;
		public int Eyebrows => this.Value.Eyebrows;
		public int EyeColor => this.Value.EyeColor;
		public int EyeShape => this.Value.EyeShape;
		public int Nose => this.Value.Nose;
		public int Jaw => this.Value.Jaw;
		public int Mouth => this.Value.Mouth;
		public int LipColor => this.Value.LipColor;
		public int BustOrTone1 => this.Value.BustOrTone1;
		public int HairColor => this.Value.HairColor;

		public INpcEquip NpcEquip
		{
			get
			{
				if (this.equip == null)
					this.equip = this.GetEquip();

				return this.equip;
			}
		}

		private INpcEquip GetEquip()
		{
			NpcEquipWrapper equip = new NpcEquipWrapper();

			if (this.Value.NpcEquip.Key > 0)
			{
				equip.MainHand = NpcItemwrapper.FromWeapon(this.Value.NpcEquip.ModelMain, false);
				equip.OffHand = NpcItemwrapper.FromWeapon(this.Value.NpcEquip.ModelSub, true);
				equip.Head = NpcItemwrapper.FromGear(this.Value.NpcEquip.ModelHead);
				equip.Body = NpcItemwrapper.FromGear(this.Value.NpcEquip.ModelBody);
				equip.Hands = NpcItemwrapper.FromGear(this.Value.NpcEquip.ModelHands);
				equip.Legs = NpcItemwrapper.FromGear(this.Value.NpcEquip.ModelLegs);
				equip.Feet = NpcItemwrapper.FromGear(this.Value.NpcEquip.ModelFeet);
				equip.Ears = NpcItemwrapper.FromGear(this.Value.NpcEquip.ModelEars);
				equip.Neck = NpcItemwrapper.FromGear(this.Value.NpcEquip.ModelNeck);
				equip.Wrists = NpcItemwrapper.FromGear(this.Value.NpcEquip.ModelWrists);
				equip.RightRing = NpcItemwrapper.FromGear(this.Value.NpcEquip.ModelRightRing);
				equip.LeftRing = NpcItemwrapper.FromGear(this.Value.NpcEquip.ModelLeftRing);

				equip.DyeMainHand = GameDataService.Instance.Dyes.Get(this.Value.NpcEquip.DyeMain.Key);
				equip.DyeOffHand = GameDataService.Instance.Dyes.Get(this.Value.NpcEquip.DyeOff.Key);
				equip.DyeHead = GameDataService.Instance.Dyes.Get(this.Value.NpcEquip.DyeHead.Key);
				equip.DyeBody = GameDataService.Instance.Dyes.Get(this.Value.NpcEquip.DyeBody.Key);
				equip.DyeHands = GameDataService.Instance.Dyes.Get(this.Value.NpcEquip.DyeHands.Key);
				equip.DyeLegs = GameDataService.Instance.Dyes.Get(this.Value.NpcEquip.DyeLegs.Key);
				equip.DyeFeet = GameDataService.Instance.Dyes.Get(this.Value.NpcEquip.DyeFeet.Key);
				equip.DyeEars = GameDataService.Instance.Dyes.Get(this.Value.NpcEquip.DyeEars.Key);
				equip.DyeNeck = GameDataService.Instance.Dyes.Get(this.Value.NpcEquip.DyeNeck.Key);
				equip.DyeWrists = GameDataService.Instance.Dyes.Get(this.Value.NpcEquip.DyeWrists.Key);
				equip.DyeRightRing = GameDataService.Instance.Dyes.Get(this.Value.NpcEquip.DyeRightRing.Key);
				equip.DyeLeftRing = GameDataService.Instance.Dyes.Get(this.Value.NpcEquip.DyeLeftRing.Key);
			}
			else
			{
				equip.MainHand = NpcItemwrapper.FromWeapon(this.Value.ModelMain, false);
				equip.OffHand = NpcItemwrapper.FromWeapon(this.Value.ModelSub, true);
				equip.Head = NpcItemwrapper.FromGear(this.Value.ModelHead);
				equip.Body = NpcItemwrapper.FromGear(this.Value.ModelBody);
				equip.Hands = NpcItemwrapper.FromGear(this.Value.ModelHands);
				equip.Legs = NpcItemwrapper.FromGear(this.Value.ModelLegs);
				equip.Feet = NpcItemwrapper.FromGear(this.Value.ModelFeet);
				equip.Ears = NpcItemwrapper.FromGear(this.Value.ModelEars);
				equip.Neck = NpcItemwrapper.FromGear(this.Value.ModelNeck);
				equip.Wrists = NpcItemwrapper.FromGear(this.Value.ModelWrists);
				equip.RightRing = NpcItemwrapper.FromGear(this.Value.ModelRightRing);
				equip.LeftRing = NpcItemwrapper.FromGear(this.Value.ModelLeftRing);

				equip.DyeMainHand = GameDataService.Instance.Dyes.Get(this.Value.DyeMain.Key);
				equip.DyeOffHand = GameDataService.Instance.Dyes.Get(this.Value.DyeOff.Key);
				equip.DyeHead = GameDataService.Instance.Dyes.Get(this.Value.DyeHead.Key);
				equip.DyeBody = GameDataService.Instance.Dyes.Get(this.Value.DyeBody.Key);
				equip.DyeHands = GameDataService.Instance.Dyes.Get(this.Value.DyeHands.Key);
				equip.DyeLegs = GameDataService.Instance.Dyes.Get(this.Value.DyeLegs.Key);
				equip.DyeFeet = GameDataService.Instance.Dyes.Get(this.Value.DyeFeet.Key);
				equip.DyeEars = GameDataService.Instance.Dyes.Get(this.Value.DyeEars.Key);
				equip.DyeNeck = GameDataService.Instance.Dyes.Get(this.Value.DyeNeck.Key);
				equip.DyeWrists = GameDataService.Instance.Dyes.Get(this.Value.DyeWrists.Key);
				equip.DyeRightRing = GameDataService.Instance.Dyes.Get(this.Value.DyeRightRing.Key);
				equip.DyeLeftRing = GameDataService.Instance.Dyes.Get(this.Value.DyeLeftRing.Key);
			}

			return equip;
		}

		public class NpcEquipWrapper : INpcEquip
		{
			public IItem MainHand { get; set; }
			public IDye DyeMainHand { get; set; }
			public IItem OffHand { get; set; }
			public IDye DyeOffHand { get; set; }
			public IItem Head { get; set; }
			public IDye DyeHead { get; set; }
			public IItem Body { get; set; }
			public IDye DyeBody { get; set; }
			public IItem Legs { get; set; }
			public IDye DyeLegs { get; set; }
			public IItem Feet { get; set; }
			public IDye DyeFeet { get; set; }
			public IItem Hands { get; set; }
			public IDye DyeHands { get; set; }
			public IItem Wrists { get; set; }
			public IDye DyeWrists { get; set; }
			public IItem Neck { get; set; }
			public IDye DyeNeck { get; set; }
			public IItem Ears { get; set; }
			public IDye DyeEars { get; set; }
			public IItem LeftRing { get; set; }
			public IDye DyeLeftRing { get; set; }
			public IItem RightRing { get; set; }
			public IDye DyeRightRing { get; set; }
		}

		public class NpcItemwrapper : IItem
		{
			public int Key => 0;
			public string Name => "Npc Item";
			public string Description => null;
			public IImage Icon => null;

			public ushort ModelSet { get; set; }
			public ushort ModelBase { get; set; }
			public ushort ModelVariant { get; set; }
			public bool HasSubModel { get; set; }
			public ushort SubModelSet { get; set; }
			public ushort SubModelBase { get; set; }
			public ushort SubModelVariant { get; set; }
			public bool IsWeapon { get; set; }

			public Classes EquipableClasses
			{
				get
				{
					return Classes.All;
				}
			}

			public static NpcItemwrapper FromWeapon(Quad weapon, bool isSub)
			{
				NpcItemwrapper item = new NpcItemwrapper();
				item.IsWeapon = true;
				item.ModelSet = (ushort)weapon.Value1;
				item.ModelBase = (ushort)weapon.Value2;
				item.ModelVariant = (ushort)weapon.Value3;

				if (isSub)
				{
					item.HasSubModel = true;
					item.SubModelSet = (ushort)weapon.Value1;
					item.SubModelBase = (ushort)weapon.Value2;
					item.SubModelVariant = (ushort)weapon.Value3;
				}

				return item;
			}

			public static NpcItemwrapper FromGear(int[] gear)
			{
				NpcItemwrapper item = new NpcItemwrapper();
				item.IsWeapon = false;
				item.ModelBase = (ushort)(gear[0] + (gear[1] * 256));
				item.ModelVariant = (ushort)gear[2];
				return item;
			}

			public bool FitsInSlot(ItemSlots slot)
			{
				return true;
			}
		}
	}
}
