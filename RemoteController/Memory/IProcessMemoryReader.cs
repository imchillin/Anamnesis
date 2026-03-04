// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Memory;

using System;
using System.Diagnostics.CodeAnalysis;

public interface IProcessMemoryReader
{
	/// <summary>
	/// Gets a value indicating whether the process is still alive.
	/// </summary>
	bool IsProcessAlive { get; }

	/// <summary>
	/// Reads a value of type <typeparamref name="T"/> from the specified memory address.
	/// </summary>
	/// <typeparam name="T">The type of value to read. The type must be a struct.</typeparam>
	/// <param name="address">The memory address to read the value from.</param>
	/// <returns>The value read from the specified memory address.</returns>
	/// <exception cref="Exception">Thrown if the address is invalid or the read operation fails after multiple attempts.</exception>
	T Read<T>(IntPtr address) where T : struct;

	/// <summary>
	/// Reads a value of type <typeparamref name="T"/> from the specified memory address.
	/// </summary>
	/// <typeparam name="T">The type of value to read. Must be a struct.</typeparam>
	/// <param name="address">The memory address to read the value from.</param>
	/// <returns>The value read from the specified memory address, or null if the read fails.</returns>
	/// <exception cref="Exception">Thrown if the specified memory address is invalid.</exception>
	T Read<T>(UIntPtr address) where T : struct;

	/// <summary>
	/// Reads a value of the specified type from the given memory address.
	/// </summary>
	/// <param name="address">The memory address to read the value from.</param>
	/// <param name="type">The type of value to read.</param>
	/// <returns>The value read from the specified memory address.</returns>
	/// <exception cref="Exception">Thrown if the address is invalid or the read operation fails after multiple attempts.</exception>
	[RequiresDynamicCode("Marshalling requires dynamic code")]
	object Read(IntPtr address, Type type);

	/// <summary>
	/// Reads memory from the specified address into the provided buffer.
	/// </summary>
	/// <param name="address">The address to read from.</param>
	/// <param name="buffer">The buffer to store the read data.</param>
	/// <param name="size">The size of the data to read.</param>
	/// <returns>True if the read operation was successful; otherwise, False.</returns>
	bool Read(UIntPtr address, byte[] buffer, UIntPtr size);

	/// <summary>
	/// Reads memory from the specified address into the provided buffer.
	/// </summary>
	/// <param name="address">The address to read from.</param>
	/// <param name="buffer">The buffer to store the read data.</param>
	/// <param name="size">The size of the data to read. If less than or equal to 0, the buffer length is used.</param>
	/// <returns>True if the read operation was successful; otherwise, False.</returns>
	bool Read(IntPtr address, byte[] buffer, int size = -1);

	/// <summary>
	/// Reads memory from the specified address into the provided span buffer.
	/// </summary>
	/// <param name="address">The address to read from.</param>
	/// <param name="buffer">The span buffer to store the read data.</param>
	/// <returns>True if the read operation was successful; otherwise, False.</returns>
	bool Read(IntPtr address, Span<byte> buffer);

	/// <summary>
	/// Reads a pointer from the specified memory address.
	/// </summary>
	/// <param name="address">The memory address to read the pointer from.</param>
	/// <returns>The pointer read from the specified memory address.</returns>
	IntPtr ReadPtr(IntPtr address);

	/// <summary>
	/// Reads a byte from the specified memory address with an optional offset.
	/// </summary>
	/// <param name="baseAddress">The base address to read from.</param>
	/// <param name="offset">The offset from the base address.</param>
	/// <returns>The byte value read from the specified address.</returns>
	byte ReadByte(nint address, int offset = 0);

	/// <summary>
	/// Reads a 16-bit integer from the specified memory address with an optional offset.
	/// </summary>
	/// <param name="baseAddress">The base address to read from.</param>
	/// <param name="offset">The offset from the base address.</param>
	/// <returns>The 16-bit integer value read from the specified address.</returns>
	short ReadInt16(nint address, int offset = 0);

	/// <summary>
	/// Reads a 32-bit integer from the specified memory address with an optional offset.
	/// </summary>
	/// <param name="baseAddress">The base address to read from.</param>
	/// <param name="offset">The offset from the base address.</param>
	/// <returns>The 32-bit integer value read from the specified address.</returns>
	int ReadInt32(nint address, int offset = 0);

	/// <summary>
	/// Reads a 64-bit integer from the specified memory address with an optional offset.
	/// </summary>
	/// <param name="baseAddress">The base address to read from.</param>
	/// <param name="offset">The offset from the base address.</param>
	/// <returns>The 64-bit integer value read from the specified address.</returns>
	long ReadInt64(nint address, int offset = 0);

	/// <summary>
	/// Writes a byte array to a specified memory address.
	/// </summary>
	/// <param name="address">The memory address to write to.</param>
	/// <param name="buffer">The byte array to write.</param>
	/// <returns>True if the write operation was successful, otherwise False.</returns>
	bool Write(IntPtr address, byte[] buffer);

	/// <summary>
	/// Writes a span buffer to a specified memory address.
	/// </summary>
	/// <param name="address">The memory address to write to.</param>
	/// <param name="buffer">The span buffer to write.</param>
	/// <returns>True if the write operation was successful, otherwise False.</returns>
	bool Write(IntPtr address, Span<byte> buffer);

	/// <summary>
	/// Writes a value of a specified type to a given memory address.
	/// </summary>
	/// <typeparam name="T">The type of the value to write. Must be a struct.</typeparam>
	/// <param name="address">The memory address to write to.</param>
	/// <param name="value">The value to write.</param>
	/// <returns>True if the write operation was successful, otherwise False.</returns>
	bool Write<T>(IntPtr address, T value) where T : struct;

	/// <summary>
	/// Writes an object value to a given memory address.
	/// </summary>
	/// <param name="address">The memory address to write to.</param>
	/// <param name="value">The object value to write.</param>
	/// <returns>True if the write operation was successful, otherwise False.</returns>
	[RequiresDynamicCode("Marshalling requires dynamic code")]
	bool Write(IntPtr address, object value);

	/// <summary>
	/// Writes an object value of a specified type to a given memory address.
	/// </summary>
	/// <param name="address">The memory address to write to.</param>
	/// <param name="value">The object value to write.</param>
	/// <param name="type">The type of the value to write.</param>
	/// <returns>True if the write operation was successful, otherwise False.</returns>
	[RequiresDynamicCode("Marshalling requires dynamic code")]
	bool Write(IntPtr address, object value, Type type);
}
