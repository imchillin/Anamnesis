// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System;

	public interface IImageSource
	{
		IImage GetImage();
	}

	public interface IImage : IDisposable
	{
		IntPtr HBitmap { get; }
	}
}
