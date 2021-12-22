// © Anamnesis.
// Licensed under the MIT license.
namespace Anamnesis.Memory
{
	using System;
	using System.Collections.Generic;

	public enum ActorTypes : byte
	{
		None = 0x00,
		Player = 0x01,
		BattleNpc = 0x02,
		EventNpc = 0x03,
		Treasure = 0x04,
		Aetheryte = 0x05,
		GatheringPoint = 0x06,
		EventObj = 0x07,
		Mount = 0x08,
		Companion = 0x09,
		Retainer = 0x0A,
		Area = 0x0B,
		Housing = 0x0C,
		Cutscene = 0x0D,
		CardStand = 0x0E,
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:File name should match first type name", Justification = "Not the first type")]
	public class ActorType
	{
		public ActorType(string name, ActorTypes value, bool isSupported)
		{
			this.Name = name;
			this.Value = value;
			this.IsTypeSupported = isSupported;
		}

		public static IEnumerable<ActorType> AllActorTypes
		{
			get
			{
				List<ActorType> actorTypes = new();

				foreach (ActorTypes value in Enum.GetValues(typeof(ActorTypes)))
				{
					var name = Enum.GetName(typeof(ActorTypes), value);
					var isSupported = IsActorTypeSupported(value);
					actorTypes.Add(new ActorType(name!, value, isSupported));
				}

				return actorTypes;
			}
		}

		public string Name { get; private set; }
		public ActorTypes Value { get; private set; }
		public bool IsTypeSupported { get; private set; }

		public static bool IsActorTypeSupported(ActorTypes actorType)
		{
			switch (actorType)
			{
				case ActorTypes.Player:
				case ActorTypes.BattleNpc:
				case ActorTypes.EventNpc:
				case ActorTypes.Companion:
				case ActorTypes.Mount:
					return true;
			}

			return false;
		}
	}
}
