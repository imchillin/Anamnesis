// © Anamnesis.
// Licensed under the MIT license.

using System.Runtime.InteropServices;

namespace Anamnesis.Memory;

public class OrnamentMemory : ActorMemory
{
	[Bind(0x2370)] public new uint OrnamentId { get; set; }
	[Bind(0x2374)] public byte CustomizeGroupId { get; set; }
}
