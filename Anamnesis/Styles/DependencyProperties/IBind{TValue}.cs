// © Anamnesis.
// Developed by W and A Walsh.
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
