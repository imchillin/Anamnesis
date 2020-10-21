// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GameData
{
	using System.Collections.Generic;
	using System.Windows.Media;
	using Anamnesis.Memory;

	public interface ICharaMakeType : IDataObject
	{
		Appearance.Genders Gender { get; }
		Appearance.Races Race { get; }
		Appearance.Tribes Tribe { get; }
		IEnumerable<ImageSource> FacialFeatures { get; }
	}
}
