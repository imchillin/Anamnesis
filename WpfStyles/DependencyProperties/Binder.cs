// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.WpfStyles.DependencyProperties
{
	using System;
	using System.Windows;

	public class Binder
	{
		public static DependencyProperty<TValue> Register<TValue, TOwner>(string propertyName, Action<DependencyObject, DependencyPropertyChangedEventArgs> changed = null)
		{
			PropertyChangedCallback changedCallback = null;
			if (changed != null)
				changedCallback = new PropertyChangedCallback(changed);

			FrameworkPropertyMetadata meta = new FrameworkPropertyMetadata(changedCallback);

			DependencyProperty dp = DependencyProperty.Register(propertyName, typeof(TValue), typeof(TOwner), meta);
			DependencyProperty<TValue> dpv = new DependencyProperty<TValue>(dp);
			return dpv;
		}
	}
}
