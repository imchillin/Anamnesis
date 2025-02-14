// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Core.Extensions;

using System;
using System.Runtime.CompilerServices;

public static class EnumExtensions
{
	/// <summary>
	/// Determines whether one or more bit fields are set in the current instance.
	/// </summary>
	/// <typeparam name="TEnum">The enumeration type of the value and flag.</typeparam>
	/// <param name="value">An enumeration value.</param>
	/// <param name="flag">An enumeration value to test.</param>
	/// <returns>true if the bit field or bit fields that are set in flag are also set in the current instance; otherwise, false.</returns>
	/// <remarks>
	/// This method provides a performance-optimized alternative to <see cref="Enum.HasFlag(Enum)"/> by avoiding boxing and using unsafe code
	/// to perform bitwise operations directly on the underlying values.
	/// </remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool HasFlagUnsafe<TEnum>(this TEnum value, TEnum flag)
		where TEnum : unmanaged, Enum
	{
		unsafe
		{
			return sizeof(TEnum) switch
			{
				1 => (*(byte*)&value & *(byte*)&flag) == *(byte*)&flag,
				2 => (*(ushort*)&value & *(ushort*)&flag) == *(ushort*)&flag,
				4 => (*(uint*)&value & *(uint*)&flag) == *(uint*)&flag,
				8 => (*(ulong*)&value & *(ulong*)&flag) == *(ulong*)&flag,
				_ => throw new ArgumentException("Unsupported enum size"),
			};
		}
	}
}
