// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.MemoryBinds
{
	using System;
	using System.ComponentModel;
	using Anamnesis;

	internal class BindWrapper<T> : BindWrapper
		where T : struct
	{
		private IMemory<T>? target;
		private INotifyPropertyChanged? ownerNotifier;
		private bool bindLock = false;

		public BindWrapper(IMemory<T> target, object owner, string propertyName)
			: base(owner, propertyName)
		{
			if (this.property.PropertyType == typeof(T?))
			{
			}
			else
			{
				if (this.property.PropertyType != typeof(T))
				{
					throw new Exception("Property: " + propertyName + " on type: " + owner.GetType() + " must match memory type: " + typeof(T) + " to be used as a memory binding.");
				}
			}

			this.ownerNotifier = owner as INotifyPropertyChanged;
			if (this.ownerNotifier == null)
				throw new Exception("Type: " + owner.GetType() + " must implement INotifyPropertyChanged to be used as a memory binding");

			this.target = target;

			target.Disposing += this.Target_Disposing;
			target.ValueChanged += this.Target_ValueChanged;
			this.ownerNotifier.PropertyChanged += this.OwnerNotifier_PropertyChanged;

			this.property.SetValue(owner, this.target.Value);
		}

		public override void Dispose()
		{
			base.Dispose();

			if (this.target != null)
			{
				this.target.ValueChanged -= this.Target_ValueChanged;
				this.target.Disposing -= this.Target_Disposing;
			}

			if (this.ownerNotifier != null)
			{
				this.ownerNotifier.PropertyChanged -= this.OwnerNotifier_PropertyChanged;
			}

			this.target = null;
			this.ownerNotifier = null;
		}

		private void OwnerNotifier_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (!this.IsAlive)
				return;

			if (this.bindLock)
				return;

			if (this.target == null)
				return;

			if (e.PropertyName == this.property.Name)
			{
				object v = this.property.GetValue(this.Owner);

				if (v == null)
					return;

				this.target.Value = (T)v;
			}
		}

		private void Target_ValueChanged(object sender, object value)
		{
			if (!this.IsAlive)
				return;

			this.bindLock = true;
			this.property.SetValue(this.Owner, value);
			this.bindLock = false;
		}

		private void Target_Disposing()
		{
			this.Dispose();
		}
	}
}
