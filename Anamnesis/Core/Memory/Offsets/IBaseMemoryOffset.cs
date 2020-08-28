// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory.Offsets
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	public interface IBaseMemoryOffset : IMemoryOffset
	{
		bool Equals(IBaseMemoryOffset other);
	}
}
