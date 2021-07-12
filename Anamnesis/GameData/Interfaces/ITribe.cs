// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData
{
	using System;
	using Anamnesis.Memory;

	public interface ITribe : IRow, IEquatable<ITribe>
	{
		Customize.Tribes Tribe { get; }
		string Feminine { get; }
		string Masculine { get; }
		string DisplayName { get; }
	}
}
