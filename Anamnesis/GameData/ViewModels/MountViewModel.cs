// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.ViewModels
{
	using System.Windows.Media;
	using Anamnesis.Character;
	using Anamnesis.Character.Utilities;
	using Anamnesis.Services;
	using Anamnesis.TexTools;
	using Lumina;
	using Lumina.Excel;
	using Lumina.Excel.GeneratedSheets;

	public class MountViewModel : ExcelRowViewModel<Mount>, INpcBase
	{
		public MountViewModel(uint key, ExcelSheet<Mount> sheet, GameData lumina)
			: base(key, sheet, lumina)
		{
			this.Race = GameDataService.Races!.Get(1);
			this.Tribe = GameDataService.Tribes!.Get(1);
		}

		public IRace Race { get; private set; }
		public ITribe Tribe { get; private set; }

		public int FacePaintColor => 0;
		public int FacePaint => 0;
		public int ExtraFeature2OrBust => 0;
		public int ExtraFeature1 => 0;
		public int Gender => 0;
		public int BodyType => 0;
		public int Height => 0;
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

		public ImageSource? Icon => this.lumina.GetImage(this.Value.Icon);
		public override string Name => this.HasName ? this.Value.Singular : $"Mount #{this.Key}";
		public override string? Description => null;
		public uint ModelCharaRow => this.Value.ModelChara.Row;
		public INpcEquip NpcEquip => new MountEquip(this.Value);
		public string TypeKey => "Npc_Mount";

		public Mod? Mod => null;
		public bool CanFavorite => true;
		public bool HasName => !string.IsNullOrEmpty(this.Value.Singular);

		public bool IsFavorite
		{
			get => FavoritesService.IsFavorite(this);
			set => FavoritesService.SetFavorite(this, value);
		}

		public class MountEquip : INpcEquip
		{
			private readonly Mount value;

			public MountEquip(Mount value)
			{
				this.value = value;
			}

			public IItem MainHand => ItemUtility.NoneItem;
			public IDye DyeMainHand => DyeUtility.NoneDye;
			public IItem OffHand => ItemUtility.NoneItem;
			public IDye DyeOffHand => DyeUtility.NoneDye;
			public IItem Head => LuminaExtensions.GetGearItem(ItemSlots.Head, (uint)this.value.EquipHead);
			public IDye DyeHead => DyeUtility.NoneDye;
			public IItem Body => LuminaExtensions.GetGearItem(ItemSlots.Head, (uint)this.value.EquipBody);
			public IDye DyeBody => DyeUtility.NoneDye;
			public IItem Legs => LuminaExtensions.GetGearItem(ItemSlots.Head, (uint)this.value.EquipLeg);
			public IDye DyeLegs => DyeUtility.NoneDye;
			public IItem Feet => LuminaExtensions.GetGearItem(ItemSlots.Head, (uint)this.value.EquipFoot);
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
