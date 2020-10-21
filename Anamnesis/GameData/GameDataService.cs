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
	using Anamnesis.GameData.Sheets;
	using Anamnesis.GameData.ViewModels;
	using Anamnesis.Memory;
	using Anamnesis.Serialization;
	using Lumina.Excel;
	using Lumina.Excel.GeneratedSheets;
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

		public static ExcelSheet<WeatherRate>? WeatherRates { get; protected set; }

		public static ReadOnlyCollection<Prop>? Props { get; private set; }

		public override Task Initialize()
		{
			this.lumina = new LuminaData(MemoryService.GamePath + "\\game\\sqpack\\");
			this.lumina.GetExcelSheet<Race>();

			Races = new Sheet<IRace, Race, RaceViewModel>(this.lumina);
			Tribes = new Sheet<ITribe, Tribe, TribeViewModel>(this.lumina);
			Dyes = new Sheet<IDye, Stain, DyeViewModel>(this.lumina);
			BaseNPCs = new Sheet<INpcBase, ENpcBase, NpcBaseViewModel>(this.lumina);
			Territories = new Sheet<ITerritoryType, TerritoryType, TerritoryTypeViewModel>(this.lumina);
			Weathers = new Sheet<IWeather, Weather, WeatherViewModel>(this.lumina);
			CharacterMakeCustomize = new CustomizeSheet(this.lumina);
			CharacterMakeTypes = new Sheet<ICharaMakeType, CharaMakeType, CharaMakeTypeViewModel>(this.lumina);

			// no view models for these
			WeatherRates = this.lumina.GetExcelSheet<WeatherRate>();

			// props from the props.json file
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
