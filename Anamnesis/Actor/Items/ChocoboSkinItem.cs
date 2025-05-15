// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Items;

using Anamnesis.GameData;
using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Anamnesis.TexTools;
using System.Runtime.CompilerServices;

public class ChocoboSkinItem : IItem
{
	public ChocoboSkinItem(INpcBase mount, ushort variant)
	{
		this.Name = mount.Name;
		this.Description = mount.Description;
		this.ModelVariant = variant;
		this.Icon = mount.Icon;
	}

	public string Name { get; private set; }
	public string? Description { get; private set; }
	public ImgRef? Icon { get; private set; }

	public ulong Model => ExcelPageExtensions.ConvertToModel(0, 1, this.ModelVariant);
	public ushort ModelSet => 0;
	public ushort ModelBase => 1;
	public ushort ModelVariant { get; private set; }
	public bool HasSubModel => false;
	public ulong SubModel => 0;
	public ushort SubModelSet => 0;
	public ushort SubModelBase => 0;
	public ushort SubModelVariant => 0;
	public Classes EquipableClasses => Classes.All;
	public bool IsWeapon => false;
	public Mod? Mod => null;
	public uint RowId => 0;
	public byte EquipLevel => 0;

	public bool IsFavorite
	{
		get => FavoritesService.IsFavorite<IItem>(this);
		set => FavoritesService.SetFavorite<IItem>(this, value);
	}

	public bool CanOwn => false;
	public bool IsOwned { get; set; }

	public ItemCategories Category => ItemCategories.Standard;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool FitsInSlot(ItemSlots slot) => slot == ItemSlots.Legs;
}