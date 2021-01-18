// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using PropertyChanged;

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

		public SkeletonWrapperViewModel(IMemoryViewModel parent, string propertyName)
			: base(parent, propertyName)
		{
		}

		[ModelField] public SkeletonViewModel? Skeleton { get; set; }
	}
}
