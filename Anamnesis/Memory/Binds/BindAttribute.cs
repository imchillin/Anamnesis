// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;

[AttributeUsage(AttributeTargets.Property)]
public class BindAttribute : Attribute
{
	public readonly int[]? Offsets;
	public readonly BindFlags Flags;
	public readonly string? OffsetPropertyName;

	public BindAttribute(int offset)
	{
		this.Offsets = new[] { offset };
	}

	public BindAttribute(string offsetProperty)
	{
		this.OffsetPropertyName = offsetProperty;
	}

	public BindAttribute(int offset, BindFlags flags)
	{
		this.Offsets = new[] { offset };
		this.Flags = flags;
	}

	public BindAttribute(int offset, int offset2, BindFlags flags)
	{
		this.Offsets = new[] { offset, offset2 };
		this.Flags = flags;
	}

	public BindAttribute(int offset, int offset2, int offset3, BindFlags flags)
	{
		this.Offsets = new[] { offset, offset2, offset3 };
		this.Flags = flags;
	}
}
