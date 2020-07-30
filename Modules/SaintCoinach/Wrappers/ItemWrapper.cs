// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.SaintCoinachModule
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using ConceptMatrix.GameData;
	using SaintCoinach.Xiv;
	using SaintCoinach.Xiv.Items;

	internal class ItemWrapper : ObjectWrapper<Item>, IItem
	{
		private ConcurrentDictionary<ItemSlots, bool> fitsInSlotsCache = new ConcurrentDictionary<ItemSlots, bool>();
		private Classes equipableClasses = Classes.None;

		public ItemWrapper(Item value)
			: base(value)
		{
		}

		public string Name
		{
			get
			{
				return this.Value.Name;
			}
		}

		public string Description
		{
			get
			{
				return this.Value.Description;
			}
		}

		public IImage Icon
		{
			get
			{
				return this.Value.Icon.ToIImage();
			}
		}

		public bool IsWeapon
		{
			get
			{
				return this.FitsInSlot(ItemSlots.MainHand) || this.FitsInSlot(ItemSlots.OffHand);
			}
		}

		public ushort ModelBase
		{
			get
			{
				return this.IsWeapon ? (ushort)this.Value.ModelMain.Value2 : (ushort)this.Value.ModelMain.Value1;
			}
		}

		public ushort ModelVariant
		{
			get
			{
				return this.IsWeapon ? (ushort)this.Value.ModelMain.Value3 : (ushort)this.Value.ModelMain.Value2;
			}
		}

		public ushort ModelSet
		{
			get
			{
				return (ushort)this.Value.ModelMain.Value1;
			}
		}

		public bool HasSubModel
		{
			get
			{
				return this.Value.ModelSub != null && this.Value.ModelSub.Value1 != 0;
			}
		}

		public ushort SubModelBase
		{
			get
			{
				return this.IsWeapon ? (ushort)this.Value.ModelSub.Value2 : (ushort)this.Value.ModelSub.Value1;
			}
		}

		public ushort SubModelVariant
		{
			get
			{
				return this.IsWeapon ? (ushort)this.Value.ModelSub.Value3 : (ushort)this.Value.ModelSub.Value2;
			}
		}

		public ushort SubModelSet
		{
			get
			{
				return (ushort)this.Value.ModelSub.Value1;
			}
		}

		public Classes EquipableClasses
		{
			get
			{
				if (this.equipableClasses == Classes.None)
				{
					foreach (Classes job in Enum.GetValues(typeof(Classes)))
					{
						if (this.CanBeUsedByClass(job))
						{
							this.equipableClasses |= job;
						}
					}
				}

				return this.equipableClasses;
			}
		}

		public bool CanBeUsedByClass(Classes job)
		{
			if (job == Classes.None)
				return false;

			if (job == Classes.All)
				return false;

			string abbreviation = job.GetAbbreviation();

			if (this.Value is Equipment eq)
			{
				foreach (ClassJob classJob in eq.ClassJobCategory.ClassJobs)
				{
					if (classJob.Abbreviation == abbreviation)
					{
						return true;
					}
				}

				return false;
			}

			return true;
		}

		public bool FitsInSlot(ItemSlots slot)
		{
			if (this.fitsInSlotsCache.ContainsKey(slot))
				return this.fitsInSlotsCache[slot];

			try
			{
				foreach (EquipSlot equipSlot in this.Value.EquipSlotCategory.PossibleSlots)
				{
					if (equipSlot.IsSlot(slot))
					{
						this.fitsInSlotsCache.TryAdd(slot, true);
						return true;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Write(new Exception("Failed to check item slot compatibility for item: " + this.Name, ex), @"Saint Coinach", Log.Severity.Error);
			}

			this.fitsInSlotsCache.TryAdd(slot, false);
			return false;
		}
	}
}
