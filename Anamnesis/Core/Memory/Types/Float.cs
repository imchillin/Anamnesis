// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	public static class Float
	{
		public static bool IsValid(float? number)
		{
			if (number == null)
				return false;

			float v = (float)number;

			if (float.IsInfinity(v))
				return false;

			if (float.IsNaN(v))
				return false;

			return true;
		}
	}
}
