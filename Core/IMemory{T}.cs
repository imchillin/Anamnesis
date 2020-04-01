// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	public interface IMemory<T>
	{
		T Value { get; }

		/// <summary>
		/// Gets the current value from the process.
		/// </summary>
		T Get();

		/// <summary>
		/// Writes a new value to the process, and returns the old value.
		/// </summary>
		void Set(T value);
	}
}
