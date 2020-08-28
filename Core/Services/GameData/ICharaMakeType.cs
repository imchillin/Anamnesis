// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GameData
{
	using System.Collections.Generic;
	using ConceptMatrix.Memory;

	public interface ICharaMakeType : IDataObject
	{
		Appearance.Genders Gender { get; }
		Appearance.Races Race { get; }
		Appearance.Tribes Tribe { get; }
		IEnumerable<IImageSource> FacialFeatures { get; }
	}
}
