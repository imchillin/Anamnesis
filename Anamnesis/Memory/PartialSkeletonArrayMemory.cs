// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System;

	// TODO: this should be an actual array
	public class PartialSkeletonArrayMemory : MemoryBase
	{
		// 448 bytes apart pointers
		[Bind(0x140, BindFlags.Pointer)] public RenderSkeletonMemory? Body { get; set; }
		[Bind(0x300, BindFlags.Pointer)] public RenderSkeletonMemory? Head { get; set; }
		[Bind(0x4C0, BindFlags.Pointer)] public RenderSkeletonMemory? Hair { get; set; }
		[Bind(0x680, BindFlags.Pointer)] public RenderSkeletonMemory? Met { get; set; }
		[Bind(0x840, BindFlags.Pointer)] public RenderSkeletonMemory? Top { get; set; }

		protected override bool CanRead(BindInfo bind)
		{
			// This is a (better) hack, but this should be a propper array, not a custom class...
			if (bind.Memory == this)
			{
				if (this.Parent is SkeletonMemory skeletonMemory)
				{
					if (skeletonMemory.Count < 1)
					{
						this.Body?.Dispose();
						this.Body = null;
					}

					if (skeletonMemory.Count < 2)
					{
						this.Head?.Dispose();
						this.Head = null;
					}

					if (skeletonMemory.Count < 3)
					{
						this.Hair?.Dispose();
						this.Hair = null;
					}

					if (skeletonMemory.Count < 4)
					{
						this.Met?.Dispose();
						this.Met = null;
					}

					if (skeletonMemory.Count < 5)
					{
						this.Top?.Dispose();
						this.Top = null;
					}

					if (skeletonMemory.Count >= 1 && bind.Name == nameof(PartialSkeletonArrayMemory.Body))
						return true;

					if (skeletonMemory.Count >= 2 && bind.Name == nameof(PartialSkeletonArrayMemory.Head))
						return true;

					if (skeletonMemory.Count >= 3 && bind.Name == nameof(PartialSkeletonArrayMemory.Hair))
						return true;

					if (skeletonMemory.Count >= 4 && bind.Name == nameof(PartialSkeletonArrayMemory.Met))
						return true;

					if (skeletonMemory.Count >= 5 && bind.Name == nameof(PartialSkeletonArrayMemory.Top))
						return true;

					return false;
				}
			}

			return base.CanRead(bind);
		}
	}
}
