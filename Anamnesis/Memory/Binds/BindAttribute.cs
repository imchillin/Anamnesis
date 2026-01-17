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
		: this([offset], BindFlags.None)
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
		: this([offset], flags)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="BindAttribute"/> class with two offsets and flags.
	/// </summary>
	/// <param name="offset1">The memory offset of the first object.</param>
	/// <param name="offset2">The memory offset of the second object.</param>
	/// <param name="flags">The binding flags.</param>
	/// <remarks>
	/// Address resolution with two offsets depends on the <see cref="BindFlags.Pointer"/> flag:
	/// <list type="bullet">
	/// <item>
	/// <description>
	/// <b>Without <see cref="BindFlags.Pointer"/>:</b>
	/// The first offset is added to the base address and dereferenced. The second offset is added to the
	/// dereferenced address as a literal.
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// <b>With <see cref="BindFlags.Pointer"/>:</b>
	/// The first offset is added to the base address and dereferenced. The second offset is added to the
	/// dereferenced address, and the result is dereferenced again.
	/// </description>
	/// </item>
	/// </list>
	/// For multiple offsets, each offset except the last is added to the address and dereferenced in
	/// sequence. The last offset is added, and if <see cref="BindFlags.Pointer"/> is set, the final
	/// address is also dereferenced.
	/// </remarks>
	public BindAttribute(int offset1, int offset2, BindFlags flags = BindFlags.None)
		: this([offset1, offset2], flags)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="BindAttribute"/> class with multiple offsets and flags.
	/// </summary>
	/// <param name="offsets">The memory offsets.</param>
	/// <param name="flags">The binding flags.</param>
	/// <remarks>
	/// For multiple offsets, each offset except the last is added to the address and dereferenced in
	/// sequence. The last offset is added, and if <see cref="BindFlags.Pointer"/> is set, the final
	/// address is also dereferenced.
	/// </remarks>
	public BindAttribute(int[] offsets, BindFlags flags = BindFlags.None)
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

	/// <summary>
	/// Gets the name of the synchronization group associated with this bind.
	/// </summary>
	/// <remarks>
	/// The idea is to use synchronization groups to filter which binds to
	/// synchronize based on context.
	/// </remarks>
	public string? SyncGroup { get; set; }
}
