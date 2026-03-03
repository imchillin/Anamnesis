// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Styles;

using Anamnesis.Memory;
using FontAwesome.Sharp;

public static class ActorTypesExtensions
{
	public static bool IsSupportedType(this ObjectTypes actorType)
	{
		return actorType switch
		{
			ObjectTypes.Player or ObjectTypes.BattleNpc or ObjectTypes.EventNpc or ObjectTypes.Companion or ObjectTypes.Mount or ObjectTypes.Ornament or ObjectTypes.Retainer => true,
			_ => false,
		};
	}

	public static IconChar GetIcon(this ObjectTypes type)
	{
		return type switch
		{
			ObjectTypes.Player => IconChar.UserAlt,
			ObjectTypes.BattleNpc => IconChar.UserShield,
			ObjectTypes.EventNpc => IconChar.UserNinja,
			ObjectTypes.Treasure => IconChar.Coins,
			ObjectTypes.Aetheryte => IconChar.Gem,
			ObjectTypes.Companion => IconChar.Cat,
			ObjectTypes.Retainer => IconChar.ConciergeBell,
			ObjectTypes.Housing => IconChar.Chair,
			ObjectTypes.Mount => IconChar.Horse,
			ObjectTypes.Ornament => IconChar.HatCowboy,
			_ => IconChar.Question,
		};
	}
}
