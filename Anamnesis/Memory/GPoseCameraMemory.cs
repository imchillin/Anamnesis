// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

public class GPoseCameraMemory : MemoryBase
{
	[Bind(0x10)] public Vector Position { get; set; }
}
