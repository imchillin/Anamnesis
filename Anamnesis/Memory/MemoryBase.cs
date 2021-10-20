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

				/*if (property.PropertyType == typeof(IntPtr))
				{
					Log.Error($"Attempt to use IntPtr as memory bind target: {property.Name} in memory model: {this.GetType()}. This is not allowed!");
					continue;
				}*/

				this.binds.Add(property.Name, new BindInfo(property, attribute));
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

		public MemoryBase? Parent { get; private set; }
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

		protected virtual bool CanRead()
		{
			if (this.Parent != null)
				return this.EnableReading && this.Parent.CanRead();

			return this.EnableReading;
		}

		protected virtual bool CanWrite()
		{
			if (this.Parent != null)
				return this.EnableWriting && this.Parent.CanWrite();

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

			if (!this.CanRead())
				return;

			lock (this)
			{
				foreach (BindInfo bind in this.binds.Values)
				{
					if (bind.IsChildMemory)
						continue;

					if (!this.ShouldBind(bind))
						continue;

					this.ReadFromMemory(bind);
				}

				foreach (BindInfo bind in this.binds.Values)
				{
					if (!bind.IsChildMemory)
						continue;

					if (!this.ShouldBind(bind))
						continue;

					this.ReadFromMemory(bind);
				}
			}
		}

		private void ReadFromMemory(BindInfo bind)
		{
			if (!this.CanRead())
				return;

			if (this.IsWriting)
				throw new Exception("Attempt to read memory while writing it");

			this.IsReading = true;

			try
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
						this.Children.Add(childMemory);
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
			if (!this.CanWrite())
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
