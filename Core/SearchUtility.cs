// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	public static class SearchUtility
	{
		public static bool Matches(string input, string[] querry)
		{
			if (querry == null)
				return true;

			input = input.ToLower();

			bool matchesSearch = true;
			foreach (string str in querry)
				matchesSearch &= input.Contains(str);

			if (!matchesSearch)
			{
				return false;
			}

			return true;
		}
	}
}
