// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

public class AnimationMemory : MemoryBase
{
	public const int AnimationSlotCount = 14;

	public enum AnimationSlots : int
	{
		FullBody = 0,
		UpperBody = 1,
		Facial = 2,
		Add = 3,
		Lips = 7,
		Parts1 = 8,
		Parts2 = 9,
		Parts3 = 10,
		Parts4 = 11,
		Overlay = 12,
	}

	[Bind(0x000)] public AnimationIdArrayMemory? AnimationIds { get; set; }
	[Bind(0x074)] public AnimationIdArrayMemory? Speeds { get; set; }
	[Bind(0x1EC)] public ushort BaseOverride { get; set; }

	public class AnimationIdArrayMemory : InplaceFixedArrayMemory<ushort>
	{
		public override int ElementSize => sizeof(ushort);
		public override int Count => AnimationSlotCount;
	}

	public class AnimationSpeedArrayMemory : InplaceFixedArrayMemory<float>
	{
		public override int ElementSize => sizeof(float);
		public override int Count => AnimationSlotCount;
	}
}
