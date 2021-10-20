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

		private int lastCount = 0;
		private IntPtr lastAddress = IntPtr.Zero;

		[Bind(0x000)] public int Count { get; protected set; }
		[Bind(0x008, BindFlags.Pointer)] public IntPtr ArrayAddress { get; protected set; }

		public T this[int index]
		{
			get => this.items[index];
		}

		public IEnumerator<T> GetEnumerator() => this.items.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => this.items.GetEnumerator();

		public override void Tick()
		{
			try
			{
				base.Tick();
			}
			catch(Exception ex)
			{
				Log.Warning(ex, "Failed to tick array");
			}
		}

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

				try
				{
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
				catch (Exception ex)
				{
					Log.Warning(ex, "Failed to map array");
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

			// did these values actually change
			if (this.lastCount == this.Count && this.lastAddress == this.ArrayAddress)
				return;

			this.lastCount = this.Count;
			this.lastAddress = this.ArrayAddress;

			this.UpdateArray();
		}
	}
}
