// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Exceptions
{
	using System;

	public class InvalidAddressException : Exception
	{
		public InvalidAddressException()
			: base("The offsets did not produce a valid memory address")
		{
		}
	}
}
