// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.MemoryBinds
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Text;

	internal class BindWrapper : IDisposable
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

		public object? Owner
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
		}
	}
}
