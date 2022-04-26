// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;

public abstract class BindInfo
{
	public readonly MemoryBase Memory;

	public BindInfo(MemoryBase memory)
	{
		this.Memory = memory;
	}

	public abstract string Name { get; }
	public abstract string Path { get; }
	public abstract Type Type { get; }
	public abstract BindFlags Flags { get; }

	public object? FreezeValue { get; set; }
	public object? LastValue { get; set; }
	public bool IsReading { get; set; } = false;
	public bool IsWriting { get; set; } = false;
	public IntPtr? LastFailureAddress { get; set; }

	public bool IsChildMemory => typeof(MemoryBase).IsAssignableFrom(this.Type);

	public abstract IntPtr GetAddress();

	public override string ToString()
	{
		return $"Bind: {this.Name} ({this.Type})";
	}
}
