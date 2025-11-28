// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

public class ExtendedWeaponMemory : WeaponSubModelMemory
{
	/// <summary>
	/// Gets or sets the off-hand weapon memory object.
	/// </summary>
	[Bind(0x028, BindFlags.Pointer)] public WeaponSubModelMemory? NextSiblingObject { get; set; }
}
