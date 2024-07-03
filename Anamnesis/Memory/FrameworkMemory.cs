// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

public class FrameworkMemory : MemoryBase
{
	[Bind(0x1778)]
	public long EorzeaTime { get; set; }

	[Bind(0x17A0)]
	public long OverrideEorzeaTime { get; set; }

	[Bind(0x17A8)]
	public bool IsTimeOverridden { get; set; }
}
