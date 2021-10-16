// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Runtime.InteropServices;
	using System.Text;

	public struct SeString
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
		public byte[]? Bytes;

		public override string ToString()
		{
			if (this.Bytes == null)
				return string.Empty;

			int i;
			for (i = 0; i < this.Bytes.Length; i++)
			{
				if (this.Bytes[i] == 0)
					break;
			}

			return Encoding.UTF8.GetString(this.Bytes, 0, i);
		}

		public void FromString(string self)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(self);

			if (bytes.Length >= 30)
				throw new Exception($"SeString value {self} excedes 30 byte limit");

			this.Bytes = bytes;
		}
	}
}
