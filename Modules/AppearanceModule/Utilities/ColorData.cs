// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Utilities
{
	using System;
	using System.Collections.Generic;
	using Anamnesis;

	using cmColor = Anamnesis.Color;
	using wpfColor = System.Windows.Media.Color;

	public static class ColorData
	{
		private static readonly Entry[] Colors;

		static ColorData()
		{
			List<Entry> colors = new List<Entry>();
			byte[] buffer = Resources.colors;

			int at = 0;
			while (at < buffer.Length)
			{
				int r = buffer[at + 0] & 0xFF;
				int g = buffer[at + 1] & 0xFF;
				int b = buffer[at + 2] & 0xFF;
				int a = buffer[at + 3] & 0xFF;

				Entry entry = default;
				entry.CmColor = new cmColor(r / 255.0f, g / 255.0f, b / 255.0f);

				wpfColor c2 = (wpfColor)entry.WpfColor;
				c2.R = buffer[at + 0];
				c2.G = buffer[at + 1];
				c2.B = buffer[at + 2];
				c2.A = buffer[at + 3];
				entry.WpfColor = c2;

				////= new Color.FromArgb((a << 24) | (r << 16) | (g << 8) | b);

				colors.Add(entry);

				at += 4;
			}

			Colors = colors.ToArray();
		}

		public static Entry[] GetSkin(Appearance.Tribes tribe, Appearance.Genders gender)
		{
			int from = GetTribeSkinStartIndex(tribe, gender);
			return Span(from, 192);
		}

		public static Entry[] GetHair(Appearance.Tribes tribe, Appearance.Genders gender)
		{
			int from = GetTribeHairStartIndex(tribe, gender);
			return Span(from, 192);
		}

		public static Entry[] GetHairHighlights()
		{
			return Span(256, 192);
		}

		public static Entry[] GetEyeColors()
		{
			return Span(0, 192);
		}

		public static Entry[] GetLimbalColors()
		{
			return Span(0, 192);
		}

		public static Entry[] GetFacePaintColor()
		{
			return Span(512, 192);
		}

		public static Entry[] GetLipColors()
		{
			List<Entry> entries = new List<Entry>();
			entries.AddRange(Span(512, 96));

			for (int i = 0; i < 32; i++)
			{
				Entry entry = default;
				entry.Skip = true;
				entries.Add(entry);
			}

			entries.AddRange(Span(1792, 96));

			return entries.ToArray();
		}

		private static Entry[] Span(int from, int count)
		{
			Entry[] entries = new Entry[count];
			Array.Copy(Colors, from, entries, 0, count);
			return entries;
		}

		private static int GetTribeSkinStartIndex(Appearance.Tribes tribe, Appearance.Genders gender)
		{
			bool isMasculine = gender == Appearance.Genders.Masculine;

			switch (tribe)
			{
				case Appearance.Tribes.Midlander: return isMasculine ? 4608 : 3328;
				case Appearance.Tribes.Highlander: return isMasculine ? 7168 : 5888;
				case Appearance.Tribes.Wildwood: return isMasculine ? 9728 : 8448;
				case Appearance.Tribes.Duskwight: return isMasculine ? 12288 : 11008;
				case Appearance.Tribes.Plainsfolk: return isMasculine ? 14848 : 13568;
				case Appearance.Tribes.Dunesfolk: return isMasculine ? 17408 : 16128;
				case Appearance.Tribes.SeekerOfTheSun: return isMasculine ? 19968 : 18688;
				case Appearance.Tribes.KeeperOfTheMoon: return isMasculine ? 22528 : 21248;
				case Appearance.Tribes.SeaWolf: return isMasculine ? 25088 : 23808;
				case Appearance.Tribes.Hellsguard: return isMasculine ? 27648 : 26368;
				case Appearance.Tribes.Raen: return isMasculine ? 28928 : 30208;
				case Appearance.Tribes.Xaela: return isMasculine ? 31488 : 32768;
				case Appearance.Tribes.Helions: return 34048;
				case Appearance.Tribes.TheLost: return 35840;
				case Appearance.Tribes.Rava: return 40448;
				case Appearance.Tribes.Veena: return 43008;
			}

			throw new Exception("Unknown tribe: " + tribe);
		}

		private static int GetTribeHairStartIndex(Appearance.Tribes tribe, Appearance.Genders gender)
		{
			bool isMasculine = gender == Appearance.Genders.Masculine;

			switch (tribe)
			{
				case Appearance.Tribes.Midlander: return isMasculine ? 4864 : 3584;
				case Appearance.Tribes.Highlander: return isMasculine ? 7424 : 6144;
				case Appearance.Tribes.Wildwood: return isMasculine ? 9984 : 8704;
				case Appearance.Tribes.Duskwight: return isMasculine ? 12544 : 11264;
				case Appearance.Tribes.Plainsfolk: return isMasculine ? 15104 : 13824;
				case Appearance.Tribes.Dunesfolk: return isMasculine ? 17664 : 16384;
				case Appearance.Tribes.SeekerOfTheSun: return isMasculine ? 20224 : 18944;
				case Appearance.Tribes.KeeperOfTheMoon: return isMasculine ? 22784 : 21504;
				case Appearance.Tribes.SeaWolf: return isMasculine ? 25344 : 24064;
				case Appearance.Tribes.Hellsguard: return isMasculine ? 27904 : 26624;
				case Appearance.Tribes.Raen: return isMasculine ? 30464 : 29184;
				case Appearance.Tribes.Xaela: return isMasculine ? 33024 : 31744;
				case Appearance.Tribes.Helions: return 34304;
				case Appearance.Tribes.TheLost: return 36608;
				case Appearance.Tribes.Rava: return 40704;
				case Appearance.Tribes.Veena: return 43264;
			}

			throw new Exception("Unknown tribe: " + tribe);
		}

		public struct Entry
		{
			public cmColor CmColor { get; set; }
			public wpfColor WpfColor { get; set; }
			public bool Skip { get; set; }
		}
	}
}
