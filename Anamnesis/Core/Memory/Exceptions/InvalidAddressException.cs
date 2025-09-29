// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory.Exceptions;

using System;

public class InvalidAddressException(string name) : Exception(name + " did not produce a valid memory address")
{
}
