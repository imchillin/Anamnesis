// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Reflection;
	using System.Text;
	using Anamnesis.Memory;
	using PropertyChanged;
	using Serilog;

	#pragma warning disable SA1649
	public delegate void ViewModelEvent(object sender);

	public interface IStructViewModel : INotifyPropertyChanged
	{
		IStructViewModel? Parent { get; }
		bool Enabled { get; set; }
		Type GetModelType();
		void SetModel(object? model);
		object? GetModel();

		TParent? GetParent<TParent>()
			where TParent : IStructViewModel;

		void RaisePropertyChanged(string propertyName);
	}

	[AddINotifyPropertyChangedInterface]
	public abstract class StructViewModelBase<T> : IStructViewModel, INotifyPropertyChanged
		where T : struct
	{
		protected T model;
		private Dictionary<string, (PropertyInfo, FieldInfo)> binds = new Dictionary<string, (PropertyInfo, FieldInfo)>();
		private bool suppressViewToModelEvents = false;

		public StructViewModelBase()
		{
			Type modelType = typeof(T);
			PropertyInfo[]? properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

			foreach (PropertyInfo property in properties)
			{
				string name = property.Name;

				ModelFieldAttribute? attribute = property.GetCustomAttribute<ModelFieldAttribute>();
				if (attribute == null)
					continue;

				string fieldName = attribute.FieldName ?? property.Name;

				FieldInfo? modelField = modelType.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
				if (modelField == null)
				{
					Log.Error($"No field for property: {name} in view model: {this.GetType()}");
					continue;
				}

				this.binds.Add(name, (property, modelField));
			}

			this.PropertyChanged += this.OnThisPropertyChanged;
			this.Enabled = true;
		}

		public StructViewModelBase(IStructViewModel? parent)
			: this()
		{
			this.Parent = parent;
		}

		public StructViewModelBase(IStructViewModel parent, string propertyName)
			: this(parent)
		{
			PropertyInfo? property = parent.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

			if (property == null)
				throw new Exception($"Unable to find property: {propertyName} on object: {this.Parent}");

			this.ParentProperty = property;
		}

		/// <summary>
		/// Called when the view is updated from the backing model. (FFXIV -> Anamnesis)
		/// </summary>
		public event ViewModelEvent? ModelChanged;

		/// <summary>
		/// Called when the model is updated from the view model. (Anamnesis -> FFXIV)
		/// </summary>
		public event ViewModelEvent? ViewModelChanged;

		/// <summary>
		/// Called when a property within the view model is changed. (from FFXIV or Anamnesis)
		/// </summary>
		public event PropertyChangedEventHandler? PropertyChanged;

		/// <summary>
		/// Gets or sets a value indicating whether updating the view model is allowed.
		/// </summary>
		public bool Enabled { get; set; }

		public IStructViewModel? Parent { get; protected set; }
		public PropertyInfo? ParentProperty { get; }

		public T? Model
		{
			get
			{
				return this.model;
			}
		}

		public virtual string Path
		{
			get
			{
				StringBuilder builder = new StringBuilder();
				builder.Append(this.GetType().Name);
				IStructViewModel? vm = this.Parent;
				while (vm != null)
				{
					builder.Append("<--");
					builder.Append(vm.GetType().Name);

					vm = vm.Parent;
				}

				return builder.ToString();
			}
		}

		protected static ILogger Log => Serilog.Log.ForContext<StructViewModelBase<T>>();

		public Type GetModelType()
		{
			return typeof(T);
		}

		public void Import(T model)
		{
			foreach ((PropertyInfo viewModelProperty, FieldInfo modelField) in this.binds.Values)
			{
				viewModelProperty.SetValue(this, modelField.GetValue(model));
			}
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
			if (!MemoryService.IsProcessAlive)
				return;

			if (model == null)
				throw new Exception("Attempt to set null model to view model");

			this.model = (T)model;

			bool changed = false;
			this.suppressViewToModelEvents = true;
			foreach ((PropertyInfo viewModelProperty, FieldInfo modelField) in this.binds.Values)
			{
				changed |= this.HandleModelToViewUpdate(viewModelProperty, modelField);
			}

			this.suppressViewToModelEvents = false;

			if (changed)
			{
				this.ModelChanged?.Invoke(this);
			}
		}

		public object? GetModel()
		{
			return this.model;
		}

		public virtual int Tick()
		{
			if (!this.Enabled)
				return 0;

			if (this.Parent != null && this.ParentProperty != null)
			{
				object? obj = this.ParentProperty.GetValue(this.Parent);
				T? val = (T?)obj;
				this.SetModel(val);
				return 0;
			}

			throw new Exception("View model is not correctly initialized");
		}

		public TParent? GetParent<TParent>()
			where TParent : IStructViewModel
		{
			if (this is TParent t)
				return t;

			if (this.Parent == null)
				return default;

			return this.Parent.GetParent<TParent>();
		}

		void IStructViewModel.RaisePropertyChanged(string propertyName)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected virtual bool HandleModelToViewUpdate(PropertyInfo viewModelProperty, FieldInfo modelField)
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
						return false;
					}
				}

				if (rhs == null || !rhs.Equals(lhs))
				{
					viewModelProperty.SetValue(this, rhs);
					this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(viewModelProperty.Name));
					this.OnModelToView(modelField.Name, rhs);
					return true;
				}
			}

			return false;
		}

		protected virtual bool HandleViewToModelUpdate(PropertyInfo viewModelProperty, FieldInfo modelField)
		{
			object? lhs = viewModelProperty.GetValue(this);
			object? rhs = modelField.GetValue(this.model);

			if (lhs is IStructViewModel vm)
				lhs = vm.GetModel();

			if (lhs == null && rhs == null)
				return false;

			if (lhs == null)
				return false;

			if (rhs == null || !rhs.Equals(lhs))
			{
				TypedReference typedReference = __makeref(this.model);
				modelField.SetValueDirect(typedReference, lhs);

				this.OnViewToModel(viewModelProperty.Name, lhs);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Called when the view model has changed the backing modal.
		/// </summary>
		protected virtual void OnViewToModel(string fieldName, object? value)
		{
			if (this.Parent != null && this.ParentProperty != null)
			{
				if (typeof(IStructViewModel).IsAssignableFrom(this.ParentProperty.PropertyType))
				{
					this.Parent.RaisePropertyChanged(this.ParentProperty.Name);
				}
				else
				{
					this.ParentProperty.SetValue(this.Parent, this.model);
				}
			}
		}

		/// <summary>
		/// Called when the backing model has changed the view model.
		/// </summary>
		protected virtual void OnModelToView(string fieldName, object? value)
		{
		}

		private void OnThisPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (!this.Enabled || this.suppressViewToModelEvents)
				return;

			if (e.PropertyName == null)
				return;

			if (!this.binds.ContainsKey(e.PropertyName))
				return;

			(PropertyInfo viewModelProperty, FieldInfo modelField) = this.binds[e.PropertyName];
			bool changed = this.HandleViewToModelUpdate(viewModelProperty, modelField);

			if (changed)
			{
				this.ViewModelChanged?.Invoke(this);
			}
		}
	}
}
