// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System.Numerics;

public class BustMemory : MemoryBase
{
	[Bind(0x068)] public Vector3 Scale { get; set; }
}
