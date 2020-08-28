// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis
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
