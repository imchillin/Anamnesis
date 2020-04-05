// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	public interface IImage
	{
		IntPtr HBitmap { get; }
		int Width { get; }
		int Height { get; }
	}
}
