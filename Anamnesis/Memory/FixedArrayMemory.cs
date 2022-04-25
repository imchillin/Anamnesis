// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;
using System.Collections.Generic;

public abstract class FixedArrayMemory<TValue> : InplaceFixedArrayMemory<TValue>, IEnumerable<TValue>
{
	[Bind(nameof(AddressOffset))] public override IntPtr ArrayAddress { get; set; }

	public virtual int AddressOffset => 0x008;
}
