// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.SaintCoinachModule
{
	using Anamnesis;
	using ConceptMatrix.GameData;
	using SaintCoinach.Xiv;

	internal class TribeWrapper : ObjectWrapper<Tribe>, ITribe
	{
		private Appearance.Tribes tribe;

		public TribeWrapper(Tribe row)
			: base(row)
		{
			this.tribe = (Appearance.Tribes)row.Key;
		}

		public Appearance.Tribes Tribe
		{
			get
			{
				return this.tribe;
			}
		}

		public string Feminine
		{
			get
			{
				return this.Value.Feminine;
			}
		}

		public string Masculine
		{
			get
			{
				return this.Value.Masculine;
			}
		}

		public string DisplayName
		{
			get
			{
				if (this.Feminine == this.Masculine)
				{
					return this.Feminine;
				}
				else
				{
					return this.Feminine + " / " + this.Masculine;
				}
			}
		}

		public override string ToString()
		{
			return this.DisplayName;
		}
	}
}
