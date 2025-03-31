// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData;

using Anamnesis.Core.Extensions;
using Anamnesis.GameData.Sheets;
using Anamnesis.Serialization.Converters;
using Anamnesis.Services;
using Anamnesis.TexTools;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

public class Equipment : IItem
{
	public enum FitsSlots
	{
		None = 0,

		MainHand = 1,
		OffHand = 2,

		Head = 4,
		Body = 8,
		Hands = 16,
		Legs = 32,
		Feet = 64,

		All = MainHand | OffHand | Head | Body | Hands | Legs | Feet,
		Weapons = MainHand | OffHand,
		Equipment = Head | Body | Hands | Legs | Feet,
	}

	[JsonIgnore] public uint RowId { get; set; }

	public string Name { get; set; } = string.Empty;
	public string? Description { get; set; }
	public string Id { get; set; } = string.Empty;
	public FitsSlots Slot { get; set; } = FitsSlots.None;
	public byte EquipLevel => 0;

	[JsonIgnore] public ulong Model => ExcelPageExtensions.ConvertToModel(this.ModelSet, this.ModelBase, this.ModelVariant);
	[JsonIgnore] public ushort ModelBase => IItemConverter.SplitString(this.Id).modelBase;
	[JsonIgnore] public ushort ModelVariant => IItemConverter.SplitString(this.Id).modelVariant;
	[JsonIgnore] public ushort ModelSet => IItemConverter.SplitString(this.Id).modelSet;

	[JsonIgnore] public bool IsWeapon => this.ModelSet != 0;
	[JsonIgnore] public bool HasSubModel => false;
	[JsonIgnore] public ulong SubModel => 0;
	[JsonIgnore] public ushort SubModelBase => 0;
	[JsonIgnore] public ushort SubModelVariant => 0;
	[JsonIgnore] public ushort SubModelSet => 0;
	[JsonIgnore] public ImgRef? Icon => null;
	[JsonIgnore] public Classes EquipableClasses => Classes.All;
	[JsonIgnore] public Mod? Mod => null;

	[JsonIgnore]
	public bool IsFavorite
	{
		get => FavoritesService.IsFavorite<IItem>(this);
		set => FavoritesService.SetFavorite<IItem>(this, nameof(FavoritesService.Favorites.Items), value);
	}

	[JsonIgnore] public bool CanOwn => false;
	[JsonIgnore] public bool IsOwned { get; set; }

	[JsonIgnore] public ItemCategories Category => ItemCategories.CustomEquipment;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool FitsInSlot(ItemSlots slot)
	{
		return slot switch
		{
			ItemSlots.MainHand => this.Slot.HasFlagUnsafe(FitsSlots.MainHand),
			ItemSlots.Head => this.Slot.HasFlagUnsafe(FitsSlots.Head),
			ItemSlots.Body => this.Slot.HasFlagUnsafe(FitsSlots.Body),
			ItemSlots.Hands => this.Slot.HasFlagUnsafe(FitsSlots.Hands),
			ItemSlots.Legs => this.Slot.HasFlagUnsafe(FitsSlots.Legs),
			ItemSlots.Feet => this.Slot.HasFlagUnsafe(FitsSlots.Feet),
			ItemSlots.OffHand => this.Slot.HasFlagUnsafe(FitsSlots.OffHand),

			_ => false,
		};
	}
}
