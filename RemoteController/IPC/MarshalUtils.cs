// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.IPC;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

/// <summary>
/// Utility methods for marshaling data between managed and unmanaged memory.
/// </summary>
[RequiresUnreferencedCode("MarshalUtils uses reflection for certain operations.")]
[RequiresDynamicCode("MarshalUtils uses Marshal.SizeOf which requires dynamic code.")]
public static class MarshalUtils
{
	[RequiresDynamicCode("MarshalUtils uses Marshal.SizeOf which requires dynamic code.")]
	private static class DelegateCache<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] TDelegate>
		where TDelegate : Delegate
	{
		public static readonly ParameterInfo[] Parameters = typeof(TDelegate).GetMethod("Invoke")!.GetParameters();
	}

	/// <summary>
	/// Writes an unmanaged value to a span.
	/// </summary>
	/// <typeparam name="T">The unmanaged type.</typeparam>
	/// <param name="destination">The destination span.</param>
	/// <param name="value">The value to write.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Write<T>(Span<byte> destination, in T value)
		where T : unmanaged
	{
		MemoryMarshal.Write(destination, in value);
	}

	/// <summary>
	/// Reads an unmanaged value from a span.
	/// </summary>
	/// <typeparam name="T">The unmanaged type.</typeparam>
	/// <param name="source">The source span.</param>
	/// <returns>The value read from the span.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T Read<T>(ReadOnlySpan<byte> source)
		where T : unmanaged
	{
		return MemoryMarshal.Read<T>(source);
	}

	/// <summary>
	/// Serializes an unmanaged value to a byte array.
	/// </summary>
	/// <typeparam name="T">The unmanaged type.</typeparam>
	/// <param name="value">The value to serialize.</param>
	/// <returns>A byte array containing the serialized value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static byte[] Serialize<T>(T value)
		where T : unmanaged
	{
		byte[] bytes = new byte[Unsafe.SizeOf<T>()];
		MemoryMarshal.Write(bytes, in value);
		return bytes;
	}

	/// <summary>
	/// Reads an unmanaged value from a span.
	/// </summary>
	/// <typeparam name="T">The unmanaged type.</typeparam>
	/// <param name="data">The source span.</param>
	/// <returns>The deserialized value, or default(T) if the span is too small.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T Deserialize<T>(ReadOnlySpan<byte> data)
		where T : unmanaged
	{
		return data.Length >= Unsafe.SizeOf<T>() ? MemoryMarshal.Read<T>(data) : default;
	}

	/// <summary>
	/// Serializes arguments into a target span.
	/// </summary>
	/// <param name="destination">
	/// The destination span to write serialized data into.
	/// </param>
	/// <param name="args">
	/// An array of objects to serialize.
	/// </param>
	/// <returns>
	/// The total number of bytes written to the destination span.
	/// </returns>
	public static int SerializeArgs(Span<byte> destination, object?[] args)
	{
		if (args.Length == 0)
			return 0;

		int offset = 0;
		for (int i = 0; i < args.Length; i++)
		{
			object? arg = args[i];
			if (arg != null)
			{
				offset += WriteToSpan(destination[offset..], arg);
			}
		}

		return offset;
	}

	/// <summary>
	/// Deserializes arguments from a payload span into an object array.
	/// </summary>
	/// <param name="payload">
	/// The payload span containing serialized data.
	/// </param>
	/// <returns>
	/// An array of deserialized objects.
	/// </returns>
	public static object?[] DeserializeArgs<TDelegate>(ReadOnlySpan<byte> payload)
		where TDelegate : Delegate
	{
		ParameterInfo[] parameters = DelegateCache<TDelegate>.Parameters;
		object?[] args = new object?[parameters.Length];

		int offset = 0;
		for (int i = 0; i < parameters.Length; i++)
		{
			args[i] = ReadFromSpan(payload[offset..], parameters[i].ParameterType, out int bytesRead);
			offset += bytesRead;
		}

		return args;
	}

	/// <summary>
	/// Deserializes arguments and a return value from a after hook detour payload span.s
	/// </summary>
	/// <typeparam name="TDelegate">Matching delegate type of the hook detour.</typeparam>
	/// <param name="payload">The serialized payload span.</param>
	/// <param name="returnType">The expected return type by the delegate.</param>
	/// <returns>
	/// A tuple containing the deserialized arguments and return value.
	/// </returns>
	public static (object?[] args, object? result) DeserializeAfterPayload<TDelegate>(ReadOnlySpan<byte> payload, Type returnType)
		where TDelegate : Delegate
	{
		int argsLength = MemoryMarshal.Read<int>(payload);
		ReadOnlySpan<byte> argsData = payload.Slice(sizeof(int), argsLength);
		ReadOnlySpan<byte> resultData = payload[(sizeof(int) + argsLength)..];

		object?[] args = DeserializeArgs<TDelegate>(argsData);
		object? result = returnType != typeof(void) ? DeserializeBoxed(resultData, returnType) : null;
		return (args, result);
	}

	/// <summary>
	/// Calculates the total byte size required to serialize the object array.
	/// </summary>
	/// <param name="args">
	/// An array of objects to compute the size for.
	/// </param>
	public static int ComputeArgsSize(object?[] args)
	{
		if (args.Length == 0)
			return 0;

		int size = 0;
		for (int i = 0; i < args.Length; ++i)
		{
			object? arg = args[i];
			if (arg != null)
				size += GetSerializedSize(arg);
		}
		return size;
	}

	/// <summary>
	/// Serializes a boxed value to a span.
	/// </summary>
	/// <param name="result">The value to serialize.</param>
	/// <param name="destination">The destination span.</param>
	/// <returns>The number of bytes written.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int SerializeBoxed(object? result, Span<byte> destination)
	{
		return result == null ? 0 : WriteToSpan(destination, result);
	}

	/// <summary>
	/// Deserializes a boxed value from a byte array.
	/// </summary>
	/// <param name="data">The source byte array.</param>
	/// <param name="returnType">The type to deserialize.</param>
	/// <returns>The deserialized value, or null.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static object? DeserializeBoxed(ReadOnlySpan<byte> data, Type returnType)
	{
		return data.Length == 0 || returnType == typeof(void) ? null : ReadFromSpan(data, returnType, out _);
	}

	/// <summary>
	/// Gets the serialized size of an object.
	/// </summary>
	/// <param name="arg">
	/// The object to compute the size for.
	/// </param>
	/// <returns>
	/// The size in bytes required to serialize the object.
	/// </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetSerializedSize(object? arg)
	{
		if (arg == null)
			return 0;

		return arg switch
		{
			bool or byte or sbyte => 1,
			short or ushort => 2,
			int or uint or float => 4,
			long or ulong or double => 8,
			nint or nuint => IntPtr.Size,
			_ => Marshal.SizeOf(arg),
		};
	}

	/// <summary>
	/// Writes an object to a byte span.
	/// </summary>
	/// <param name="dest">The destination byte span.</param>
	/// <param name="arg"><The object to serialize.</param>
	/// <returns>
	/// The number of bytes written to the span.
	/// </returns>
	public static int WriteToSpan(Span<byte> dest, object arg) => arg switch
	{
		bool b => WriteBool(dest, b),
		byte by => WriteByte(dest, by),
		sbyte sb => WriteSByte(dest, sb),
		short s => WritePrimitive(dest, s),
		ushort us => WritePrimitive(dest, us),
		int i => WritePrimitive(dest, i),
		uint ui => WritePrimitive(dest, ui),
		long l => WritePrimitive(dest, l),
		ulong ul => WritePrimitive(dest, ul),
		float f => WritePrimitive(dest, f),
		double d => WritePrimitive(dest, d),
		nint ptr => WritePrimitive(dest, ptr),
		nuint uptr => WritePrimitive(dest, uptr),
		_ => WriteStruct(dest, arg),
	};

	/// <summary>
	/// Reads an object of the specified type from a byte span.
	/// </summary>
	/// <param name="data">The source byte span.</param>
	/// <param name="type">The type to interpret the data as.</param>
	/// <param name="bytesRead">The number of bytes read from the span.</param>
	/// <returns>
	/// The deserialized object, or null if the type is unsupported or data is insufficient.
	/// </returns>
	public static object? ReadFromSpan(ReadOnlySpan<byte> data, Type type, out int bytesRead)
	{
		if (type == typeof(bool)) return ReadBool(data, out bytesRead);
		if (type == typeof(byte)) return ReadByte(data, out bytesRead);
		if (type == typeof(sbyte)) return ReadSByte(data, out bytesRead);
		if (type == typeof(short)) return ReadPrimitive<short>(data, out bytesRead);
		if (type == typeof(ushort)) return ReadPrimitive<ushort>(data, out bytesRead);
		if (type == typeof(int)) return ReadPrimitive<int>(data, out bytesRead);
		if (type == typeof(uint)) return ReadPrimitive<uint>(data, out bytesRead);
		if (type == typeof(long)) return ReadPrimitive<long>(data, out bytesRead);
		if (type == typeof(ulong)) return ReadPrimitive<ulong>(data, out bytesRead);
		if (type == typeof(float)) return ReadPrimitive<float>(data, out bytesRead);
		if (type == typeof(double)) return ReadPrimitive<double>(data, out bytesRead);
		if (type == typeof(nint)) return ReadPrimitive<nint>(data, out bytesRead);
		if (type == typeof(nuint)) return ReadPrimitive<nuint>(data, out bytesRead);

		// Handle structs and other complex types
		bytesRead = Marshal.SizeOf(type);
		if (data.Length < bytesRead)
			return null;

		nint ptr = Marshal.AllocHGlobal(bytesRead);
		try
		{
			unsafe
			{
				fixed (byte* pData = data)
					Buffer.MemoryCopy(pData, (void*)ptr, bytesRead, bytesRead);
			}
			return Marshal.PtrToStructure(ptr, type);
		}
		finally
		{
			Marshal.FreeHGlobal(ptr);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int WriteBool(Span<byte> dest, bool value)
	{
		dest[0] = value ? (byte)1 : (byte)0;
		return 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int WriteByte(Span<byte> dest, byte value)
	{
		dest[0] = value;
		return 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int WriteSByte(Span<byte> dest, sbyte value)
	{
		dest[0] = (byte)value;
		return 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int WritePrimitive<T>(Span<byte> dest, T value)
		where T : unmanaged
	{
		MemoryMarshal.Write(dest, in value);
		return Unsafe.SizeOf<T>();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int WriteStruct(Span<byte> dest, object value)
	{
		int size = Marshal.SizeOf(value);
		nint ptr = Marshal.AllocHGlobal(size);
		try
		{
			Marshal.StructureToPtr(value, ptr, false);
			unsafe
			{
				new ReadOnlySpan<byte>((void*)ptr, size).CopyTo(dest);
			}
			return size;
		}
		finally
		{
			Marshal.FreeHGlobal(ptr);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool ReadBool(ReadOnlySpan<byte> data, out int bytesRead)
	{
		bytesRead = 1;
		return data[0] != 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static byte ReadByte(ReadOnlySpan<byte> data, out int bytesRead)
	{
		bytesRead = 1;
		return data[0];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static sbyte ReadSByte(ReadOnlySpan<byte> data, out int bytesRead)
	{
		bytesRead = 1;
		return (sbyte)data[0];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static T ReadPrimitive<T>(ReadOnlySpan<byte> data, out int bytesRead)
		where T : unmanaged
	{
		bytesRead = Unsafe.SizeOf<T>();
		return MemoryMarshal.Read<T>(data);
	}
}
