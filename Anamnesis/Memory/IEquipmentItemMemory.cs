// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System.ComponentModel;

public interface IEquipmentItemMemory : INotifyPropertyChanged
{
	ushort Base { get; set; }
	byte Dye { get; set; }
	ushort Set { get; set; }

	public void Clear(bool isPlayer);
}
