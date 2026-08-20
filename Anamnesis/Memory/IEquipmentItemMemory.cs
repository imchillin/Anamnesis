// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using Anamnesis.GameData;
using System.ComponentModel;

public interface IEquipmentItemMemory : INotifyPropertyChanged
{
	ushort Base { get; set; }
	byte Dye { get; set; }
	byte Dye2 { get; set; }
	ushort Set { get; set; }
	IItem? EquippedItem { get; set; }

	public void SwapDyeChannels();
	public void Clear(bool isPlayer);
}
