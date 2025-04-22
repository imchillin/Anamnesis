// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.Core.Extensions;
using Anamnesis.GameData;
using Anamnesis.GameData.Sheets;
using Anamnesis.Serialization.Converters;
using Anamnesis.Services;
using Anamnesis.TexTools;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

/// <summary>Represents a custom equipment item.</summary>
public class Equipment : IItem, IRow
{
	/// <inheritdoc/>
	[JsonIgnore] public uint RowId { get; set; }

	/// <inheritdoc/>
	public string Name { get; set; } = string.Empty;

	/// <inheritdoc/>
	public string? Description { get; set; }

	/// <summary>
	/// Gets or sets the ID of the equipment.
	/// </summary>
	/// <remarks>
	/// This identifier is deserialized from the JSON file.
	/// </remarks>
	public string Id { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the slots that this equipment fits in.
	/// </summary>
	public ItemSlots Slot { get; set; } = ItemSlots.None;

	/// <inheritdoc/>
	public byte EquipLevel => 0;

	/// <inheritdoc/>
	[JsonIgnore] public ulong Model => ExcelPageExtensions.ConvertToModel(this.ModelSet, this.ModelBase, this.ModelVariant);

	/// <inheritdoc/>
	[JsonIgnore] public ushort ModelSet => IItemConverter.SplitString(this.Id).modelSet;

	/// <inheritdoc/>
	[JsonIgnore] public ushort ModelBase => IItemConverter.SplitString(this.Id).modelBase;

	/// <inheritdoc/>
	[JsonIgnore] public ushort ModelVariant => IItemConverter.SplitString(this.Id).modelVariant;

	/// <inheritdoc/>
	[JsonIgnore] public bool IsWeapon => this.ModelSet != 0;

	/// <inheritdoc/>
	[JsonIgnore] public bool HasSubModel => false;

	/// <inheritdoc/>
	[JsonIgnore] public ulong SubModel => 0;

	/// <inheritdoc/>
	[JsonIgnore] public ushort SubModelSet => 0;

	/// <inheritdoc/>
	[JsonIgnore] public ushort SubModelBase => 0;

	/// <inheritdoc/>
	[JsonIgnore] public ushort SubModelVariant => 0;

	/// <inheritdoc/>
	[JsonIgnore] public ImgRef? Icon => null;

	/// <inheritdoc/>
	[JsonIgnore] public Classes EquipableClasses => Classes.All;

	/// <inheritdoc/>
	[JsonIgnore] public Mod? Mod => null;

	/// <inheritdoc/>
	[JsonIgnore]
	public bool IsFavorite
	{
		get => FavoritesService.IsFavorite<IItem>(this);
		set => FavoritesService.SetFavorite<IItem>(this, nameof(FavoritesService.Favorites.Items), value);
	}

	/// <inheritdoc/>
	[JsonIgnore] public bool CanOwn => false;

	/// <inheritdoc/>
	[JsonIgnore] public bool IsOwned { get; set; }

	/// <inheritdoc/>
	[JsonIgnore] public ItemCategories Category => ItemCategories.CustomEquipment;

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool FitsInSlot(ItemSlots slot)
	{
		return slot switch
		{
			ItemSlots.MainHand => this.Slot.HasFlagUnsafe(ItemSlots.MainHand),
			ItemSlots.OffHand => this.Slot.HasFlagUnsafe(ItemSlots.OffHand),
			ItemSlots.Head => this.Slot.HasFlagUnsafe(ItemSlots.Head),
			ItemSlots.Body => this.Slot.HasFlagUnsafe(ItemSlots.Body),
			ItemSlots.Hands => this.Slot.HasFlagUnsafe(ItemSlots.Hands),
			ItemSlots.Waist => this.Slot.HasFlagUnsafe(ItemSlots.Waist),
			ItemSlots.Legs => this.Slot.HasFlagUnsafe(ItemSlots.Legs),
			ItemSlots.Feet => this.Slot.HasFlagUnsafe(ItemSlots.Feet),
			ItemSlots.Ears => this.Slot.HasFlagUnsafe(ItemSlots.Ears),
			ItemSlots.Neck => this.Slot.HasFlagUnsafe(ItemSlots.Neck),
			ItemSlots.Wrists => this.Slot.HasFlagUnsafe(ItemSlots.Wrists),
			ItemSlots.RightRing => this.Slot.HasFlagUnsafe(ItemSlots.RightRing),
			ItemSlots.LeftRing => this.Slot.HasFlagUnsafe(ItemSlots.LeftRing),
			ItemSlots.Glasses => this.Slot.HasFlagUnsafe(ItemSlots.Glasses),
			ItemSlots.SoulCrystal => this.Slot.HasFlagUnsafe(ItemSlots.SoulCrystal),
			_ => false,
		};
	}
}
