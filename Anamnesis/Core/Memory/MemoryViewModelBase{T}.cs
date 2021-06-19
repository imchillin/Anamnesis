// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Collections.Generic;
	using System.Printing;
	using System.Reflection;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Documents;
	using Anamnesis.Memory.Types;

#pragma warning disable SA1649
	[Flags]
	public enum MemoryModes
	{
		None = 0,

		Read = 1,
		Write = 2,

		ReadWrite = Read | Write,
	}

	public interface IMemoryViewModel : IStructViewModel, IDisposable
	{
		IntPtr? Pointer { get; set; }
		int Tick();
		bool WriteToMemory(bool force = false);
		bool ReadFromMemory(bool force = false);

		void SetMemoryMode(MemoryModes mode);

		void AddChild(IMemoryViewModel child);
		void RemoveChild(IMemoryViewModel child);
	}

	public abstract class MemoryViewModelBase<T> : StructViewModelBase<T>, IMemoryViewModel
		where T : struct
	{
		private Dictionary<string, (PropertyInfo, FieldInfo, object?)> freezeValues = new Dictionary<string, (PropertyInfo, FieldInfo, object?)>();
		private bool isAllFrozen = false;
		private List<IMemoryViewModel> children = new List<IMemoryViewModel>();

		public MemoryViewModelBase(IntPtr pointer, IMemoryViewModel? parent)
			: base(parent)
		{
			this.Parent = parent;
			if (pointer == IntPtr.Zero)
				throw new Exception("Attempt to create memory view model with invalid address");

			this.Pointer = pointer;

			if (parent == null)
			{
				MemoryService.RegisterViewModel(this);
			}
			else
			{
				parent.AddChild(this);
			}

			this.Tick();
		}

		public IntPtr? Pointer { get; set; }
		public MemoryModes MemoryMode { get; set; } = MemoryModes.ReadWrite;

		public new IMemoryViewModel? Parent
		{
			get => (IMemoryViewModel?)base.Parent;
			set => base.Parent = value;
		}

		public bool Freeze
		{
			get => this.isAllFrozen;
			set
			{
				this.isAllFrozen = value;
				this.FreezeAll(value);
			}
		}

		public override string Path
		{
			get
			{
				StringBuilder builder = new StringBuilder();
				builder.Append(this.GetType().Name);
				builder.Append("(0x");
				builder.Append(this.Pointer?.ToString("x"));
				builder.Append(")");

				IStructViewModel? vm = this.Parent;
				while (vm != null)
				{
					if (vm is IMemoryViewModel memVm)
					{
						builder.Append("<--");
						builder.Append(vm.GetType().Name);
						builder.Append("(0x");
						builder.Append(memVm.Pointer?.ToString("x"));
						builder.Append(")");
					}

					vm = vm.Parent;
				}

				return builder.ToString();
			}
		}

		public void SetMemoryMode(MemoryModes mode)
		{
			this.MemoryMode = mode;
		}

		public void Dispose()
		{
			for (int i = this.children.Count - 1; i >= 0; i--)
			{
				this.children[i].Dispose();
			}

			if (this.children.Count > 0)
				Log.Warning("not all memory view model children were removed during disposal");

			if (this.Parent == null)
			{
				MemoryService.ClearViewModel(this);
			}
			else
			{
				this.Parent.RemoveChild(this);
			}
		}

		public void AddChild(IMemoryViewModel child)
		{
			lock (this)
			{
				this.children.Add(child);
			}
		}

		public void RemoveChild(IMemoryViewModel child)
		{
			lock (this)
			{
				this.children.Remove(child);
			}
		}

		public override int Tick()
		{
			int count = 0;

			lock (this)
			{
				if (!this.Enabled)
					return count;

				if (this.Pointer != null)
				{
					this.ReadFromMemory();
				}
				else
				{
					return base.Tick();
				}

				foreach (IMemoryViewModel child in this.children)
				{
					count++;
					count += child.Tick();
				}
			}

			return count;
		}

		public bool WriteToMemory(bool force = false)
		{
			if (this.Pointer == null)
				return false;

			if (!force && !this.MemoryMode.HasFlag(MemoryModes.Write))
				return false;

			MemoryService.Write((IntPtr)this.Pointer, this.model, "Model Updated");
			return true;
		}

		public async Task<bool> ReadFromMemoryAsync(bool force = false)
		{
			return await Task<bool>.Run(() =>
			{
				return this.ReadFromMemory(force);
			});
		}

		public bool ReadFromMemory(bool force = false)
		{
			if (this.Pointer == null)
				return false;

			if (!force && !this.MemoryMode.HasFlag(MemoryModes.Read))
				return false;

			T? model = MemoryService.Read<T>((IntPtr)this.Pointer);

			if (model == null)
				throw new Exception($"Failed to read memory: {typeof(T)}");

			this.SetModel(model);

			return true;
		}

		public void FreezeAll(bool freeze)
		{
			PropertyInfo[] properties = this.GetType().GetProperties();
			foreach (PropertyInfo property in properties)
			{
				if (property.GetCustomAttribute<ModelFieldAttribute>() == null)
					continue;

				this.FreezeValue(property.Name, freeze);
			}
		}

		public void FreezeValue(string name, bool freeze, object? value = null)
		{
			if (freeze)
			{
				PropertyInfo? property = this.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
				FieldInfo? field = typeof(T).GetField(name, BindingFlags.Public | BindingFlags.Instance);

				if (property == null)
					throw new Exception($"Unable to locate property to freeze with name: {name}");

				if (field == null)
					throw new Exception($"Unable to locate field to freeze with name: {name}");

				if (value == null)
				{
					value = property.GetValue(this);
				}
				else
				{
					property.SetValue(this, value);
				}

				this.freezeValues.Add(name, (property, field, value));
			}
			else
			{
				this.freezeValues.Remove(name);
			}
		}

		public bool IsValueFrozen(string name)
		{
			return this.freezeValues.ContainsKey(name);
		}

		protected override void OnViewToModel(string fieldName, object? value)
		{
			if (this.Pointer != null)
			{
				this.WriteToMemory();
			}
			else
			{
				base.OnViewToModel(fieldName, value);
			}

			if (this.freezeValues.ContainsKey(fieldName))
			{
				(PropertyInfo property, FieldInfo field, object? val) = this.freezeValues[fieldName];
				val = value;
				this.freezeValues[fieldName] = (property, field, val);
			}
		}

		protected override void OnModelToView(string fieldName, object? value)
		{
			if (this.freezeValues.ContainsKey(fieldName))
			{
				(PropertyInfo property, FieldInfo field, object? val) = this.freezeValues[fieldName];
				property.SetValue(this, val);

				if (this.HandleViewToModelUpdate(property, field))
				{
					this.OnViewToModel(fieldName, val);
				}
			}
		}

		protected override bool HandleModelToViewUpdate(PropertyInfo viewModelProperty, FieldInfo modelField)
		{
			// special case for the property being a viewModel and the field being a pointer to more memory
			if (modelField.FieldType == typeof(IntPtr) && typeof(IMemoryViewModel).IsAssignableFrom(viewModelProperty.PropertyType))
			{
				ModelFieldAttribute? modelFieldAttribute = viewModelProperty.GetCustomAttribute<ModelFieldAttribute>();

				object? lhs = viewModelProperty.GetValue(this);
				IntPtr? rhs = (IntPtr?)modelField.GetValue(this.model);

				IntPtr desiredPointer = IntPtr.Zero;

				if (rhs != null)
				{
					desiredPointer = (IntPtr)rhs;

					if (modelFieldAttribute != null && modelFieldAttribute.Offsets != null)
					{
						foreach (int offset in modelFieldAttribute.Offsets)
						{
							desiredPointer += offset;
							desiredPointer = MemoryService.ReadPtr(desiredPointer);
						}
					}
				}

				// bad pointers
				bool isValidPointer = true;

				// not a valid pointer
				if (desiredPointer == IntPtr.Zero)
				{
					isValidPointer = false;
				}
				else
				{
					// TODO: This may prevent valid pointers from working correctly, but is necessary to prevent
					// extended appearance pointing to invalid memory when transformed into a monster.
					long v = desiredPointer.ToInt64();
					if (v < 0x0000000200000000)
					{
						isValidPointer = false;
					}
				}

				if (!isValidPointer)
				{
					bool wasNull = lhs == null;

					if (!wasNull && lhs is IMemoryViewModel lhsVm)
					{
						lhsVm.Dispose();
					}

					viewModelProperty.SetValue(this, null);
					return wasNull;
				}

				IMemoryViewModel vm;
				if (lhs != null)
				{
					vm = (IMemoryViewModel)lhs;

					if (vm.Pointer == desiredPointer)
					{
						return false;
					}

					vm.Pointer = desiredPointer;
				}
				else
				{
					lhs = Activator.CreateInstance(viewModelProperty.PropertyType, desiredPointer, this) as IMemoryViewModel;
				}

				if (lhs == null)
					throw new Exception($"Failed to create instance of view model: {viewModelProperty.PropertyType}");

				viewModelProperty.SetValue(this, lhs);
				this.OnModelToView(modelField.Name, rhs);
				return true;
			}
			else
			{
				return base.HandleModelToViewUpdate(viewModelProperty, modelField);
			}
		}

		protected override bool HandleViewToModelUpdate(PropertyInfo viewModelProperty, FieldInfo modelField)
		{
			// special case for the property being a viewModel and the field being a pointer to more memory
			if (modelField.FieldType == typeof(IntPtr) && typeof(IMemoryViewModel).IsAssignableFrom(viewModelProperty.PropertyType))
			{
				// We do not support setting a pointer from a viewmodel. read only!
				return false;
			}
			else
			{
				return base.HandleViewToModelUpdate(viewModelProperty, modelField);
			}
		}
	}
}
