// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;

	public class SkeletonMemory : MemoryBase
	{
		[Bind(0x140, BindFlags.Pointer)] public BonesMemory? Body { get; set; }
		[Bind(0x300, BindFlags.Pointer)] public BonesMemory? Head { get; set; }
		[Bind(0x4C0, BindFlags.Pointer)] public BonesMemory? Hair { get; set; }
		[Bind(0x680, BindFlags.Pointer)] public BonesMemory? Met { get; set; }
		[Bind(0x840, BindFlags.Pointer)] public BonesMemory? Top { get; set; }

		protected override bool ShouldBind(BindInfo bind)
		{
			// This is a hack, but only player models seem to have  Head, Gair, Met, and Top skeletons. everyone else gets gibberish memory
			// that really confuses the marshaler
			if (this.Parent?.Parent?.Parent is ActorMemory actor)
			{
				if (actor.ModelType != 0 && bind.Name != nameof(SkeletonMemory.Body))
				{
					return false;
				}
			}

			return base.ShouldBind(bind);
		}
	}
}
