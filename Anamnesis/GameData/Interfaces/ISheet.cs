// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.GameData
{
	using System.Collections.Generic;

	public interface ISheet<T> : IEnumerable<T>
		where T : IRow
	{
		bool Contains(int key);

		T Get(int key);
		T Get(byte key);
	}
}
