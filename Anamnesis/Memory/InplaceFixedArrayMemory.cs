// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using PropertyChanged;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Represents an inplace fixed-length array in memory.
/// </summary>
/// <typeparam name="TValue">The array element type.</typeparam>
public abstract class InplaceFixedArrayMemory<TValue> : MemoryBase, IEnumerable<TValue>, IArrayMemory
{
	private static readonly bool s_isMemoryObject = typeof(MemoryBase).IsAssignableFrom(typeof(TValue));
	private readonly List<TValue> items = new();

	private int lastLength = -1;
	private IntPtr lastAddress = IntPtr.Zero;

	/// <summary>Gets or sets the address of the array in memory.</summary>
	/// <remarks>
	/// Since the array is inplace, the data's address is the same as the array.
	/// </remarks>
	public virtual IntPtr ArrayAddress
	{
		get => this.Address;
		set => this.Address = value;
	}

	/// <summary> Gets the array's length.</summary>
	public abstract int Length { get; }

	/// <summary>Gets the size of each element in the array.</summary>
	/// <note>
	/// This property is expected to be constant for the lifetime of the object.
	/// </note>
	[DoNotNotify]
	public abstract int ElementSize { get; }

	/// <summary>Gets or sets the element at the specified index.</summary>
	/// <param name="index">The index of the element.</param>
	[DoNotNotify]
	public TValue this[int index]
	{
		get
		{
			if (index < 0 || index >= this.items.Count)
				throw new ArgumentOutOfRangeException(nameof(index), "Index was out of range. Must be non-negative and less than the size of the collection.");

			return this.items[index];
		}
		set
		{
			if (index < 0 || index >= this.items.Count)
				throw new ArgumentOutOfRangeException(nameof(index), "Index was out of range. Must be non-negative and less than the size of the collection.");

			this.items[index] = value;
		}
	}

	/// <summary>
	/// Returns an enumerator that iterates through the collection.
	/// </summary>
	/// <returns>An enumerator for the item collection.</returns>
	/// <remarks>
	/// This method creates a copy of the items list to avoid concurrency issues.
	/// As a result, it might not reflect the latest state of the array.
	/// </remarks>
	public IEnumerator<TValue> GetEnumerator()
	{
		// Create a copy of the items list to avoid concurrency issues
		List<TValue> itemsCopy;
		lock (this.items)
		{
			itemsCopy = new List<TValue>(this.items);
		}

		return itemsCopy.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

	/// <inheritdoc/>
	/// <remarks>
	/// This is method intended to be called by <see cref="MemoryBase"/>.
	/// Use <see cref="MemoryBase.Synchronize"/> instead if you want to sync the array
	/// with the latest game memory state.
	/// </remarks>
	/// <note>
	/// This method does not claim the array memory object's read lock.
	/// If not called by <see cref="MemoryBase"/>, make sure to claim the read lock.
	/// </note>
	public virtual void ReadArrayMemory()
	{
		// Update only if the array count or address have changed
		if (this.lastLength == this.Length && this.lastAddress == this.ArrayAddress)
			return;

		this.lastLength = this.Length;
		this.lastAddress = this.ArrayAddress;

		// Recreate array if either address or length mismatch the last state
		lock (this.items)
		{
			try
			{
				// Remove all existing items as they're most likely invalid
				foreach (TValue item in this.items)
				{
					if (item is MemoryBase memory)
					{
						ReleaseLocksOn(memory);
						memory.Dispose();
						this.Children.Remove(memory);
					}
				}

				this.items.Clear();

				// Don't attempt to read if the address is invalid or the length is invalid
				if (this.ArrayAddress == IntPtr.Zero || this.Length <= 0)
					return;

				// Pre-allocate the list capacity to avoid dynamic resizing
				this.items.Capacity = this.Length;

				// Read array elements
				IntPtr address = this.ArrayAddress;
				for (int i = 0; i < this.Length; i++)
				{
					if (s_isMemoryObject)
					{
						TValue instance = Activator.CreateInstance<TValue>();
						if (instance is not MemoryBase memory)
							throw new Exception($"Failed to create instance of type: {typeof(TValue)}");

						memory.Parent = this;
						memory.ParentBind = new ArrayBindInfo(this, i);
						memory.Address = address;
						ClaimLocksOn(memory);
						this.Children.Add(memory);
						this.items.Add(instance);
					}
					else
					{
						object? instance = MemoryService.Read(address, typeof(TValue));
						if (instance is TValue instanceValue)
						{
							this.items.Add(instanceValue);
						}
						else
						{
							Log.Warning($"Failed to read array element at address: {address}");
						}
					}

					address += this.ElementSize;
				}
			}
			catch (Exception ex)
			{
				Log.Warning(ex, "Failed to read array object from memory");
			}
		}
	}

	/// <summary>
	/// Represents binding information for an array element.
	/// </summary>
	public sealed class ArrayBindInfo : BindInfo
	{
		/// <summary>Gets the index of the array element.</summary>
		public readonly int Index;

		/// <summary>
		/// Initializes a new instance of the <see cref="ArrayBindInfo"/> class.
		/// </summary>
		/// <param name="memory">The memory object instance.</param>
		/// <param name="index">The index of the array element.</param>
		public ArrayBindInfo(MemoryBase memory, int index)
			: base(memory)
		{
			this.Index = index;
		}

		/// <inheritdoc/>
		public override string Name => this.Index.ToString();

		/// <inheritdoc/>
		public override string Path => $"[{this.Index}]";

		/// <inheritdoc/>
		public override Type Type => typeof(TValue);

		/// <inheritdoc/>
		public override BindFlags Flags => BindFlags.None;

		/// <inheritdoc/>
		/// <exception cref="NotSupportedException">
		/// This method is not supported. Use <see cref="MemoryBase.Address"/> instead.
		/// </exception>
		public override IntPtr GetAddress()
		{
			throw new NotSupportedException();
		}
	}
}
