// © Anamnesis.
// Licensed under the MIT license.
namespace Anamnesis.Memory;

using Anamnesis.Styles;
using System;
using System.Collections.Generic;

public enum ObjectTypes : byte
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
	ReactionEvent = 0x0E,
	Ornament = 0x0F,
	CardStand = 0x10,
}

public class ObjectType(string name, ObjectTypes value)
{
	private static readonly List<ObjectType> s_allActorTypes;

	static ObjectType()
	{
		s_allActorTypes = new List<ObjectType>();
		foreach (ObjectTypes value in Enum.GetValues<ObjectTypes>())
		{
			var name = Enum.GetName(value);
			s_allActorTypes.Add(new ObjectType(name!, value));
		}
	}

	public static IEnumerable<ObjectType> AllActorTypes => s_allActorTypes;
	public string Name { get; private set; } = name;
	public ObjectTypes Value { get; private set; } = value;
	public bool IsSupportedType { get; private set; } = value.IsSupportedType();
}
