// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Styles
{
	using Anamnesis.Memory;
	using FontAwesome.Sharp;

	public static class ActorTypesExtensions
	{
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
			}

			return IconChar.Question;
		}
	}
}
