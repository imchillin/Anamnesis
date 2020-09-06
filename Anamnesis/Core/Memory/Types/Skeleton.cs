// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using PropertyChanged;

	[StructLayout(LayoutKind.Explicit)]
	public struct Skeleton
	{
		[FieldOffset(0x140)] public IntPtr Body;
		////[FieldOffset(0x300)] public IntPtr Head;
		////[FieldOffset(0x4C0)] public IntPtr Hair;
		////[FieldOffset(0x680)] public IntPtr Met;
		////[FieldOffset(0x840)] public IntPtr Top;
	}

	public class SkeletonViewModel : MemoryViewModelBase<Skeleton>
	{
		public SkeletonViewModel(IntPtr pointer, IStructViewModel? parent = null)
			: base(pointer, parent)
		{
		}

		public SkeletonViewModel(IMemoryViewModel parent, string propertyName)
			: base(parent, propertyName)
		{
		}

		[ModelField] public BonesViewModel? Body { get; set; }
		////[ModelField] public BonesViewModel? Head { get; set; }
		////[ModelField] public BonesViewModel? Hair { get; set; }
		////[ModelField] public BonesViewModel? Met { get; set; }
		////[ModelField] public BonesViewModel? Top { get; set; }
	}
}
