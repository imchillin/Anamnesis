// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.SaintCoinachModule
{
	using System.Collections.Generic;
	using Anamnesis;
	using Anamnesis.GameData;
	using Anamnesis.Memory;
	using SaintCoinach.Xiv;

	internal class RaceWrapper : ObjectWrapper<Race>, IRace
	{
		private Appearance.Races race;

		public RaceWrapper(Race row)
			: base(row)
		{
			this.race = (Appearance.Races)row.Key;
		}

		public Appearance.Races Race
		{
			get
			{
				return this.race;
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

		public IEnumerable<ITribe> Tribes
		{
			get
			{
				List<ITribe> tribes = new List<ITribe>();
				foreach (Appearance.Tribes tribe in this.Race.GetTribes())
				{
					tribes.Add(GameDataService.Instance.Tribes.Get((byte)tribe));
				}

				return tribes;
			}
		}

		public override string ToString()
		{
			return this.DisplayName;
		}
	}
}
