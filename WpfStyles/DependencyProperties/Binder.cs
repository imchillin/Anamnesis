// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.WpfStyles.DependencyProperties
{
	using System;
	using System.Windows;

	public class Binder
	{
		public static DependencyProperty<TValue> Register<TValue, TOwner>(string propertyName, Action<DependencyObject, DependencyPropertyChangedEventArgs> changed)
		{
			DependencyProperty dp = DependencyProperty.Register(propertyName, typeof(TValue), typeof(TOwner), new FrameworkPropertyMetadata(new PropertyChangedCallback(changed)));
			DependencyProperty<TValue> dpv = new DependencyProperty<TValue>(dp);
			return dpv;
		}
	}
}
