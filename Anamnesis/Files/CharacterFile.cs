// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Files
{
	using System;
	using System.Threading.Tasks;
	using Anamnesis;
	using Anamnesis.Files.Infos;
	using Anamnesis.Files.Types;
	using Anamnesis.Memory;
	using Anamnesis.Services;

	#pragma warning disable SA1402, SA1649
	public class CharacterFileInfo : JsonFileInfoBase<CharacterFile>
	{
		public override string Extension => "char";
		public override string Name => "Anamnesis Character File";
		public override IFileSource FileSource => new LocalFileSource("Local Files", "Anamnesis", "Characters");
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
			AppearanceHair = 8,
			AppearanceFace = 16,
			AppearanceBody = 32,
			AppearanceExtended = 64,

			All = EquipmentGear | EquipmentAccessories | AppearanceHair | AppearanceFace | AppearanceBody | AppearanceExtended,
		}

		public SaveModes SaveMode { get; set; } = SaveModes.All;

		public int ModelType { get; set; } = 0;

		// appearance
		public Appearance.Races? Race { get; set; }
		public Appearance.Genders? Gender { get; set; }
		public Appearance.Ages? Age { get; set; }
		public byte? Height { get; set; }
		public Appearance.Tribes? Tribe { get; set; }
		public byte? Head { get; set; }
		public byte? Hair { get; set; }
		public bool? EnableHighlights { get; set; }
		public byte? Skintone { get; set; }
		public byte? REyeColor { get; set; }
		public byte? HairTone { get; set; }
		public byte? Highlights { get; set; }
		public Appearance.FacialFeature? FacialFeatures { get; set; }
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
		public Color? SkinTint { get; set; }
		public Color? SkinGlow { get; set; }
		public Color? LeftEyeColor { get; set; }
		public Color? RightEyeColor { get; set; }
		public Color? LimbalRingColor { get; set; }
		public Color? HairTint { get; set; }
		public Color? HairGlow { get; set; }
		public Color? HighlightTint { get; set; }
		public Color4? LipTint { get; set; }
		public Vector? BustScale { get; set; }
		public float? Transparency { get; set; }
		public float? FeatureScale { get; set; }

		public void Read(ActorViewModel actor, SaveModes mode)
		{
			Log.Write("Writing appearance to file", "AppearanceFile");

			this.ModelType = actor.ModelType;

			if (!actor.IsCustomizable())
				return;

			this.SaveMode = mode;

			if (this.IncludeSection(SaveModes.EquipmentGear, mode))
			{
				if (actor.MainHand != null)
					this.MainHand = new WeaponSave(actor.MainHand);
				////this.MainHand.Color = actor.GetValue(Offsets.Main.MainHandColor);
				////this.MainHand.Scale = actor.GetValue(Offsets.Main.MainHandScale);

				if (actor.OffHand != null)
					this.OffHand = new WeaponSave(actor.OffHand);
				////this.OffHand.Color = actor.GetValue(Offsets.Main.OffhandColor);
				////this.OffHand.Scale = actor.GetValue(Offsets.Main.OffhandScale);

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
				////this.HairTint = actor.GetValue(Offsets.Main.HairColor);
				////this.HairGlow = actor.GetValue(Offsets.Main.HairGloss);
				////this.HighlightTint = actor.GetValue(Offsets.Main.HairHiglight);
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
				////this.LeftEyeColor = actor.GetValue(Offsets.Main.LeftEyeColor);
				////this.RightEyeColor = actor.GetValue(Offsets.Main.RightEyeColor);
				////this.LimbalRingColor = actor.GetValue(Offsets.Main.LimbalColor);

				////this.LipTint = new Color4(actor.GetValue(Offsets.Main.MouthColor), actor.GetValue(Offsets.Main.MouthGloss));
			}

			if (this.IncludeSection(SaveModes.AppearanceBody, mode))
			{
				this.Height = actor.Customize?.Height;
				this.Skintone = actor.Customize?.Skintone;
				this.EarMuscleTailSize = actor.Customize?.EarMuscleTailSize;
				this.TailEarsType = actor.Customize?.TailEarsType;
				this.Bust = actor.Customize?.Bust;

				////this.SkinTint = actor.GetValue(Offsets.Main.SkinColor);
				////this.SkinGlow = actor.GetValue(Offsets.Main.SkinGloss);
				////this.BustScale = actor.GetValue(Offsets.Main.BustScale);
				////this.Transparency = actor.GetValue(Offsets.Main.Transparency);
				////this.FeatureScale = actor.GetValue(Offsets.Main.UniqueFeatureScale);
			}
		}

		public async Task Apply(ActorViewModel actor, SaveModes mode)
		{
			Log.Write("Reading appearance from file", "AppearanceFile");

			ActorRefreshService.Instance.AutomaticRefreshEnabled = false;

			actor.ModelType = this.ModelType;

			if (!actor.IsCustomizable() || actor.Customize == null)
				return;

			if (this.IncludeSection(SaveModes.EquipmentGear, mode))
			{
				this.MainHand?.Write(actor.MainHand);
				this.OffHand?.Write(actor.OffHand);
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
				actor.Customize.Hair = (byte)this.Hair!;
				actor.Customize.EnableHighlights = (bool)this.EnableHighlights!;
				actor.Customize.HairTone = (byte)this.HairTone!;
				actor.Customize.Highlights = (byte)this.Highlights!;
			}

			if (this.IncludeSection(SaveModes.AppearanceFace, mode) || this.IncludeSection(SaveModes.AppearanceBody, mode))
			{
				actor.Customize.Race = (Appearance.Races)this.Race!;
				actor.Customize.Gender = (Appearance.Genders)this.Gender!;
				actor.Customize.Tribe = (Appearance.Tribes)this.Tribe!;
				actor.Customize.Age = (Appearance.Ages)this.Age!;
			}

			if (this.IncludeSection(SaveModes.AppearanceFace, mode))
			{
				actor.Customize.Head = (byte)this.Head!;
				actor.Customize.REyeColor = (byte)this.REyeColor!;
				actor.Customize.FacialFeatures = (Appearance.FacialFeature)this.FacialFeatures!;
				actor.Customize.LimbalEyes = (byte)this.LimbalEyes!;
				actor.Customize.Eyebrows = (byte)this.Eyebrows!;
				actor.Customize.LEyeColor = (byte)this.LEyeColor!;
				actor.Customize.Eyes = (byte)this.Eyes!;
				actor.Customize.Nose = (byte)this.Nose!;
				actor.Customize.Jaw = (byte)this.Jaw!;
				actor.Customize.Mouth = (byte)this.Mouth!;
				actor.Customize.LipsToneFurPattern = (byte)this.LipsToneFurPattern!;
				actor.Customize.FacePaint = (byte)this.FacePaint!;
				actor.Customize.FacePaintColor = (byte)this.FacePaintColor!;
			}

			if (this.IncludeSection(SaveModes.AppearanceBody, mode))
			{
				actor.Customize.Height = (byte)this.Height!;
				actor.Customize.Skintone = (byte)this.Skintone!;
				actor.Customize.EarMuscleTailSize = (byte)this.EarMuscleTailSize!;
				actor.Customize.TailEarsType = (byte)this.TailEarsType!;
				actor.Customize.Bust = (byte)this.Bust!;
			}

			await Task.Delay(100);

			ActorRefreshService.Refresh();

			while (ActorRefreshService.Instance.PendingRefresh || ActorRefreshService.Instance.IsRefreshing)
				await Task.Delay(100);

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
			}

			if (this.IncludeSection(SaveModes.AppearanceHair, mode))
			{
				actor.SetValue(Offsets.Main.HairColor, this.HairTint);
				actor.SetValue(Offsets.Main.HairGloss, this.HairGlow);
				actor.SetValue(Offsets.Main.HairHiglight, this.HighlightTint);
			}

			if (this.IncludeSection(SaveModes.AppearanceFace, mode))
			{
				actor.SetValue(Offsets.Main.LeftEyeColor, this.LeftEyeColor);
				actor.SetValue(Offsets.Main.RightEyeColor, this.RightEyeColor);
				actor.SetValue(Offsets.Main.LimbalColor, this.LimbalRingColor);

				if (this.LipTint != null)
				{
					using IMarshaler<Color> lipTintMem = actor.GetMemory(Offsets.Main.MouthColor);
					using IMarshaler<float> lipGlossMem = actor.GetMemory(Offsets.Main.MouthGloss);

					Color4 c = (Color4)this.LipTint;
					lipTintMem.Value = c.Color;
					lipGlossMem.Value = c.A;
				}
			}

			if (this.IncludeSection(SaveModes.AppearanceBody, mode))
			{
				actor.SetValue(Offsets.Main.SkinColor, this.SkinTint);
				actor.SetValue(Offsets.Main.SkinGloss, this.SkinGlow);
				actor.SetValue(Offsets.Main.Transparency, this.Transparency);
				actor.SetValue(Offsets.Main.BustScale, this.BustScale);
				actor.SetValue(Offsets.Main.UniqueFeatureScale, this.FeatureScale);
			}*/

			ActorRefreshService.Instance.AutomaticRefreshEnabled = true;
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
				vm.Base = this.ModelBase;
				vm.Variant = this.ModelVariant;
				vm.Dye = this.DyeId;
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
