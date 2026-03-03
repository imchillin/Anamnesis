// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

public class OrnamentMemory : ActorMemory
{
	[Bind(0x2384)] public byte AttachmentPoint { get; set; }
}
