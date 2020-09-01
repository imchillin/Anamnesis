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
		private IMemoryViewModel? parent;
		private PropertyInfo? parentProperty;

		public MemoryViewModelBase(IntPtr pointer)
			: base()
		{
			this.Pointer = pointer;

			MemoryService.RegisterViewModel(this);

			this.Tick();
		}

		public MemoryViewModelBase(IMemoryViewModel parent, string propertyName)
			: base(parent, propertyName)
		{
			this.parent = parent;
			PropertyInfo? property = this.parent.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

			if (property == null)
				throw new Exception($"Unable to find property: {propertyName} on object: {this.parent}");

			this.parentProperty = property;
		}

		public IntPtr? Pointer
		{
			get;
			private set;
		}

		public void Tick()
		{
			lock (this)
			{
				if (this.Pointer != null)
				{
					T? model = MemoryService.Read<T>((IntPtr)this.Pointer);

					if (model == null)
						throw new Exception($"Failed to read memory: {typeof(T)}");

					this.SetModel(model);
				}
				else if (this.parent != null && this.parentProperty != null)
				{
					object? obj = this.parentProperty.GetValue(this.parent);
					T? val = (T?)obj;
					this.SetModel(val);
				}
				else
				{
					throw new Exception("Memory view model is not correctly initialized");
				}
			}
		}

		protected override void OnViewToModel(string fieldName, object? value)
		{
			if (this.Pointer != null)
			{
				MemoryService.Write((IntPtr)this.Pointer, this.model);
			}
			else if (this.parent != null && this.parentProperty != null)
			{
				// TODO: ensure propertychanged is raised
				this.parentProperty.SetValue(this.parent, this.model);
			}
		}

		protected override void OnModelToView(string fieldName, object? value)
		{
		}

		protected override void HandleModelToviewUpdate(PropertyInfo viewModelProperty, FieldInfo modelField)
		{
			// special case for the property being a viewModel and the field being a pointer to more memory
			if (modelField.FieldType == typeof(IntPtr) && typeof(IMemoryViewModel).IsAssignableFrom(viewModelProperty.PropertyType))
			{
				ViewModelOffsetAttribute? offset = viewModelProperty.GetCustomAttribute<ViewModelOffsetAttribute>();

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
						return;
					}
				}

				lhs = Activator.CreateInstance(viewModelProperty.PropertyType, desiredPointer) as IMemoryViewModel;

				if (lhs == null)
					throw new Exception($"Failed to create instance of view model: {viewModelProperty.PropertyType}");

				viewModelProperty.SetValue(this, lhs);
				this.OnModelToView(modelField.Name, rhs);
			}
			else
			{
				base.HandleModelToviewUpdate(viewModelProperty, modelField);
			}
		}

		protected override void HandleViewToModelUpdate(PropertyInfo viewModelProperty, FieldInfo modelField)
		{
			// special case for the property being a viewModel and the field being a pointer to more memory
			if (modelField.FieldType == typeof(IntPtr) && typeof(IMemoryViewModel).IsAssignableFrom(viewModelProperty.PropertyType))
			{
				// We do not support setting a pointer from a viewmodel. read only!
			}
			else
			{
				base.HandleViewToModelUpdate(viewModelProperty, modelField);
			}
		}
	}
}
