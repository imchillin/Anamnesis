// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Collections.Generic;
	using System.Printing;
	using System.Reflection;
	using System.Text;
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

	public interface IMemoryViewModel : IStructViewModel
	{
		IntPtr? Pointer { get; set; }
		void Tick();
		bool WriteToMemory(bool force = false);
		bool ReadFromMemory(bool force = false);
	}

	public abstract class MemoryViewModelBase<T> : StructViewModelBase<T>, IMemoryViewModel
		where T : struct
	{
		private Dictionary<string, (PropertyInfo, object?)> freezeValues = new Dictionary<string, (PropertyInfo, object?)>();

		public MemoryViewModelBase(IntPtr pointer, IStructViewModel? parent = null)
			: base(parent)
		{
			this.Parent = parent;
			if (pointer == IntPtr.Zero)
				throw new Exception("Attempt to create memory view model with invalid address");

			this.Pointer = pointer;

			MemoryService.RegisterViewModel(this);

			this.Tick();
		}

		public MemoryViewModelBase(IMemoryViewModel parent, string propertyName)
			: base(parent, propertyName)
		{
		}

		public IntPtr? Pointer { get; set; }
		public MemoryModes MemoryMode { get; set; } = MemoryModes.ReadWrite;

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

		public override void Tick()
		{
			lock (this)
			{
				if (!this.Enabled)
					return;

				if (this.Pointer != null)
				{
					this.ReadFromMemory();
				}
				else
				{
					base.Tick();
				}
			}
		}

		public bool WriteToMemory(bool force = false)
		{
			if (this.Pointer == null)
				return false;

			if (!force && !this.MemoryMode.HasFlag(MemoryModes.Write))
				return false;

			MemoryService.Write((IntPtr)this.Pointer, this.model);
			return true;
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

		public void FreezeValue(string name, bool freeze, object? value = null)
		{
			if (freeze)
			{
				Type type = this.GetType();
				PropertyInfo? field = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);

				if (field == null)
					throw new Exception($"Unable to locate field to freeze with name: {name}");

				if (value == null)
				{
					value = field.GetValue(this);
				}
				else
				{
					field.SetValue(this, value);
				}

				this.freezeValues.Add(name, (field, value));
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
				(PropertyInfo property, object? val) = this.freezeValues[fieldName];
				val = value;
				this.freezeValues[fieldName] = (property, val);
			}
		}

		protected override void OnModelToView(string fieldName, object? value)
		{
			if (this.freezeValues.ContainsKey(fieldName))
			{
				(PropertyInfo property, object? val) = this.freezeValues[fieldName];
				property.SetValue(this, val);
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

				// not a valid pointer
				if (desiredPointer == IntPtr.Zero)
					return false;

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
