// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.EquipmentModule.Views
{
	using System;
	using System.ComponentModel;
	using System.Windows.Media.Media3D;
	using ConceptMatrix;
	using ConceptMatrix.Injection;
	using ConceptMatrix.Services;

	public class EquipmentItemViewModel : EquipmentBaseViewModel
	{
		public EquipmentItemViewModel(Equipment target, ItemSlots slot)
			: base(slot)
		{
			Equipment.Item item = target.GetItem(slot);

			if (item == null)
				return;

			// hmm. doesn't seem to be any reason you _cant_ dye accessories...
			// TODO: test what happens when you dye an accessory.
			this.CanDye = slot != ItemSlots.Ears
				&& slot != ItemSlots.Neck
				&& slot != ItemSlots.Wrists
				&& slot != ItemSlots.LeftRing
				&& slot != ItemSlots.RightRing;

			this.CanColor = false;
			this.CanScale = false;
		}

		protected override void Apply()
		{
			throw new NotImplementedException();
		}

		private IDye GetDye(Equipment.Item item)
		{
			// None
			if (item.Dye == 0)
				return null;

			foreach (IDye dye in this.gameData.Dyes.All)
			{
				if (dye.Id == item.Dye)
				{
					return dye;
				}
			}

			return null;
		}
	}
}
