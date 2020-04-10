// Concept Matrix 3.
// Licensed under the MIT license.

namespace System.Collections.Generic
{
	public static class IEnumerableExtensions
	{
		public static T First<T>(this IEnumerable<T> self)
		{
			foreach (T val in self)
			{
				return val;
			}

			return default;
		}
	}
}
