// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Memory;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

/// <summary>
/// An memory reader for direct read/write into the process' memory space.
/// </summary>
public sealed unsafe class InProcessMemoryReader : IProcessMemoryReader
{
	/// <inheritdoc/>
	public bool IsProcessAlive => true; // Always alive since the controller is running in the same process.

	/// <inheritdoc/>
	public T Read<T>(IntPtr address) where T : struct
	{
		return Unsafe.Read<T>((void*)address);
	}

	/// <inheritdoc/>
	public T Read<T>(UIntPtr address) where T : struct
	{
		return Unsafe.Read<T>((void*)address);
	}

	/// <inheritdoc/>
	[RequiresDynamicCode("Marshalling requires dynamic code")]
	public object Read(IntPtr address, Type type)
	{
		return Marshal.PtrToStructure(address, type)!;
	}

	/// <inheritdoc/>
	public bool Read(UIntPtr address, byte[] buffer, UIntPtr size)
	{
		new Span<byte>((void*)address, (int)size).CopyTo(buffer);
		return true;
	}

	/// <inheritdoc/>
	public bool Read(IntPtr address, byte[] buffer, int size = -1)
	{
		if (size <= 0) size = buffer.Length;
		new Span<byte>((void*)address, size).CopyTo(buffer);
		return true;
	}

	/// <inheritdoc/>
	public bool Read(IntPtr address, Span<byte> buffer)
	{
		new Span<byte>((void*)address, buffer.Length).CopyTo(buffer);
		return true;
	}

	/// <inheritdoc/>
	public IntPtr ReadPtr(IntPtr address) => *(IntPtr*)address;

	/// <inheritdoc/>
	public byte ReadByte(nint address, int offset = 0) => *(byte*)(address + offset);

	/// <inheritdoc/>
	public short ReadInt16(nint address, int offset = 0) => *(short*)(address + offset);

	/// <inheritdoc/>
	public int ReadInt32(nint address, int offset = 0) => *(int*)(address + offset);

	/// <inheritdoc/>
	public long ReadInt64(nint address, int offset = 0) => *(long*)(address + offset);

	/// <inheritdoc/>
	public bool Write(IntPtr address, byte[] buffer)
	{
		buffer.CopyTo(new Span<byte>((void*)address, buffer.Length));
		return true;
	}

	/// <inheritdoc/>
	public bool Write(IntPtr address, Span<byte> buffer)
	{
		buffer.CopyTo(new Span<byte>((void*)address, buffer.Length));
		return true;
	}

	/// <inheritdoc/>
	public bool Write<T>(IntPtr address, T value) where T : struct
	{
		Unsafe.Write((void*)address, value);
		return true;
	}

	/// <inheritdoc/>
	[RequiresDynamicCode("Marshalling requires dynamic code")]
	public bool Write(IntPtr address, object value)
	{
		Marshal.StructureToPtr(value, address, false);
		return true;
	}

	/// <inheritdoc/>
	[RequiresDynamicCode("Marshalling requires dynamic code")]
	public bool Write(IntPtr address, object value, Type type)
	{
		Marshal.StructureToPtr(value, address, false);
		return true;
	}
}
