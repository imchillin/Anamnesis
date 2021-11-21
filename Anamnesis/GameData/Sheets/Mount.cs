// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Sheets
{
	using System;
	using System.Windows.Media;
	using Anamnesis.Character.Utilities;
	using Anamnesis.Services;
	using Anamnesis.TexTools;
	using Lumina;
	using Lumina.Data;
	using Lumina.Excel;
	using Lumina.Excel.GeneratedSheets;

	[Sheet("Mount", 2247824408u)]
	public class Mount : ExcelRow, INpcBase
	{
		private string? name;

		private byte customizeRow;

		private int equipHead;
		private int equipBody;
		private int equipLeg;
		private int equipFoot;

		private MountAppearance? appearance;

		public string Name => this.name ?? $"Mount #{this.RowId}";
		public string Description { get; private set; } = string.Empty;
		public uint ModelCharaRow { get; private set; }

		public ImageSource? Icon { get; private set; }
		public Mod? Mod => null;
		public bool CanFavorite => true;
		public bool HasName => this.name != null;
		public string TypeKey => "Npc_Mount";

		public bool IsFavorite
		{
			get => FavoritesService.IsFavorite(this);
			set => FavoritesService.SetFavorite(this, value);
		}

		public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
		{
			base.PopulateData(parser, gameData, language);

			this.name = parser.ReadString(0);
			this.ModelCharaRow = (uint)parser.ReadColumn<int>(8);
			this.customizeRow = parser.ReadColumn<byte>(16);

			this.equipHead = parser.ReadColumn<int>(25);
			this.equipBody = parser.ReadColumn<int>(26);
			this.equipLeg = parser.ReadColumn<int>(27);
			this.equipFoot = parser.ReadColumn<int>(28);

			this.Icon = parser.ReadImageReference<ushort>(30);
		}

		public INpcAppearance? GetAppearance()
		{
			if (this.appearance == null)
				this.appearance = new MountAppearance(this);

			return this.appearance;
		}

		public class MountAppearance : INpcAppearance
		{
			public MountAppearance(Mount mount)
			{
				this.ModelCharaRow = mount.ModelCharaRow;

				ExcelSheet<BNpcCustomize>? customizeSheet = GameDataService.GetSheet<BNpcCustomize>();
				BNpcCustomize? customize = customizeSheet?.GetOrDefault(mount.customizeRow);
				if (customize != null)
				{
					this.Race = GameDataService.Races.Get(Math.Max(customize.Race.Row, 1));
					this.Tribe = GameDataService.Tribes.Get(Math.Max(customize.Tribe.Row, 1));

					this.FacePaintColor = customize.FacePaintColor;
					this.FacePaint = customize.FacePaint;

					// These were flipped
					this.ExtraFeature1 = customize.ExtraFeature2OrBust;
					this.ExtraFeature2OrBust = customize.ExtraFeature1;

					this.Gender = customize.Gender;
					this.BodyType = customize.BodyType;
					this.Height = customize.Height;
					this.Face = customize.Face;
					this.HairStyle = customize.HairStyle;
					this.EnableHairHighlight = customize.HairHighlight > 1;
					this.SkinColor = customize.SkinColor;
					this.EyeHeterochromia = customize.EyeHeterochromia;
					this.HairHighlightColor = customize.HairHighlightColor;
					this.FacialFeature = customize.FacialFeature;
					this.FacialFeatureColor = customize.FacialFeatureColor;
					this.Eyebrows = customize.Eyebrows;
					this.EyeColor = customize.EyeColor;
					this.EyeShape = customize.EyeShape;
					this.Nose = customize.Nose;
					this.Jaw = customize.Jaw;
					this.Mouth = customize.Mouth;
					this.LipColor = customize.LipColor;
					this.BustOrTone1 = customize.BustOrTone1;
					this.HairColor = customize.HairColor;
				}

				this.Head = LuminaExtensions.GetGearItem(ItemSlots.Head, (uint)mount.equipHead);
				this.Body = LuminaExtensions.GetGearItem(ItemSlots.Body, (uint)mount.equipBody);
				this.Legs = LuminaExtensions.GetGearItem(ItemSlots.Legs, (uint)mount.equipLeg);
				this.Feet = LuminaExtensions.GetGearItem(ItemSlots.Feet, (uint)mount.equipFoot);
			}

			public uint ModelCharaRow { get; private set; }
			public int FacePaintColor { get; private set; }
			public int FacePaint { get; private set; }
			public int ExtraFeature2OrBust { get; private set; }
			public int ExtraFeature1 { get; private set; }
			public Race? Race { get; private set; }
			public int Gender { get; private set; }
			public int BodyType { get; private set; }
			public int Height { get; private set; }
			public Tribe? Tribe { get; private set; }
			public int Face { get; private set; }
			public int HairStyle { get; private set; }
			public bool EnableHairHighlight { get; private set; }
			public int SkinColor { get; private set; }
			public int EyeHeterochromia { get; private set; }
			public int HairHighlightColor { get; private set; }
			public int FacialFeature { get; private set; }
			public int FacialFeatureColor { get; private set; }
			public int Eyebrows { get; private set; }
			public int EyeColor { get; private set; }
			public int EyeShape { get; private set; }
			public int Nose { get; private set; }
			public int Jaw { get; private set; }
			public int Mouth { get; private set; }
			public int LipColor { get; private set; }
			public int BustOrTone1 { get; private set; }
			public int HairColor { get; private set; }

			public IItem MainHand => ItemUtility.NoneItem;
			public IDye DyeMainHand => DyeUtility.NoneDye;
			public IItem OffHand => ItemUtility.NoneItem;
			public IDye DyeOffHand => DyeUtility.NoneDye;
			public IItem Head { get; private set; }
			public IDye DyeHead => DyeUtility.NoneDye;
			public IItem Body { get; private set; }
			public IDye DyeBody => DyeUtility.NoneDye;
			public IItem Legs { get; private set; }
			public IDye DyeLegs => DyeUtility.NoneDye;
			public IItem Feet { get; private set; }
			public IDye DyeFeet => DyeUtility.NoneDye;
			public IItem Hands => ItemUtility.NoneItem;
			public IDye DyeHands => DyeUtility.NoneDye;
			public IItem Wrists => ItemUtility.NoneItem;
			public IDye DyeWrists => DyeUtility.NoneDye;
			public IItem Neck => ItemUtility.NoneItem;
			public IDye DyeNeck => DyeUtility.NoneDye;
			public IItem Ears => ItemUtility.NoneItem;
			public IDye DyeEars => DyeUtility.NoneDye;
			public IItem LeftRing => ItemUtility.NoneItem;
			public IDye DyeLeftRing => DyeUtility.NoneDye;
			public IItem RightRing => ItemUtility.NoneItem;
			public IDye DyeRightRing => DyeUtility.NoneDye;
		}
	}
}
