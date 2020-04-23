// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	public static class IBaseOffsetExtensions
	{
		/// <summary>
		/// Creates a two-way binding between this memory object and the given property.
		/// </summary>
		/// <param name="self">this.</param>
		/// <param name="offset">the offset to bind to.</param>
		/// <param name="owner">the object that owns the property.</param>
		/// <param name="propertyName">the property to bind.</param>
		public static void Bind<T>(this IBaseMemoryOffset self, IMemoryOffset<T> offset, object owner, string propertyName)
			where T : struct
		{
			self.GetMemory(offset).Bind(owner, propertyName);
		}
	}
}
