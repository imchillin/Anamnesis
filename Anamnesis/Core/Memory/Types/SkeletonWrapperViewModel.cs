// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class SkeletonWrapperViewModel : MemoryViewModelBase<SkeletonWrapper>
	{
		public SkeletonWrapperViewModel(IntPtr pointer, IMemoryViewModel? parent)
			: base(pointer, parent)
		{
		}

		[ModelField] public SkeletonViewModel? Skeleton { get; set; }
	}
}
