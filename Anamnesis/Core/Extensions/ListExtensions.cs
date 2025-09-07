// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Core.Extensions;

using System;
using System.Collections.Generic;

public static class ListExtensions
{
	private static readonly Random s_random = new Random();

	public static void Shuffle<T>(this IList<T> list)
	{
		int n = list.Count;
		while (n > 1)
		{
			n--;
			int k = s_random.Next(n + 1);
			(list[n], list[k]) = (list[k], list[n]);
		}
	}
}
