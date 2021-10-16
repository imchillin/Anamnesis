// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class TransformViewModel : StructViewModelBase<Transform>
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
}
