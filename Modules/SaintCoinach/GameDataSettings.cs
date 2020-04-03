// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.SaintCoinachModule
{
	using System;
	using ConceptMatrix.Services;

	[Serializable]
	public class GameDataSettings : SettingsBase
	{
		public string InstallationPath { get; set; }
	}
}
