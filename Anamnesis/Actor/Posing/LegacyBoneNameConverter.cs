// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Posing;

using System.Collections.Generic;

public static class LegacyBoneNameConverter
{
	private static readonly Dictionary<string, string> ModernToLegacy = new Dictionary<string, string>();
	private static readonly Dictionary<string, string> LegacyToModern = new Dictionary<string, string>()
		{
			// Body
			{ "Root", "n_root" },
			{ "Abdomen", "n_hara" },
			{ "Throw", "n_throw" },
			{ "Waist", "j_kosi" },
			{ "SpineA", "j_sebo_a" },
			{ "LegLeft", "j_asi_a_l" },
			{ "LegRight", "j_asi_a_r" },
			{ "HolsterLeft", "j_buki2_kosi_l" },
			{ "HolsterRight", "j_buki2_kosi_r" },
			{ "SheatheLeft", "j_buki_kosi_l" },
			{ "SheatheRight", "j_buki_kosi_r" },
			{ "SpineB", "j_sebo_b" },
			{ "ClothBackALeft", "j_sk_b_a_l" },
			{ "ClothBackARight", "j_sk_b_a_r" },
			{ "ClothFrontALeft", "j_sk_f_a_l" },
			{ "ClothFrontARight", "j_sk_f_a_r" },
			{ "ClothSideALeft", "j_sk_s_a_l" },
			{ "ClothSideARight", "j_sk_s_a_r" },
			{ "KneeLeft", "j_asi_b_l" },
			{ "KneeRight", "j_asi_b_r" },
			{ "BreastLeft", "j_mune_l" },
			{ "BreastRight", "j_mune_r" },
			{ "SpineC", "j_sebo_c" },
			{ "ClothBackBLeft", "j_sk_b_b_l" },
			{ "ClothBackBRight", "j_sk_b_b_r" },
			{ "ClothFrontBLeft", "j_sk_f_b_l" },
			{ "ClothFrontBRight", "j_sk_f_b_r" },
			{ "ClothSideBLeft", "j_sk_s_b_l" },
			{ "ClothSideBRight", "j_sk_s_b_r" },
			{ "CalfLeft", "j_asi_c_l" },
			{ "CalfRight", "j_asi_c_r" },
			{ "ScabbardLeft", "j_buki_sebo_l" },
			{ "ScabbardRight", "j_buki_sebo_r" },
			{ "Neck", "j_kubi" },
			{ "ClavicleLeft", "j_sako_l" },
			{ "ClavicleRight", "j_sako_r" },
			{ "ClothBackCLeft", "j_sk_b_c_l" },
			{ "ClothBackCRight", "j_sk_b_c_r" },
			{ "ClothFrontCLeft", "j_sk_f_c_l" },
			{ "ClothFrontCRight", "j_sk_f_c_r" },
			{ "ClothSideCLeft", "j_sk_s_c_l" },
			{ "ClothSideCRight", "j_sk_s_c_r" },
			{ "PoleynLeft", "n_hizasoubi_l" },
			{ "PoleynRight", "n_hizasoubi_r" },
			{ "FootLeft", "j_asi_d_l" },
			{ "FootRight", "j_asi_d_r" },
			{ "Head", "j_kao" },
			{ "ArmLeft", "j_ude_a_l" },
			{ "ArmRight", "j_ude_a_r" },
			{ "PauldronLeft", "n_kataarmor_l" },
			{ "PauldronRight", "n_kataarmor_r" },
			{ "ToesLeft", "j_asi_e_l" },
			{ "ToesRight", "j_asi_e_r" },
			{ "HairA", "j_kami_a" },
			{ "HairFrontLeft", "j_kami_f_l" },
			{ "HairFrontRight", "j_kami_f_r" },
			{ "EarLeft", "j_mimi_l" },
			{ "EarRight", "j_mimi_r" },
			{ "ForearmLeft", "j_ude_b_l" },
			{ "ForearmRight", "j_ude_b_r" },
			{ "ShoulderLeft", "n_hkata_l" },
			{ "ShoulderRight", "n_hkata_r" },
			{ "HairB", "j_kami_b" },
			{ "HandLeft", "j_te_l" },
			{ "HandRight", "j_te_r" },
			{ "ShieldLeft", "n_buki_tate_l" },
			{ "ShieldRight", "n_buki_tate_r" },
			{ "EarringALeft", "n_ear_a_l" },
			{ "EarringARight", "n_ear_a_r" },
			{ "ElbowLeft", "n_hhiji_l" },
			{ "ElbowRight", "n_hhiji_r" },
			{ "CouterLeft", "n_hijisoubi_l" },
			{ "CouterRight", "n_hijisoubi_r" },
			{ "WristLeft", "n_hte_l" },
			{ "WristRight", "n_hte_r" },
			{ "IndexALeft", "j_hito_a_l" },
			{ "IndexARight", "j_hito_a_r" },
			{ "PinkyALeft", "j_ko_a_l" },
			{ "PinkyARight", "j_ko_a_r" },
			{ "RingALeft", "j_kusu_a_l" },
			{ "RingARight", "j_kusu_a_r" },
			{ "MiddleALeft", "j_naka_a_l" },
			{ "MiddleARight", "j_naka_a_r" },
			{ "ThumbALeft", "j_oya_a_l" },
			{ "ThumbARight", "j_oya_a_r" },
			{ "WeaponLeft", "n_buki_l" },
			{ "WeaponRight", "n_buki_r" },
			{ "EarringBLeft", "n_ear_b_l" },
			{ "EarringBRight", "n_ear_b_r" },
			{ "IndexBLeft", "j_hito_b_l" },
			{ "IndexBRight", "j_hito_b_r" },
			{ "PinkyBLeft", "j_ko_b_l" },
			{ "PinkyBRight", "j_ko_b_r" },
			{ "RingBLeft", "j_kusu_b_l" },
			{ "RingBRight", "j_kusu_b_r" },
			{ "MiddleBLeft", "j_naka_b_l" },
			{ "MiddleBRight", "j_naka_b_r" },
			{ "ThumbBLeft", "j_oya_b_l" },
			{ "ThumbBRight", "j_oya_b_r" },
			{ "TailA", "n_sippo_a" },
			{ "TailB", "n_sippo_b" },
			{ "TailC", "n_sippo_c" },
			{ "TailD", "n_sippo_d" },
			{ "TailE", "n_sippo_e" },

			// Head
			{ "RootHead", "j_kao" },
			{ "Jaw", "j_ago" },
			{ "EyelidLowerLeft", "j_f_dmab_l" },
			{ "EyelidLowerRight", "j_f_dmab_r" },
			{ "EyeLeft", "j_f_eye_l" },
			{ "EyeRight", "j_f_eye_r" },
			{ "Nose", "j_f_hana" },
			{ "CheekLeft", "j_f_hoho_l" },
			{ "CheekRight", "j_f_hoho_r" },
			{ "LipsLeft", "j_f_lip_l" },
			{ "LipsRight", "j_f_lip_r" },
			{ "EyebrowLeft", "j_f_mayu_l" },
			{ "EyebrowRight", "j_f_mayu_r" },
			{ "Bridge", "j_f_memoto" },
			{ "BrowLeft", "j_f_miken_l" },
			{ "BrowRight", "j_f_miken_r" },
			{ "LipUpperA", "j_f_ulip_a" },
			{ "EyelidUpperLeft", "j_f_umab_l" },
			{ "EyelidUpperRight", "j_f_umab_r" },
			{ "LipLowerA", "j_f_dlip_a" },
			{ "LipUpperB", "j_f_ulip_b" },
			{ "LipLowerB", "j_f_dlip_b" },

			// Viera Ears
			{ "VieraEar01ALeft", "j_zera_a_l" },
			{ "VieraEar01ARight", "j_zera_a_r" },
			{ "VieraEar01BLeft", "j_zera_b_l" },
			{ "VieraEar01BRight", "j_zera_b_r" },
			{ "VieraEar02ALeft", "j_zerb_a_l" },
			{ "VieraEar02ARight", "j_zerb_a_r" },
			{ "VieraEar02BLeft", "j_zerb_b_l" },
			{ "VieraEar02BRight", "j_zerb_b_r" },
			{ "VieraEar03ALeft", "j_zerc_a_l" },
			{ "VieraEar03ARight", "j_zerc_a_r" },
			{ "VieraEar03BLeft", "j_zerc_b_l" },
			{ "VieraEar03BRight", "j_zerc_b_r" },
			{ "VieraEar04ALeft", "j_zerd_a_l" },
			{ "VieraEar04ARight", "j_zerd_a_r" },
			{ "VieraEar04BLeft", "j_zerd_b_l" },
			{ "VieraEar04BRight", "j_zerd_b_r" },
			{ "VieraLipLowerA", "j_f_dlip_a" },
			{ "VieraLipUpperB", "j_f_ulip_b" },
			{ "VieraLipLowerB", "j_f_dlip_b" },

			// Hrothgar Faces
			{ "HrothWhiskersLeft", "j_f_hige_l" },
			{ "HrothWhiskersRight", "j_f_hige_r" },
			{ "HrothEyebrowLeft", "j_f_mayu_l" },
			{ "HrothEyebrowRight", "j_f_mayu_r" },
			{ "HrothBridge", "j_f_memoto" },
			{ "HrothBrowLeft", "j_f_miken_l" },
			{ "HrothBrowRight", "j_f_miken_r" },
			{ "HrothJawUpper", "j_f_uago" },	// might be flipped with Lip Upper...
			{ "HrothLipUpper", "j_f_ulip" },
			{ "HrothEyelidUpperLeft", "j_f_umab_l" },
			{ "HrothEyelidUpperRight", "j_f_umab_r" },
			{ "HrothLipsLeft", "n_f_lip_l" },
			{ "HrothLipsRight", "n_f_lip_r" },
			{ "HrothLipUpperLeft", "n_f_ulip_l" },
			{ "HrothLipUpperRight", "n_f_ulip_r" },
			{ "HrothLipLower", "j_f_dlip" },
		};

	static LegacyBoneNameConverter()
	{
		foreach ((string legacy, string modern) in LegacyToModern)
		{
			// Skip over duplicate modern keys, since Hroth and Viera both duplicate bones.
			if (ModernToLegacy.ContainsKey(modern))
				continue;

			ModernToLegacy.Add(modern, legacy);
		}
	}

	public static string? GetModernName(string legacyName)
	{
		string? name = null;
		LegacyToModern.TryGetValue(legacyName, out name);
		return name;
	}

	public static string? GetLegacyName(string modernName)
	{
		string? name = null;
		ModernToLegacy.TryGetValue(modernName, out name);
		return name;
	}
}
