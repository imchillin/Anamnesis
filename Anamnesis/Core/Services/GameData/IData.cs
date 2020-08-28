// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GameData
{
	using System.Collections.Generic;

	public interface IData<T>
		where T : IDataObject
	{
		IEnumerable<T> All { get; }

		T Get(int key);
		T Get(byte key);
	}
}
