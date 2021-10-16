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

		private readonly BindInfo[] binds;

		public MemoryBase()
		{
			PropertyInfo[]? properties = this.GetType().GetProperties();

			List<BindInfo> binds = new List<BindInfo>();
			foreach (PropertyInfo property in properties)
			{
				BindAttribute? attribute = property.GetCustomAttribute<BindAttribute>();

				if (attribute == null)
					continue;

				if (property.PropertyType == typeof(IntPtr))
				{
					Log.Error($"Attempt to use IntPtr as memory bind target: {property.Name} in memory model: {this.GetType()}. This is not allowed!");
					continue;
				}

				binds.Add(new BindInfo(property, attribute));
			}

			binds.Sort((a, b) =>
			{
				return a.IsChildMemory.CompareTo(b.IsChildMemory);
			});

			this.binds = binds.ToArray();
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		public enum BindFlags
		{
			None = 0,
			Pointer = 1,
			ActorRefresh = 2,
		}

		public MemoryBase? Parent { get; private set; }

		protected static ILogger Log => Serilog.Log.ForContext<MemoryBase>();

		public virtual void SetAddress(IntPtr address)
		{
			this.Address = address;

			try
			{
				this.ReadFromMemory();
			}
			catch (Exception ex)
			{
				throw new Exception($"Failed to read {this.GetType().Name} from memory address: {address}", ex);
			}
		}

		public void Dispose()
		{
			this.Address = IntPtr.Zero;
		}

		public void Tick()
		{
			if (this.Address == IntPtr.Zero)
				return;

			this.ReadFromMemory();
		}

		protected bool IsFrozen(string propertyName)
		{
			return false;
		}

		protected void SetFrozen(string propertyName, bool freeze, object? value = null)
		{
			throw new NotImplementedException();
		}

		protected virtual bool ShouldBind(BindInfo bind)
		{
			return true;
		}

		private void ReadFromMemory()
		{
			if (this.Address == IntPtr.Zero)
				return;

			foreach (BindInfo bind in this.binds)
			{
				if (!this.ShouldBind(bind))
					continue;

				this.ReadFromMemory(bind);
			}
		}

		private void ReadFromMemory(BindInfo bind)
		{
			IntPtr bindAddress = this.Address + bind.Offsets[0];

			if (bind.Offsets.Length > 1 && !bind.Flags.HasFlag(BindFlags.Pointer))
				throw new Exception("Bind address has multiple offsets but is not a pointer. This is not supported.");

			if (typeof(MemoryBase).IsAssignableFrom(bind.Type))
			{
				if (bind.Flags.HasFlag(BindFlags.Pointer))
				{
					bindAddress = MemoryService.Read<IntPtr>(bindAddress);

					for (int i = 1; i < bind.Offsets.Length; i++)
					{
						bindAddress += bind.Offsets[i];
						bindAddress = MemoryService.Read<IntPtr>(bindAddress);
					}
				}

				if (bindAddress == IntPtr.Zero)
					return;

				MemoryBase? childMemory = bind.Property.GetValue(this) as MemoryBase;

				if (childMemory == null)
				{
					childMemory = Activator.CreateInstance(bind.Type) as MemoryBase;

					if (childMemory == null)
						throw new Exception($"Failed to create instance of child memory type: {bind.Type}");

					childMemory.Parent = this;
				}

				// Has this bind changed
				if (childMemory.Address == bindAddress)
					return;

				try
				{
					childMemory.SetAddress(bindAddress);
					bind.Property.SetValue(this, childMemory);
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

				bind.Property.SetValue(this, memValue);
			}

			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(bind.Property.Name));
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
			public readonly PropertyInfo Property;
			public readonly BindAttribute Attribute;

			public BindInfo(PropertyInfo property, BindAttribute attribute)
			{
				this.Property = property;
				this.Attribute = attribute;
			}

			public string Name => this.Property.Name;
			public int[] Offsets => this.Attribute.Offsets;
			public Type Type => this.Property.PropertyType;
			public BindFlags Flags => this.Attribute.Flags;

			public bool IsChildMemory => typeof(MemoryBase).IsAssignableFrom(this.Type);

			public override string ToString()
			{
				return $"Bind: {this.Name} ({this.Type})";
			}
		}
	}
}
