// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	[AttributeUsage(AttributeTargets.Property)]
	public class ViewModelOffsetAttribute : Attribute
	{
		public readonly int Offset;

		public ViewModelOffsetAttribute(int offset)
		{
			this.Offset = offset;
		}
	}
}
