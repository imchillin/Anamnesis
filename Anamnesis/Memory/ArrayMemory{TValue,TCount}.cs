// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;
using System.Collections.Generic;

public abstract class ArrayMemory<TValue, TCount> : FixedArrayMemory<TValue>, IEnumerable<TValue>
	where TCount : struct
{
	[Bind(nameof(CountOffset))] public TCount ArrayCount { get; set; }

	public virtual int CountOffset => 0x000;

	public override int Count
	{
		get
		{
			// Kinda hacky, but no support for generic numbers yet!
			if (this.ArrayCount is int i)
				return i;

			if (this.ArrayCount is short s)
				return s;

			if (this.ArrayCount is long l)
				return (int)l;

			if (this.ArrayCount is ulong r)
				return (int)r;

			if (this.ArrayCount is ushort u)
				return u;

			throw new Exception($"Array count type: {typeof(TCount)} is not a number!");
		}
	}
}
