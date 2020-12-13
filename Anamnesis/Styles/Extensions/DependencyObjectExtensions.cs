// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace System.Windows
{
	using System.Collections.Generic;
	using System.Windows.Media;

	public static class DependencyObjectExtensions
	{
		public static T? FindParent<T>(this DependencyObject child)
			where T : DependencyObject
		{
			DependencyObject parentObject = VisualTreeHelper.GetParent(child);

			if (parentObject == null)
				return null;

			T? parent = parentObject as T;

			if (parent != null)
			{
				return parent;
			}
			else
			{
				return parentObject.FindParent<T>();
			}
		}

		public static List<T> FindChildren<T>(this DependencyObject self)
		{
			List<T> results = new List<T>();
			self.FindChildren<T>(ref results);
			return results;
		}

		public static void FindChildren<T>(this DependencyObject self, ref List<T> results)
		{
			int children = VisualTreeHelper.GetChildrenCount(self);
			for (int i = 0; i < children; i++)
			{
				DependencyObject? child = VisualTreeHelper.GetChild(self, i);

				if (child is T tChild)
					results.Add(tChild);

				child.FindChildren(ref results);
			}
		}
	}
}
