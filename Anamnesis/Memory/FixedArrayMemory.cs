// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a fixed-length array with data stored as a pointer elsewhere in memory.
/// </summary>
/// <typeparam name="TValue">The array element type.</typeparam>
public abstract class FixedArrayMemory<TValue> : InplaceFixedArrayMemory<TValue>
{
	/// <summary>Gets or sets the address for the array's data in memory.</summary>
	[Bind(nameof(AddressOffset))] public override IntPtr ArrayAddress { get; set; }

	/// <summary>Gets the offset of the array address.</summary>
	public virtual int AddressOffset => 0x000;

	/// <inheritdoc/>
	public override void ReadArrayMemory(List<MemoryBase> locked)
	{
		if (this.Binds.TryGetValue(nameof(this.ArrayAddress), out PropertyBindInfo? arrayAddress)
			&& this.CanRead(arrayAddress))
		{
			this.ReadFromMemory(arrayAddress, locked);
		}

		base.ReadArrayMemory(locked);
	}
}
