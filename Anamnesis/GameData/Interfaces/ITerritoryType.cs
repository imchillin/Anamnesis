// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData
{
	using System.Collections.Generic;

	public interface ITerritoryType : IRow
	{
		string Place { get; }
		string Region { get; }
		string Zone { get; }

		List<IWeather> Weathers { get; }
	}
}
