// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GameData
{
	public interface INpcResident : IDataObject
	{
		public string Singular { get; }
		public string Plural { get; }
		public string Title { get; }

		public INpcBase Appearance { get; }

		public string Name { get; }
	}
}
