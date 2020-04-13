// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.SaintCoinachModule
{
	using ConceptMatrix.GameData;
	using SaintCoinach.Xiv;

	internal class TitleWrapper : ObjectWrapper<Title>, ITitle
	{
		public TitleWrapper(Title row)
			: base(row)
		{
		}
	}
}
