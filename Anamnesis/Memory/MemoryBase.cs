// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Reflection;
	using Anamnesis.Services;
	using Serilog;

	public abstract class MemoryBase : INotifyPropertyChanged, IDisposable
	{
		public IntPtr Address = IntPtr.Zero;

		public bool EnableReading = true;
		public bool EnableWriting = true;

		protected readonly List<MemoryBase> Children = new List<MemoryBase>();
		private readonly Dictionary<string, BindInfo> binds = new Dictionary<string, BindInfo>();

		public MemoryBase()
		{
			PropertyInfo[]? properties = this.GetType().GetProperties();

			List<BindInfo> binds = new List<BindInfo>();
			foreach (PropertyInfo property in properties)
			{
				BindAttribute? attribute = property.GetCustomAttribute<BindAttribute>();

				if (attribute == null)
					continue;

				this.binds.Add(property.Name, new BindInfo(this, property, attribute));
			}

			this.PropertyChanged += this.OnSelfPropertyChanged;
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		public enum BindFlags
		{
			None = 0,
			Pointer = 1,
			ActorRefresh = 2,
		}

		public MemoryBase? Parent { get; set; }
		public bool IsReading { get; private set; }
		public bool IsWriting { get; private set; }

		public virtual int Size { get => throw new NotSupportedException(); }

		protected static ILogger Log => Serilog.Log.ForContext<MemoryBase>();

		public virtual void SetAddress(IntPtr address)
		{
			this.Address = address;

			try
			{
				this.ReadAllFromMemory();
			}
			catch (Exception ex)
			{
				throw new Exception($"Failed to read {this.GetType().Name} from memory address: {address}", ex);
			}
		}

		public void Dispose()
		{
			if (this.Parent != null)
				this.Parent.Children.Remove(this);

			this.Address = IntPtr.Zero;
			this.Parent = null;

			for (int i = this.Children.Count - 1; i >= 0; i--)
			{
				this.Children[i].Dispose();
			}

			this.Children.Clear();
		}

		public virtual void Tick()
		{
			if (this.Address == IntPtr.Zero)
				return;

			this.ReadAllFromMemory();

			foreach (MemoryBase child in this.Children)
			{
				child.Tick();
			}
		}

		public IntPtr GetAddressOfProperty(string propertyName)
		{
			BindInfo? bind;
			if (!this.binds.TryGetValue(propertyName, out bind))
				throw new Exception("Attempt to get address of property that is not a bind");

			return bind.GetAddress(this.Address);
		}

		protected bool IsFrozen(string propertyName)
		{
			BindInfo? bind;
			if (!this.binds.TryGetValue(propertyName, out bind))
				throw new Exception("Attempt to freeze value that is not a bind");

			return bind.FreezeValue != null;
		}

		protected void SetFrozen(string propertyName, bool freeze, object? value = null)
		{
			BindInfo? bind;
			if (!this.binds.TryGetValue(propertyName, out bind))
				throw new Exception("Attempt to freeze value that is not a bind");

			if (bind.IsChildMemory)
				throw new NotSupportedException("Attempt to freeze child memory");

			if (freeze)
			{
				if (value == null)
					value = bind.Property.GetValue(this);

				bind.FreezeValue = value;
				bind.Property.SetValue(this, value);
			}
			else
			{
				bind.FreezeValue = null;
			}
		}

		protected virtual bool CanRead(BindInfo bind)
		{
			if (this.Parent != null)
				return this.EnableReading && this.Parent.CanRead(bind);

			return this.EnableReading;
		}

		protected virtual bool CanWrite(BindInfo bind)
		{
			if (this.Parent != null)
				return this.EnableWriting && this.Parent.CanWrite(bind);

			return this.EnableWriting;
		}

		protected virtual void ActorRefresh(string propertyName)
		{
			if (this.Parent != null)
			{
				this.Parent.ActorRefresh(propertyName);
			}
		}

		protected virtual void OnSelfPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			// Dont process property changes if we are reading memory, since these will just be changes from memory
			// and we only care about changes from anamnesis here.
			if (this.IsReading)
				return;

			if (string.IsNullOrEmpty(e.PropertyName))
				return;

			BindInfo? bind;
			if (!this.binds.TryGetValue(e.PropertyName, out bind))
				return;

			object? val = bind.Property.GetValue(this);
			if (val == null || bind.Flags.HasFlag(BindFlags.Pointer))
				return;

			if (!this.CanWrite(bind))
				return;

			lock (this)
			{
				this.WriteToMemory(bind);
			}

			if (bind.Flags.HasFlag(BindFlags.ActorRefresh))
			{
				this.ActorRefresh(e.PropertyName);
			}
		}

		private void ReadAllFromMemory()
		{
			if (this.Address == IntPtr.Zero)
				return;

			lock (this)
			{
				foreach (BindInfo bind in this.binds.Values)
				{
					if (bind.IsChildMemory)
						continue;

					if (!this.CanRead(bind))
						continue;

					this.ReadFromMemory(bind);
				}

				foreach (BindInfo bind in this.binds.Values)
				{
					if (!bind.IsChildMemory)
						continue;

					if (!this.CanRead(bind))
						continue;

					this.ReadFromMemory(bind);
				}
			}
		}

		private void ReadFromMemory(BindInfo bind)
		{
			if (!this.CanRead(bind))
				return;

			if (this.IsWriting)
				throw new Exception("Attempt to read memory while writing it");

			this.IsReading = true;

			try
			{
				IntPtr bindAddress = bind.GetAddress(this.Address);

				if (bindAddress == IntPtr.Zero)
					return;

				if (typeof(MemoryBase).IsAssignableFrom(bind.Type))
				{
					MemoryBase? childMemory = bind.Property.GetValue(this) as MemoryBase;

					if (childMemory == null)
					{
						childMemory = Activator.CreateInstance(bind.Type) as MemoryBase;

						if (childMemory == null)
							throw new Exception($"Failed to create instance of child memory type: {bind.Type}");

						childMemory.Parent = this;
						this.Children.Add(childMemory);
					}

					// Has this bind changed
					if (childMemory.Address == bindAddress)
						return;

					try
					{
						bind.Property.SetValue(this, childMemory);
						childMemory.SetAddress(bindAddress);
					}
					catch (Exception ex)
					{
						Log.Warning(ex, $"Failed to bind to child memory: {bind.Name}");
					}
				}
				else
				{
					object memValue = MemoryService.Read(bindAddress, bind.Type);
					object? currentValue = bind.Property.GetValue(this);

					if (currentValue == null)
						throw new Exception($"Failed to get bind value: {bind.Name} from memory: {this.GetType()}");

					// Has this bind changed
					if (currentValue.Equals(memValue))
						return;

					if (bind.FreezeValue != null)
					{
						memValue = bind.FreezeValue;
						this.IsReading = false;
						this.WriteToMemory(bind);
						this.IsReading = true;
					}

					bind.Property.SetValue(this, memValue);
				}

				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(bind.Property.Name));
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				this.IsReading = false;
			}
		}

		private void WriteToMemory(BindInfo bind)
		{
			if (!this.CanWrite(bind))
				return;

			if (this.IsReading)
				throw new Exception("Attempt to write memory while reading it");

			this.IsWriting = true;

			try
			{
				if (bind.Flags.HasFlag(BindFlags.Pointer))
					throw new NotSupportedException("Attempt to write a pointer value to memory.");

				if (bind.Offsets.Length > 1 && !bind.Flags.HasFlag(BindFlags.Pointer))
					throw new Exception("Bind address has multiple offsets but is not a pointer. This is not supported.");

				IntPtr bindAddress = this.Address + bind.Offsets[0];
				object? val = bind.Property.GetValue(this);

				if (val == null)
					throw new Exception("Attempt to write null value to memory");

				MemoryService.Write(bindAddress, val, $"memory: {this} bind: {bind} changed");
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				this.IsWriting = false;
			}
		}

		[AttributeUsage(AttributeTargets.Property)]
		public class BindAttribute : Attribute
		{
			public readonly int[] Offsets;
			public readonly BindFlags Flags;

			public BindAttribute(int offset)
			{
				this.Offsets = new[] { offset };
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

		public class BindInfo
		{
			public readonly MemoryBase Memory;
			public readonly PropertyInfo Property;
			public readonly BindAttribute Attribute;

			public BindInfo(MemoryBase memory, PropertyInfo property, BindAttribute attribute)
			{
				this.Memory = memory;
				this.Property = property;
				this.Attribute = attribute;
			}

			public string Name => this.Property.Name;
			public int[] Offsets => this.Attribute.Offsets;
			public Type Type => this.Property.PropertyType;
			public BindFlags Flags => this.Attribute.Flags;

			public object? FreezeValue { get; set; }

			public bool IsChildMemory => typeof(MemoryBase).IsAssignableFrom(this.Type);

			public IntPtr GetAddress(IntPtr objAddress)
			{
				IntPtr bindAddress = objAddress + this.Offsets[0];

				if (this.Offsets.Length > 1 && !this.Flags.HasFlag(BindFlags.Pointer))
					throw new Exception("Bind address has multiple offsets but is not a pointer. This is not supported.");

				if (typeof(MemoryBase).IsAssignableFrom(this.Type))
				{
					if (this.Flags.HasFlag(BindFlags.Pointer))
					{
						bindAddress = MemoryService.Read<IntPtr>(bindAddress);

						for (int i = 1; i < this.Offsets.Length; i++)
						{
							bindAddress += this.Offsets[i];
							bindAddress = MemoryService.Read<IntPtr>(bindAddress);
						}
					}
				}

				return bindAddress;
			}

			public override string ToString()
			{
				return $"Bind: {this.Name} ({this.Type})";
			}
		}
	}
}
