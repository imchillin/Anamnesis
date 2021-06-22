// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData
{
	using Anamnesis.TexTools;

	public interface INpcResident : IRow
	{
		public string Singular { get; }
		public string Plural { get; }
		public string Title { get; }

		public INpcBase? Appearance { get; }

		Mod? Mod { get; }
	}
}
