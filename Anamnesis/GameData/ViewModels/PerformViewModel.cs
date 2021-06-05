// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.GameData.ViewModels
{
	using System;
	using System.Windows.Media;
	using Anamnesis.Services;
	using Anamnesis.TexTools;
	using Lumina;
	using Lumina.Excel;
	using Lumina.Excel.GeneratedSheets;

	public class PerformViewModel : ExcelRowViewModel<Perform>, IItem
	{
		private ushort modelSet;
		private ushort modelBase;
		private ushort modelVariant;

		public PerformViewModel(uint key, ExcelSheet<Perform> sheet, GameData lumina)
			: base(key, sheet, lumina)
		{
			LuminaExtensions.GetModel(this.Value.ModelKey, true, out this.modelSet, out this.modelBase, out this.modelVariant);

			this.Mod = TexToolsService.GetMod(this);
		}

		public override string Name => this.Value.Instrument;

		ImageSource? IItem.Icon => null;
		ushort IItem.ModelSet => this.modelSet;
		ushort IItem.ModelBase => this.modelBase;
		ushort IItem.ModelVariant => this.modelVariant;
		bool IItem.HasSubModel => false;
		ushort IItem.SubModelSet => 0;
		ushort IItem.SubModelBase => 0;
		ushort IItem.SubModelVariant => 0;
		Classes IItem.EquipableClasses => Classes.All;
		bool IItem.IsWeapon => true;

		public Mod? Mod { get; private set; }

		public bool IsFavorite
		{
			get => FavoritesService.IsFavorite(this);
			set => FavoritesService.SetFavorite(this, value);
		}

		public bool FitsInSlot(ItemSlots slot)
		{
			return slot == ItemSlots.MainHand;
		}
	}
}
