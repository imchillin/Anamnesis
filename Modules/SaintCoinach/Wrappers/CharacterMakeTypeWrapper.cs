// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.SaintCoinachModule
{
	using System;
	using System.Collections.Generic;
	using ConceptMatrix;
	using ConceptMatrix.Services;
	using SaintCoinach.Xiv;

	internal class CharacterMakeTypeWrapper : ObjectWrapper<CharaMakeType>, ICharaMakeType
	{
		private List<FacialFeatureWrapper> facialFeatureList;

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

		public IEnumerable<IFacialFeature> FacialFeatures
		{
			get
			{
				if (this.facialFeatureList == null)
				{
					this.facialFeatureList = new List<FacialFeatureWrapper>();

					try
					{
						// this is almost the exact code as "this.Value.FacialFeatureIcon" would execute, but renamed to "FacialFeatureOption"
						// since "FacialFeatureIcon" throws an exception.
						for (int i = 0; i < 56; i++)
						{
							SaintCoinach.Imaging.ImageFile imageFile = this.Value.AsImage("FacialFeatureOption", i);

							if (imageFile is null)
								continue;

							FacialFeatureWrapper feature = new FacialFeatureWrapper();
							feature.Icon = imageFile.ToIImage();
							this.facialFeatureList.Add(feature);
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

		internal class FacialFeatureWrapper : IFacialFeature
		{
			public int Value
			{
				get;
				set;
			}

			public IImage Icon
			{
				get;
				set;
			}
		}
	}
}
