// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.SaintCoinachModule
{
	using Anamnesis.GameData;
	using SaintCoinach.Xiv;

	internal class TitleWrapper : ObjectWrapper<Title>, ITitle
	{
		public TitleWrapper(Title row)
			: base(row)
		{
		}
	}
}
