// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	public class RenderSkeletonMemory : MemoryBase
	{
		////[Bind(0x000, BindFlags.Pointer)] public IntPtr HkAnimationFile;
		[Bind(0x010)] public TransformArrayMemory? Transforms { get; set; }

		public class TransformArrayMemory : ArrayMemory<TransformMemory, int>
		{
		}
	}
}
