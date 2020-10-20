// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Threading.Tasks;
	using Anamnesis.Character;
	using Anamnesis.GameData;
	using Anamnesis.Memory;
	using Anamnesis.Serialization;

	using LuminaData = Lumina.Lumina;

	public abstract class GameDataService : ServiceBase<GameDataService>
	{
		private LuminaData? lumina;

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

		public static ReadOnlyCollection<Prop>? Props { get; private set; }

		public override Task Initialize()
		{
			this.lumina = new LuminaData(MemoryService.GamePath + "\\game\\sqpack\\");

			try
			{
				List<Prop> propList = SerializerService.DeserializeFile<List<Prop>>("Props.json");

				propList.Sort((a, b) =>
				{
					return a.Name.CompareTo(b.Name);
				});

				Props = propList.AsReadOnly();
			}
			catch (Exception ex)
			{
				Log.Write(new Exception("Failed to load props list", ex));
			}

			return base.Initialize();
		}
	}
}
