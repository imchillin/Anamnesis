// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System.Numerics;

public class GPoseCameraMemory : MemoryBase
{
	[Bind(0x10)] public Vector3 Position { get; set; }
}
