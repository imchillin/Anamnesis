// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.SaintCoinachModule
{
	using ConceptMatrix.Services;
	using SaintCoinach.Xiv;

	internal class ItemWrapper : ObjectWrapper<Item>, IItem
	{
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
	}
}
