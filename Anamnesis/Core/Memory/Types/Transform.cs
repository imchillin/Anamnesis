// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Explicit)]
	public struct Transform
	{
		[FieldOffset(0x00)]
		public Vector Position;

		[FieldOffset(0x10)]
		public Quaternion Rotation;

		[FieldOffset(0x20)]
		public Vector Scale;
	}

	public class TransformViewModel : MemoryViewModelBase<Transform>
	{
		public TransformViewModel(IntPtr pointer, IStructViewModel? parent = null)
			: base(pointer, parent)
		{
		}

		public TransformViewModel(IMemoryViewModel parent, string propertyName)
			: base(parent, propertyName)
		{
		}

		[ModelField] public Vector Position { get; set; }
		[ModelField] public Quaternion Rotation { get; set; }
		[ModelField] public Vector Scale { get; set; }
	}
}
