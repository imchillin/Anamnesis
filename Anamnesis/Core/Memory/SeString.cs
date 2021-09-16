// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Core.Memory
{
	using System;
	using System.Text;

	public static class SeString
	{
		public static string FromSeStringBytes(byte[] self)
		{
			int i;
			for (i = 0; i < self.Length; i++)
			{
				if (self[i] == 0)
					break;
			}

			return Encoding.UTF8.GetString(self, 0, i);
		}

		public static byte[] ToSeStringBytes(string self)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(self);

			if (bytes.Length >= 30)
			{
				throw new Exception($"{self} too long");
			}

			return bytes;
		}
	}
}
