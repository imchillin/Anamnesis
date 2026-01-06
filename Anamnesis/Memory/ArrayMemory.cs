// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents an array with a length and data stored as a pointer elsewhere in memory.
/// </summary>
/// <typeparam name="TValue">The array element type.</typeparam>
/// <typeparam name="TCount">The array length type.</typeparam>
public abstract class ArrayMemory<TValue, TCount> : FixedArrayMemory<TValue>
	where TCount : struct
{
	/// <summary>Gets or sets the address for the array's length in memory.</summary>
	[Bind(nameof(LengthOffset))] public TCount ArrayLength { get; set; }

	/// <summary>Gets the offset of the array length.</summary>
	public virtual int LengthOffset => 0x008;

	/// <inheritdoc/>
	public override int Length => Convert.ToInt32(this.ArrayLength);
}
