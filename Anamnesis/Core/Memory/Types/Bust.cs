// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Explicit)]
	public struct Bust
	{
		[FieldOffset(0x68)] public Vector Scale;
	}

	public class BustViewModel : MemoryViewModelBase<Bust>
	{
		public BustViewModel(IntPtr pointer, IStructViewModel? parent = null)
			: base(pointer, parent)
		{
		}

		[ModelField] public Vector Scale { get; set; }
	}
}
