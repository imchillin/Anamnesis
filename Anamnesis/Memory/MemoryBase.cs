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

		protected readonly List<MemoryBase> Children = new();
		private readonly Dictionary<string, PropertyBindInfo> binds = new();
		private readonly HashSet<BindInfo> delayedBinds = new();

		public MemoryBase()
		{
			PropertyInfo[]? properties = this.GetType().GetProperties();

			List<BindInfo> binds = new List<BindInfo>();
			foreach (PropertyInfo property in properties)
			{
				BindAttribute? attribute = property.GetCustomAttribute<BindAttribute>();

				if (attribute == null)
					continue;

				this.binds.Add(property.Name, new PropertyBindInfo(this, property, attribute));
			}

			this.PropertyChanged += this.OnSelfPropertyChanged;
		}

		public event PropertyChangedEventHandler? PropertyChanged;

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
				this.Tick();
			}
			catch (Exception ex)
			{
				throw new Exception($"Failed to initially tick {this.GetType().Name} from memory address: {address}", ex);
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

					if (child.ParentBind.Flags.HasFlag(BindFlags.OnlyInGPose) && !GposeService.Instance.IsGpose)
					{
						continue;
					}
				}

				child.Tick();
			}
		}

		public IntPtr GetAddressOfProperty(string propertyName)
		{
			PropertyBindInfo? bind;
			if (!this.binds.TryGetValue(propertyName, out bind))
				throw new Exception("Attempt to get address of property that is not a bind");

			return bind.GetAddress();
		}

		protected bool IsFrozen(string propertyName)
		{
			PropertyBindInfo? bind;
			if (!this.binds.TryGetValue(propertyName, out bind))
				throw new Exception("Attempt to freeze value that is not a bind");

			return bind.FreezeValue != null;
		}

		protected void SetFrozen(string propertyName, bool freeze, object? value = null)
		{
			PropertyBindInfo? bind;
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

		protected void OnPropertyChanged(BindInfo bind, object? oldValue, object? newValue, PropertyChange.Origins origin)
		{
			PropertyChange change = new(bind, oldValue, newValue, origin);
			this.HandlePropertyChanged(change);
		}

		protected virtual void HandlePropertyChanged(PropertyChange change)
		{
			if (this.Parent != null)
			{
				if (this.ParentBind == null)
					throw new Exception("Parent was not null, but parent bind was!");

				change.AddPath(this.ParentBind);
				this.Parent.HandlePropertyChanged(change);
			}
		}

		protected virtual void WriteDelayedBinds()
		{
			lock (this)
			{
				foreach (PropertyBindInfo bind in this.delayedBinds)
				{
					// If we still cant write this bind, just skip it.
					if (!this.CanWrite(bind))
						continue;

					object? oldVal = bind.LastValue;

					this.WriteToMemory(bind);

					if (!bind.Flags.HasFlag(BindFlags.DontRecordHistory))
					{
						var origin = HistoryService.IsRestoring ? PropertyChange.Origins.History : PropertyChange.Origins.User;
						this.OnPropertyChanged(bind, oldVal, bind.LastValue, origin);
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

			PropertyBindInfo? bind;
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

			object? oldVal = bind.LastValue;

			lock (this)
			{
				this.WriteToMemory(bind);
			}

			if (!bind.Flags.HasFlag(BindFlags.DontRecordHistory))
			{
				var origin = HistoryService.IsRestoring ? PropertyChange.Origins.History : PropertyChange.Origins.User;
				this.OnPropertyChanged(bind, oldVal, bind.LastValue, origin);
			}

			bind.LastValue = val;
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
				foreach (PropertyBindInfo bind in this.binds.Values)
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

				foreach (PropertyBindInfo bind in this.binds.Values)
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

		private void ReadFromMemory(PropertyBindInfo bind)
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

				if (bindAddress == bind.LastFailureAddress)
					return;

				if (typeof(MemoryBase).IsAssignableFrom(bind.Type))
				{
					MemoryBase? childMemory = bind.Property.GetValue(this) as MemoryBase;

					bool isNew = false;

					if (childMemory == null && bindAddress != IntPtr.Zero)
					{
						isNew = true;
						childMemory = Activator.CreateInstance(bind.Type) as MemoryBase;

						if (childMemory == null)
						{
							throw new Exception($"Failed to create instance of child memory type: {bind.Type}");
						}
					}

					if (childMemory == null)
						return;

					if (childMemory.Address == bindAddress)
						return;

					try
					{
						if (bindAddress == IntPtr.Zero)
						{
							this.OnPropertyChanged(bind, bind.Property.GetValue(this), null, PropertyChange.Origins.Game);

							bind.Property.SetValue(this, null);
							bind.LastValue = null;
							this.Children.Remove(childMemory);
						}
						else
						{
							childMemory.SetAddress(bindAddress);
							this.OnPropertyChanged(bind, bind.Property.GetValue(this), childMemory, PropertyChange.Origins.Game);
							bind.Property.SetValue(this, childMemory);
							bind.LastValue = childMemory;

							if (isNew)
							{
								childMemory.Parent = this;
								childMemory.ParentBind = bind;
								this.Children.Add(childMemory);
							}
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

					this.OnPropertyChanged(bind, bind.Property.GetValue(this), memValue, PropertyChange.Origins.Game);
					bind.Property.SetValue(this, memValue);
					bind.LastValue = memValue;
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

		private void WriteToMemory(PropertyBindInfo bind)
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
				bind.LastValue = val;
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
	}
}