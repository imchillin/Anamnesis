// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Reflection;
	using Anamnesis.Services;
	using PropertyChanged;
	using Serilog;

	[AddINotifyPropertyChangedInterface]
	public abstract class MemoryBase : INotifyPropertyChanged, IDisposable
	{
		public IntPtr Address = IntPtr.Zero;

		public bool EnableReading = true;
		public bool EnableWriting = true;

		protected readonly List<MemoryBase> Children = new List<MemoryBase>();
		private readonly Dictionary<string, BindInfo> binds = new Dictionary<string, BindInfo>();
		private readonly HashSet<BindInfo> delayedBinds = new HashSet<BindInfo>();

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
			DontCacheOffsets = 4,
			OnlyInGPose = 8,
		}

		[DoNotNotify]
		public MemoryBase? Parent { get; set; }

		[DoNotNotify]
		public BindInfo? ParentBind { get; set; }

		[DoNotNotify]
		public bool IsReading { get; private set; }

		[DoNotNotify]
		public bool IsWriting { get; private set; }

		protected static ILogger Log => Serilog.Log.ForContext<MemoryBase>();

		public virtual void SetAddress(IntPtr address)
		{
			if (this.Address == address)
				return;

			Log.Verbose($"Changing addressof {this.GetType()} from: {this.Address} to {address}");
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
			this.ParentBind = null;

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
				if (child.ParentBind != null)
				{
					if (!this.CanRead(child.ParentBind))
					{
						continue;
					}
				}

				child.Tick();
			}
		}

		public IntPtr GetAddressOfProperty(string propertyName)
		{
			BindInfo? bind;
			if (!this.binds.TryGetValue(propertyName, out bind))
				throw new Exception("Attempt to get address of property that is not a bind");

			return bind.GetAddress();
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

		protected virtual void WriteDelayedBinds()
		{
			lock (this)
			{
				foreach (BindInfo bind in this.delayedBinds)
				{
					// If we still cant write this bind, just skip it.
					if (!this.CanWrite(bind))
						continue;

					this.WriteToMemory(bind);

					if (bind.Flags.HasFlag(BindFlags.ActorRefresh))
					{
						this.ActorRefresh(bind.Property.Name);
					}
				}
			}

			this.delayedBinds.Clear();

			foreach (MemoryBase? child in this.Children)
			{
				child.WriteDelayedBinds();
			}
		}

		protected virtual void OnSelfPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (string.IsNullOrEmpty(e.PropertyName))
				return;

			BindInfo? bind;
			if (!this.binds.TryGetValue(e.PropertyName, out bind))
				return;

			// Dont process property changes if we are reading memory, since these will just be changes from memory
			// and we only care about changes from anamnesis here.
			if (bind.IsReading)
				return;

			object? val = bind.Property.GetValue(this);
			if (val == null || bind.Flags.HasFlag(BindFlags.Pointer))
				return;

			if (bind.FreezeValue != null)
				bind.FreezeValue = bind.Property.GetValue(this);

			if (!this.CanWrite(bind))
			{
				// If this bind couldn't be written right now, add it to the delayed bind list
				// to attempt to write later.
				this.delayedBinds.Add(bind);
				return;
			}

			lock (this)
			{
				this.WriteToMemory(bind);
			}

			if (bind.Flags.HasFlag(BindFlags.ActorRefresh))
			{
				this.ActorRefresh(e.PropertyName);
			}
		}

		protected virtual void RaisePropertyChanged(string propertyName)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

					try
					{
						this.ReadFromMemory(bind);
					}
					catch (Exception ex)
					{
						throw new Exception($"Failed to read {this.GetType()} - {bind.Name}", ex);
					}
				}

				foreach (BindInfo bind in this.binds.Values)
				{
					if (!bind.IsChildMemory)
						continue;

					if (!this.CanRead(bind))
						continue;

					try
					{
						this.ReadFromMemory(bind);
					}
					catch (Exception ex)
					{
						throw new Exception($"Failed to read {this.GetType()} - {bind.Name}", ex);
					}
				}
			}
		}

		private void ReadFromMemory(BindInfo bind)
		{
			if (!this.CanRead(bind))
				return;

			if (bind.IsWriting)
				throw new Exception("Attempt to read memory while writing it");

			this.IsReading = true;
			bind.IsReading = true;

			try
			{
				if (bind.Flags.HasFlag(BindFlags.OnlyInGPose) && !GposeService.Instance.IsGpose)
					return;

				IntPtr bindAddress = bind.GetAddress();

				if (bindAddress == IntPtr.Zero || bindAddress == bind.LastFailureAddress)
					return;

				if (typeof(MemoryBase).IsAssignableFrom(bind.Type))
				{
					MemoryBase? childMemory = bind.Property.GetValue(this) as MemoryBase;

					bool isNew = false;

					if (childMemory == null)
					{
						isNew = true;
						childMemory = Activator.CreateInstance(bind.Type) as MemoryBase;

						if (childMemory == null)
						{
							throw new Exception($"Failed to create instance of child memory type: {bind.Type}");
						}
					}

					// Has this bind changed
					if (childMemory.Address == bindAddress)
						return;

					try
					{
						childMemory.SetAddress(bindAddress);
						bind.Property.SetValue(this, childMemory);

						if (isNew)
						{
							childMemory.Parent = this;
							childMemory.ParentBind = bind;
							this.Children.Add(childMemory);
						}
					}
					catch (Exception ex)
					{
						Log.Warning(ex, $"Failed to bind to child memory: {bind.Name}");
						bind.LastFailureAddress = bindAddress;
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
						bind.IsReading = false;
						this.WriteToMemory(bind);
						this.IsReading = true;
						bind.IsReading = true;
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
				bind.IsReading = false;
			}
		}

		private void WriteToMemory(BindInfo bind)
		{
			if (!this.CanWrite(bind))
				return;

			if (bind.IsReading)
				throw new Exception("Attempt to write memory while reading it");

			this.IsWriting = true;
			bind.IsWriting = true;

			try
			{
				if (bind.Flags.HasFlag(BindFlags.Pointer))
					throw new NotSupportedException("Attempt to write a pointer value to memory.");

				IntPtr bindAddress = bind.GetAddress();
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
				bind.IsWriting = false;
			}
		}

		[AttributeUsage(AttributeTargets.Property)]
		public class BindAttribute : Attribute
		{
			public readonly int[]? Offsets;
			public readonly BindFlags Flags;
			public readonly string? OffsetPropertyName;

			public BindAttribute(int offset)
			{
				this.Offsets = new[] { offset };
			}

			public BindAttribute(string offsetProperty)
			{
				this.OffsetPropertyName = offsetProperty;
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
			public readonly PropertyInfo? OffsetProperty;

			private int[]? offsets;

			public BindInfo(MemoryBase memory, PropertyInfo property, BindAttribute attribute)
			{
				this.Memory = memory;
				this.Property = property;
				this.Attribute = attribute;

				if (attribute.OffsetPropertyName != null)
				{
					Type memoryType = memory.GetType();
					this.OffsetProperty = memoryType.GetProperty(attribute.OffsetPropertyName);
				}
			}

			public string Name => this.Property.Name;
			public Type Type => this.Property.PropertyType;
			public BindFlags Flags => this.Attribute.Flags;

			public object? FreezeValue { get; set; }
			public bool IsReading { get; set; } = false;
			public bool IsWriting { get; set; } = false;
			public IntPtr? LastFailureAddress { get; set; }

			public bool IsChildMemory => typeof(MemoryBase).IsAssignableFrom(this.Type);

			public IntPtr GetAddress()
			{
				if (this.offsets == null)
					this.offsets = this.GetOffsets();

				IntPtr bindAddress = this.Memory.Address + this.offsets[0];

				if (this.offsets.Length > 1 && !this.Flags.HasFlag(BindFlags.Pointer))
					throw new Exception("Bind address has multiple offsets but is not a pointer. This is not supported.");

				if (typeof(MemoryBase).IsAssignableFrom(this.Type))
				{
					if (this.Flags.HasFlag(BindFlags.Pointer))
					{
						bindAddress = MemoryService.Read<IntPtr>(bindAddress);

						for (int i = 1; i < this.offsets.Length; i++)
						{
							bindAddress += this.offsets[i];
							bindAddress = MemoryService.Read<IntPtr>(bindAddress);
						}
					}
				}
				else if (this.Flags.HasFlag(BindFlags.Pointer))
				{
					bindAddress = MemoryService.Read<IntPtr>(bindAddress);
				}

				if (this.Flags.HasFlag(BindFlags.DontCacheOffsets))
					this.offsets = null;

				return bindAddress;
			}

			public int[] GetOffsets()
			{
				if (this.Attribute.Offsets != null)
					return this.Attribute.Offsets;

				if (this.OffsetProperty != null)
				{
					object? offsetValue = this.OffsetProperty.GetValue(this.Memory);

					if (offsetValue is int[] offsetInts)
						return offsetInts;

					if (offsetValue is int offset)
						return new int[] { offset };

					throw new Exception($"Unknown offset type: {offsetValue} bind: {this}");
				}

				throw new Exception($"No offsets for bind: {this}");
			}

			public override string ToString()
			{
				return $"Bind: {this.Name} ({this.Type})";
			}
		}
	}
}