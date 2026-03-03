// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Interop.Types;

using System;
using System.Runtime.InteropServices;

/// <summary>
/// Represents a Havok array container (hkArray/hkArrayBase).
/// </summary>
/// <remarks>
/// Based on FFXIVClientStructs' array implementation for
/// hkArray, which in turn is based on the Havok SDK's implementation.
/// </remarks>
/// <typeparam name="T">The unmanaged element type.</typeparam>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct HkArray<T>
	where T : unmanaged
{
	/// <summary>
	/// Flags used in the capacity field.
	/// </summary>
	[Flags]
	public enum ArrayFlags : uint
	{
		CapacityMask = 0x3FFFFFFF,
		FlagMask = 0xC0000000,
		DontDeallocate = 0x80000000,
		AllocatedFromSpu = 0x40000000,
	}

	/// <summary>
	/// Pointer to the array data.
	/// </summary>
	public T* Data;

	/// <summary>
	/// Number of elements in the array.
	/// </summary>
	public int Length;

	/// <summary>
	/// Capacity and flags combined.
	/// </summary>
	public int CapacityAndFlags;

	/// <summary>
	/// Gets the capacity of the array.
	/// </summary>
	public readonly int Capacity => this.CapacityAndFlags & (int)ArrayFlags.CapacityMask;

	/// <summary>
	/// Gets the flags of the array.
	/// </summary>
	public readonly ArrayFlags Flags => (ArrayFlags)((uint)this.CapacityAndFlags & (uint)ArrayFlags.FlagMask);

	/// <summary>
	/// Gets a value indicating whether the array is valid.
	/// </summary>
	public readonly bool IsValid => this.Data != null && this.Length > 0;

	/// <summary>
	/// Gets or sets the element at the specified index.
	/// </summary>
	public T this[int index]
	{
		readonly get => this.Data[index];
		set => this.Data[index] = value;
	}

	/// <summary>
	/// Gets or sets the element at the specified index.
	/// </summary>
	public T this[uint index]
	{
		readonly get => this.Data[index];
		set => this.Data[index] = value;
	}

	/// <summary>Gets a pointer to the element at the specified index.</summary>
	/// <param name="index">The index of the element.</param>
	/// <returns>A pointer to the element.</returns>
	public readonly T* GetPointer(int index) => this.Data + index;
}