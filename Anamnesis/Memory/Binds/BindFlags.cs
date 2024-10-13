// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

/// <summary>
/// Flags used to specify binding behaviors in memory operations.
/// </summary>
public enum BindFlags
{
	None = 0,
	Pointer = 1,
	ActorRefresh = 2,
	DontCacheOffsets = 4,
	OnlyInGPose = 8,
	DontRecordHistory = 16,
	WeaponRefresh = 32,
}
