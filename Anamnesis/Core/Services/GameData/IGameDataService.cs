// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GameData
{
	public interface IGameDataService : IService
	{
		IData<IRace> Races { get; }
		IData<ITribe> Tribes { get; }
		IData<IItem> Items { get; }
		IData<IDye> Dyes { get; }
		IData<INpcBase> BaseNPCs { get; }
		IData<ITerritoryType> Territories { get; }
		IData<IWeather> Weathers { get; }
		ICharaMakeCustomizeData CharacterMakeCustomize { get; }
		IData<ICharaMakeType> CharacterMakeTypes { get; }
		IData<INpcResident> ResidentNPCs { get; }
		IData<ITitle> Titles { get; }
		IData<IStatus> Statuses { get; }
	}
}
