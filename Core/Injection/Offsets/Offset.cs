// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Offsets
{
	using System;
	using System.Collections.Generic;

	[Serializable]
	public class Offset : IMemoryOffset
	{
		public Offset(params ulong[] offsets)
		{
			this.Offsets = offsets;
		}

		public ulong[] Offsets
		{
			get;
			private set;
		}

		public string Name
		{
			get;
			set;
		}

		public static implicit operator Offset(ulong offset)
		{
			return new Offset(offset);
		}

		public static implicit operator Offset(ulong[] offsets)
		{
			return new Offset(offsets);
		}

		// Special cast for int form constant values.
		public static implicit operator Offset(int[] offsets)
		{
			List<ulong> longOffsets = new List<ulong>();

			foreach (int offset in offsets)
				longOffsets.Add(Convert.ToUInt64(offset));

			return new Offset(longOffsets.ToArray());
		}

		public override string ToString()
		{
			Type type = this.GetType();
			string typeName = type.Name;

			if (type.IsGenericType)
			{
				typeName = typeName.Split('`')[0];
				typeName += "<";

				Type[] generics = type.GetGenericArguments();
				for (int i = 0; i < generics.Length; i++)
				{
					if (i > 1)
						typeName += ", ";

					typeName += generics[i].Name;
				}

				typeName += ">";
			}

			string val = string.Empty;
			for (int i = 0; i < this.Offsets.Length; i++)
			{
				if (i > 1)
					val += ", ";

				val += this.Offsets[i].ToString("X2");
			}

			return typeName + " [" + val + "]";
		}
	}
}
