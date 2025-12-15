// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

/// <summary>
/// Represents a drawable scene object in memory.
/// </summary>
public class DrawObjectMemory : SceneObjectMemory
{
	/// <summary>
	/// Gets or sets the flags for the drawable object.
	/// </summary>
	[Bind(0x88)] public byte Flags { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the object is visible.
	/// </summary>
	public bool IsVisible
	{
		get => (this.Flags & 0x09) == 0x09;
		set => this.Flags = (byte)(value ? this.Flags | 0x09 : this.Flags & ~0x09);
	}
}
