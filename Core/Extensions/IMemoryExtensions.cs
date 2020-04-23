// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Reflection;
	using System.Runtime.CompilerServices;

	public static class IMemoryExtensions
	{
		private static readonly ConditionalWeakTable<object, Binds> BindsLookup = new ConditionalWeakTable<object, Binds>();

		/// <summary>
		/// Creates a two-way binding between this memory object and the given property.
		/// </summary>
		/// <param name="self">this.</param>
		/// <param name="owner">the object that owns the property.</param>
		/// <param name="propertyName">the property to bind.</param>
		public static void Bind<T>(this IMemory<T> self, object owner, string propertyName)
			where T : struct
		{
			Binds binds = BindsLookup.GetOrCreateValue(owner);
			binds.Bind<T>(self, owner, propertyName);
		}

		private class Binds
		{
			private List<BindWrapper> bindings = new List<BindWrapper>();

			public void Bind<T>(IMemory<T> memory, object owner, string propertyName)
				where T : struct
			{
				for (int i = this.bindings.Count - 1; i >= 0; i--)
				{
					if (this.bindings[i].PropertyName == propertyName || !this.bindings[i].IsAlive)
					{
						this.bindings[i].Dispose();
						this.bindings.RemoveAt(i);
					}
				}

				this.bindings.Add(new BindWrapper<T>(memory, owner, propertyName));
			}
		}

		private class BindWrapper : IDisposable
		{
			protected readonly PropertyInfo property;
			private readonly WeakReference<object> owner;

			public BindWrapper(object owner, string propertyName)
			{
				PropertyInfo prop = owner.GetType().GetProperty(propertyName);
				if (prop == null)
					throw new Exception("Unable to locate property by name: " + propertyName + " on type: " + owner.GetType());

				this.property = prop;
				this.owner = new WeakReference<object>(owner);

				this.IsAlive = true;
			}

			public object Owner
			{
				get
				{
					object owner;
					if (!this.owner.TryGetTarget(out owner))
					{
						this.Dispose();
						return null;
					}

					return owner;
				}
			}

			public string PropertyName
			{
				get
				{
					return this.property.Name;
				}
			}

			public bool IsAlive
			{
				get;
				private set;
			}

			public virtual void Dispose()
			{
				this.IsAlive = false;
				Log.Write("disposing of memory bind");
			}
		}

		private class BindWrapper<T> : BindWrapper
			where T : struct
		{
			private IMemory<T> target;
			private INotifyPropertyChanged ownerNotifier;
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
}
