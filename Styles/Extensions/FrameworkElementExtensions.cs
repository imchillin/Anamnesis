// Concept Matrix 3.
// Licensed under the MIT license.

namespace System.Windows
{
	using System;
	using System.Windows.Media.Animation;

	public static class FrameworkElementExtensions
	{
		public static void Animate(this FrameworkElement self, DependencyProperty property, double to, int durationMs)
		{
			Storyboard story = new Storyboard();
			DoubleAnimation anim = new DoubleAnimation(to, new Duration(TimeSpan.FromMilliseconds(durationMs)));
			Storyboard.SetTarget(anim, self);
			Storyboard.SetTargetProperty(anim, new PropertyPath(property));
			story.Children.Add(anim);
			story.Begin();
		}
	}
}
