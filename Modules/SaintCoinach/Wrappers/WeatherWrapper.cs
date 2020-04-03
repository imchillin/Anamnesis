// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.SaintCoinachModule
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using ConceptMatrix.Services;
	using SaintCoinach.Xiv;

	internal class WeatherWrapper : ObjectWrapper<Weather>, IWeather
	{
		public WeatherWrapper(Weather row)
			: base(row)
		{
		}
	}
}
