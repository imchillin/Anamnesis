// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character
{
	using System;
	using Anamnesis.Character.Items;
	using Anamnesis.Character.Utilities;
	using Anamnesis.GameData;
	using Anamnesis.GameData.Sheets;
	using Anamnesis.Services;
	using Anamnesis.TexTools;

	[Serializable]
	public class Monster : INpcResident, IJsonRow
	{
		public enum Types
		{
			Unknown,

			Character,
			Mount,
			Minion,
			Effect,
			Monster,
		}

		public uint Key { get; set; }

		public int ModelType { get; set; }
		public string Name { get; set; } = string.Empty;
		public Types Type { get; set; } = Types.Unknown;

		public ushort Head { get; set; } = 1;
		public ushort Body { get; set; } = 1;
		public ushort Legs { get; set; } = 1;
		public ushort Feet { get; set; } = 1;
		public ushort Hands { get; set; } = 1;

		public INpcBase? Appearance => new MonsterAppearance(this);
		public bool CanCustomize => this.Type == Types.Character;
		public string? Description => null;
		public string Singular => this.Name;
		public string Plural => this.Name;
		public string Title => this.Type + " - " + this.ModelType.ToString();

		public Mod? Mod => TexToolsService.GetMod(this);

		public bool IsFavorite
		{
			get => FavoritesService.IsFavorite(this);
			set => FavoritesService.SetFavorite(this, value);
		}

		public class MonsterAppearance : INpcBase, INpcEquip
		{
			public MonsterAppearance(Monster monster)
			{
				this.ModelType = monster.ModelType;

				this.Head = new DummyItem(0, monster.Head, 0);
				this.Body = new DummyItem(0, monster.Body, 0);
				this.Legs = new DummyItem(0, monster.Legs, 0);
				this.Feet = new DummyItem(0, monster.Feet, 0);
				this.Hands = new DummyItem(0, monster.Hands, 0);

				this.MainHand = ItemUtility.NoneItem;
				this.OffHand = ItemUtility.NoneItem;
			}

			public int ModelType { get; set; }
			public IItem MainHand { get; set; }
			public IItem OffHand { get; set; }
			public IItem Head { get; set; }
			public IItem Body { get; set; }
			public IItem Legs { get; set; }
			public IItem Feet { get; set; }
			public IItem Hands { get; set; }

			public int FacePaintColor => 0;
			public int FacePaint => 0;
			public int ExtraFeature2OrBust => 0;
			public int ExtraFeature1 => 0;
			public IRace Race => GameDataService.Races.Get(1);
			public int Gender => 1;
			public int BodyType => 0;
			public int Height => 0;
			public ITribe Tribe => GameDataService.Tribes.Get(1);
			public int Face => 1;
			public int HairStyle => 1;
			public int HairHighlight => 1;
			public int SkinColor => 1;
			public int EyeHeterochromia => 1;
			public int HairHighlightColor => 1;
			public int FacialFeature => 0;
			public int FacialFeatureColor => 1;
			public int Eyebrows => 1;
			public int EyeColor => 1;
			public int EyeShape => 1;
			public int Nose => 1;
			public int Jaw => 1;
			public int Mouth => 1;
			public int LipColor => 0;
			public int BustOrTone1 => 0;
			public int HairColor => 0;
			public uint Key => 0;
			public string Name => "Invalid";
			public string? Description => null;
			public INpcEquip NpcEquip => this;
			public IDye DyeMainHand => ItemUtility.NoneDye;
			public IDye DyeOffHand => ItemUtility.NoneDye;
			public IDye DyeHead => ItemUtility.NoneDye;
			public IDye DyeBody => ItemUtility.NoneDye;
			public IDye DyeLegs => ItemUtility.NoneDye;
			public IDye DyeFeet => ItemUtility.NoneDye;
			public IDye DyeHands => ItemUtility.NoneDye;
			public IDye DyeWrists => ItemUtility.NoneDye;
			public IDye DyeNeck => ItemUtility.NoneDye;
			public IDye DyeEars => ItemUtility.NoneDye;
			public IDye DyeLeftRing => ItemUtility.NoneDye;
			public IDye DyeRightRing => ItemUtility.NoneDye;
			public IItem Wrists => ItemUtility.NoneItem;
			public IItem Neck => ItemUtility.NoneItem;
			public IItem Ears => ItemUtility.NoneItem;
			public IItem LeftRing => ItemUtility.NoneItem;
			public IItem RightRing => ItemUtility.NoneItem;
		}
	}
}
