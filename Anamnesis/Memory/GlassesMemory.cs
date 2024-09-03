// © Anamnesis.
// Licensed under the MIT license.
namespace Anamnesis.Memory;

using Anamnesis.GameData.Excel;
using System.ComponentModel;

public class GlassesMemory : MemoryBase, INotifyPropertyChanged
{
	[Bind(0x000, BindFlags.ActorRefresh)] public ushort GlassesId { get; set; }

	public void Clear()
	{
		this.GlassesId = 0;
	}

	public void Equip(Glasses glasses)
	{
		this.GlassesId = glasses.GlassesId;
	}

	public bool Is(Glasses? glasses)
	{
		if (glasses == null)
			return false;

		return this.GlassesId == glasses.GlassesId;
	}
}
