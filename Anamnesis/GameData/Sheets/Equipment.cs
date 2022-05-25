// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData;

using System.Text.Json.Serialization;
using Anamnesis.GameData.Sheets;
using Anamnesis.Serialization.Converters;
using Anamnesis.Services;
using Anamnesis.TexTools;

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

	[JsonIgnore] public ushort ModelBase => IItemConverter.SplitString(this.Id).modelBase;
	[JsonIgnore] public ushort ModelVariant => IItemConverter.SplitString(this.Id).modelVariant;
	[JsonIgnore] public ushort ModelSet => IItemConverter.SplitString(this.Id).modelSet;

	[JsonIgnore] public bool IsWeapon => this.ModelSet != 0;
	[JsonIgnore] public bool HasSubModel => false;
	[JsonIgnore] public ushort SubModelBase => 0;
	[JsonIgnore] public ushort SubModelVariant => 0;
	[JsonIgnore] public ushort SubModelSet => 0;
	[JsonIgnore] public ImageReference? Icon => null;
	[JsonIgnore] public Classes EquipableClasses => Classes.All;
	[JsonIgnore] public Mod? Mod => null;

	[JsonIgnore]
	public bool IsFavorite
	{
		get => FavoritesService.IsFavorite(this);
		set => FavoritesService.SetFavorite(this, value);
	}

	[JsonIgnore] public bool CanOwn => false;
	[JsonIgnore] public bool IsOwned { get; set; }

	[JsonIgnore] public ItemCategories Category => ItemCategories.CustomEquipment;

	public bool FitsInSlot(ItemSlots slot)
	{
		return slot switch
		{
			ItemSlots.MainHand => this.Slot.HasFlag(FitsSlots.MainHand),
			ItemSlots.Head => this.Slot.HasFlag(FitsSlots.Head),
			ItemSlots.Body => this.Slot.HasFlag(FitsSlots.Body),
			ItemSlots.Hands => this.Slot.HasFlag(FitsSlots.Hands),
			ItemSlots.Legs => this.Slot.HasFlag(FitsSlots.Legs),
			ItemSlots.Feet => this.Slot.HasFlag(FitsSlots.Feet),
			ItemSlots.OffHand => this.Slot.HasFlag(FitsSlots.OffHand),

			_ => false,
		};
	}
}
