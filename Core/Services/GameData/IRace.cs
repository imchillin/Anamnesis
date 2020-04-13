// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GameData
{
	using System.Collections.Generic;

	public interface IRace : IDataObject
	{
		Appearance.Races Race { get; }
		string Feminine { get; }
		string Masculine { get; }
		string DisplayName { get; }

		IEnumerable<ITribe> Tribes { get; }
	}
}
