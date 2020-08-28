// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.WpfStyles.DependencyProperties
{
	using System;
	using System.Reflection;
	using System.Windows;
	using System.Windows.Data;

	public class Binder
	{
		public static DependencyProperty<TValue> Register<TValue, TOwner>(string propertyName, BindMode mode)
		{
			return Register<TValue, TOwner>(propertyName, null, mode);
		}

		public static DependencyProperty<TValue> Register<TValue, TOwner>(string propertyName, Action<TOwner, TValue>? changed = null, BindMode mode = BindMode.TwoWay)
		{
			PropertyInfo? property = typeof(TOwner).GetProperty(propertyName);
			if (property == null)
				throw new Exception("Failed to locate property: \"" + propertyName + "\" on type: \"" + typeof(TOwner) + "\" for binding.");

			Action<DependencyObject, DependencyPropertyChangedEventArgs> changedb = (d, e) =>
			{
				if (d is TOwner owner && e.NewValue is TValue value)
				{
					changed?.Invoke(owner, value);
				}
			};

			FrameworkPropertyMetadata meta = new FrameworkPropertyMetadata(new PropertyChangedCallback(changedb));
			meta.BindsTwoWayByDefault = mode == BindMode.TwoWay;
			meta.DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
			meta.Inherits = true;
			DependencyProperty dp = DependencyProperty.Register(propertyName, typeof(TValue), typeof(TOwner), meta);
			DependencyProperty<TValue> dpv = new DependencyProperty<TValue>(dp);
			return dpv;
		}
	}

	#pragma warning disable SA1201
	public enum BindMode
	{
		OneWay,
		TwoWay,
	}
}
