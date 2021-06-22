// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Runtime.InteropServices;

	// We dont know what this structure is
	[StructLayout(LayoutKind.Explicit)]
	public struct SkeletonWrapper
	{
		[FieldOffset(0x68)] public IntPtr Skeleton;
	}

	public class SkeletonWrapperViewModel : MemoryViewModelBase<SkeletonWrapper>
	{
		public SkeletonWrapperViewModel(IntPtr pointer, IMemoryViewModel? parent)
			: base(pointer, parent)
		{
		}

		[ModelField] public SkeletonViewModel? Skeleton { get; set; }
	}
}
