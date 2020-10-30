// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GameData
{
	using System.Collections.Generic;

	public interface ITerritoryType : IRow
	{
		string Name { get; }
		string Place { get; }
		string Region { get; }
		string Zone { get; }

		List<IWeather> Weathers { get; }
	}
}
