// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Memory
{
	using System;
	using System.Text;

	public class StringMemory : MemoryBase<string>
	{
		private int length = 32;
		private bool zeroTerminated = true;

		public StringMemory(ProcessInjection process, UIntPtr address)
			: base(process, address)
		{
		}

		protected override string Read()
		{
			byte[] read = this.ReadBytes(this.length);

			if (this.zeroTerminated)
			{
				return Encoding.UTF8.GetString(read).Split('\0')[0];
			}
			else
			{
				return Encoding.UTF8.GetString(read);
			}
		}

		protected override void Write(string value)
		{
			value = value.Replace("\0", string.Empty);
			value += "\0\0\0\0";

			byte[] data = Encoding.UTF8.GetBytes(value);
			this.WriteBytes(data);
		}
	}
}
