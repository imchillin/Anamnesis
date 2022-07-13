// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Extensions;

using System.Windows;
using System.Windows.Media.Animation;

public static class StoryboardExtensions
{
	public static void BeginStoryboard(this FrameworkElement self, string name, double speed = 1.0)
	{
		Storyboard sb = self.GetResource<Storyboard>(name);
		sb.Begin();
		sb.SpeedRatio = speed;
	}

	public static void StopStoryboard(this FrameworkElement self, string name)
	{
		Storyboard sb = self.GetResource<Storyboard>(name);
		sb.Stop();
	}
}
