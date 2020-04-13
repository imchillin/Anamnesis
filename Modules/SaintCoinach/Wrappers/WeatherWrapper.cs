// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.SaintCoinachModule
{
	using ConceptMatrix.GameData;
	using SaintCoinach.Xiv;

	internal class WeatherWrapper : ObjectWrapper<Weather>, IWeather
	{
		public WeatherWrapper(Weather row)
			: base(row)
		{
		}
	}
}
