// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Styles.DependencyProperties
{
	using System.Windows;

	public class DependencyProperty<TValue> : IBind<TValue>
	{
		private DependencyProperty dp;

		public DependencyProperty(DependencyProperty dp)
		{
			this.dp = dp;
		}

		public TValue Get(DependencyObject control)
		{
			return (TValue)control.GetValue(this.dp);
		}

		public void Set(DependencyObject control, TValue value)
		{
			TValue old = this.Get(control);

			if (old != null && old.Equals(value))
				return;

			control.SetValue(this.dp, value);
		}
	}
}
