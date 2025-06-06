﻿// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Items;

using Anamnesis.GameData;
using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Anamnesis.TexTools;
using System.Runtime.CompilerServices;

public class InvisibleHeadItem : IItem
{
	public string Name => LocalizationService.GetString("Item_InvisibleHead");
	public string Description => LocalizationService.GetString("Item_InvisibleHeadDesc");
	public ImgRef? Icon => GameDataService.Items.GetRow(10032).Icon;

	public ulong Model => ExcelPageExtensions.ConvertToModel(0, 6121, 254);
	public ushort ModelSet => 0;
	public ushort ModelBase => 6121;
	public ushort ModelVariant => 254;
	public bool HasSubModel => false;
	public ulong SubModel => 0;
	public ushort SubModelSet => 0;
	public ushort SubModelBase => 0;
	public ushort SubModelVariant => 0;
	public Classes EquipableClasses => Classes.All;
	public bool IsWeapon => false;
	public Mod? Mod => null;
	public uint RowId => 1;
	public byte EquipLevel => 0;

	public bool IsFavorite
	{
		get => FavoritesService.IsFavorite<IItem>(this);
		set => FavoritesService.SetFavorite<IItem>(this, nameof(FavoritesService.Favorites.Items), value);
	}

	public bool CanOwn => false;
	public bool IsOwned { get; set; }

	public ItemCategories Category => ItemCategories.Standard;

	public ItemFavoriteCategory FavoriteItemCategory => ItemFavoriteCategory.OneOffItem;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool FitsInSlot(ItemSlots slot) => slot == ItemSlots.Head;
}