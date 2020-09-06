// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Printing;
	using System.Reflection;
	using System.Text;

#pragma warning disable SA1649
	public interface IMemoryViewModel : IStructViewModel
	{
		IntPtr? Pointer { get; }
		void Tick();
	}

	public abstract class MemoryViewModelBase<T> : StructViewModelBase<T>, IMemoryViewModel
		where T : struct
	{
		public MemoryViewModelBase(IntPtr pointer, IStructViewModel? parent = null)
			: base()
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

		public IntPtr? Pointer { get; }

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
					T? model = MemoryService.Read<T>((IntPtr)this.Pointer);

					if (model == null)
						throw new Exception($"Failed to read memory: {typeof(T)}");

					this.SetModel(model);
				}
				else
				{
					base.Tick();
				}
			}
		}

		protected override void OnViewToModel(string fieldName, object? value)
		{
			if (this.Pointer != null)
			{
				MemoryService.Write((IntPtr)this.Pointer, this.model);
			}
			else
			{
				base.OnViewToModel(fieldName, value);
			}
		}

		protected override void OnModelToView(string fieldName, object? value)
		{
		}

		protected override bool HandleModelToViewUpdate(PropertyInfo viewModelProperty, FieldInfo modelField)
		{
			// special case for the property being a viewModel and the field being a pointer to more memory
			if (modelField.FieldType == typeof(IntPtr) && typeof(IMemoryViewModel).IsAssignableFrom(viewModelProperty.PropertyType))
			{
				ModelFieldAttribute? offset = viewModelProperty.GetCustomAttribute<ModelFieldAttribute>();

				object? lhs = viewModelProperty.GetValue(this);
				IntPtr? rhs = (IntPtr?)modelField.GetValue(this.model);

				IntPtr desiredPointer = IntPtr.Zero;

				if (rhs != null)
				{
					desiredPointer = (IntPtr)rhs;

					if (offset != null)
					{
						desiredPointer += offset.Offset;
					}
				}

				IMemoryViewModel vm;
				if (lhs != null)
				{
					vm = (IMemoryViewModel)lhs;

					if (vm.Pointer == desiredPointer)
					{
						return false;
					}
				}

				// not a valid pointer
				if (desiredPointer == IntPtr.Zero)
					return false;

				lhs = Activator.CreateInstance(viewModelProperty.PropertyType, desiredPointer, this) as IMemoryViewModel;

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
