// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Reflection;
	using PropertyChanged;
	using SimpleLog;

	#pragma warning disable SA1649
	public interface IStructViewModel : INotifyPropertyChanged
	{
		Type GetModelType();
		void SetModel(object? model);
		object? GetModel();

		void RaisePropertyChanged(string propertyName);
	}

	[AddINotifyPropertyChangedInterface]
	public abstract class StructViewModelBase<T> : IStructViewModel, INotifyPropertyChanged
		where T : struct
	{
		protected static readonly Logger Log = SimpleLog.Log.GetLogger("StructViewModels");

		protected T model;
		private Dictionary<string, (PropertyInfo, FieldInfo)> binds = new Dictionary<string, (PropertyInfo, FieldInfo)>();

		private IStructViewModel? parent;
		private PropertyInfo? parentProperty;

		public StructViewModelBase()
		{
			Type modelType = typeof(T);
			PropertyInfo[]? properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

			foreach (PropertyInfo property in properties)
			{
				string name = property.Name;

				FieldInfo? modelField = modelType.GetField(property.Name, BindingFlags.Public | BindingFlags.Instance);
				if (modelField == null)
				{
					Log.Write(Severity.Warning, $"No field for property: {name} in view model: {this.GetType()}");
					continue;
				}

				this.binds.Add(name, (property, modelField));
			}

			this.PropertyChanged += this.OnThisPropertyChanged;
		}

		public StructViewModelBase(IStructViewModel parent, string propertyName)
			: this()
		{
			this.parent = parent;
			PropertyInfo? property = this.parent.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

			if (property == null)
				throw new Exception($"Unable to find property: {propertyName} on object: {this.parent}");

			this.parentProperty = property;
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		public Type GetModelType()
		{
			return typeof(T);
		}

		public void SetModel(object? model)
		{
			if (model is T tModel)
			{
				this.SetModel(tModel);
			}
			else
			{
				throw new Exception($"Invalid model type. Expected: {typeof(T)}, got: {model?.GetType()}");
			}
		}

		public void SetModel(T? model)
		{
			if (model == null)
				throw new Exception("Attempt to set null model to view model");

			this.model = (T)model;

			foreach ((PropertyInfo viewModelProperty, FieldInfo modelField) in this.binds.Values)
			{
				this.HandleModelToviewUpdate(viewModelProperty, modelField);
			}
		}

		public object? GetModel()
		{
			return this.model;
		}

		public virtual void Tick()
		{
			if (this.parent != null && this.parentProperty != null)
			{
				object? obj = this.parentProperty.GetValue(this.parent);
				T? val = (T?)obj;
				this.SetModel(val);
				return;
			}

			throw new Exception("View model is not correctly initialized");
		}

		void IStructViewModel.RaisePropertyChanged(string propertyName)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected virtual void HandleModelToviewUpdate(PropertyInfo viewModelProperty, FieldInfo modelField)
		{
			lock (this)
			{
				object? lhs = viewModelProperty.GetValue(this);
				object? rhs = modelField.GetValue(this.model);

				if (typeof(IStructViewModel).IsAssignableFrom(viewModelProperty.PropertyType))
				{
					IStructViewModel? vm = null;

					if (lhs != null)
						vm = (IStructViewModel)lhs;

					if (vm == null)
						vm = Activator.CreateInstance(viewModelProperty.PropertyType, this, viewModelProperty.Name) as IStructViewModel;

					if (vm == null)
						throw new Exception($"Failed to create instance of view model: {viewModelProperty.PropertyType}");

					vm.SetModel(rhs);
					rhs = vm;
				}
				else
				{
					if (modelField.FieldType != viewModelProperty.PropertyType)
						throw new Exception($"view model: {this.GetType()} property: {modelField.Name} type: {viewModelProperty.PropertyType} does not match backing model field type: {modelField.FieldType}");

					if (lhs == null && rhs == null)
					{
						return;
					}
				}

				if (rhs == null || !rhs.Equals(lhs))
				{
					viewModelProperty.SetValue(this, rhs);
					this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(viewModelProperty.Name));
					this.OnModelToView(modelField.Name, rhs);
				}
			}
		}

		protected virtual void HandleViewToModelUpdate(PropertyInfo viewModelProperty, FieldInfo modelField)
		{
			lock (this)
			{
				object? lhs = viewModelProperty.GetValue(this);
				object? rhs = modelField.GetValue(this.model);

				if (lhs is IStructViewModel vm)
					lhs = vm.GetModel();

				if (lhs == null && rhs == null)
					return;

				if (lhs == null)
					return;

				if (rhs == null || !rhs.Equals(lhs))
				{
					TypedReference typedReference = __makeref(this.model);
					modelField.SetValueDirect(typedReference, lhs);

					this.OnViewToModel(viewModelProperty.Name, lhs);
				}
			}
		}

		/// <summary>
		/// Called when the view model has changed the backing modal.
		/// </summary>
		protected virtual void OnViewToModel(string fieldName, object? value)
		{
			if (this.parent != null && this.parentProperty != null)
			{
				if (typeof(IStructViewModel).IsAssignableFrom(this.parentProperty.PropertyType))
				{
					this.parent.RaisePropertyChanged(this.parentProperty.Name);
				}
				else
				{
					this.parentProperty.SetValue(this.parent, this.model);
				}
			}
		}

		/// <summary>
		/// Called when the backing model has changed the view model.
		/// </summary>
		protected abstract void OnModelToView(string fieldName, object? value);

		private void OnThisPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (!this.binds.ContainsKey(e.PropertyName))
				return;

			(PropertyInfo viewModelProperty, FieldInfo modelField) = this.binds[e.PropertyName];
			this.HandleViewToModelUpdate(viewModelProperty, modelField);
		}
	}
}
