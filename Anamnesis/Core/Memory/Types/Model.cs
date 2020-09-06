// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using PropertyChanged;

	[StructLayout(LayoutKind.Explicit)]
	public struct Model
	{
		[FieldOffset(0x50)] public Transform Transform;
		[FieldOffset(0xA0)] public IntPtr Skeleton;
	}

	public class ModelViewModel : MemoryViewModelBase<Model>
	{
		public ModelViewModel(IntPtr pointer, IStructViewModel? parent = null)
			: base(pointer, parent)
		{
		}

		public ModelViewModel(IMemoryViewModel parent, string propertyName)
			: base(parent, propertyName)
		{
		}

		[ModelField] public TransformViewModel? Transform { get; set; }
		[ModelField] public SkeletonWrapperViewModel? Skeleton { get; set; }
	}
}
