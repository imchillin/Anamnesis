// © Anamnesis.
// Developed by W and A Walsh.
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
		public Appearance.Races Race => (Appearance.Races)this.Key;
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
					Appearance.Races.Hyur => new[]
					{
						GameDataService.Tribes.Get((byte)Appearance.Tribes.Midlander),
						GameDataService.Tribes.Get((byte)Appearance.Tribes.Highlander),
					},

					Appearance.Races.Elezen => new[]
					{
						GameDataService.Tribes.Get((byte)Appearance.Tribes.Wildwood),
						GameDataService.Tribes.Get((byte)Appearance.Tribes.Duskwight),
					},

					Appearance.Races.Lalafel => new[]
					{
						GameDataService.Tribes.Get((byte)Appearance.Tribes.Plainsfolk),
						GameDataService.Tribes.Get((byte)Appearance.Tribes.Dunesfolk),
					},

					Appearance.Races.Miqote => new[]
					{
						GameDataService.Tribes.Get((byte)Appearance.Tribes.SeekerOfTheSun),
						GameDataService.Tribes.Get((byte)Appearance.Tribes.KeeperOfTheMoon),
					},

					Appearance.Races.Roegadyn => new[]
					{
						GameDataService.Tribes.Get((byte)Appearance.Tribes.SeaWolf),
						GameDataService.Tribes.Get((byte)Appearance.Tribes.Hellsguard),
					},

					Appearance.Races.AuRa => new[]
					{
						GameDataService.Tribes.Get((byte)Appearance.Tribes.Raen),
						GameDataService.Tribes.Get((byte)Appearance.Tribes.Xaela),
					},

					Appearance.Races.Hrothgar => new[]
					{
						GameDataService.Tribes.Get((byte)Appearance.Tribes.Helions),
						GameDataService.Tribes.Get((byte)Appearance.Tribes.TheLost),
					},

					Appearance.Races.Viera => new[]
					{
						GameDataService.Tribes.Get((byte)Appearance.Tribes.Rava),
						GameDataService.Tribes.Get((byte)Appearance.Tribes.Veena),
					},

					_ => throw new Exception($"Unrecognized race {this.Race}"),
				};
			}
		}
	}
}
