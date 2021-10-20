// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.ComponentModel;

	public class ArrayMemory<T> : MemoryBase, IEnumerable<T>
		where T : MemoryBase
	{
		private readonly List<T> items = new();

		[Bind(0x000)] public int Count { get; protected set; }
		[Bind(0x008, BindFlags.Pointer)] public IntPtr ArrayAddress { get; protected set; }

		public T this[int index]
		{
			get => this.items[index];
		}

		public IEnumerator<T> GetEnumerator() => this.items.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => this.items.GetEnumerator();

		protected void UpdateArray()
		{
			lock (this.items)
			{
				foreach (T item in this.items)
				{
					item.Dispose();
				}

				this.items.Clear();

				if (this.ArrayAddress == IntPtr.Zero || this.Count <= 0)
					return;

				// Odd hack, but this pointer becomes gibberish _alot_
				if ((int)this.ArrayAddress < 0)
					return;

				IntPtr address = this.ArrayAddress;
				for (int i = 0; i < this.Count; i++)
				{
					T instance = Activator.CreateInstance<T>();
					instance.Parent = this;
					instance.SetAddress(address);
					this.items.Add(instance);

					address += instance.Size;
				}
			}
		}

		protected override void OnSelfPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			base.OnSelfPropertyChanged(sender, e);

			if (e.PropertyName != nameof(this.Count) && e.PropertyName != nameof(this.ArrayAddress))
				return;

			if (!this.IsReading)
				throw new Exception("Array properties should only change while reading memory");

			this.UpdateArray();
		}
	}
}
