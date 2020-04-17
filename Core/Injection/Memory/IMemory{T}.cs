// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System;
	using System.ComponentModel;

	public interface IMemory<T> : INotifyPropertyChanged, IDisposable
	{
		event ValueChangedEventHandler ValueChanged;
		event DisposingEventHandler Disposing;

		/// <summary>
		/// Gets or sets a value used to identify this memory when logging. Does not need to be set.
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// Gets or sets the value in memory.
		/// </summary>
		T Value { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the value should be frozen in memory, preventing ffxiv from overwriting it.
		/// </summary>
		bool Freeze { get; set; }
	}

	#pragma warning disable SA1201
	public delegate void ValueChangedEventHandler(object sender, object value);
	public delegate void DisposingEventHandler();
}
