// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Styles;

using Anamnesis.Memory;
using FontAwesome.Sharp;

public static class ActorTypesExtensions
{
	public static bool IsSupportedType(this ActorTypes actorType)
	{
		return actorType switch
		{
			ActorTypes.Player or ActorTypes.BattleNpc or ActorTypes.EventNpc or ActorTypes.Companion or ActorTypes.Mount or ActorTypes.Ornament or ActorTypes.Retainer => true,
			_ => false,
		};
	}

	public static IconChar GetIcon(this ActorTypes type)
	{
		return type switch
		{
			ActorTypes.Player => IconChar.UserAlt,
			ActorTypes.BattleNpc => IconChar.UserShield,
			ActorTypes.EventNpc => IconChar.UserNinja,
			ActorTypes.Treasure => IconChar.Coins,
			ActorTypes.Aetheryte => IconChar.Gem,
			ActorTypes.Companion => IconChar.Cat,
			ActorTypes.Retainer => IconChar.ConciergeBell,
			ActorTypes.Housing => IconChar.Chair,
			ActorTypes.Mount => IconChar.Horse,
			ActorTypes.Ornament => IconChar.HatCowboy,
			_ => IconChar.Question,
		};
	}
}
