// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Styles.DependencyProperties
{
	using System;
	using System.Reflection;
	using System.Windows;
	using System.Windows.Data;

	public class Binder
	{
		public static DependencyProperty<TValue> Register<TValue, TOwner>(string propertyName, BindMode mode)
		{
			Action<DependencyObject, DependencyPropertyChangedEventArgs> callback = (d, e) => { };
			return Register<TValue, TOwner>(propertyName, new PropertyChangedCallback(callback), mode);
		}

		public static DependencyProperty<TValue> Register<TValue, TOwner>(string propertyName, Action<TOwner, TValue>? changed = null, BindMode mode = BindMode.TwoWay)
		{
			Action<DependencyObject, DependencyPropertyChangedEventArgs> callback = (d, e) =>
			{
				if (d is TOwner owner && e.NewValue is TValue value)
				{
					changed?.Invoke(owner, value);
				}
			};

			return Register<TValue, TOwner>(propertyName, new PropertyChangedCallback(callback), mode);
		}

		public static DependencyProperty<TValue> Register<TValue, TOwner>(string propertyName, Action<TOwner, TValue, TValue> changed, BindMode mode = BindMode.TwoWay)
		{
			Action<DependencyObject, DependencyPropertyChangedEventArgs> callback = (d, e) =>
			{
				if (d is TOwner owner && e.OldValue is TValue oldValue && e.NewValue is TValue newValue)
				{
					changed?.Invoke(owner, oldValue, newValue);
				}
			};

			return Register<TValue, TOwner>(propertyName, new PropertyChangedCallback(callback), mode);
		}

		private static DependencyProperty<TValue> Register<TValue, TOwner>(string propertyName, PropertyChangedCallback callback, BindMode mode)
		{
			PropertyInfo? property = typeof(TOwner).GetProperty(propertyName);
			if (property == null)
				throw new Exception("Failed to locate property: \"" + propertyName + "\" on type: \"" + typeof(TOwner) + "\" for binding.");

			FrameworkPropertyMetadata meta = new FrameworkPropertyMetadata(new PropertyChangedCallback(callback));
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
