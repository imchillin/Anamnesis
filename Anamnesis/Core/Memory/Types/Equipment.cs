// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Runtime.InteropServices;
	using Anamnesis.GameData;
	using PropertyChanged;

	[StructLayout(LayoutKind.Sequential)]
	public struct Equipment
	{
		public Item Head;
		public Item Chest;
		public Item Arms;
		public Item Legs;
		public Item Feet;
		public Item Ear;
		public Item Neck;
		public Item Wrist;
		public Item RFinger;
		public Item LFinger;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct Item
	{
		public ushort Base;
		public byte Variant;
		public byte Dye;
	}

	[AddINotifyPropertyChangedInterface]
	public class EquipmentViewModel : StructViewModelBase<Equipment>
	{
		public EquipmentViewModel(IMemoryViewModel parent, string propertyName)
			: base(parent, propertyName)
		{
		}

		[ModelField] public ItemViewModel? Head { get; set; }
		[ModelField] public ItemViewModel? Chest { get; set; }
		[ModelField] public ItemViewModel? Arms { get; set; }
		[ModelField] public ItemViewModel? Legs { get; set; }
		[ModelField] public ItemViewModel? Feet { get; set; }
		[ModelField] public ItemViewModel? Ear { get; set; }
		[ModelField] public ItemViewModel? Neck { get; set; }
		[ModelField] public ItemViewModel? Wrist { get; set; }
		[ModelField] public ItemViewModel? RFinger { get; set; }
		[ModelField] public ItemViewModel? LFinger { get; set; }
	}

	#pragma warning disable SA1402

	[AddINotifyPropertyChangedInterface]
	public class ItemViewModel : StructViewModelBase<Item>
	{
		public ItemViewModel(EquipmentViewModel parent, string propertyName)
			: base(parent, propertyName)
		{
		}

		[ModelField] public ushort Base { get; set; }
		[ModelField] public byte Variant { get; set; }
		[ModelField] public byte Dye { get; set; }

		public void Clear()
		{
			this.Base = 0;
			this.Variant = 0;
			this.Dye = 0;
		}

		public void Equip(IItem item)
		{
			this.Base = item.ModelBase;
			this.Variant = (byte)item.ModelVariant;
		}
	}
}
