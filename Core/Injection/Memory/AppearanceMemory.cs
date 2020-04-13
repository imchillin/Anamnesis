// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Memory
{
	using System;

	public class AppearanceMemory : MemoryBase<Appearance>
	{
		public AppearanceMemory(ProcessInjection process, UIntPtr address)
			: base(process, address, 26)
		{
		}

		protected override Appearance Read(ref byte[] data)
		{
			Appearance value = default;
			value.Race = (Appearance.Races)data[0];
			value.Gender = (Appearance.Genders)data[1];
			value.Age = (Appearance.Ages)data[2];
			value.Height = data[3];
			value.Tribe = (Appearance.Tribes)data[4];
			value.Head = data[5];
			value.Hair = data[6];
			value.Highlights = data[7];
			value.Skintone = data[8];
			value.REyeColor = data[9];
			value.HairTone = data[10];
			value.HighlightTone = data[11];
			value.FacialFeatures = (Appearance.FacialFeature)data[12];
			value.LimbalEyes = data[13];
			value.Eyebrows = data[14];
			value.LEyeColor = data[15];
			value.Eyes = data[16];
			value.Nose = data[17];
			value.Jaw = data[18];
			value.Lips = data[19];
			value.LipsToneFurPattern = data[20];
			value.EarMuscleTailSize = data[21];
			value.TailEarsType = data[22];
			value.Bust = data[23];
			value.FacePaint = data[24];
			value.FacePaintColor = data[25];
			return value;
		}

		protected override void Write(Appearance value, ref byte[] data)
		{
			data[0] = (byte)value.Race;
			data[1] = (byte)value.Gender;
			data[2] = (byte)value.Age;
			data[3] = value.Height;
			data[4] = (byte)value.Tribe;
			data[5] = value.Head;
			data[6] = value.Hair;
			data[7] = value.Highlights;
			data[8] = value.Skintone;
			data[9] = value.REyeColor;
			data[10] = value.HairTone;
			data[11] = value.HighlightTone;
			data[12] = (byte)value.FacialFeatures;
			data[13] = value.LimbalEyes;
			data[14] = value.Eyebrows;
			data[15] = value.LEyeColor;
			data[16] = value.Eyes;
			data[17] = value.Nose;
			data[18] = value.Jaw;
			data[19] = value.Lips;
			data[20] = value.LipsToneFurPattern;
			data[21] = value.EarMuscleTailSize;
			data[22] = value.TailEarsType;
			data[23] = value.Bust;
			data[24] = value.FacePaint;
			data[25] = value.FacePaintColor;
		}
	}
}
