// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.SaintCoinachModule
{
	using System;
	using System.Collections.Generic;
	using Anamnesis;
	using Anamnesis.GameData;
	using Anamnesis.Memory;
	using SaintCoinach.Xiv;

	internal class CharacterMakeTypeWrapper : ObjectWrapper<CharaMakeType>, ICharaMakeType
	{
		private List<IImageSource> facialFeatureList;

		public CharacterMakeTypeWrapper(CharaMakeType row)
			: base(row)
		{
		}

		public Appearance.Genders Gender
		{
			get
			{
				return (Appearance.Genders)this.Value.Gender;
			}
		}

		public Appearance.Races Race
		{
			get
			{
				return (Appearance.Races)this.Value.Race.Key;
			}
		}

		public Appearance.Tribes Tribe
		{
			get
			{
				return (Appearance.Tribes)this.Value.Tribe.Key;
			}
		}

		public IEnumerable<IImageSource> FacialFeatures
		{
			get
			{
				if (this.facialFeatureList == null)
				{
					this.facialFeatureList = new List<IImageSource>();

					try
					{
						// FacialFeatureOption[0][0] -> FacialFeatureOption[7][6]
						for (int i = 0; i <= 7; i++)
						{
							for (int j = 0; j <= 6; j++)
							{
								SaintCoinach.Imaging.ImageFile imageFile = this.Value.AsImage("FacialFeatureOption", i, j);

								if (imageFile is null)
									continue;

								this.facialFeatureList.Add(imageFile.ToIImage());
							}
						}
					}
					catch (Exception ex)
					{
						Log.Write(new Exception("Failed to load facial features", ex), "Saint Coinach", Log.Severity.Log);
					}
				}

				return this.facialFeatureList;
			}
		}
	}
}
