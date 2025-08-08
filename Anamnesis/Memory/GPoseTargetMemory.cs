// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System.Numerics;

public class GPoseTargetMemory : MemoryBase
{
	[Bind(0xB0)] public Vector3 Position { get; set; }
}
