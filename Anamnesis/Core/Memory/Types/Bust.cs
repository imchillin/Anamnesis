// © Anamnesis.
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
		public BustViewModel(IntPtr pointer, IMemoryViewModel? parent)
			: base(pointer, parent)
		{
		}

		[ModelField] public Vector Scale { get; set; }
	}
}
