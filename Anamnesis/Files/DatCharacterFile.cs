// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files
{
	using System;
	using System.IO;
	using System.Text;
	using System.Text.RegularExpressions;
	using Anamnesis.Memory;
	using Serilog;

	public class DatCharacterFile : FileBase, IUpgradeCharacterFile
	{
		public byte[]? Data;

		public override string FileExtension => ".dat";
		public override string FileRegex => "FFXIV_CHARA_[0-9]+";
		public override string TypeName => "Ffxiv Character appearance save";
		public override Func<FileSystemInfo, string> GetFilename => GetName;

		public static string GetName(FileSystemInfo file)
		{
			string fileName = Path.GetFileNameWithoutExtension(file.FullName);

			try
			{
				string slotNumber = fileName.Replace("FFXIV_CHARA_", string.Empty);

				FileStream stream = new FileStream(file.FullName, FileMode.Open);
				stream.Seek(0x30, SeekOrigin.Begin);
				using BinaryReader reader = new BinaryReader(stream);

				byte[] bytes = reader.ReadBytes(164);
				string desc = Encoding.ASCII.GetString(bytes);
				Regex.Replace(desc, @"(?![ -~]|\r|\n).", string.Empty);
				string name = desc.Substring(0, Math.Min(desc.Length, 50));

				return $"{slotNumber} - {name}";
			}
			catch (Exception)
			{
				Log.Warning($"Failed to get dat file name from file");
			}

			return fileName;
		}

		public CharacterFile Upgrade()
		{
			if (this.Data == null)
				throw new Exception("Dat Appearance Fila has no data.");

			CharacterFile file = new CharacterFile();
			file.Race = (ActorCustomizeMemory.Races)this.Data[0];
			file.Gender = (ActorCustomizeMemory.Genders)this.Data[1];
			file.Age = (ActorCustomizeMemory.Ages)this.Data[2];
			file.Height = this.Data[3];
			file.Tribe = (ActorCustomizeMemory.Tribes)this.Data[4];
			file.Head = this.Data[5];
			file.Hair = this.Data[6];
			file.EnableHighlights = this.Data[7] != 0;
			file.Skintone = this.Data[8];
			file.REyeColor = this.Data[9];
			file.HairTone = this.Data[10];
			file.Highlights = this.Data[11];
			file.FacialFeatures = (ActorCustomizeMemory.FacialFeature)this.Data[12];
			file.LimbalEyes = this.Data[13];
			file.Eyebrows = this.Data[14];
			file.LEyeColor = this.Data[15];
			file.Eyes = this.Data[16];
			file.Nose = this.Data[17];
			file.Jaw = this.Data[18];
			file.Mouth = this.Data[19];
			file.LipsToneFurPattern = this.Data[20];
			file.EarMuscleTailSize = this.Data[21];
			file.TailEarsType = this.Data[22];
			file.Bust = this.Data[23];
			file.FacePaint = this.Data[24];
			file.FacePaintColor = this.Data[25];
			return file;
		}

		public override void Serialize(Stream stream)
		{
			throw new NotSupportedException();
		}

		public override FileBase Deserialize(Stream stream)
		{
			DatCharacterFile file = new DatCharacterFile();

			using BinaryReader reader = new BinaryReader(stream);

			stream.Seek(0x10, SeekOrigin.Begin);
			file.Data = reader.ReadBytes(26);

			stream.Seek(0x30, SeekOrigin.Begin);

			return file;
		}
	}
}
