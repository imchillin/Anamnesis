// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Reflection;

	#pragma warning disable SA1649
	public interface IMemoryViewModel : IStructViewModel
	{
		IntPtr? Pointer { get; }
		void Tick();
	}

	public abstract class MemoryViewModelBase<T> : StructViewModelBase<T>, IMemoryViewModel
		where T : struct
	{
		private IntPtr? pointer;

		public MemoryViewModelBase(IntPtr pointer)
			: base()
		{
			if (pointer == IntPtr.Zero)
				throw new Exception("Attempt to create memory view model with invalid address");

			this.pointer = pointer;

			MemoryService.RegisterViewModel(this);

			this.Tick();
		}

		public MemoryViewModelBase(IMemoryViewModel parent, string propertyName)
			: base(parent, propertyName)
		{
		}

		public IntPtr? Pointer
		{
			get
			{
				return this.pointer;
			}
		}

		public override void Tick()
		{
			lock (this)
			{
				if (this.Locked)
					return;

				if (this.pointer != null)
				{
					T? model = MemoryService.Read<T>((IntPtr)this.pointer);

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
			if (this.pointer != null)
			{
				MemoryService.Write((IntPtr)this.pointer, this.model);
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

				lhs = Activator.CreateInstance(viewModelProperty.PropertyType, desiredPointer) as IMemoryViewModel;

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
