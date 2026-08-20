// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Items;
using Anamnesis.GameData;
using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Anamnesis.TexTools;
using Lumina;
using System.Runtime.CompilerServices;

public class DummyItem : IItem
{
	public DummyItem(ushort modelSet, ushort modelBase, ushort modelVariant)
	{
		this.ModelSet = modelSet;
		this.ModelBase = modelBase;
		this.ModelVariant = modelVariant;
		this.Model = ExcelPageExtensions.ConvertToModel(modelSet, modelBase, modelVariant);
		this.IsWeapon = modelSet != 0;
	}

	public DummyItem(ulong model, bool isWeapon = false)
	{
		this.Model = model;
		this.IsWeapon = isWeapon;
		LuminaExtensions.GetModel(model, isWeapon, out ushort modelSet, out ushort modelBase, out ushort modelVariant);
		this.ModelSet = modelSet;
		this.ModelBase = modelBase;
		this.ModelVariant = modelVariant;
	}

	public uint RowId => 0;
	public bool IsWeapon { get; private set; }
	public bool HasSubModel => false;
	public string Name => LocalizationService.GetString("Item_Unknown");
	public string Description => string.Empty;
	public ImgRef? Icon => null;
	public Classes EquipableClasses => Classes.All;
	public Mod? Mod => null;
	public byte EquipLevel => 0;

	public ulong Model { get; private set; }
	public ushort ModelBase { get; private set; }
	public ushort ModelVariant { get; private set; }
	public ushort ModelSet { get; private set; }

	public ulong SubModel => ExcelPageExtensions.ConvertToModel(this.SubModelSet, this.SubModelBase, this.SubModelVariant);
	public ushort SubModelBase { get; }
	public ushort SubModelVariant { get; }
	public ushort SubModelSet { get; }

	public bool IsFavorite
	{
		get => FavoritesService.IsFavorite<IItem>(this);
		set => FavoritesService.SetFavorite<IItem>(this, value);
	}

	public bool CanOwn => false;
	public bool IsOwned { get; set; }

	public ItemCategories Category => ItemCategories.Standard;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public virtual bool FitsInSlot(ItemSlots slot) => true;
}
