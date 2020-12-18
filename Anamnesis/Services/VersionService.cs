// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System.IO;
	using System.Threading.Tasks;
	using Anamnesis.Memory;

	public class VersionService : ServiceBase<VersionService>
	{
		private const string SupportedVersion = "2020.12.02.0000.0000";

		public override Task Initialize()
		{
			string file = MemoryService.GamePath + "game/ffxivgame.ver";
			string gameVer = File.ReadAllText(file);

			if (gameVer != SupportedVersion)
			{
				Log.Write(SimpleLog.Severity.Error, LocalizationService.GetStringFormatted("Error_WrongVersion", gameVer));
			}

			return base.Initialize();
		}
	}
}
