// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Posing;

using Anamnesis.Memory;
using System.Collections.Generic;

public static class LinkedBones
{
	public static readonly List<LinkSet> Links = new()
	{
		// Eyes
		new("j_f_eye_r", "j_f_eye_l"),

		// Viera Ears: Femenine: Link 'Ears A' and 'Ears B' for ears 1, 2, & 4 (a, b, & d)
		new(ActorCustomizeMemory.Tribes.Rava, ActorCustomizeMemory.Genders.Feminine, "j_zera_a_l", "j_zerb_a_l", "j_zerd_a_l"),
		new(ActorCustomizeMemory.Tribes.Rava, ActorCustomizeMemory.Genders.Feminine, "j_zera_a_r", "j_zerb_a_r", "j_zerd_a_r"),
		new(ActorCustomizeMemory.Tribes.Rava, ActorCustomizeMemory.Genders.Feminine, "j_zera_b_l", "j_zerb_b_l", "j_zerd_b_l"),
		new(ActorCustomizeMemory.Tribes.Rava, ActorCustomizeMemory.Genders.Feminine, "j_zera_b_r", "j_zerb_b_r", "j_zerd_b_r"),
		new(ActorCustomizeMemory.Tribes.Veena, ActorCustomizeMemory.Genders.Feminine, "j_zera_a_l", "j_zerb_a_l", "j_zerd_a_l"),
		new(ActorCustomizeMemory.Tribes.Veena, ActorCustomizeMemory.Genders.Feminine, "j_zera_a_r", "j_zerb_a_r", "j_zerd_a_r"),
		new(ActorCustomizeMemory.Tribes.Veena, ActorCustomizeMemory.Genders.Feminine, "j_zera_b_l", "j_zerb_b_l", "j_zerd_b_l"),
		new(ActorCustomizeMemory.Tribes.Veena, ActorCustomizeMemory.Genders.Feminine, "j_zera_b_r", "j_zerb_b_r", "j_zerd_b_r"),

		// Viera Ears: Masculine: Link 'Ears A' and 'Ears B' for ears 1, 3, & 4 (a, c, & d)
		new(ActorCustomizeMemory.Tribes.Rava, ActorCustomizeMemory.Genders.Masculine, "j_zera_a_l", "j_zerc_a_l", "j_zerd_a_l"),
		new(ActorCustomizeMemory.Tribes.Rava, ActorCustomizeMemory.Genders.Masculine, "j_zera_a_r", "j_zerc_a_r", "j_zerd_a_r"),
		new(ActorCustomizeMemory.Tribes.Rava, ActorCustomizeMemory.Genders.Masculine, "j_zera_b_l", "j_zerc_b_l", "j_zerd_b_l"),
		new(ActorCustomizeMemory.Tribes.Rava, ActorCustomizeMemory.Genders.Masculine, "j_zera_b_r", "j_zerc_b_r", "j_zerd_b_r"),
		new(ActorCustomizeMemory.Tribes.Veena, ActorCustomizeMemory.Genders.Masculine, "j_zera_a_l", "j_zerc_a_l", "j_zerd_a_l"),
		new(ActorCustomizeMemory.Tribes.Veena, ActorCustomizeMemory.Genders.Masculine, "j_zera_a_r", "j_zerc_a_r", "j_zerd_a_r"),
		new(ActorCustomizeMemory.Tribes.Veena, ActorCustomizeMemory.Genders.Masculine, "j_zera_b_l", "j_zerc_b_l", "j_zerd_b_l"),
		new(ActorCustomizeMemory.Tribes.Veena, ActorCustomizeMemory.Genders.Masculine, "j_zera_b_r", "j_zerc_b_r", "j_zerd_b_r"),
	};

	public class LinkSet
	{
		public readonly ActorCustomizeMemory.Tribes? Tribe;
		public readonly ActorCustomizeMemory.Genders? Gender;
		public readonly HashSet<string> Bones;

		public LinkSet(ActorCustomizeMemory.Tribes tribe, ActorCustomizeMemory.Genders gender, params string[] bones)
			: this(bones)
		{
			this.Tribe = tribe;
			this.Gender = gender;
		}

		public LinkSet(params string[] bones)
		{
			this.Bones = new(bones);
		}

		public bool Contains(string boneName)
		{
			return this.Bones.Contains(boneName);
		}
	}
}
