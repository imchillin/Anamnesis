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
		public Customize.Races Race => (Customize.Races)this.Key;
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
					Customize.Races.Hyur => new[]
					{
						GameDataService.Tribes.Get((byte)Customize.Tribes.Midlander),
						GameDataService.Tribes.Get((byte)Customize.Tribes.Highlander),
					},

					Customize.Races.Elezen => new[]
					{
						GameDataService.Tribes.Get((byte)Customize.Tribes.Wildwood),
						GameDataService.Tribes.Get((byte)Customize.Tribes.Duskwight),
					},

					Customize.Races.Lalafel => new[]
					{
						GameDataService.Tribes.Get((byte)Customize.Tribes.Plainsfolk),
						GameDataService.Tribes.Get((byte)Customize.Tribes.Dunesfolk),
					},

					Customize.Races.Miqote => new[]
					{
						GameDataService.Tribes.Get((byte)Customize.Tribes.SeekerOfTheSun),
						GameDataService.Tribes.Get((byte)Customize.Tribes.KeeperOfTheMoon),
					},

					Customize.Races.Roegadyn => new[]
					{
						GameDataService.Tribes.Get((byte)Customize.Tribes.SeaWolf),
						GameDataService.Tribes.Get((byte)Customize.Tribes.Hellsguard),
					},

					Customize.Races.AuRa => new[]
					{
						GameDataService.Tribes.Get((byte)Customize.Tribes.Raen),
						GameDataService.Tribes.Get((byte)Customize.Tribes.Xaela),
					},

					Customize.Races.Hrothgar => new[]
					{
						GameDataService.Tribes.Get((byte)Customize.Tribes.Helions),
						GameDataService.Tribes.Get((byte)Customize.Tribes.TheLost),
					},

					Customize.Races.Viera => new[]
					{
						GameDataService.Tribes.Get((byte)Customize.Tribes.Rava),
						GameDataService.Tribes.Get((byte)Customize.Tribes.Veena),
					},

					_ => throw new Exception($"Unrecognized race {this.Race}"),
				};
			}
		}
	}
}
