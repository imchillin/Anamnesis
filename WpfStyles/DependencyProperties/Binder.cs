// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.WpfStyles.DependencyProperties
{
	using System;
	using System.Reflection;
	using System.Windows;

	public class Binder
	{
		public static DependencyProperty<TValue> Register<TValue, TOwner>(string propertyName, Action<TOwner, TValue> changed = null)
		{
			Action<DependencyObject, DependencyPropertyChangedEventArgs> changedb = (d, e) =>
			{
				if (d is TOwner owner && e.NewValue is TValue value)
				{
					changed?.Invoke(owner, value);
				}

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
