// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Reflection;
	using System.Runtime.CompilerServices;
	using ConceptMatrix.Memory;
	using ConceptMatrix.MemoryBinds;

	public static class IMemoryExtensions
	{
		/// <summary>
		/// Creates a two-way binding between this memory object and the given property.
		/// </summary>
		/// <param name="self">this.</param>
		/// <param name="owner">the object that owns the property.</param>
		/// <param name="propertyName">the property to bind.</param>
		public static void Bind<T>(this IMarshaler<T> self, object owner, string propertyName)
			where T : struct
		{
			BindUtility.Bind<T>(self, owner, propertyName);
		}

		/// <summary>
		/// Creates a two-way binding between this memory object and the given property.
		/// </summary>
		/// <param name="self">this.</param>
		/// <param name="owner">the object that owns the property.</param>
		/// <param name="propertyName">the property to bind.</param>
		public static void UnBind<T>(this IMarshaler<T> self, object owner, string propertyName)
			where T : struct
		{
			BindUtility.Clear(owner, propertyName);
		}
	}
}
