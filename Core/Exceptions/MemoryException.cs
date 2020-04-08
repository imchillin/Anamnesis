// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Exceptions
{
	using System;

	public class MemoryException : Exception
	{
		public MemoryException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
