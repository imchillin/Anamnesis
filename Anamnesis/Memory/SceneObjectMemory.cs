// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

/// <summary>
/// Represents a scene (i.e., GPose) object in memory.
/// </summary>
public class SceneObjectMemory : MemoryBase
{
	// NOTE: Do not declare NextSiblingObject in the base scene object class!
	// Object traversal explodes during memory synchronization.
	// [Bind(0x028, BindFlags.Pointer)] public SceneObjectMemory? NextSiblingObject { get; set; }

	[Bind(0x030, BindFlags.Pointer)] public SceneObjectMemory? ChildObject { get; set; }

	/// <summary>
	/// Gets or sets the scene object's transform memory (position, rotation, scale).
	/// </summary>
	[Bind(0x050)] public TransformMemory? Transform { get; set; }
}
