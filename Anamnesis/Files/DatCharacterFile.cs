// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files;

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Anamnesis.GameData.Excel;
using Anamnesis.GameData.Sheets;
using Anamnesis.Memory;
using Anamnesis.Services;
using Serilog;
using Anamnesis.Windows;

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

		ActorCustomizeMemory customize = actor.Customize;

		CharaMakeType? makeType = null;
		if (GameDataService.CharacterMakeTypes != null)
		{
			foreach (CharaMakeType set in GameDataService.CharacterMakeTypes)
			{
				if (set.Tribe != customize.Tribe || set.Gender != customize.Gender)
					continue;

				makeType = set;
				break;
			}
		}

		if (makeType == null || makeType.Voices == null)
			return;

		// Validate options
		bool validate = this.ValidateAllowedOptions(makeType, customize);
		if (!validate)
		{
			GenericDialog.Show("This character uses custom features that are not available in the character creator.", "Failed to save appearance");
			return;
		}

		bool useDefaultHair = false;
		bool useDefaultFacePaint = false;

		CharaMakeCustomize? hair = GameDataService.CharacterMakeCustomize.GetFeature(CustomizeSheet.Features.Hair, customize.Tribe, customize.Gender, customize.Hair);

		if (hair != null)
		{
			// Check hairstyle is compatible with their face type (for hrothgars)
			bool isIncompatible = hair.FaceType != 0 && hair.FaceType != customize.Head;
			if (isIncompatible)
			{
				GenericDialog.Show("This character uses a hairstyle that is not compatible with their face type.", "Failed to save appearance");
				return;
			}
			else
			{
				useDefaultHair = hair.IsPurchasable;
			}
		}
		else
		{
			useDefaultHair = true;
		}

		if (customize.FacePaint != 0)
		{
			CharaMakeCustomize? facePaint = GameDataService.CharacterMakeCustomize.GetFeature(CustomizeSheet.Features.FacePaint, customize.Tribe, customize.Gender, customize.FacePaint);
			useDefaultFacePaint = facePaint == null || facePaint.IsPurchasable;
		}

		if (useDefaultHair || useDefaultFacePaint)
		{
			GenericDialog.Show("Aesthetician-exclusive hairstyle or face paint has been reverted to its default value.", "Notice");
		}

		// Appearance Data
		byte[] saveData = new byte[]
		{
			(byte)customize.Race,
			(byte)customize.Gender,
			0x01,
			customize.Height,
			(byte)customize.Tribe,
			customize.Head,
			(byte)(useDefaultHair ? 0x01 : customize.Hair),
			(byte)(customize.EnableHighlights ? 0x80 : 0x00),
			customize.Skintone,
			customize.REyeColor,
			customize.HairTone,
			customize.Highlights,
			(byte)customize.FacialFeatures,
			customize.FacialFeatureColor,
			customize.Eyebrows,
			customize.LEyeColor,
			customize.Eyes,
			customize.Nose,
			customize.Jaw,
			customize.Mouth,
			customize.LipsToneFurPattern,
			customize.EarMuscleTailSize,
			customize.TailEarsType,
			customize.Bust,
			(byte)(useDefaultFacePaint ? 0x00 : customize.FacePaint),
			customize.FacePaintColor,
			makeType.Voices.Contains(actor.Voice) ? actor.Voice : makeType.Voices[0],
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

		using MemoryStream stream = new MemoryStream(buffer);
		using BinaryWriter writer = new BinaryWriter(stream);

		// Save Data
		writer.Write(0x2013FF14); // Magic
		writer.Write(0x03);  // Version
		writer.Seek(0x08, 0);
		writer.Write(chkDigest); // Checksum
		writer.Seek(0x10, 0);
		writer.Write(saveData); // Appearance + Timestamp

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
