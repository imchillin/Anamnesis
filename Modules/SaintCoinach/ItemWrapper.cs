// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.SaintCoinachModule
{
	using ConceptMatrix.Services;
	using SaintCoinach.Xiv;

	public class ItemWrapper : IItem
	{
		private Item item;

		public ItemWrapper(Item item)
		{
			this.item = item;
		}

		public string Name
		{
			get
			{
				return this.item.Name;
			}
		}
	}
}
