// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System;
	using System.ComponentModel;
	using System.Reflection;

	public static class IMemoryExtensions
	{
		/// <summary>
		/// Creates a two-way binding between this memory object and the given property.
		/// </summary>
		/// <param name="self">this.</param>
		/// <param name="owner">the object that owns the property.</param>
		/// <param name="propertyName">the property to bind.</param>
		public static void Bind<T>(this IMemory<T> self, object owner, string propertyName)
		{
			BindWrapper<T> bind = new BindWrapper<T>(self, owner, propertyName);
		}

		private class BindWrapper<T>
		{
			private IMemory<T> target;
			private PropertyInfo property;
			private object owner;
			private INotifyPropertyChanged ownerNotifier;

			public BindWrapper(IMemory<T> target, object owner, string propertyName)
			{
				PropertyInfo prop = owner.GetType().GetProperty(propertyName);
				if (prop == null)
					throw new Exception("Unable to locate property by name: " + propertyName + " on type: " + owner.GetType());

				if (prop.PropertyType != typeof(T))
					throw new Exception("Property: " + propertyName + " on type: " + owner.GetType() + " must match memory type: " + typeof(T) + " to be used as a memory binding.");

				this.ownerNotifier = owner as INotifyPropertyChanged;
				if (this.ownerNotifier == null)
					throw new Exception("Type: " + owner.GetType() + " must implement INotifyPropertyChanged to be used as a memory binding");

				this.target = target;
				this.property = prop;
				this.owner = owner;

				target.Disposing += this.Target_Disposing;
				target.ValueChanged += this.Target_ValueChanged;
				this.ownerNotifier.PropertyChanged += this.OwnerNotifier_PropertyChanged;

				prop.SetValue(owner, this.target.Value);
			}

			private void OwnerNotifier_PropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				if (e.PropertyName == this.property.Name)
				{
					this.target.Value = (T)this.property.GetValue(this.owner);
				}
			}

			private void Target_ValueChanged(object sender, object value)
			{
				this.property.SetValue(this.owner, value);
			}

			private void Target_Disposing()
			{
				this.target.ValueChanged -= this.Target_ValueChanged;
				this.target.Disposing -= this.Target_Disposing;
				this.ownerNotifier.PropertyChanged -= this.OwnerNotifier_PropertyChanged;

				this.target = null;
				this.owner = null;
				this.property = null;
				this.ownerNotifier = null;
			}
		}
	}
}
