// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System.ComponentModel;

	public interface IEquipmentItemMemory : INotifyPropertyChanged
	{
		ushort Base { get; set; }
		byte Dye { get; set; }
	}
}
