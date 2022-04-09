// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Reflection;

	public abstract class ArrayMemory<TValue, TCount> : MemoryBase, IEnumerable<TValue>
		where TCount : struct
	{
		private readonly List<TValue> items = new();

		private int lastCount = 0;
		private IntPtr lastAddress = IntPtr.Zero;

		[Bind(nameof(CountOffset))] public TCount ArrayCount { get; set; }
		[Bind(nameof(AddressOffset))] public IntPtr ArrayAddress { get; set; }

		public virtual int CountOffset => 0x000;
		public virtual int AddressOffset => 0x008;
		public abstract int ElementSize { get; }
		public int ItemCount => this.items.Count;

		public int Count
		{
			get
			{
				// Kinda hacky, but no support for generic numbers yet!
				if (this.ArrayCount is int i)
					return i;

				if (this.ArrayCount is short s)
					return s;

				if (this.ArrayCount is long l)
					return (int)l;

				if (this.ArrayCount is ulong r)
					return (int)r;

				if (this.ArrayCount is ushort u)
					return u;

				throw new Exception($"Array count type: {typeof(TCount)} is not a number!");
			}
		}

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
			catch(Exception ex)
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
			private readonly int index;

			public ArrayBindInfo(MemoryBase memory, int index)
				: base(memory)
			{
				this.index = index;
			}

			public override string Name => this.index.ToString();
			public override string Path => $"[{this.index}]";
			public override Type Type => typeof(TValue);
			public override BindFlags Flags => BindFlags.None;

			public override IntPtr GetAddress()
			{
				throw new NotSupportedException();
			}
		}
	}
}
