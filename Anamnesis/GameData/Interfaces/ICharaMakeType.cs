// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData
{
	using System.Collections.Generic;
	using System.Windows.Media;
	using Anamnesis.Memory;

	public interface ICharaMakeType : IRow
	{
		Customize.Genders Gender { get; }
		Customize.Races Race { get; }
		Customize.Tribes Tribe { get; }
		IEnumerable<ImageSource> FacialFeatures { get; }
	}
}
