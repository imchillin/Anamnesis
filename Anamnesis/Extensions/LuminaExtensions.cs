// © Anamnesis.
// Licensed under the MIT license.

namespace Lumina
{
	using System;
	using System.Reflection;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using Anamnesis.Character.Utilities;
	using Anamnesis.GameData;
	using global::Lumina.Data.Files;
	using global::Lumina.Excel.GeneratedSheets;
	using global::Lumina.Extensions;
	using Serilog;
	using LuminaData = global::Lumina.GameData;

	public static class LuminaExtensions
	{
		public static IItem GetWeaponItem(ItemSlots slot, ulong val)
		{
			if (val == 0)
				return ItemUtility.NoneItem;

			short modelSet = (short)val;
			short modelBase = (short)(val >> 16);
			short modelVariant = (short)(val >> 32);

			if (modelSet < 0 || modelBase < 0 || modelVariant < 0)
			{
				Log.Information($"Invalid item value: {val}");

				modelSet = 0;
				modelBase = 0;
				modelVariant = 0;
			}

			return ItemUtility.GetItem(slot, (ushort)modelSet, (ushort)modelBase, (ushort)modelVariant);
		}

		public static IItem GetGearItem(ItemSlots slot, uint val)
		{
			if (val == 0)
				return ItemUtility.NoneItem;

			short modelSet = 0;
			short modelBase = (short)val;
			short modelVariant = (short)(val >> 16);

			if (modelSet < 0 || modelBase < 0 || modelVariant < 0)
			{
				Log.Information($"Invalid item value: {val}");

				modelSet = 0;
				modelBase = 0;
				modelVariant = 0;
			}

			return ItemUtility.GetItem(slot, (ushort)modelSet, (ushort)modelBase, (ushort)modelVariant);
		}

		public static void GetModel(ulong val, bool isWeapon, out ushort modelSet, out ushort modelBase, out ushort modelVariant)
		{
			if (isWeapon)
			{
				modelSet = (ushort)val;
				modelBase = (ushort)(val >> 16);
				modelVariant = (ushort)(val >> 32);
			}
			else
			{
				modelSet = 0;
				modelBase = (ushort)val;
				modelVariant = (ushort)(val >> 16);
			}
		}

		public static ImageSource? GetImage(this LuminaData self, uint imageId)
		{
			return self.GetImage((int)imageId);
		}

		public static ImageSource? GetImage(this LuminaData self, int imageId)
		{
			TexFile? tex = self.GetIcon(imageId);

			if (tex == null)
				return null;

			return tex.GetImage();
		}

		public static ImageSource? GetImage(this TexFile self)
		{
			if (self == null)
				return null;

			BitmapSource bmp = BitmapSource.Create(self.Header.Width, self.Header.Height, 96, 96, PixelFormats.Bgra32, null, self.ImageData, self.Header.Width * 4);
			bmp.Freeze();

			return bmp;
		}

		public static bool Contains(this ClassJobCategory self, Classes classJob)
		{
			string abr = classJob.GetAbbreviation();
			PropertyInfo? property = self.GetType().GetProperty(abr, BindingFlags.Public | BindingFlags.Instance);

			if (property == null)
				throw new Exception($"Unable to find ClassJob: {abr}");

			object? val = property.GetValue(self);

			if (val == null)
				throw new Exception($"Unable to find ClassJob Value: {abr}");

			return (bool)val;
		}

		public static Classes ToFlags(this ClassJobCategory self)
		{
			Classes classes = Classes.None;

			foreach (Classes? job in Enum.GetValues(typeof(Classes)))
			{
				if (job == null || job == Classes.None || job == Classes.All)
					continue;

				if (self.Contains((Classes)job))
				{
					classes |= (Classes)job;
				}
			}

			return classes;
		}

		public static bool Contains(this EquipSlotCategory self, ItemSlots slot)
		{
			switch (slot)
			{
				case ItemSlots.MainHand: return self.MainHand != 0;
				case ItemSlots.Head: return self.Head != 0;
				case ItemSlots.Body: return self.Body != 0;
				case ItemSlots.Hands: return self.Gloves != 0;
				case ItemSlots.Waist: return self.Waist != 0;
				case ItemSlots.Legs: return self.Legs != 0;
				case ItemSlots.Feet: return self.Feet != 0;
				case ItemSlots.OffHand: return self.OffHand != 0;
				case ItemSlots.Ears: return self.Ears != 0;
				case ItemSlots.Neck: return self.Neck != 0;
				case ItemSlots.Wrists: return self.Wrists != 0;
				case ItemSlots.RightRing: return self.FingerR != 0;
				case ItemSlots.LeftRing: return self.FingerL != 0;
				case ItemSlots.SoulCrystal: return self.SoulCrystal != 0;
			}

			return false;
		}
	}
}
