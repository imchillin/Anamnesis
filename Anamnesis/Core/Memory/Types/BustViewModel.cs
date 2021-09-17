// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class BustViewModel : MemoryViewModelBase<Bust>
	{
		public BustViewModel(IntPtr pointer, IMemoryViewModel? parent)
			: base(pointer, parent)
		{
		}

		[ModelField] public Vector Scale { get; set; }
	}
}
