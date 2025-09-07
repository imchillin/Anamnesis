// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Utilities;

using Anamnesis.Actor.Items;
using System;

public static class DyeUtility
{
	public static readonly DummyNoneDye NoneDye = new();

	[Flags]
	public enum DyeTarget
	{
		None = 0,
		Clothing = 1,
		Accessories = 2,
		Weapons = 4,

		All = Clothing | Accessories | Weapons,
	}

	[Flags]
	public enum DyeSlot
	{
		None = 0,
		First = 1,
		Second = 2,

		All = First | Second,
	}
}
