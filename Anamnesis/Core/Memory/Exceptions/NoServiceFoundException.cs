// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory.Exceptions;

using System;

public class ServiceNotFoundException(Type service)
	: Exception($"No service found: {service}")
{
}
