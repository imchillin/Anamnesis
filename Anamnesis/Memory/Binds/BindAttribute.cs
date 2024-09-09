// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;
using System.Diagnostics;

/// <summary>
/// Attribute to bind properties with specific memory offsets and flags.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class BindAttribute : Attribute
{
	/// <summary>
	/// Initializes a new instance of the <see cref="BindAttribute"/> class with a single offset.
	/// </summary>
	/// <param name="offset">The memory offset.</param>
	public BindAttribute(int offset)
		: this(new[] { offset }, BindFlags.None)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="BindAttribute"/> class with an offset property.
	/// </summary>
	/// <param name="offsetProperty">The name of the property that provides the offset.</param>
	public BindAttribute(string offsetProperty)
	{
		Debug.Assert(!string.IsNullOrWhiteSpace(offsetProperty), "Offset property name cannot be null or whitespace.");

		this.OffsetPropertyName = offsetProperty;
		this.Flags = BindFlags.None;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="BindAttribute"/> class with a single offset and flags.
	/// </summary>
	/// <param name="offset">The memory offset.</param>
	/// <param name="flags">The binding flags.</param>
	public BindAttribute(int offset, BindFlags flags)
		: this(new[] { offset }, flags)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="BindAttribute"/> class with two offsets and flags.
	/// </summary>
	/// <param name="offset1">The memory offset of the first object.</param>
	/// <param name="offset2">The memory offset of the second object.</param>
	/// <param name="flags">The binding flags.</param>
	/// <remarks>
	/// When providing two offsets, the first offset will be used to find the base address and the
	/// second offset will be added to the base address to get the final address.
	/// </remarks>
	public BindAttribute(int offset1, int offset2, BindFlags flags)
		: this(new[] { offset1, offset2 }, flags)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="BindAttribute"/> class with multiple offsets and flags.
	/// </summary>
	/// <param name="offsets">The memory offsets.</param>
	/// <param name="flags">The binding flags.</param>
	/// <remarks>
	/// When providing multiple offsets, the address is calculated by sequentially navigating through
	/// memory pointers using each offset until the final address is reached.
	/// </remarks>
	public BindAttribute(int[] offsets, BindFlags flags)
	{
		Debug.Assert(offsets != null, "Offsets array cannot be null.");
		Debug.Assert(offsets.Length > 0, "Offsets array cannot be empty.");

		this.Offsets = offsets;
		this.Flags = flags;
	}

	/// <summary>
	/// Gets the memory offsets.
	/// </summary>
	public int[]? Offsets { get; }

	/// <summary>
	/// Gets the binding flags.
	/// </summary>
	public BindFlags Flags { get; }

	/// <summary>
	/// Gets the name of the property that provides the offset.
	/// </summary>
	/// <remarks>
	/// This is used to bind to a property that provides the offset at runtime.
	/// If this is not <c>null</c>, then <see cref="Offsets"/> will be ignored.
	/// </remarks>
	public string? OffsetPropertyName { get; }
}
