// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Memory.Offsets
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	public interface IMemoryOffset
	{
		ulong[] Offsets
		{
			get;
		}

		string Name
		{
			get;
		}
	}
}
