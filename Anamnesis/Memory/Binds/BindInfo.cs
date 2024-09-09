// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;
using System.Threading;

/// <summary>
/// Represents the base class for binding information.
/// </summary>
public abstract class BindInfo
{
	/// <summary>Internal read state.</summary>
	private int isReading = 0;

	/// <summary>Internal read state.</summary>
	private int isWriting = 0;

	/// <summary>
	/// Initializes a new instance of the <see cref="BindInfo"/> class.
	/// </summary>
	/// <param name="memory">The memory base instance.</param>
	/// <exception cref="ArgumentNullException">Thrown when memory is null.</exception>
	public BindInfo(MemoryBase memory)
	{
		this.Memory = memory ?? throw new ArgumentNullException(nameof(memory));
	}

	/// <summary>
	/// Gets the memory base instance.
	/// </summary>
	public MemoryBase Memory { get; }

	/// <summary>
	/// Gets the name of the bind.
	/// </summary>
	public abstract string Name { get; }

	/// <summary>
	/// Gets the path of the bind.
	/// </summary>
	public abstract string Path { get; }

	/// <summary>
	/// Gets the type of the bind.
	/// </summary>
	public abstract Type Type { get; }

	/// <summary>
	/// Gets the bind flags.
	/// </summary>
	public abstract BindFlags Flags { get; }

	/// <summary>
	/// Gets or sets the freeze value.
	/// </summary>
	public object? FreezeValue { get; set; }

	/// <summary>
	/// Gets or sets the last value.
	/// </summary>
	public object? LastValue { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the bind is being read.
	/// </summary>
	/// <remarks>
	/// This property is atomic and thread safe.
	/// </remarks>
	public bool IsReading
	{
		get => Interlocked.CompareExchange(ref this.isReading, 0, 0) == 1;
		set => Interlocked.Exchange(ref this.isReading, value ? 1 : 0);
	}

	/// <summary>
	/// Gets or sets a value indicating whether the bind is being written.
	/// </summary>
	/// <remarks>
	/// This property is atomic and thread safe.
	/// </remarks>
	public bool IsWriting
	{
		get => Interlocked.CompareExchange(ref this.isWriting, 0, 0) == 1;
		set => Interlocked.Exchange(ref this.isWriting, value ? 1 : 0);
	}

	/// <summary>
	/// Gets or sets the last failure address.
	/// </summary>
	public IntPtr? LastFailureAddress { get; set; }

	/// <summary>
	/// Gets a value indicating whether this bind is for child memory.
	/// </summary>
	public bool IsChildMemory => typeof(MemoryBase).IsAssignableFrom(this.Type);

	/// <summary>
	/// Gets the address of the bind.
	/// </summary>
	/// <returns>The address of the bind.</returns>
	public abstract IntPtr GetAddress();

	/// <summary>
	/// Returns a string that represents the current object.
	/// </summary>
	/// <returns>A string that represents the current object.</returns>
	public override string ToString() => $"Bind: {this.Name} ({this.Type})";
}
