// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

/// <summary>
/// Represents binding information for a property.
/// </summary>
public class PropertyBindInfo : BindInfo
{
	/// <summary>The property information.</summary>
	public readonly PropertyInfo Property;

	/// <summary>The bind attribute associated with the property.</summary>
	public readonly BindAttribute Attribute;

	/// <summary>
	/// The property that provides the offset, if any.
	/// </summary>
	/// <remarks>
	/// An offset property needs to provided if no offsets are provided in the bind attribute.
	/// </remarks>
	public readonly PropertyInfo? OffsetProperty;

	/// <summary>Lock object for offset caching.</summary>
	private readonly object offsetLock = new();

	/// <summary>Cached offsets for the property.</summary>
	private int[]? cachedOffsets;

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

		if (attribute.OffsetPropertyName != null)
		{
			Type memoryType = memory.GetType();
			this.OffsetProperty = memoryType.GetProperty(attribute.OffsetPropertyName);
		}
	}

	/// <summary>Gets the name of the bound property.</summary>
	public override string Name => this.Property.Name;

	/// <summary>Gets the path of the bound property.</summary>
	public override string Path => $".{this.Name}";

	/// <summary>Gets the type of the bound property.</summary>
	public override Type Type => this.Property.PropertyType;

	/// <summary>Gets the bind flags.</summary>
	public override BindFlags Flags => this.Attribute.Flags;

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
		int[] offsets;
		lock (this.offsetLock)
		{
			// Get offsets if they are not cached
			this.cachedOffsets ??= this.GetOffsetsInternal();
			offsets = this.cachedOffsets;
		}

		if (offsets == null || offsets.Length == 0)
			throw new NullReferenceException("Cached offsets are not initialized.");

		if (offsets.Length > 1 && !this.Flags.HasFlag(BindFlags.Pointer))
			throw new InvalidOperationException("Bind address has multiple offsets but is not a pointer. This is not supported.");

		IntPtr bindAddress = this.Memory.Address + offsets[0];

		if (typeof(MemoryBase).IsAssignableFrom(this.Type))
		{
			if (this.Flags.HasFlag(BindFlags.Pointer))
			{
				bindAddress = MemoryService.Read<IntPtr>(bindAddress);

				for (int i = 1; i < offsets.Length; i++)
				{
					bindAddress += offsets[i];
					bindAddress = MemoryService.Read<IntPtr>(bindAddress);
				}
			}
		}
		else if (this.Flags.HasFlag(BindFlags.Pointer))
		{
			bindAddress = MemoryService.Read<IntPtr>(bindAddress);
		}

		if (this.Flags.HasFlag(BindFlags.DontCacheOffsets))
		{
			lock (this.offsetLock)
			{
				this.cachedOffsets = null;
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
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int[] GetOffsetsInternal()
	{
		if (this.Attribute.Offsets != null)
			return this.Attribute.Offsets;

		if (this.OffsetProperty != null)
		{
			object? offsetValue = this.OffsetProperty.GetValue(this.Memory);

			if (offsetValue is int[] offsetInts)
				return offsetInts;

			if (offsetValue is int offset)
				return new int[] { offset };

			throw new InvalidOperationException($"Unknown offset type: {offsetValue} bind: {this}");
		}

		throw new InvalidOperationException($"No offsets for bind: {this}");
	}
}
