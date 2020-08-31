// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using Anamnesis.GameData;

	public abstract class GameDataService : ServiceBase<GameDataService>
	{
		public static IData<IRace>? Races { get; protected set; }
		public static IData<ITribe>? Tribes { get; protected set; }
		public static IData<IItem>? Items { get; protected set; }
		public static IData<IDye>? Dyes { get; protected set; }
		public static IData<INpcBase>? BaseNPCs { get; protected set; }
		public static IData<ITerritoryType>? Territories { get; protected set; }
		public static IData<IWeather>? Weathers { get; protected set; }
		public static ICharaMakeCustomizeData? CharacterMakeCustomize { get; protected set; }
		public static IData<ICharaMakeType>? CharacterMakeTypes { get; protected set; }
		public static IData<INpcResident>? ResidentNPCs { get; protected set; }
		public static IData<ITitle>? Titles { get; protected set; }
		public static IData<IStatus>? Statuses { get; protected set; }
	}
}
