// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using Anamnesis.Core.Extensions;
using System;
using System.Diagnostics;
using System.Reflection;

/// <summary>
/// Represents binding information for a property.
/// </summary>
public class PropertyBindInfo : BindInfo
{
	/// <summary>The property information.</summary>
	public readonly PropertyInfo Property;

	/// <summary>The bind attribute associated with the property.</summary>
	public readonly BindAttribute Attribute;

	/// <summary>Cached bind flags.</summary>
	private readonly BindFlags flags;

	/// <summary>Cached offsets for the property.</summary>
	private readonly int[] cachedOffsets;

	/// <summary>
	/// Initializes a new instance of the <see cref="PropertyBindInfo"/> class.
	/// </summary>
	/// <param name="memory">The memory base instance.</param>
	/// <param name="property">The property information.</param>
	/// <param name="attribute">The bind attribute associated with the property.</param>
	public PropertyBindInfo(MemoryBase memory, PropertyInfo property, BindAttribute attribute)
		: base(memory)
	{
		this.Property = property;
		this.Attribute = attribute;

		Debug.Assert(this.Property != null, "Property is null");
		Debug.Assert(this.Attribute != null, "Attribute is null");

		this.cachedOffsets = this.GetOffsetsInternal();

		this.flags = this.Attribute.Flags;

		if (this.cachedOffsets == null || this.cachedOffsets.Length == 0)
			throw new NullReferenceException("Cached offsets are not initialized.");

		if (this.cachedOffsets.Length > 1 && !this.flags.HasFlagUnsafe(BindFlags.Pointer))
			throw new InvalidOperationException("Bind address has multiple offsets but is not a pointer. This is not supported.");
	}

	/// <summary>Gets the name of the bound property.</summary>
	public override string Name => this.Property.Name;

	/// <summary>Gets the path of the bound property.</summary>
	public override string Path => $".{this.Name}";

	/// <summary>Gets the type of the bound property.</summary>
	public override Type Type => this.Property.PropertyType;

	/// <summary>Gets the bind flags.</summary>
	public override BindFlags Flags => this.flags;

	/// <summary>
	/// Gets the address of the bind.
	/// </summary>
	/// <returns>The address of the bind.</returns>
	/// <exception cref="InvalidOperationException">
	/// Thrown when multiple offsets are provided to the bind attribute but
	/// the bind not a pointer.
	/// </exception>
	/// <exception cref="NullReferenceException">
	/// Thrown when the method fails to (cached) retrieve offsets.
	/// </exception>
	public override IntPtr GetAddress()
	{
		IntPtr bindAddress = this.Memory.Address + this.cachedOffsets[0];

		if (this.flags.HasFlagUnsafe(BindFlags.Pointer))
		{
			bindAddress = MemoryService.Read<IntPtr>(bindAddress);

			for (int i = 1; i < this.cachedOffsets.Length; i++)
			{
				bindAddress += this.cachedOffsets[i];
				bindAddress = MemoryService.Read<IntPtr>(bindAddress);
			}
		}

		return bindAddress;
	}

	/// <summary>
	/// Gets the offsets for the property.
	/// </summary>
	/// <returns>An array of offsets.</returns>
	/// <exception cref="InvalidOperationException">
	/// Thrown when the offset type is unknown or when no offset(s)
	/// and offset property are provided.
	/// </exception>
	private int[] GetOffsetsInternal()
	{
		if (this.Attribute.Offsets != null)
			return this.Attribute.Offsets;

		if (this.Attribute.OffsetPropertyName != null)
		{
			Type memoryType = this.Memory.GetType();
			PropertyInfo? offsetProperty = memoryType.GetProperty(this.Attribute.OffsetPropertyName);
			if (offsetProperty != null)
			{
				object? offsetValue = offsetProperty.GetValue(this.Memory);

				if (offsetValue is int[] offsetInts)
					return offsetInts;

				if (offsetValue is int offset)
					return new int[] { offset };

				throw new InvalidOperationException($"Unknown offset type: {offsetValue} bind: {this}");
			}
		}

		throw new InvalidOperationException($"No offsets for bind: {this}");
	}
}
