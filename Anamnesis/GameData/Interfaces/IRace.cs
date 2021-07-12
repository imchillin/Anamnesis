// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData
{
	using System.Collections.Generic;
	using Anamnesis.Memory;

	public interface IRace : IRow
	{
		Customize.Races Race { get; }
		string Feminine { get; }
		string Masculine { get; }
		string DisplayName { get; }

		ITribe[] Tribes { get; }
	}
}
