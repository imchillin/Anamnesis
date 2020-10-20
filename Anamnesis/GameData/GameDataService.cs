// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Security.Policy;
	using System.Threading.Tasks;
	using Anamnesis.Character;
	using Anamnesis.GameData;
	using Anamnesis.GameData.Wrappers;
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

		public static ReadOnlyCollection<Prop>? Props { get; private set; }

		public override Task Initialize()
		{
			this.lumina = new LuminaData(MemoryService.GamePath + "\\game\\sqpack\\");
			this.lumina.GetExcelSheet<Race>();

			Races = new Database<IRace, Race, RaceWrapper>(this.lumina);

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

		private class Database<TInterfaceType, TConcreteType, TWrapperType> : IData<TInterfaceType>
			where TInterfaceType : IDataObject
			where TConcreteType : class, IExcelRow
			where TWrapperType : ExcelRowWrapper<TConcreteType>, TInterfaceType
		{
			private ExcelSheet<TConcreteType> excel;
			private Dictionary<int, TWrapperType> wrapperCache = new Dictionary<int, TWrapperType>();
			private List<TInterfaceType>? all;

			public Database(LuminaData lumina)
			{
				this.excel = lumina.GetExcelSheet<TConcreteType>();
			}

			public IEnumerable<TInterfaceType> All
			{
				get
				{
					if (this.all == null)
					{
						this.all = new List<TInterfaceType>();
						for (int i = 1; i < this.excel.RowCount; i++)
						{
							this.all.Add(this.Get(i));
						}
					}

					return this.all;
				}
			}

			public TInterfaceType Get(int key)
			{
				if (!this.wrapperCache.ContainsKey(key))
				{
					TWrapperType? wrapper = Activator.CreateInstance(typeof(TWrapperType), key, this.excel) as TWrapperType;

					if (wrapper == null)
						throw new Exception($"Failed to create instance of Lumina data wrapper: {typeof(TWrapperType)}");

					this.wrapperCache.Add(key, wrapper);
				}

				return this.wrapperCache[key];
			}

			public TInterfaceType Get(byte key)
			{
				return this.Get((int)key);
			}
		}
	}
}
