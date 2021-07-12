// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files
{
	using System;
	using System.IO;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using Anamnesis.Files.Infos;
	using Anamnesis.Files.Types;
	using Anamnesis.Memory;
	using Anamnesis.Services;
	using Paths = System.IO.Path;

	#pragma warning disable SA1402, SA1649
	public class DatCharacterFileInfo : FileInfoBase<DatCharacterFile>
	{
		public override string Extension => "dat";
		public override string Name => "Ffxiv Character appearance save";
		public override IFileSource[] FileSources => new[] { new DatFileSource() };

		protected override DatCharacterFile Deserialize(Stream stream)
		{
			DatCharacterFile file = new DatCharacterFile();

			using BinaryReader reader = new BinaryReader(stream);

			stream.Seek(0x10, SeekOrigin.Begin);
			file.Data = reader.ReadBytes(26);

			stream.Seek(0x30, SeekOrigin.Begin);

			return file;
		}

		protected override void Serialize(DatCharacterFile file, Stream stream)
		{
			throw new NotSupportedException();
		}
	}

	public class DatCharacterFile : FileBase
	{
		public byte[]? Data;

		public CharacterFile Upgrade()
		{
			if (this.Data == null)
				throw new Exception("Dat Appearance Fila has no data.");

			CharacterFile file = new CharacterFile();
			file.Race = (Customize.Races)this.Data[0];
			file.Gender = (Customize.Genders)this.Data[1];
			file.Age = (Customize.Ages)this.Data[2];
			file.Height = this.Data[3];
			file.Tribe = (Customize.Tribes)this.Data[4];
			file.Head = this.Data[5];
			file.Hair = this.Data[6];
			file.EnableHighlights = this.Data[7] != 0;
			file.Skintone = this.Data[8];
			file.REyeColor = this.Data[9];
			file.HairTone = this.Data[10];
			file.Highlights = this.Data[11];
			file.FacialFeatures = (Customize.FacialFeature)this.Data[12];
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
	}
}
