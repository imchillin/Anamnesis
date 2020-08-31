// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Reflection;

	#pragma warning disable SA1649
	public interface IMemoryViewModel
	{
		void Tick();
	}

	public abstract class MemoryViewModelBase<T> : StructViewModelBase<T>, IMemoryViewModel
		where T : struct
	{
		private IMemoryViewModel? parent;
		private PropertyInfo? parentProperty;

		public MemoryViewModelBase(IntPtr pointer)
		{
			this.Pointer = pointer;

			MemoryService.RegisterViewModel(this);

			this.Tick();
		}

		public MemoryViewModelBase(IMemoryViewModel parent, string propertyName)
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
	}
}
