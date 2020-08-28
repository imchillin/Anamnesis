// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.MemoryBinds
{
	using System.Collections.Generic;
	using System.Runtime.CompilerServices;
	using ConceptMatrix.Memory;

	public static class BindUtility
	{
		private static readonly ConditionalWeakTable<object, Binds> BindsLookup = new ConditionalWeakTable<object, Binds>();

		/// <summary>
		/// Creates a two-way binding between this memory object and the given property.
		/// </summary>
		/// <param name="memory">the memory to bind.</param>
		/// <param name="owner">the object that owns the property.</param>
		/// <param name="propertyName">the property to bind.</param>
		public static void Bind<T>(IMemory<T> memory, object owner, string propertyName)
			where T : struct
		{
			Binds binds = BindsLookup.GetOrCreateValue(owner);
			binds.Bind<T>(memory, owner, propertyName);
		}

		/// <summary>
		/// Removes a binding.
		/// </summary>
		/// <param name="owner">the object that owns the property.</param>
		/// <param name="propertyName">the property to bind.</param>
		public static void Clear(object owner, string propertyName)
		{
			Binds binds = BindsLookup.GetOrCreateValue(owner);
			binds.Clear(propertyName);
		}

		/// <summary>
		/// Clears all bindings on the given object.
		/// </summary>
		/// <param name="owner">the object that owns the property.</param>
		public static void ClearAll(object owner)
		{
			Binds binds = BindsLookup.GetOrCreateValue(owner);
			binds.ClearAll();
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

			public void Clear(string propertyName)
			{
				for (int i = this.bindings.Count - 1; i >= 0; i--)
				{
					if (this.bindings[i].PropertyName == propertyName || !this.bindings[i].IsAlive)
					{
						this.bindings[i].Dispose();
						this.bindings.RemoveAt(i);
					}
				}
			}

			public void ClearAll()
			{
				for (int i = this.bindings.Count - 1; i >= 0; i--)
				{
					this.bindings[i].Dispose();
				}

				this.bindings.Clear();
			}
		}
	}
}
