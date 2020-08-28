// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.SaintCoinachModule
{
	using Anamnesis;
	using Anamnesis.GameData;
	using SaintCoinach.Xiv;

	internal class WeatherWrapper : ObjectWrapper<Weather>, IWeather
	{
		public WeatherWrapper(Weather row)
			: base(row)
		{
		}

		public string Name
		{
			get
			{
				return this.Value.Name;
			}
		}

		public string Description
		{
			get
			{
				return this.Value.Description;
			}
		}

		public IImageSource Icon
		{
			get
			{
				return this.Value.Icon.ToIImage();
			}
		}
	}
}
