// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
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
