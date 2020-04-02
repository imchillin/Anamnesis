// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System;
	using System.ComponentModel;

	public interface IMemory<T> : INotifyPropertyChanged, IDisposable
	{
		/// <summary>
		/// Gets or sets the value in memory.
		/// </summary>
		T Value { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the value should be frozen in memory, preventing ffxiv from overwriting it.
		/// </summary>
		bool Freeze { get; set; }
	}
}
