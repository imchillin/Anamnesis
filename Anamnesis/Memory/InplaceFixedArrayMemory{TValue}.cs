// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Reflection;

	public abstract class InplaceFixedArrayMemory<TValue> : MemoryBase, IEnumerable<TValue>
	{
		private readonly List<TValue> items = new();

		private int lastCount = 0;
		private IntPtr lastAddress = IntPtr.Zero;

		public virtual IntPtr ArrayAddress
		{
			get => this.Address;
			set => throw new NotSupportedException();
		}

		public abstract int ElementSize { get; }
		public int ItemCount => this.items.Count;

		public abstract int Count { get; }

		public TValue this[int index]
		{
			get => this.items[index];
		}

		public IEnumerator<TValue> GetEnumerator() => this.items.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => this.items.GetEnumerator();

		public override void Tick()
		{
			try
			{
				base.Tick();
			}
			catch (Exception ex)
			{
				Log.Warning(ex, "Failed to tick array");
			}
		}

		protected void UpdateArray()
		{
			lock (this.items)
			{
				foreach (TValue item in this.items)
				{
					if (item is MemoryBase memory)
					{
						memory.Dispose();
						this.Children.Remove(memory);
					}
				}

				this.items.Clear();

				if (this.ArrayAddress == IntPtr.Zero || this.Count <= 0)
					return;

				// !!
				if (this.Count > 2000)
					return;

				try
				{
					IntPtr address = this.ArrayAddress;
					for (int i = 0; i < this.Count; i++)
					{
						if (typeof(MemoryBase).IsAssignableFrom(typeof(TValue)))
						{
							TValue instance = Activator.CreateInstance<TValue>();
							MemoryBase? memory = instance as MemoryBase;

							if (memory == null)
								throw new Exception($"Faield to create instance of type: {typeof(TValue)}");

							memory.Parent = this;
							memory.ParentBind = new ArrayBindInfo(this, i);
							memory.SetAddress(address);
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
						}

						address += this.ElementSize;
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

			if (!this.IsReading)
				return;

			// did these values actually change
			if (this.lastCount == this.Count && this.lastAddress == this.ArrayAddress)
				return;

			this.lastCount = this.Count;
			this.lastAddress = this.ArrayAddress;

			this.UpdateArray();
		}

		public class ArrayBindInfo : BindInfo
		{
			public readonly int Index;

			public ArrayBindInfo(MemoryBase memory, int index)
				: base(memory)
			{
				this.Index = index;
			}

			public override string Name => this.Index.ToString();
			public override string Path => $"[{this.Index}]";
			public override Type Type => typeof(TValue);
			public override BindFlags Flags => BindFlags.None;

			public override IntPtr GetAddress()
			{
				throw new NotSupportedException();
			}
		}
	}
}
