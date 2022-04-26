// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;
using System.Reflection;

public class PropertyBindInfo : BindInfo
{
	public readonly PropertyInfo Property;
	public readonly BindAttribute Attribute;
	public readonly PropertyInfo? OffsetProperty;

	private int[]? offsets;

	public PropertyBindInfo(MemoryBase memory, PropertyInfo property, BindAttribute attribute)
		: base(memory)
	{
		this.Property = property;
		this.Attribute = attribute;

		if (attribute.OffsetPropertyName != null)
		{
			Type memoryType = memory.GetType();
			this.OffsetProperty = memoryType.GetProperty(attribute.OffsetPropertyName);
		}
	}

	public override string Name => this.Property.Name;
	public override string Path => $".{this.Name}";
	public override Type Type => this.Property.PropertyType;
	public override BindFlags Flags => this.Attribute.Flags;

	public override IntPtr GetAddress()
	{
		if (this.offsets == null)
			this.offsets = this.GetOffsets();

		IntPtr bindAddress = this.Memory.Address + this.offsets[0];

		if (this.offsets.Length > 1 && !this.Flags.HasFlag(BindFlags.Pointer))
			throw new Exception("Bind address has multiple offsets but is not a pointer. This is not supported.");

		if (typeof(MemoryBase).IsAssignableFrom(this.Type))
		{
			if (this.Flags.HasFlag(BindFlags.Pointer))
			{
				bindAddress = MemoryService.Read<IntPtr>(bindAddress);

				for (int i = 1; i < this.offsets.Length; i++)
				{
					bindAddress += this.offsets[i];
					bindAddress = MemoryService.Read<IntPtr>(bindAddress);
				}
			}
		}
		else if (this.Flags.HasFlag(BindFlags.Pointer))
		{
			bindAddress = MemoryService.Read<IntPtr>(bindAddress);
		}

		if (this.Flags.HasFlag(BindFlags.DontCacheOffsets))
			this.offsets = null;

		return bindAddress;
	}

	public int[] GetOffsets()
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

			throw new Exception($"Unknown offset type: {offsetValue} bind: {this}");
		}

		throw new Exception($"No offsets for bind: {this}");
	}
}
