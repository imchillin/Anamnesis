// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;
	using System.Reflection;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class SkeletonViewModel : MemoryViewModelBase<Skeleton>
	{
		public SkeletonViewModel(IntPtr pointer, IMemoryViewModel? parent)
			: base(pointer, parent)
		{
		}

		[ModelField] public BonesViewModel? Body { get; set; }
		[ModelField] public BonesViewModel? Head { get; set; }
		[ModelField] public BonesViewModel? Hair { get; set; }
		[ModelField] public BonesViewModel? Met { get; set; }
		[ModelField] public BonesViewModel? Top { get; set; }

		protected override bool HandleModelToViewUpdate(PropertyInfo viewModelProperty, FieldInfo modelField)
		{
			// This is a hack, but only player models seem to have  Head, Gair, Met, and Top skeletons. everyone else gets gibberish memory
			// that really confuses the marshaler
			if (this.Parent?.Parent?.Parent is ActorMemory actor)
			{
				if (actor.ModelType != 0 && viewModelProperty.Name != nameof(SkeletonViewModel.Body))
				{
					return false;
				}
			}

			return base.HandleModelToViewUpdate(viewModelProperty, modelField);
		}
	}
}
