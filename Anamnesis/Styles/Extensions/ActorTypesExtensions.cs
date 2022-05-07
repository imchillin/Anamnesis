// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Styles;

using Anamnesis.Memory;
using FontAwesome.Sharp;

public static class ActorTypesExtensions
{
	public static bool IsSupportedType(this ActorTypes actorType)
	{
		switch (actorType)
		{
			case ActorTypes.Player:
			case ActorTypes.BattleNpc:
			case ActorTypes.EventNpc:
			case ActorTypes.Companion:
			case ActorTypes.Mount:
			case ActorTypes.Ornament:
			case ActorTypes.Retainer:
				return true;
		}

		return false;
	}

	public static IconChar GetIcon(this ActorTypes type)
	{
		switch (type)
		{
			case ActorTypes.Player: return IconChar.UserAlt;
			case ActorTypes.BattleNpc: return IconChar.UserShield;
			case ActorTypes.EventNpc: return IconChar.UserNinja;
			case ActorTypes.Treasure: return IconChar.Coins;
			case ActorTypes.Aetheryte: return IconChar.Gem;
			case ActorTypes.Companion: return IconChar.Cat;
			case ActorTypes.Retainer: return IconChar.ConciergeBell;
			case ActorTypes.Housing: return IconChar.Chair;
			case ActorTypes.Mount: return IconChar.Horse;
			case ActorTypes.Ornament: return IconChar.HatCowboy;
		}

		return IconChar.Question;
	}
}
