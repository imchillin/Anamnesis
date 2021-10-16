// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.ViewModels
{
	using System;
	using System.Collections.Generic;
	using Anamnesis.Memory;
	using Anamnesis.Services;
	using Lumina;
	using Lumina.Excel;
	using Lumina.Excel.GeneratedSheets;

	public class RaceViewModel : ExcelRowViewModel<Race>, IRace
	{
		public RaceViewModel(uint key, ExcelSheet<Race> sheet, GameData lumina)
			: base(key, sheet, lumina)
		{
		}

		public override string Name => this.Race.ToString();
		public ActorCustomizeMemory.Races Race => (ActorCustomizeMemory.Races)this.Key;
		public string Feminine => this.Value.Feminine;
		public string Masculine => this.Value.Masculine;
		public string DisplayName => this.Value.Masculine;

		public ITribe[] Tribes
		{
			get
			{
				if (GameDataService.Tribes == null)
					throw new Exception("No Tribes list in game data service");

				return this.Race switch
				{
					ActorCustomizeMemory.Races.Hyur => new[]
					{
						GameDataService.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Midlander),
						GameDataService.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Highlander),
					},

					ActorCustomizeMemory.Races.Elezen => new[]
					{
						GameDataService.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Wildwood),
						GameDataService.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Duskwight),
					},

					ActorCustomizeMemory.Races.Lalafel => new[]
					{
						GameDataService.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Plainsfolk),
						GameDataService.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Dunesfolk),
					},

					ActorCustomizeMemory.Races.Miqote => new[]
					{
						GameDataService.Tribes.Get((byte)ActorCustomizeMemory.Tribes.SeekerOfTheSun),
						GameDataService.Tribes.Get((byte)ActorCustomizeMemory.Tribes.KeeperOfTheMoon),
					},

					ActorCustomizeMemory.Races.Roegadyn => new[]
					{
						GameDataService.Tribes.Get((byte)ActorCustomizeMemory.Tribes.SeaWolf),
						GameDataService.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Hellsguard),
					},

					ActorCustomizeMemory.Races.AuRa => new[]
					{
						GameDataService.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Raen),
						GameDataService.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Xaela),
					},

					ActorCustomizeMemory.Races.Hrothgar => new[]
					{
						GameDataService.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Helions),
						GameDataService.Tribes.Get((byte)ActorCustomizeMemory.Tribes.TheLost),
					},

					ActorCustomizeMemory.Races.Viera => new[]
					{
						GameDataService.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Rava),
						GameDataService.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Veena),
					},

					_ => throw new Exception($"Unrecognized race {this.Race}"),
				};
			}
		}
	}
}
