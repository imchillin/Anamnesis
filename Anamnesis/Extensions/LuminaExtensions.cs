// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Extensions
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Anamnesis.Character.Utilities;
	using Anamnesis.GameData;

	public static class LuminaExtensions
	{
		public static IItem GetItem(ItemSlots slot, ulong val)
		{
			// ulong = unsigned 64-bit int = 8 byte
			byte[] bytes = BitConverter.GetBytes(val);

			short modelSet;
			short modelBase;
			short modelVariant;

			if (slot == ItemSlots.MainHand || slot == ItemSlots.OffHand)
			{
				modelSet = (short)val;
				modelBase = (short)(val >> 16);
				modelVariant = (short)(val >> 32);
			}
			else
			{
				modelSet = 0;
				modelBase = (short)val;
				modelVariant = (short)(val >> 16);
			}

			return ItemUtility.GetItem(slot, (ushort)modelSet, (ushort)modelBase, (ushort)modelVariant);
		}
	}
}
