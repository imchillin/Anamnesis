// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Memory.Exceptions
{
	using System;

	public class MemoryException : Exception
	{
		public MemoryException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public MemoryException(string message)
			: base(message)
		{
		}
	}
}
