// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Styles.DependencyProperties
{
	using System.Windows;

	public interface IBind<TValue>
	{
		TValue Get(DependencyObject control);
		void Set(DependencyObject control, TValue value);
	}
}
