// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System;

	public interface IImage
	{
		IntPtr HBitmap { get; }
		int Width { get; }
		int Height { get; }
	}
}
