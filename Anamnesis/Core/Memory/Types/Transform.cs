// © Anamnesis.
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

	public class TransformViewModel : StructViewModelBase<Transform>, ITransform
	{
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

	#pragma warning disable SA1402
	public class TransformPtrViewModel : MemoryViewModelBase<Transform>, ITransform
	{
		public TransformPtrViewModel(IntPtr pointer, IMemoryViewModel? parent)
			: base(pointer, parent)
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
