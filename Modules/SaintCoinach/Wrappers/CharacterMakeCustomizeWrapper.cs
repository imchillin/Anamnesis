// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.SaintCoinachModule
{
	using ConceptMatrix.Services;
	using SaintCoinach.Xiv;

	internal class CharacterMakeCustomizeWrapper : ObjectWrapper<CharaMakeCustomize>, ICharaMakeCustomize
	{
		public CharacterMakeCustomizeWrapper(CharaMakeCustomize value)
			: base(value)
		{
		}
	}
}
