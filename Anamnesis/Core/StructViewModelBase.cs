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

	[AddINotifyPropertyChangedInterface]
	public abstract class StructViewModelBase<T> : INotifyPropertyChanged
		where T : struct
	{
		protected static readonly Logger Log = SimpleLog.Log.GetLogger("StructViewModels");

		protected T model;
		private Dictionary<string, (PropertyInfo, FieldInfo)> binds = new Dictionary<string, (PropertyInfo, FieldInfo)>();

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

				if (modelField.FieldType != property.PropertyType)
					throw new Exception($"view model: {this.GetType()} property: {name} type: {property.PropertyType} does not match backing model field type: {modelField.FieldType}");

				this.binds.Add(name, (property, modelField));
			}

			this.PropertyChanged += this.OnThisPropertyChanged;
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		protected void SetModel(T? model)
		{
			if (model == null)
				throw new Exception("Attempt to set null model to view model");

			this.model = (T)model;

			foreach ((PropertyInfo viewModelProperty, FieldInfo modelField) in this.binds.Values)
			{
				object? lhs = viewModelProperty.GetValue(this);
				object? rhs = modelField.GetValue(this.model);

				if (lhs == null && rhs == null)
					continue;

				if (rhs == null || !rhs.Equals(lhs))
				{
					viewModelProperty.SetValue(this, rhs);
					this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(viewModelProperty.Name));
					this.OnModelToView(modelField.Name, rhs);
				}
			}
		}

		/// <summary>
		/// Called when the view model has changed the backing modal.
		/// </summary>
		protected abstract void OnViewToModel(string fieldName, object? value);

		/// <summary>
		/// Called when the backing model has changed the view model.
		/// </summary>
		protected abstract void OnModelToView(string fieldName, object? value);

		private void OnThisPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (!this.binds.ContainsKey(e.PropertyName))
				return;

			(PropertyInfo viewModelProperty, FieldInfo modelField) = this.binds[e.PropertyName];

			object? lhs = viewModelProperty.GetValue(this);
			object? rhs = modelField.GetValue(this.model);

			if (lhs == null && rhs == null)
				return;

			if (rhs == null || !rhs.Equals(lhs))
			{
				object? value = viewModelProperty.GetValue(this);
				modelField.SetValue(this.model, value);

				this.OnViewToModel(e.PropertyName, value);
			}
		}
	}
}
