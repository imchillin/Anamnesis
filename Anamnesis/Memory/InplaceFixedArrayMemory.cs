// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using PropertyChanged;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;

/// <summary>
/// Represents an inplace fixed-length array in memory.
/// </summary>
/// <typeparam name="TValue">The array element type.</typeparam>
public abstract class InplaceFixedArrayMemory<TValue> : MemoryBase, IEnumerable<TValue>, IArrayMemory
{
	/// <summary>
	/// Safety bound to prevent excessive allocation from corrupt memory reads.
	/// </summary>
	/// <remarks>
	/// Values exceeding this are assumed invalid, as no game arrays are this large.
	/// </remarks>
	private const int ARRAY_LEN_UPPER_BOUND = 10000;
	private const int STACKALLOC_THRESHOLD = 256;

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

	/// <inheritdoc/>
	[DoNotNotify]
	public virtual int ElementSize => throw new NotImplementedException();

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
	public virtual void ReadArrayMemory(List<MemoryBase> locked)
	{
		int arrayLength = this.Length;

		// If invalid, dispose stale objects
		if (this.ArrayAddress == IntPtr.Zero || this.ArrayAddress.ToInt64() < 0 || arrayLength <= 0 || arrayLength > ARRAY_LEN_UPPER_BOUND)
		{
			if (this.items.Count > 0)
			{
				lock (this.items)
				{
					foreach (var item in this.items)
					{
						if (item is IDisposable disposable)
							disposable.Dispose();
					}

					this.items.Clear();
					this.Children.Clear();
				}
			}

			return;
		}

		bool structureChanged = this.lastLength != arrayLength || this.lastAddress != this.ArrayAddress;
		this.lastLength = arrayLength;
		this.lastAddress = this.ArrayAddress;

		lock (this.items)
		{
			try
			{
				IntPtr arrayAddress = this.ArrayAddress;
				int elementSize = this.ElementSize;

				if (structureChanged)
				{
					if (s_isMemoryObject)
					{
						foreach (var item in this.items)
						{
							if (item is IDisposable disposable)
								disposable.Dispose();
						}
					}

					this.items.Clear();
					this.Children.Clear();
					this.items.Capacity = arrayLength;

					if (s_isMemoryObject)
					{
						IntPtr currentAddress = arrayAddress;
						for (int i = 0; i < arrayLength; ++i)
						{
							TValue instance = Activator.CreateInstance<TValue>();
							if (instance is not MemoryBase memory)
								throw new Exception($"Failed to create instance of type: {typeof(TValue)}");

							memory.Parent = this;
							memory.ParentBind = new ArrayBindInfo(this, i);
							memory.Address = currentAddress;
							ClaimLocks(memory);
							SetIsSynchronizing(memory, true);
							this.items.Add(instance);
							this.Children.Add(memory);
							locked.Add(memory);

							currentAddress += elementSize;
						}
					}
				}

				if (arrayLength == 0)
					return;

				int totalByteSize;
				try
				{
					checked
					{
						totalByteSize = arrayLength * elementSize;
					}
				}
				catch (OverflowException)
				{
					Log.Warning($"Failed to bulk read array at address: 0x{arrayAddress:X}: Byte size overflow.");
					return;
				}

				byte[]? rentedBuffer = null;
				Span<byte> bufferSpan = totalByteSize <= STACKALLOC_THRESHOLD
					? stackalloc byte[totalByteSize]
					: (rentedBuffer = ArrayPool<byte>.Shared.Rent(totalByteSize)).AsSpan(0, totalByteSize);

				try
				{
					if (MemoryService.Read(arrayAddress, bufferSpan))
					{
						unsafe
						{
							fixed (byte* bufferPtr = bufferSpan)
							{
								if (s_isMemoryObject)
								{
									for (int i = 0; i < arrayLength; ++i)
									{
										byte* elementPtr = bufferPtr + (i * elementSize);
										if (this.items[i] is MemoryBase memory)
										{
											memory.ReadFromBuffer(locked, elementPtr, elementSize);
										}
									}
								}
								else
								{
									Type type = typeof(TValue);
									Type readType = type;

									if (type.IsEnum)
										readType = type.GetEnumUnderlyingType();
									else if (type == typeof(bool))
										readType = typeof(MemoryService.OneByteBool);

									for (int i = 0; i < arrayLength; ++i)
									{
										IntPtr elementPtr = (IntPtr)(bufferPtr + (i * elementSize));
										object? val = Marshal.PtrToStructure(elementPtr, readType);

										if (val != null)
										{
											TValue item;
											if (type.IsEnum)
												item = (TValue)Enum.ToObject(type, val);
											else if (val is MemoryService.OneByteBool obb)
												item = (TValue)(object)obb.Value;
											else
												item = (TValue)val;

											if (structureChanged)
												this.items.Add(item);
											else
												this.items[i] = item;
										}
									}
								}
							}
						}
					}
					else
					{
						Log.Warning($"Failed to bulk read array at address: 0x{arrayAddress:X}");
					}
				}
				finally
				{
					if (rentedBuffer != null)
						ArrayPool<byte>.Shared.Return(rentedBuffer);
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
	/// <remarks>
	/// Initializes a new instance of the <see cref="ArrayBindInfo"/> class.
	/// </remarks>
	/// <param name="memory">The memory object instance.</param>
	/// <param name="index">The index of the array element.</param>
	public sealed class ArrayBindInfo(MemoryBase memory, int index) : BindInfo(memory)
	{
		/// <summary>Gets the index of the array element.</summary>
		public readonly int Index = index;

		/// <inheritdoc/>
		public override string Name => this.Index.ToString();

		/// <inheritdoc/>
		public override string Path => $"[{this.Index}]";

		/// <inheritdoc/>
		public override Type Type => typeof(TValue);

		/// <inheritdoc/>
		public override string? SyncGroup => throw new NotSupportedException();

		/// <inheritdoc/>
		public override BindFlags Flags => BindFlags.None;

		private InplaceFixedArrayMemory<TValue> TypedMemory => (InplaceFixedArrayMemory<TValue>)this.Memory;

		/// <inheritdoc/>
		/// <exception cref="NotSupportedException">
		/// This method is not supported. Use <see cref="MemoryBase.Address"/> instead.
		/// </exception>
		public override IntPtr GetAddress()
		{
			return this.TypedMemory.ArrayAddress + (this.Index * this.TypedMemory.ElementSize);
		}
	}
}
