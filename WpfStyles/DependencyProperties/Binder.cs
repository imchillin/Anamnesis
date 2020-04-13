// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.WpfStyles.DependencyProperties
{
	using System;
	using System.Reflection;
	using System.Windows;
	using System.Windows.Data;

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
			};

			FrameworkPropertyMetadata meta = new FrameworkPropertyMetadata(new PropertyChangedCallback(changedb));
			meta.BindsTwoWayByDefault = true;
			meta.DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
			meta.Inherits = true;
			DependencyProperty dp = DependencyProperty.Register(propertyName, typeof(TValue), typeof(TOwner), meta);
			DependencyProperty<TValue> dpv = new DependencyProperty<TValue>(dp);
			return dpv;
		}
	}
}
