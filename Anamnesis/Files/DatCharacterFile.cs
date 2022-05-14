// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files;

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Anamnesis.GameData.Excel;
using Anamnesis.Memory;
using Anamnesis.Services;
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
			throw new Exception("Dat Appearance File has no data.");

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

	public bool ValidateAllowedOptions(CharaMakeType makeType, ActorCustomizeMemory customize)
	{
		if (makeType == null || makeType.CustomizeRanges == null)
			return false;

		Dictionary<int, int> validate = new Dictionary<int, int>()
		{
			{ 5, customize.Head },
			{ 14, customize.Eyebrows },
			{ 16, customize.Eyes & ~0x80 },
			{ 17, customize.Nose },
			{ 18, customize.Jaw },
			{ 19, customize.Lips },
		};
		if (makeType.CustomizeRanges.ContainsKey(22))
			validate.Add(22, customize.TailEarsType);

		foreach (var option in validate)
		{
			bool valid = makeType.CustomizeRanges[option.Key].InRange(option.Value);
			if (!valid)
				return false;
		}

		return true;
	}

	public void WriteToFile(ActorMemory actor)
	{
		if (actor.Customize == null)
			return;

		CharaMakeType? makeType = null;
		if (GameDataService.CharacterMakeTypes != null)
		{
			foreach (CharaMakeType set in GameDataService.CharacterMakeTypes)
			{
				if (set.Tribe != actor.Customize.Tribe || set.Gender != actor.Customize.Gender)
					continue;

				makeType = set;
				break;
			}
		}

		if (makeType == null)
			return;

		bool validate = this.ValidateAllowedOptions(makeType, actor.Customize);
		if (!validate)
			throw new Exception("This character uses custom features that are not available in the character creator.");

		// Appearance Data
		byte[] saveData = new byte[]
		{
			(byte)actor.Customize.Race,
			(byte)actor.Customize.Gender,
			(byte)actor.Customize.Age,
			actor.Customize.Height,
			(byte)actor.Customize.Tribe,
			actor.Customize.Head,
			actor.Customize.Hair,
			(byte)(actor.Customize.EnableHighlights ? 0x80 : 0x00),
			actor.Customize.Skintone,
			actor.Customize.REyeColor,
			actor.Customize.HairTone,
			actor.Customize.Highlights,
			(byte)actor.Customize.FacialFeatures,
			actor.Customize.FacialFeatureColor,
			actor.Customize.Eyebrows,
			actor.Customize.LEyeColor,
			actor.Customize.Eyes,
			actor.Customize.Nose,
			actor.Customize.Jaw,
			actor.Customize.Mouth,
			actor.Customize.LipsToneFurPattern,
			actor.Customize.EarMuscleTailSize,
			actor.Customize.TailEarsType,
			actor.Customize.Bust,
			actor.Customize.FacePaint,
			actor.Customize.FacePaintColor,
			makeType.DefaultVoice, // TODO: Default
			0x00,

			// Timestamp
			0x00, 0x00, 0x00, 0x00,
		};

		// Generate timestamp
		byte[] unixTime = BitConverter.GetBytes(DateTimeOffset.Now.ToUnixTimeSeconds());
		if (!BitConverter.IsLittleEndian)
			Array.Reverse(unixTime);
		Array.Copy(unixTime, 0, saveData, 0x1C, 4);

		// Calculate checksum
		int checksum = 0;
		for (int i = 0; i < saveData.Length; i++)
			checksum ^= saveData[i] << (i % 24);

		byte[] chkDigest = BitConverter.GetBytes(checksum);
		if (!BitConverter.IsLittleEndian)
			Array.Reverse(chkDigest);

		// Write Data
		byte[] buffer = new byte[0xD4];

		using (MemoryStream stream = new MemoryStream(buffer))
		{
			using (BinaryWriter writer = new BinaryWriter(stream))
			{
				// Save Data
				writer.Write(0x2013FF14); // Magic
				writer.Write(0x03);
				writer.Seek(0x08, 0);
				writer.Write(chkDigest);
				writer.Seek(0x10, 0);
				writer.Write(saveData);
			}
		}

		this.Data = buffer;
	}

	public override void Serialize(Stream stream)
	{
		stream.Write(this.Data);
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
