// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;
using Anamnesis.Actor.Utilities;
using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Anamnesis.TexTools;
using Lumina;
using Lumina.Data;
using Lumina.Excel;

using ExcelRow = Anamnesis.GameData.Sheets.ExcelRow;

[Sheet("Mount", 0x33b2e4b2)]
public class Mount : ExcelRow, INpcBase
{
	private string? name;

	private int equipHead;
	private int equipBody;
	private int equipLeg;
	private int equipFoot;

	private MountAppearance? appearance;

	public string Name => this.name ?? $"{this.TypeName} #{this.RowId}";
	public string Description { get; private set; } = string.Empty;
	public uint ModelCharaRow { get; private set; }
	public byte MountCustomizeRow { get; private set; }

	public ImageReference? Icon { get; private set; }
	public Mod? Mod => null;
	public bool CanFavorite => true;
	public bool HasName => this.name != null;
	public string TypeName => "Mount";

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
		this.MountCustomizeRow = parser.ReadColumn<byte>(16);

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

			this.Head = LuminaExtensions.GetGearItem(ItemSlots.Head, (uint)mount.equipHead);
			this.Body = LuminaExtensions.GetGearItem(ItemSlots.Body, (uint)mount.equipBody);
			this.Legs = LuminaExtensions.GetGearItem(ItemSlots.Legs, (uint)mount.equipLeg);
			this.Feet = LuminaExtensions.GetGearItem(ItemSlots.Feet, (uint)mount.equipFoot);
		}

		public uint ModelCharaRow { get; private set; }
		public int FacePaintColor => 0;
		public int FacePaint => 0;
		public int ExtraFeature2OrBust => 0;
		public int ExtraFeature1 => 0;
		public Race? Race => null;
		public int Gender => 0;
		public int BodyType => 0;
		public int Height => 0;
		public Tribe? Tribe => null;
		public int Face => 0;
		public int HairStyle => 0;
		public bool EnableHairHighlight => false;
		public int SkinColor => 0;
		public int EyeHeterochromia => 0;
		public int HairHighlightColor => 0;
		public int FacialFeature => 0;
		public int FacialFeatureColor => 0;
		public int Eyebrows => 0;
		public int EyeColor => 0;
		public int EyeShape => 0;
		public int Nose => 0;
		public int Jaw => 0;
		public int Mouth => 0;
		public int LipColor => 0;
		public int BustOrTone1 => 0;
		public int HairColor => 0;

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
