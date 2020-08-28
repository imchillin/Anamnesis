// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Files
{
	using System;
	using System.IO;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using ConceptMatrix.Memory;
	using Paths = System.IO.Path;

	public class DatAppearanceFile : FileBase, IFileSource.IFile
	{
		public static readonly FileType FileType = new FileType("dat", "Appearance", typeof(DatAppearanceFile), false, null, Deserialize, Serialize);

		private byte[] data;

		public override FileType Type => FileType;

		public string Name { get; set; }
		public int SaveSlot { get; set; }
		public string Description { get; set; }

		public static FileBase Deserialize(Stream stream)
		{
			DatAppearanceFile file = new DatAppearanceFile();

			using BinaryReader reader = new BinaryReader(stream);

			stream.Seek(0x10, SeekOrigin.Begin);
			file.data = reader.ReadBytes(26);

			stream.Seek(0x30, SeekOrigin.Begin);

			file.Description = Regex.Replace(Encoding.ASCII.GetString(reader.ReadBytes(164)), @"(?![ -~]|\r|\n).", string.Empty);
			file.Name = file.SaveSlot + ". " + file.Description.Substring(0, Math.Min(file.Description.Length, 50));

			return file;
		}

		public static void Serialize(Stream stream, FileBase file)
		{
			throw new NotSupportedException();
		}

		public AppearanceFile Upgrade()
		{
			AppearanceFile file = new AppearanceFile();
			file.Race = (Appearance.Races)this.data[0];
			file.Gender = (Appearance.Genders)this.data[1];
			file.Age = (Appearance.Ages)this.data[2];
			file.Height = this.data[3];
			file.Tribe = (Appearance.Tribes)this.data[4];
			file.Head = this.data[5];
			file.Hair = this.data[6];
			file.EnableHighlights = this.data[7] != 0;
			file.Skintone = this.data[8];
			file.REyeColor = this.data[9];
			file.HairTone = this.data[10];
			file.Highlights = this.data[11];
			file.FacialFeatures = (Appearance.FacialFeature)this.data[12];
			file.LimbalEyes = this.data[13];
			file.Eyebrows = this.data[14];
			file.LEyeColor = this.data[15];
			file.Eyes = this.data[16];
			file.Nose = this.data[17];
			file.Jaw = this.data[18];
			file.Mouth = this.data[19];
			file.LipsToneFurPattern = this.data[20];
			file.EarMuscleTailSize = this.data[21];
			file.TailEarsType = this.data[22];
			file.Bust = this.data[23];
			file.FacePaint = this.data[24];
			file.FacePaintColor = this.data[25];
			return file;
		}

		public Task Delete()
		{
			return Task.CompletedTask;
		}

		internal static DatAppearanceFile FromDat(string path)
		{
			string name = Paths.GetFileNameWithoutExtension(path);
			int saveSlot = int.Parse(name.Substring(12));

			using FileStream stream = new FileStream(path, FileMode.Open);
			DatAppearanceFile file = (DatAppearanceFile)Deserialize(stream);
			file.Path = path;
			file.SaveSlot = saveSlot + 1;

			return file;
		}
	}
}
