// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.SaintCoinachModule
{
	using ConceptMatrix;
	using ConceptMatrix.GameData;
	using SaintCoinach.Xiv;

	internal class CharacterMakeCustomizeWrapper : ObjectWrapper<CharaMakeCustomize>, ICharaMakeCustomize
	{
		public CharacterMakeCustomizeWrapper(CharaMakeCustomize value)
			: base(value)
		{
			this.Icon = value.Icon.ToIImage();
			this.FeatureId = (byte)value.FeatureID;
		}

		public IImage Icon { get; private set; }
		public byte FeatureId { get; private set; }
	}
}
