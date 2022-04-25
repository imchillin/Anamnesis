// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory.Exceptions;

using System;

public class InvalidAddressException : Exception
{
	public InvalidAddressException(string name)
		: base(name + " did not produce a valid memory address")
	{
	}
}
