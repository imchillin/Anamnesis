// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Core;

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal ref struct BufferWriter(Span<byte> span)
{
	private Span<byte> span = span;

	public int Position { get; private set; } = 0;

	public void Write<T>(in T value)
		where T : unmanaged
	{
		MemoryMarshal.Write(this.span[this.Position..], in value);
		this.Position += Unsafe.SizeOf<T>();
	}

	public void WriteByte(byte value)
	{
		this.span[this.Position++] = value;
	}

	public void WriteSpan(ReadOnlySpan<byte> data)
	{
		data.CopyTo(this.span[this.Position..]);
		this.Position += data.Length;
	}
}
