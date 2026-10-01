// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using Anamnesis.GameData;
using System.ComponentModel;

public interface IEquipmentItemMemory : INotifyPropertyChanged
{
	ushort Set { get; set; }
	ushort Base { get; set; }
	ushort Variant { get; set; }
	byte Dye { get; set; }
	byte Dye2 { get; set; }
	IItem? EquippedItem { get; set; }

	public void SwapDyeChannels();
	public void Clear(bool isPlayer);
}
