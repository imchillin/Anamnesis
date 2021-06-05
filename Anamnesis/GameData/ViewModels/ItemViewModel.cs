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

	public class ItemViewModel : ExcelRowViewModel<Item>, IItem
	{
		private ushort modelSet;
		private ushort modelBase;
		private ushort modelVariant;

		private ushort subModelSet;
		private ushort subModelBase;
		private ushort subModelVariant;

		private ClassJobCategory? classJob;

		public ItemViewModel(uint key, ExcelSheet<Item> sheet, GameData lumina)
			: base(key, sheet, lumina)
		{
			this.classJob = this.Value.ClassJobCategory.Value;

			LuminaExtensions.GetModel(this.Value.ModelMain, this.IsWeapon, out this.modelSet, out this.modelBase, out this.modelVariant);
			LuminaExtensions.GetModel(this.Value.ModelSub, this.IsWeapon, out this.subModelSet, out this.subModelBase, out this.subModelVariant);

			this.Mod = TexToolsService.GetMod(this);
		}

		public override string Name => this.Value.Name;
		public override string? Description => this.Value.Description;
		public ImageSource? Icon => this.lumina.GetImage(this.Value.Icon);
		public ushort ModelSet => this.modelSet;
		public ushort ModelBase => this.modelBase;
		public ushort ModelVariant => this.modelVariant;
		public bool HasSubModel => this.Value.ModelSub != 0;
		public ushort SubModelSet => this.subModelSet;
		public ushort SubModelBase => this.subModelBase;
		public ushort SubModelVariant => this.subModelVariant;
		public Classes EquipableClasses => this.classJob != null ? this.classJob.ToFlags() : Classes.None;

		public bool IsWeapon => this.FitsInSlot(ItemSlots.MainHand) || this.FitsInSlot(ItemSlots.OffHand);

		public Mod? Mod { get; private set; }

		public bool IsFavorite
		{
			get => FavoritesService.IsFavorite(this);
			set => FavoritesService.SetFavorite(this, value);
		}

		public bool FitsInSlot(ItemSlots slot)
		{
			return this.Value.EquipSlotCategory.Value?.Contains(slot) ?? false;
		}
	}
}
