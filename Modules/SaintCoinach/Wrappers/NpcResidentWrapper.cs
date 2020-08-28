// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.SaintCoinachModule
{
	using Anamnesis.GameData;
	using SaintCoinach.Xiv;

	internal class NpcResidentWrapper : ObjectWrapper<ENpcResident>, INpcResident
	{
		private INpcBase appearance;

		public NpcResidentWrapper(ENpcResident row)
			: base(row)
		{
		}

		public string Singular => this.Value.Singular;
		public string Plural => this.Value.Plural;
		public string Title => this.Value.Title;

		public INpcBase Appearance
		{
			get
			{
				if (this.appearance == null)
					this.appearance = GameDataService.Instance.BaseNPCs.Get(this.Key);

				return this.appearance;
			}
		}

		public string Name
		{
			get
			{
				if (!string.IsNullOrEmpty(this.Singular))
					return this.Singular;

				if (!string.IsNullOrEmpty(this.Plural))
					return this.Plural;

				if (!string.IsNullOrEmpty(this.Title))
					return this.Title;

				return this.Key.ToString();
			}
		}
	}
}
