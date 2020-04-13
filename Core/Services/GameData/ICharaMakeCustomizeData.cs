// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GameData
{
	using System.Collections.Generic;

	public interface ICharaMakeCustomizeData : IData<ICharaMakeCustomize>
	{
		List<ICharaMakeCustomize> GetHair(Appearance.Tribes tribe, Appearance.Genders gender);
		ICharaMakeCustomize GetHair(Appearance.Tribes tribe, Appearance.Genders gender, byte featureId);
	}
}
