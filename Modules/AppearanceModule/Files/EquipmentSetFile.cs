// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Files
{
	using System;
	using ConceptMatrix.AppearanceModule.ViewModels;
	using ConceptMatrix.AppearanceModule.Views;

	[Serializable]
	public class EquipmentSetFile : FileBase
	{
		public static readonly FileType FileType = new FileType("cm3eq", "Equipment Set", typeof(EquipmentSetFile));

		public Weapon MainHand { get; set; } = new Weapon();
		public Weapon OffHand { get; set; } = new Weapon();

		public Item Head { get; set; } = new Item();
		public Item Body { get; set; } = new Item();
		public Item Hands { get; set; } = new Item();
		public Item Legs { get; set; } = new Item();
		public Item Feet { get; set; } = new Item();
		public Item Ears { get; set; } = new Item();
		public Item Neck { get; set; } = new Item();
		public Item Wrists { get; set; } = new Item();
		public Item LeftRing { get; set; } = new Item();
		public Item RightRing { get; set; } = new Item();

		public override FileType GetFileType()
		{
			return FileType;
		}

		public void Read(EquipmentEditor vm)
		{
			this.MainHand.Read(vm.MainHand);
			this.OffHand.Read(vm.OffHand);
			this.Head.Read(vm.Head);
			this.Body.Read(vm.Body);
			this.Hands.Read(vm.Hands);
			this.Legs.Read(vm.Legs);
			this.Feet.Read(vm.Feet);
			this.Ears.Read(vm.Ears);
			this.Neck.Read(vm.Neck);
			this.Wrists.Read(vm.Wrists);
			this.LeftRing.Read(vm.LeftRing);
			this.RightRing.Read(vm.RightRing);
		}

		public void Write(EquipmentEditor vm)
		{
			if (this.MainHand.ModelSet != 0)
				this.MainHand.Write(vm.MainHand);

			if (vm.OffHand.ModelSet != 0)
				this.OffHand.Write(vm.OffHand);

			this.Head.Write(vm.Head);
			this.Body.Write(vm.Body);
			this.Hands.Write(vm.Hands);
			this.Legs.Write(vm.Legs);
			this.Feet.Write(vm.Feet);
			this.Ears.Write(vm.Ears);
			this.Neck.Write(vm.Neck);
			this.Wrists.Write(vm.Wrists);
			this.LeftRing.Write(vm.LeftRing);
			this.RightRing.Write(vm.RightRing);
		}

		[Serializable]
		public class Weapon : Item
		{
			public Color Color { get; set; }
			public Vector Scale { get; set; }
			public ushort ModelSet { get; set; }

			public override void Write(EquipmentBaseViewModel to)
			{
				to.Color = this.Color;
				to.Scale = this.Scale;
				to.ModelSet = this.ModelSet;

				base.Write(to);
			}

			public override void Read(EquipmentBaseViewModel from)
			{
				this.Color = from.Color;
				this.Scale = from.Scale;
				this.ModelSet = from.ModelSet;

				base.Read(from);
			}
		}

		[Serializable]
		public class Item
		{
			public ushort ModelBase { get; set; }
			public ushort ModelVariant { get; set; }
			public byte DyeId { get; set; }

			public virtual void Write(EquipmentBaseViewModel to)
			{
				to.ModelBase = this.ModelBase;
				to.ModelVariant = this.ModelVariant;
				to.DyeId = this.DyeId;
			}

			public virtual void Read(EquipmentBaseViewModel from)
			{
				this.ModelBase = from.ModelBase;
				this.ModelVariant = from.ModelVariant;
				this.DyeId = from.DyeId;
			}
		}
	}
}
