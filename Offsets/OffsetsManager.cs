// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Offsets
{
	using System;
	using System.IO;
	using System.Net;
	using System.Xml.Serialization;

	public static class OffsetsManager
	{
		public enum Regions
		{
			Live,
			China,
			Korea,
		}

		public static OffsetsRoot LoadSettings(Regions region)
		{
			string localPath = GetLocalPath(region);
			string remotePath = GetRemotePath(region);

			OffsetsRoot? currentSettings = null;
			if (File.Exists(localPath))
			{
				using (StreamReader reader = new StreamReader(localPath))
				{
					currentSettings = Deserialize(reader);
				}
			}

			try
			{
				string xmlStr;
				ServicePointManager.SecurityProtocol = (ServicePointManager.SecurityProtocol & SecurityProtocolType.Ssl3) | (SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12);

				using (WebClient client = new WebClient())
				{
					xmlStr = client.DownloadString(remotePath);
				}

				OffsetsRoot newSettings = Deserialize(new StringReader(xmlStr));

				// newer settings, save them.
				if (currentSettings == null || currentSettings.Value.LastUpdated != newSettings.LastUpdated)
				{
					File.WriteAllText(localPath, xmlStr);
					currentSettings = newSettings;
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to download new offsets", ex);
			}

			if (currentSettings == null)
				throw new Exception("Failed to load offset settings.");

			return (OffsetsRoot)currentSettings;
		}

		private static OffsetsRoot Deserialize(TextReader reader)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(OffsetsRoot), string.Empty);
			XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
			ns.Add(string.Empty, string.Empty);

			try
			{
				return (OffsetsRoot)serializer.Deserialize(reader);
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to deserialize offsets", ex);
			}
		}

		private static string GetLocalPath(Regions region)
		{
			switch (region)
			{
				case Regions.Live: return @"./OffsetSettings.xml";
				case Regions.China: return @"./OffsetSettingsCN.xml";
				case Regions.Korea: return @"./OffsetSettingsKO.xml";
			}

			throw new Exception($"Unsupported region: {region}");
		}

		private static string GetRemotePath(Regions region)
		{
			switch (region)
			{
				case Regions.Live: return @"https://raw.githubusercontent.com/imchillin/CMTool/master/ConceptMatrix/OffsetSettings.xml";
				case Regions.China: return @"https://raw.githubusercontent.com/imchillin/CMTool/master/ConceptMatrix/OffsetSettingsCN.xml";
				case Regions.Korea: return @"https://raw.githubusercontent.com/imchillin/CMTool/master/ConceptMatrix/OffsetSettingsKO.xml";
			}

			throw new Exception($"Unsupported region: {region}");
		}
	}
}
