// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData
{
	using System.Collections.Generic;
	using System.Windows.Media;
	using Anamnesis.Memory;

	public interface ICharaMakeType : IRow
	{
		ActorCustomizeMemory.Genders Gender { get; }
		ActorCustomizeMemory.Races Race { get; }
		ActorCustomizeMemory.Tribes Tribe { get; }
		IEnumerable<ImageSource> FacialFeatures { get; }
	}
}
