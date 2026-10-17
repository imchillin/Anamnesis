// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files;

using Anamnesis.GameData.Excel;
using Anamnesis.GameData.Sheets;
using Anamnesis.GUI.Dialogs;
using Anamnesis.Memory;
using Anamnesis.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Unicode;

public class DatCharacterFile : FileBase, IUpgradeCharacterFile
{
	public byte[]? Data;

	private const uint FILE_SIGNATURE = 0x2013FF14;
	private const int DAT_FILE_SCHEMA_VERSION = 0x08;

	private const int CHECKSUM_OFFSET = 0x08;
	private const int PAYLOAD_OFFSET = 0x10;
	private const int DESCRIPTION_OFFSET = 0x30;
	private const int DESCRIPTION_LENGTH = 164;
	private const int TOTAL_FILE_SIZE = 0xD4;

	private const int CUSTOMIZE_DATA_LENGTH = 26;
	private const int TIMESTAMP_OFFSET_IN_PAYLOAD = 0x1C;
	private const int TIMESTAMP_LENGTH = 4;
	private const int CHECKSUM_SHIFT_MODULO = 24;

	private const byte FACE_PAINT_REVERSED_FLAG = 0x80;
	private const byte EYES_SMALL_IRIS_FLAG = 0x80;
	private const byte HAIR_HIGHLIGHTS_FLAG = 0x80;

	private const byte DEFAULT_HAIR = 0x01;
	private const byte DEFAULT_FACE_PAINT = 0x00;

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

			using FileStream stream = new(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			stream.Seek(DESCRIPTION_OFFSET, SeekOrigin.Begin);
			using BinaryReader reader = new(stream);

			byte[] descBytes = reader.ReadBytes(DESCRIPTION_LENGTH);
			int nullIdx = Array.IndexOf(descBytes, (byte)0);
			if (nullIdx < 0)
				nullIdx = descBytes.Length;

			string desc = Encoding.UTF8.GetString(descBytes, 0, nullIdx).Trim();
			if (string.IsNullOrWhiteSpace(desc))
				return slotNumber;

			// Handle multi-character Unicode to prevent slicing across a UTF-16 surrogate pair
			string name = desc;
			if (name.Length > 50)
			{
				name = name[..50];
				if (char.IsHighSurrogate(name[^1]))
					name = name[..^1];
			}

			return $"{slotNumber} - {name}";
		}
		catch (Exception)
		{
			Log.Warning($"Failed to get dat file name from file");
		}

		return fileName;
	}

	public static bool ValidateAllowedOptions(CharaMakeType makeType, ActorCustomizeMemory customize)
	{
		if (makeType.CustomizeRanges == null || makeType.CustomizeRanges.Count == 0)
			return false;

		// Keys correspond to the customization byte offsets in ActorCustomizeMemory / CharaMakeType
		var validate = new Dictionary<int, int>()
		{
			{ 5, customize.Head },
			{ 14, customize.Eyebrows },
			{ 16, customize.Eyes & ~EYES_SMALL_IRIS_FLAG },
			{ 17, customize.Nose },
			{ 18, customize.Jaw },
			{ 19, customize.Lips },
		};

		if (makeType.CustomizeRanges.ContainsKey(22))
			validate.Add(22, customize.TailEarsType);

		foreach (var option in validate)
		{
			if (!makeType.CustomizeRanges.TryGetValue(option.Key, out var range))
				continue;

			bool valid = range.InRange(option.Value);
			if (!valid)
				return false;
		}

		return true;
	}

	public CharacterFile Upgrade()
	{
		if (this.Data == null)
			throw new InvalidOperationException("DAT character appearance file has no data.");

		int offset = (this.Data.Length >= PAYLOAD_OFFSET + CUSTOMIZE_DATA_LENGTH && BitConverter.ToUInt32(this.Data, 0) == FILE_SIGNATURE)
			? PAYLOAD_OFFSET
			: 0;

		if (this.Data.Length < offset + CUSTOMIZE_DATA_LENGTH)
			throw new InvalidDataException("DAT character appearance file data is too short.");

		var file = new CharacterFile
		{
			Race = (ActorCustomizeMemory.Races)this.Data[offset + 0],
			Gender = (ActorCustomizeMemory.Genders)this.Data[offset + 1],
			Age = (ActorCustomizeMemory.Ages)this.Data[offset + 2],
			Height = this.Data[offset + 3],
			Tribe = (ActorCustomizeMemory.Tribes)this.Data[offset + 4],
			Head = this.Data[offset + 5],
			Hair = this.Data[offset + 6],
			EnableHighlights = this.Data[offset + 7] != 0,
			Skintone = this.Data[offset + 8],
			REyeColor = this.Data[offset + 9],
			HairTone = this.Data[offset + 10],
			Highlights = this.Data[offset + 11],
			FacialFeatures = (ActorCustomizeMemory.FacialFeature)this.Data[offset + 12],
			LimbalEyes = this.Data[offset + 13],
			Eyebrows = this.Data[offset + 14],
			LEyeColor = this.Data[offset + 15],
			Eyes = this.Data[offset + 16],
			Nose = this.Data[offset + 17],
			Jaw = this.Data[offset + 18],
			Mouth = this.Data[offset + 19],
			LipsToneFurPattern = this.Data[offset + 20],
			EarMuscleTailSize = this.Data[offset + 21],
			TailEarsType = this.Data[offset + 22],
			Bust = this.Data[offset + 23],
			FacePaint = this.Data[offset + 24],
			FacePaintColor = this.Data[offset + 25],
		};

		return file;
	}

	public void WriteToFile(ObjectHandle<ActorMemory> handle) => handle.Do(this.WriteToFile);

	public override void Serialize(Stream stream)
	{
		if (this.Data == null)
			throw new InvalidOperationException("Cannot serialize DAT file because appearance data was not generated.");

		stream.Write(this.Data);
	}

	public override FileBase Deserialize(Stream stream)
	{
		var file = new DatCharacterFile();

		using var reader = new BinaryReader(stream);
		file.Data = reader.ReadBytes((int)stream.Length);

		return file;
	}

	private void WriteToFile(ActorMemory actor)
	{
		if (actor.DrawData.Customize == null)
			return;

		ActorCustomizeMemory customize = actor.DrawData.Customize;

		CharaMakeType? makeType = null;
		if (GameDataService.CharacterMakeTypes != null)
		{
			foreach (CharaMakeType set in GameDataService.CharacterMakeTypes)
			{
				if (set.CustomizeTribe != customize.Tribe || set.Gender != customize.Gender)
					continue;

				makeType = set;
				break;
			}
		}

		if (!makeType.HasValue || makeType.Value.Voices.Count == 0)
		{
			GenericDialog.Show("Could not find character creation parameters for this race/tribe/gender.", "Failed to save appearance");
			return;
		}

		// Validate options
		bool validate = ValidateAllowedOptions(makeType.Value, customize);
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
			bool isIncompatible = hair.Value.FaceType != 0 && hair.Value.FaceType != customize.Head;
			if (isIncompatible)
			{
				GenericDialog.Show("This character uses a hairstyle that is not compatible with their face type.", "Failed to save appearance");
				return;
			}
			else
			{
				useDefaultHair = hair.Value.IsPurchasable;
			}
		}
		else
		{
			useDefaultHair = true;
		}

		byte facePaintId = (byte)(customize.FacePaint & ~FACE_PAINT_REVERSED_FLAG);
		if (facePaintId != 0)
		{
			CharaMakeCustomize? facePaint = GameDataService.CharacterMakeCustomize.GetFeature(CustomizeSheet.Features.FacePaint, customize.Tribe, customize.Gender, facePaintId);
			useDefaultFacePaint = facePaint == null || facePaint.Value.IsPurchasable;
		}

		if (useDefaultHair || useDefaultFacePaint)
		{
			GenericDialog.Show("Aesthetician-exclusive hairstyle or face paint has been reverted to its default value.", "Notice");
		}

		// Appearance Data
		var saveData = new byte[]
		{
			(byte)customize.Race,
			(byte)customize.Gender,
			(byte)ActorCustomizeMemory.Ages.Normal,
			customize.Height,
			(byte)customize.Tribe,
			customize.Head,
			useDefaultHair ? DEFAULT_HAIR : customize.Hair,
			(byte)(customize.EnableHighlights ? HAIR_HIGHLIGHTS_FLAG : 0x00),
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
			useDefaultFacePaint ? DEFAULT_FACE_PAINT : customize.FacePaint,
			customize.FacePaintColor,
			makeType.Value.Voices.Contains(actor.Voice) ? actor.Voice : makeType.Value.Voices[0],
			0x00,

			// Timestamp
			0x00, 0x00, 0x00, 0x00,
		};

		// Generate timestamp
		byte[] unixTime = BitConverter.GetBytes((uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds());
		if (!BitConverter.IsLittleEndian)
			Array.Reverse(unixTime);
		Array.Copy(unixTime, 0, saveData, TIMESTAMP_OFFSET_IN_PAYLOAD, TIMESTAMP_LENGTH);

		// Write Data
		byte[] buffer = new byte[TOTAL_FILE_SIZE];

		using (var stream = new MemoryStream(buffer))
		using (var writer = new BinaryWriter(stream))
		{
			writer.Write(FILE_SIGNATURE);
			writer.Write(DAT_FILE_SCHEMA_VERSION);
			writer.Seek(PAYLOAD_OFFSET, SeekOrigin.Begin);
			writer.Write(saveData);

			// Populate description if available
			string description = actor.DisplayName;
			if (!string.IsNullOrWhiteSpace(description))
			{
				// Handle multi-byte UTF-8
				Span<byte> descSpan = buffer.AsSpan(DESCRIPTION_OFFSET, DESCRIPTION_LENGTH - 1);
				Utf8.FromUtf16(description.AsSpan(), descSpan, out _, out _);
			}
		}

		// Calculate checksum over all bytes from PAYLOAD_OFFSET to the end of the file
		int checksum = 0;
		for (int i = PAYLOAD_OFFSET; i < buffer.Length; i++)
			checksum ^= buffer[i] << ((i - PAYLOAD_OFFSET) % CHECKSUM_SHIFT_MODULO);

		byte[] chkDigest = BitConverter.GetBytes(checksum);
		if (!BitConverter.IsLittleEndian)
			Array.Reverse(chkDigest);

		Array.Copy(chkDigest, 0, buffer, CHECKSUM_OFFSET, chkDigest.Length);

		this.Data = buffer;
	}
}
