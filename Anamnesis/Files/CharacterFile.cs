// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files
{
	using System;
	using System.IO;
	using System.Text.Json.Serialization;
	using System.Threading.Tasks;
	using Anamnesis;
	using Anamnesis.Character.Views;
	using Anamnesis.Files.Infos;
	using Anamnesis.Files.Types;
	using Anamnesis.Memory;
	using Anamnesis.Services;
	using Serilog;

#pragma warning disable SA1402, SA1649
	public class CharacterFileInfo : JsonFileInfoBase<CharacterFile>
	{
		public override string Extension => "chara";
		public override string Name => "Anamnesis Character File";
		public override Type? LoadOptionsViewType => typeof(CharacterFileOptions);
		public override Type? SaveOptionsViewType => typeof(CharacterFileOptions);
		public override IFileSource[] FileSources => new[] { new LocalFileSource("Local Files", SettingsService.Current.DefaultCharacterDirectory) };
	}

	[Serializable]
	public class CharacterFile : FileBase
	{
		[Flags]
		public enum SaveModes
		{
			None = 0,

			EquipmentGear = 1,
			EquipmentAccessories = 2,
			EquipmentWeapons = 4,
			AppearanceHair = 8,
			AppearanceFace = 16,
			AppearanceBody = 32,
			AppearanceExtended = 64,

			All = EquipmentGear | EquipmentAccessories | EquipmentWeapons | AppearanceHair | AppearanceFace | AppearanceBody | AppearanceExtended,
		}

		public SaveModes SaveMode { get; set; } = SaveModes.All;

		public string? Nickname { get; set; } = null;
		public int ModelType { get; set; } = 0;

		// appearance
		public Customize.Races? Race { get; set; }
		public Customize.Genders? Gender { get; set; }
		public Customize.Ages? Age { get; set; }
		public byte? Height { get; set; }
		public Customize.Tribes? Tribe { get; set; }
		public byte? Head { get; set; }
		public byte? Hair { get; set; }
		public bool? EnableHighlights { get; set; }
		public byte? Skintone { get; set; }
		public byte? REyeColor { get; set; }
		public byte? HairTone { get; set; }
		public byte? Highlights { get; set; }
		public Customize.FacialFeature? FacialFeatures { get; set; }
		public byte? LimbalEyes { get; set; }
		public byte? Eyebrows { get; set; }
		public byte? LEyeColor { get; set; }
		public byte? Eyes { get; set; }
		public byte? Nose { get; set; }
		public byte? Jaw { get; set; }
		public byte? Mouth { get; set; }
		public byte? LipsToneFurPattern { get; set; }
		public byte? EarMuscleTailSize { get; set; }
		public byte? TailEarsType { get; set; }
		public byte? Bust { get; set; }
		public byte? FacePaint { get; set; }
		public byte? FacePaintColor { get; set; }

		// weapons
		public WeaponSave? MainHand { get; set; }
		public WeaponSave? OffHand { get; set; }

		// equipment
		public ItemSave? HeadGear { get; set; }
		public ItemSave? Body { get; set; }
		public ItemSave? Hands { get; set; }
		public ItemSave? Legs { get; set; }
		public ItemSave? Feet { get; set; }
		public ItemSave? Ears { get; set; }
		public ItemSave? Neck { get; set; }
		public ItemSave? Wrists { get; set; }
		public ItemSave? LeftRing { get; set; }
		public ItemSave? RightRing { get; set; }

		// extended appearance
		// NOTE: extended weapon values are stored in the WeaponSave
		public Color? SkinColor { get; set; }
		public Color? SkinGloss { get; set; }
		public Color? LeftEyeColor { get; set; }
		public Color? RightEyeColor { get; set; }
		public Color? LimbalRingColor { get; set; }
		public Color? HairColor { get; set; }
		public Color? HairGloss { get; set; }
		public Color? HairHighlight { get; set; }
		public Color4? MouthColor { get; set; }
		public Vector? BustScale { get; set; }
		public float? Transparency { get; set; }
		public float? MuscleTone { get; set; }
		public float? HeightMultiplier { get; set; }

		public static async Task<SaveModes> Save(ActorViewModel? actor, SaveModes mode = SaveModes.All)
		{
			if (actor == null)
				return mode;

			SaveResult result = await FileService.Save<CharacterFile>();

			if (string.IsNullOrEmpty(result.Path) || result.Info == null)
				return CharacterFileOptions.Result;

			CharacterFile file = new CharacterFile();
			file.WriteToFile(actor, mode);

			using FileStream stream = new FileStream(result.Path, FileMode.Create);
			result.Info.SerializeFile(file, stream);

			return CharacterFileOptions.Result;
		}

		public void WriteToFile(ActorViewModel actor, SaveModes mode)
		{
			this.Nickname = actor.Nickname;
			this.ModelType = actor.ModelType;

			if (actor.Customize == null)
				return;

			this.SaveMode = mode;

			if (this.IncludeSection(SaveModes.EquipmentWeapons, mode))
			{
				if (actor.MainHand != null)
					this.MainHand = new WeaponSave(actor.MainHand);
				////this.MainHand.Color = actor.GetValue(Offsets.Main.MainHandColor);
				////this.MainHand.Scale = actor.GetValue(Offsets.Main.MainHandScale);

				if (actor.OffHand != null)
					this.OffHand = new WeaponSave(actor.OffHand);
				////this.OffHand.Color = actor.GetValue(Offsets.Main.OffhandColor);
				////this.OffHand.Scale = actor.GetValue(Offsets.Main.OffhandScale);
			}

			if (this.IncludeSection(SaveModes.EquipmentGear, mode))
			{
				if (actor.Equipment?.Head != null)
					this.HeadGear = new ItemSave(actor.Equipment.Head);

				if (actor.Equipment?.Chest != null)
					this.Body = new ItemSave(actor.Equipment.Chest);

				if (actor.Equipment?.Arms != null)
					this.Hands = new ItemSave(actor.Equipment.Arms);

				if (actor.Equipment?.Legs != null)
					this.Legs = new ItemSave(actor.Equipment.Legs);

				if (actor.Equipment?.Feet != null)
					this.Feet = new ItemSave(actor.Equipment.Feet);
			}

			if (this.IncludeSection(SaveModes.EquipmentAccessories, mode))
			{
				if (actor.Equipment?.Ear != null)
					this.Ears = new ItemSave(actor.Equipment.Ear);

				if (actor.Equipment?.Neck != null)
					this.Neck = new ItemSave(actor.Equipment.Neck);

				if (actor.Equipment?.Wrist != null)
					this.Wrists = new ItemSave(actor.Equipment.Wrist);

				if (actor.Equipment?.LFinger != null)
					this.LeftRing = new ItemSave(actor.Equipment.LFinger);

				if (actor.Equipment?.RFinger != null)
					this.RightRing = new ItemSave(actor.Equipment.RFinger);
			}

			if (this.IncludeSection(SaveModes.AppearanceHair, mode))
			{
				this.Hair = actor.Customize?.Hair;
				this.EnableHighlights = actor.Customize?.EnableHighlights;
				this.HairTone = actor.Customize?.HairTone;
				this.Highlights = actor.Customize?.Highlights;
				this.HairColor = actor.ModelObject?.ExtendedAppearance?.HairColor;
				this.HairGloss = actor.ModelObject?.ExtendedAppearance?.HairGloss;
				this.HairHighlight = actor.ModelObject?.ExtendedAppearance?.HairHighlight;
			}

			if (this.IncludeSection(SaveModes.AppearanceFace, mode) || this.IncludeSection(SaveModes.AppearanceBody, mode))
			{
				this.Race = actor.Customize?.Race;
				this.Gender = actor.Customize?.Gender;
				this.Tribe = actor.Customize?.Tribe;
				this.Age = actor.Customize?.Age;
			}

			if (this.IncludeSection(SaveModes.AppearanceFace, mode))
			{
				this.Head = actor.Customize?.Head;
				this.REyeColor = actor.Customize?.REyeColor;
				this.LimbalEyes = actor.Customize?.LimbalEyes;
				this.FacialFeatures = actor.Customize?.FacialFeatures;
				this.Eyebrows = actor.Customize?.Eyebrows;
				this.LEyeColor = actor.Customize?.LEyeColor;
				this.Eyes = actor.Customize?.Eyes;
				this.Nose = actor.Customize?.Nose;
				this.Jaw = actor.Customize?.Jaw;
				this.Mouth = actor.Customize?.Mouth;
				this.LipsToneFurPattern = actor.Customize?.LipsToneFurPattern;
				this.FacePaint = actor.Customize?.FacePaint;
				this.FacePaintColor = actor.Customize?.FacePaintColor;
				this.LeftEyeColor = actor.ModelObject?.ExtendedAppearance?.LeftEyeColor;
				this.RightEyeColor = actor.ModelObject?.ExtendedAppearance?.RightEyeColor;
				this.LimbalRingColor = actor.ModelObject?.ExtendedAppearance?.LimbalRingColor;
				this.MouthColor = actor.ModelObject?.ExtendedAppearance?.MouthColor;
			}

			if (this.IncludeSection(SaveModes.AppearanceBody, mode))
			{
				this.Height = actor.Customize?.Height;
				this.Skintone = actor.Customize?.Skintone;
				this.EarMuscleTailSize = actor.Customize?.EarMuscleTailSize;
				this.TailEarsType = actor.Customize?.TailEarsType;
				this.Bust = actor.Customize?.Bust;

				this.HeightMultiplier = actor.ModelObject?.Height;
				this.SkinColor = actor.ModelObject?.ExtendedAppearance?.SkinColor;
				this.SkinGloss = actor.ModelObject?.ExtendedAppearance?.SkinGloss;
				this.MuscleTone = actor.ModelObject?.ExtendedAppearance?.MuscleTone;
				this.BustScale = actor.ModelObject?.Bust?.Scale;
				this.Transparency = actor.Transparency;
			}
		}

		public async Task Apply(ActorViewModel actor, SaveModes mode)
		{
			if (actor.Customize == null)
				return;

			Log.Information("Reading appearance from file");

			actor.AutomaticRefreshEnabled = false;
			actor.MemoryMode = MemoryModes.None;

			if (actor.ModelObject?.ExtendedAppearance != null)
				actor.ModelObject.ExtendedAppearance.MemoryMode = MemoryModes.None;

			if (!string.IsNullOrEmpty(this.Nickname))
				actor.Nickname = this.Nickname;

			actor.ModelType = this.ModelType;

			if (this.IncludeSection(SaveModes.EquipmentWeapons, mode))
			{
				this.MainHand?.Write(actor.MainHand);
				this.OffHand?.Write(actor.OffHand);
			}

			if (this.IncludeSection(SaveModes.EquipmentGear, mode))
			{
				this.HeadGear?.Write(actor.Equipment?.Head);
				this.Body?.Write(actor.Equipment?.Chest);
				this.Hands?.Write(actor.Equipment?.Arms);
				this.Legs?.Write(actor.Equipment?.Legs);
				this.Feet?.Write(actor.Equipment?.Feet);
			}

			if (this.IncludeSection(SaveModes.EquipmentAccessories, mode))
			{
				this.Ears?.Write(actor.Equipment?.Ear);
				this.Neck?.Write(actor.Equipment?.Neck);
				this.Wrists?.Write(actor.Equipment?.Wrist);
				this.RightRing?.Write(actor.Equipment?.RFinger);
				this.LeftRing?.Write(actor.Equipment?.LFinger);
			}

			if (this.IncludeSection(SaveModes.AppearanceHair, mode))
			{
				if (this.Hair != null)
					actor.Customize.Hair = (byte)this.Hair;

				if (this.EnableHighlights != null)
					actor.Customize.EnableHighlights = (bool)this.EnableHighlights;

				if (this.HairTone != null)
					actor.Customize.HairTone = (byte)this.HairTone;

				if (this.Highlights != null)
					actor.Customize.Highlights = (byte)this.Highlights;
			}

			if (this.IncludeSection(SaveModes.AppearanceFace, mode) || this.IncludeSection(SaveModes.AppearanceBody, mode))
			{
				if (this.Race != null)
					actor.Customize.Race = (Customize.Races)this.Race;

				if (this.Gender != null)
					actor.Customize.Gender = (Customize.Genders)this.Gender;

				if (this.Tribe != null)
					actor.Customize.Tribe = (Customize.Tribes)this.Tribe;

				if (this.Age != null)
					actor.Customize.Age = (Customize.Ages)this.Age;
			}

			if (this.IncludeSection(SaveModes.AppearanceFace, mode))
			{
				if (this.Head != null)
					actor.Customize.Head = (byte)this.Head;

				if (this.REyeColor != null)
					actor.Customize.REyeColor = (byte)this.REyeColor;

				if (this.FacialFeatures != null)
					actor.Customize.FacialFeatures = (Customize.FacialFeature)this.FacialFeatures;

				if (this.LimbalEyes != null)
					actor.Customize.LimbalEyes = (byte)this.LimbalEyes;

				if (this.Eyebrows != null)
					actor.Customize.Eyebrows = (byte)this.Eyebrows;

				if (this.LEyeColor != null)
					actor.Customize.LEyeColor = (byte)this.LEyeColor;

				if (this.Eyes != null)
					actor.Customize.Eyes = (byte)this.Eyes;

				if (this.Nose != null)
					actor.Customize.Nose = (byte)this.Nose;

				if (this.Jaw != null)
					actor.Customize.Jaw = (byte)this.Jaw;

				if (this.Mouth != null)
					actor.Customize.Mouth = (byte)this.Mouth;

				if (this.LipsToneFurPattern != null)
					actor.Customize.LipsToneFurPattern = (byte)this.LipsToneFurPattern;

				if (this.FacePaint != null)
					actor.Customize.FacePaint = (byte)this.FacePaint;

				if (this.FacePaintColor != null)
					actor.Customize.FacePaintColor = (byte)this.FacePaintColor;
			}

			if (this.IncludeSection(SaveModes.AppearanceBody, mode))
			{
				if (this.Height != null)
					actor.Customize.Height = (byte)this.Height;

				if (this.Skintone != null)
					actor.Customize.Skintone = (byte)this.Skintone;

				if (this.EarMuscleTailSize != null)
					actor.Customize.EarMuscleTailSize = (byte)this.EarMuscleTailSize;

				if (this.TailEarsType != null)
					actor.Customize.TailEarsType = (byte)this.TailEarsType;

				if (this.Bust != null)
					actor.Customize.Bust = (byte)this.Bust;
			}

			actor.WriteToMemory(true);
			await actor.RefreshAsync();

			// Setting customize values will reset the extended appearance, which me must read.
			await actor.ReadFromMemoryAsync(true);

			if (actor.ModelObject?.ExtendedAppearance != null)
				await actor.ModelObject.ExtendedAppearance.ReadFromMemoryAsync(true);

			// write everything that is reset by actor refreshes
			/*if (this.IncludeSection(SaveModes.EquipmentGear, mode))
			{
				if (this.MainHand != null)
				{
					actor.SetValue(Offsets.Main.MainHandColor, this.MainHand.Color);
					actor.SetValue(Offsets.Main.MainHandScale, this.MainHand.Scale);
				}

				if (this.OffHand != null)
				{
					actor.SetValue(Offsets.Main.OffhandColor, this.OffHand.Color);
					actor.SetValue(Offsets.Main.OffhandScale, this.OffHand.Scale);
				}
			}*/

			if (actor.ModelObject?.ExtendedAppearance != null)
			{
				bool usedExAppearance = false;

				if (this.IncludeSection(SaveModes.AppearanceHair, mode))
				{
					actor.ModelObject.ExtendedAppearance.HairColor = this.HairColor ?? actor.ModelObject.ExtendedAppearance.HairColor;
					actor.ModelObject.ExtendedAppearance.HairGloss = this.HairGloss ?? actor.ModelObject.ExtendedAppearance.HairGloss;
					actor.ModelObject.ExtendedAppearance.HairHighlight = this.HairHighlight ?? actor.ModelObject.ExtendedAppearance.HairHighlight;

					usedExAppearance |= this.HairColor != null;
					usedExAppearance |= this.HairGloss != null;
					usedExAppearance |= this.HairHighlight != null;
				}

				if (this.IncludeSection(SaveModes.AppearanceFace, mode))
				{
					actor.ModelObject.ExtendedAppearance.LeftEyeColor = this.LeftEyeColor ?? actor.ModelObject.ExtendedAppearance.LeftEyeColor;
					actor.ModelObject.ExtendedAppearance.RightEyeColor = this.RightEyeColor ?? actor.ModelObject.ExtendedAppearance.RightEyeColor;
					actor.ModelObject.ExtendedAppearance.LimbalRingColor = this.LimbalRingColor ?? actor.ModelObject.ExtendedAppearance.LimbalRingColor;
					actor.ModelObject.ExtendedAppearance.MouthColor = this.MouthColor ?? actor.ModelObject.ExtendedAppearance.MouthColor;

					usedExAppearance |= this.LeftEyeColor != null;
					usedExAppearance |= this.RightEyeColor != null;
					usedExAppearance |= this.LimbalRingColor != null;
					usedExAppearance |= this.MouthColor != null;
				}

				if (this.IncludeSection(SaveModes.AppearanceBody, mode))
				{
					actor.ModelObject.ExtendedAppearance.SkinColor = this.SkinColor ?? actor.ModelObject.ExtendedAppearance.SkinColor;
					actor.ModelObject.ExtendedAppearance.SkinGloss = this.SkinGloss ?? actor.ModelObject.ExtendedAppearance.SkinGloss;
					actor.ModelObject.ExtendedAppearance.MuscleTone = this.MuscleTone ?? actor.ModelObject.ExtendedAppearance.MuscleTone;
					actor.Transparency = this.Transparency ?? actor.Transparency;
					actor.ModelObject.Height = this.HeightMultiplier ?? actor.ModelObject.Height;

					if (actor.ModelObject.Bust?.Scale != null)
						actor.ModelObject.Bust.Scale = this.BustScale ?? actor.ModelObject.Bust.Scale;

					usedExAppearance |= this.SkinColor != null;
					usedExAppearance |= this.SkinGloss != null;
					usedExAppearance |= this.MuscleTone != null;
				}

				////actor.ModelObject.ExtendedAppearance.Freeze = usedExAppearance;
				actor.ModelObject.ExtendedAppearance.MemoryMode = MemoryModes.ReadWrite;
				actor.ModelObject.ExtendedAppearance.WriteToMemory(true);
			}

			actor.MemoryMode = MemoryModes.ReadWrite;
			actor.WriteToMemory(true);
			actor.AutomaticRefreshEnabled = true;
		}

		private bool IncludeSection(SaveModes section, SaveModes mode)
		{
			return this.SaveMode.HasFlag(section) && mode.HasFlag(section);
		}

		[Serializable]
		public class WeaponSave
		{
			public WeaponSave()
			{
			}

			public WeaponSave(WeaponViewModel from)
			{
				this.ModelSet = from.Set;
				this.ModelBase = from.Base;
				this.ModelVariant = from.Variant;
				this.DyeId = from.Dye;
			}

			public Color Color { get; set; }
			public Vector Scale { get; set; }
			public ushort ModelSet { get; set; }
			public ushort ModelBase { get; set; }
			public ushort ModelVariant { get; set; }
			public byte DyeId { get; set; }

			public void Write(WeaponViewModel? vm)
			{
				if (vm == null)
					return;

				vm.Set = this.ModelSet;

				// sanity check values
				if (vm.Set != 0)
				{
					vm.Base = this.ModelBase;
					vm.Variant = this.ModelVariant;
					vm.Dye = this.DyeId;
				}
				else
				{
					vm.Base = 0;
					vm.Variant = 0;
					vm.Dye = 0;
				}
			}
		}

		[Serializable]
		public class ItemSave
		{
			public ItemSave()
			{
			}

			public ItemSave(ItemViewModel from)
			{
				this.ModelBase = from.Base;
				this.ModelVariant = from.Variant;
				this.DyeId = from.Dye;
			}

			public ushort ModelBase { get; set; }
			public byte ModelVariant { get; set; }
			public byte DyeId { get; set; }

			public void Write(ItemViewModel? vm)
			{
				if (vm == null)
					return;

				vm.Base = this.ModelBase;
				vm.Variant = this.ModelVariant;
				vm.Dye = this.DyeId;
			}
		}
	}
}
