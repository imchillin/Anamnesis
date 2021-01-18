// © Anamnesis.
// Developed by W and A Walsh.
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

	public class TransformViewModel : MemoryViewModelBase<Transform>, ITransform
	{
		public TransformViewModel(IntPtr pointer, IMemoryViewModel? parent)
			: base(pointer, parent)
		{
		}

		public TransformViewModel(IMemoryViewModel parent, string propertyName)
			: base(parent, propertyName)
		{
		}

		public bool CanTranslate => true;
		[ModelField] public Vector Position { get; set; }

		public bool CanRotate => true;
		[ModelField] public Quaternion Rotation { get; set; }

		public bool CanScale => true;
		[ModelField] public Vector Scale { get; set; }
	}
}
