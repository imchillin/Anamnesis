// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;

/// <summary>
/// A constant buffer that is used to send values to GPU shaders.
/// </summary>
/// <remarks>
/// Credited to FFXIVClientStructs as this implementation is based on
/// their work, with some modifications to fit Anamnesis' syntax.
/// </remarks>
/// <typeparam name="T">
/// The underlying type of the constant buffer.
/// </typeparam>
public class ConstantBufferMemory<T> : MemoryBase
	where T : class
{
	[Bind(0x024)] public int Flags { get; set; }
	[Bind(0x028, BindFlags.Pointer)] public T? UnsafeSourcePtr { get; set; }

	public T? TryGetSourcePtr() => (this.Flags & 0x4003) == 0 ? this.UnsafeSourcePtr : null;

	protected override bool CanRead(BindInfo bind)
	{
		if (bind.Name == nameof(this.UnsafeSourcePtr))
		{
			// Don't allow reading if the source pointer indicates that the buffer is invalid
			if ((this.Flags & 0x4003) != 0)
			{
				if (this.UnsafeSourcePtr != null)
				{
					if (this.UnsafeSourcePtr is IDisposable disposable)
						disposable.Dispose();

					this.UnsafeSourcePtr = null;
				}

				return false;
			}
		}

		return base.CanRead(bind);
	}
}
