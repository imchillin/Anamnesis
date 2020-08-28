// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GameData
{
	using System.Collections.Generic;

	public interface ITerritoryType : IDataObject
	{
		string Name { get; }
		string Place { get; }
		string Region { get; }
		string Zone { get; }
		string Background { get; }
		List<IWeather> Weathers { get; }
	}
}
