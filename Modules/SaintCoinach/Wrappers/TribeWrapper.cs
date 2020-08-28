// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.SaintCoinachModule
{
	using Anamnesis.GameData;
	using Anamnesis.Memory;
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

		public bool Equals(ITribe other)
		{
			return this.tribe == other.Tribe;
		}

		public override string ToString()
		{
			return this.DisplayName;
		}
	}
}
