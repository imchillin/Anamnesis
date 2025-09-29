// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis;

using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

public static class VisualUtils
{
	/// <summary>
	/// Finds all visual children of a specific type within a given <see cref="DependencyObject"/>.
	/// </summary>
	/// <typeparam name="T">The target type of visual children to find.</typeparam>
	/// <param name="depObj">The parent <see cref="DependencyObject"/> to search within.</param>
	/// <returns>An enumerable of all found visual children of the specified type.</returns>
	/// <remarks>
	/// Based on code from https://stackoverflow.com/a/9229255.
	/// </remarks>
	public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj)
		where T : DependencyObject
	{
		if (depObj == null)
			yield break;

		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
		{
			DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
			if (child is T t)
				yield return t;

			foreach (T childOfChild in FindVisualChildren<T>(child))
				yield return childOfChild;
		}
	}
}