// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.Core.Extensions;
using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Anamnesis.TexTools;
using Lumina.Excel;
using System.Runtime.CompilerServices;

/// <summary>
/// Represents companion equipment (e.g. chocobo bardings) data from the game data.
/// </summary>
[Sheet("BuddyEquip", 0xB429792A)]
public readonly struct BuddyEquip(ExcelPage page, uint offset, uint row)
	: IExcelRow<BuddyEquip>
{
	/// <inheritdoc/>
	public readonly uint RowId => row;

	/// <summary>Gets the singular name of the equipment object.</summary>
	public readonly string Name => page.ReadString(offset + 8, offset).ToString();

	/// <summary>Gets the equipment object's head slot item (if any).</summary>
	public BuddyItem? Head
	{
		get
		{
			int headData = page.ReadInt32(offset + 20);
			ushort headBase = (ushort)headData;
			ushort headVariant = (ushort)(headData >> 16);
			ushort headIcon = page.ReadUInt16(offset + 32);

			if (headBase == 0 && headVariant == 0)
				return null;

			return new(row, this.Name, ItemSlots.Head, headBase, headVariant, headIcon);
		}
	}

	/// <summary>Gets the equipment object's body slot item (if any).</summary>
	public BuddyItem? Body
	{
		get
		{
			int bodyData = page.ReadInt32(offset + 24);
			ushort bodyBase = (ushort)bodyData;
			ushort bodyVariant = (ushort)(bodyData >> 16);
			ushort bodyIcon = page.ReadUInt16(offset + 34);

			if (bodyBase == 0 && bodyVariant == 0)
				return null;

			return new(row, this.Name, ItemSlots.Body, bodyBase, bodyVariant, bodyIcon);
		}
	}

	/// <summary>Gets the equipment object's feet slot item (if any).</summary>
	public BuddyItem? Feet
	{
		get
		{
			int legsData = page.ReadInt32(offset + 28);
			ushort legsBase = (ushort)legsData;
			ushort legsVariant = (ushort)(legsData >> 16);
			ushort legsIcon = page.ReadUInt16(offset + 36);

			if (legsBase != 0 || legsVariant != 0)
				return new(row, this.Name, ItemSlots.Feet, legsBase, legsVariant, legsIcon);

			return null;
		}
	}

	/// <summary>
	/// Based on a given integer, return the ItemSlot value for which it is aliased.
	/// This is used when loading favorites to easily look up the ItemSlot for the favorite BuddyEquip.
	/// </summary>
	public static ItemSlots GetBuddyEquipItemSlotByIntAlias(uint value)
	{
		return value switch
		{
			1 => ItemSlots.Head,
			2 => ItemSlots.Body,
			5 => ItemSlots.Feet,
			_ => (ItemSlots)value
		};
	}

	/// <summary>
	/// Creates a new instance of the <see cref="BuddyEquip"/> struct.
	/// </summary>
	/// <param name="page">The Excel page.</param>
	/// <param name="offset">The offset within the page.</param>
	/// <param name="row">The row ID.</param>
	/// <returns>A new instance of the <see cref="BuddyEquip"/> struct.</returns>
	static BuddyEquip IExcelRow<BuddyEquip>.Create(ExcelPage page, uint offset, uint row) =>
		new(page, offset, row);

	/// <summary>Represents a companion equipment item.</summary>
	public class BuddyItem(uint row, string name, ItemSlots slot, ushort modelBase, ushort modelVariant, ushort icon): IItem
	{
		/// <inheritdoc/>
		public string Name { get; private set; } = name;

		/// <summary>Gets the slot(s) that this item fits in.</summary>
		public ItemSlots Slot { get; private set; } = slot;

		/// <inheritdoc/>
		public ImgRef? Icon { get; private set; } = new(icon);

		/// <inheritdoc/>
		public uint RowId => row;

		/// <inheritdoc/>
		public string? Description => null;

		/// <inheritdoc/>
		public ulong Model => 0;

		/// <inheritdoc/>
		public ushort ModelSet => 0;

		/// <inheritdoc/>
		public ushort ModelBase { get; private set; } = modelBase;

		/// <inheritdoc/>
		public ushort ModelVariant { get; private set; } = modelVariant;

		/// <inheritdoc/>
		public bool HasSubModel => false;

		/// <inheritdoc/>
		public ulong SubModel => 0;

		/// <inheritdoc/>
		public ushort SubModelSet => 0;

		/// <inheritdoc/>
		public ushort SubModelBase => 0;

		/// <inheritdoc/>
		public ushort SubModelVariant => 0;

		/// <inheritdoc/>
		public Classes EquipableClasses => Classes.All;

		/// <inheritdoc/>
		public bool IsWeapon => false;

		/// <inheritdoc/>
		public ItemCategories Category => ItemCategories.None;

		/// <inheritdoc/>
		public ItemFavoriteCategory FavoriteItemCategory => ItemFavoriteCategory.BuddyEquipment;

		/// <inheritdoc/>
		public Mod? Mod => null;

		/// <inheritdoc/>
		public bool IsFavorite
		{
			get => FavoritesService.IsFavorite<IItem>(this);
			set => FavoritesService.SetFavorite<IItem>(this, nameof(FavoritesService.Favorites.Items), value);
		}


		/// <inheritdoc/>
		public bool CanOwn => false;

		/// <inheritdoc/>
		public bool IsOwned { get; set; }

		/// <inheritdoc/>
		public byte EquipLevel => 0;

		/// <inheritdoc/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool FitsInSlot(ItemSlots slot) => this.Slot.HasFlagUnsafe(slot);

		/// <summary>
		/// For this BuddyItem, return the integer alias for its ItemSlot value.
		/// This is used when saving favorites to store which ItemSlot this BuddyEquip is favorited.
		/// </summary>
		public uint GetBuddyEquipSlotIntAlias()
		{
			return this.Slot switch
			{
				ItemSlots.Head => 1,
				ItemSlots.Body => 2,
				ItemSlots.Feet => 5,
				_ => (uint)this.Slot
			};
		}
	}
}
