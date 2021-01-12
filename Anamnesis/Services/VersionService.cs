// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System.IO;
	using System.Threading.Tasks;
	using Anamnesis.Memory;
	using Anamnesis.Updater;

	public class VersionService : ServiceBase<VersionService>
	{
		public override Task Initialize()
		{
			string versionStr = File.ReadAllText(UpdateService.VersionFile);
			versionStr = versionStr.Split('\r')[1].Trim();

			string file = MemoryService.GamePath + "game/ffxivgame.ver";
			string gameVer = File.ReadAllText(file);

			if (gameVer != versionStr)
			{
				Log.Error(LocalizationService.GetStringFormatted("Error_WrongVersion", gameVer));
			}

			return base.Initialize();
		}
	}
}
