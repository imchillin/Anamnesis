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
}
