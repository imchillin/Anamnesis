// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.WpfStyles.DependencyProperties
{
	using System;
	using System.Reflection;
	using System.Windows;

	public class Binder
	{
		public static DependencyProperty<TValue> Register<TValue, TOwner>(string propertyName, Action<DependencyObject, DependencyPropertyChangedEventArgs> changed = null)
		{
			Action<DependencyObject, DependencyPropertyChangedEventArgs> changedb = (d, e) =>
			{
				changed?.Invoke(d, e);

				PropertyInfo prop = d.GetType().GetProperty(propertyName);
				prop.SetValue(d, e.NewValue);
			};

			FrameworkPropertyMetadata meta = new FrameworkPropertyMetadata(new PropertyChangedCallback(changedb));
			DependencyProperty dp = DependencyProperty.Register(propertyName, typeof(TValue), typeof(TOwner), meta);
			DependencyProperty<TValue> dpv = new DependencyProperty<TValue>(dp);
			return dpv;
		}
	}
}
