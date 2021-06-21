// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System.Text.RegularExpressions;

	public static class SearchUtility
	{
		#pragma warning disable SA1011
		public static bool Matches(string? input, string[]? query)
		{
			if (input == null)
				return false;

			if (query == null)
				return true;

			input = input.ToLower();
			input = Regex.Replace(input, @"[^\w\d\s]", string.Empty);

			bool matchesSearch = true;
			foreach (string str in query)
			{
				// ignore 'the'
				if (str == "the")
					continue;

				// ignore all symbols
				string strB = Regex.Replace(str, @"[^\w\d\s]", string.Empty);
				matchesSearch &= input.Contains(strB);
			}

			if (!matchesSearch)
			{
				return false;
			}

			return true;
		}
	}
}
