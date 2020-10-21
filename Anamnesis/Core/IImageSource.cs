// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;

	[Obsolete]
	public interface IImageSource
	{
		IImage GetImage();
	}

	[Obsolete]
	public interface IImage : IDisposable
	{
		IntPtr HBitmap { get; }
	}
}
