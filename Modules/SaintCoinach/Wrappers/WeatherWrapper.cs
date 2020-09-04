// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.SaintCoinachModule
{
	using System;
	using Anamnesis;
	using Anamnesis.GameData;
	using SaintCoinach.Xiv;

	internal class WeatherWrapper : ObjectWrapper<Weather>, IWeather
	{
		public WeatherWrapper(Weather row)
			: base(row)
		{
		}

		public ushort WeatherId
		{
			get
			{
				// this is super weird.
				byte[] bytes = { (byte)this.Key, (byte)this.Key };
				return BitConverter.ToUInt16(bytes, 0);
			}
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
