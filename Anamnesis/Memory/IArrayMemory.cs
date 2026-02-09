// © Anamnesis.
// Licensed under the MIT license.

using PropertyChanged;
using System.Collections.Generic;

namespace Anamnesis.Memory;

/// <summary>Defines an interface for array memory operations.</summary>
public interface IArrayMemory
{
	/// <summary>
	/// Gets the size of each element in the array.
	/// </summary>
	/// <note>
	/// This property is expected to be constant for the lifetime of the object.
	/// </note>
	[DoNotNotify]
	public abstract int ElementSize { get; }

	/// <summary>Reads the array from memory.</summary>
	/// <param name="locked">A list of locked memory objects.</param>
	public abstract void ReadArrayMemory(List<MemoryBase> locked);
}
