// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData;

using System.Collections.Generic;

public interface ISheet<T> : IEnumerable<T>
	where T : IRow
{
	bool Contains(uint key);

	T Get(uint key);
	T Get(byte key);
}
