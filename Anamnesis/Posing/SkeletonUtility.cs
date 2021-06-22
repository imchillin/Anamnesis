// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.PoseModule
{
	using System;
	using System.Collections.Generic;
	using Anamnesis.Memory;

	public static class SkeletonUtility
	{
		/// <summary>
		/// Maps the CMTool bone names to their index within the Skeleton Body bones transform array.
		/// </summary>
		public static readonly Dictionary<string, int> BodyBoneIndexLookup = new Dictionary<string, int>()
		{
			{ "Root", 0 },
			{ "Abdomen", 1 },
			{ "Throw", 2 },
			{ "Waist", 3 },
			{ "SpineA", 4 },
			{ "LegsLeft", 5 },
			{ "LegsRight", 6 },
			{ "LegLeft", 5 },
			{ "LegRight", 6 },
			{ "HolsterLeft", 7 },
			{ "HolsterRight", 8 },
			{ "SheatheLeft", 9 },
			{ "SheatheRight", 10 },
			{ "SpineB", 11 },
			{ "ClothBackALeft", 12 },
			{ "ClothBackARight", 13 },
			{ "ClothFrontALeft", 14 },
			{ "ClothFrontARight", 15 },
			{ "ClothSideALeft", 16 },
			{ "ClothSideARight", 17 },
			{ "KneeLeft", 18 },
			{ "KneeRight", 19 },
			{ "BreastLeft", 20 },
			{ "BreastRight", 21 },
			{ "SpineC", 22 },
			{ "ClothBackBLeft", 23 },
			{ "ClothBackBRight", 24 },
			{ "ClothFrontBLeft", 25 },
			{ "ClothFrontBRight", 26 },
			{ "ClothSideBLeft", 27 },
			{ "ClothSideBRight", 28 },
			{ "CalfLeft", 29 },
			{ "CalfRight", 30 },
			{ "ScabbardLeft", 31 },
			{ "ScabbardRight", 32 },
			{ "Neck", 33 },
			{ "ClavicleLeft", 34 },
			{ "ClavicleRight", 35 },
			{ "ClothBackCLeft", 36 },
			{ "ClothBackCRight", 37 },
			{ "ClothFrontCLeft", 38 },
			{ "ClothFrontCRight", 39 },
			{ "ClothSideCLeft", 40 },
			{ "ClothSideCRight", 41 },
			{ "PoleynLeft", 42 },
			{ "PoleynRight", 43 },
			{ "FootLeft", 44 },
			{ "FootRight", 45 },
			{ "Head", 46 },
			{ "ArmLeft", 47 },
			{ "ArmRight", 48 },
			{ "PauldronLeft", 49 },
			{ "PauldronRight", 50 },
			{ "Unknown00", 51 },
			{ "ToesLeft", 52 },
			{ "ToesRight", 53 },
			{ "HairA", 54 },
			{ "HairFrontLeft", 55 },
			{ "HairFrontRight", 56 },
			{ "EarLeft", 57 },
			{ "EarRight", 58 },
			{ "ForearmLeft", 59 },
			{ "ForearmRight", 60 },
			{ "ShoulderLeft", 61 },
			{ "ShoulderRight", 62 },
			{ "HairB", 63 },
			{ "HandLeft", 64 },
			{ "HandRight", 65 },
			{ "ShieldLeft", 66 },
			{ "ShieldRight", 67 },
			{ "EarringALeft", 68 },
			{ "EarringARight", 69 },
			{ "ElbowLeft", 70 },
			{ "ElbowRight", 71 },
			{ "CouterLeft", 72 },
			{ "CouterRight", 73 },
			{ "WristLeft", 74 },
			{ "WristRight", 75 },
			{ "IndexALeft", 76 },
			{ "IndexARight", 77 },
			{ "PinkyALeft", 78 },
			{ "PinkyARight", 79 },
			{ "RingALeft", 80 },
			{ "RingARight", 81 },
			{ "MiddleALeft", 82 },
			{ "MiddleARight", 83 },
			{ "ThumbALeft", 84 },
			{ "ThumbARight", 85 },
			{ "WeaponLeft", 86 },
			{ "WeaponRight", 87 },
			{ "EarringBLeft", 88 },
			{ "EarringBRight", 89 },
			{ "IndexBLeft", 90 },
			{ "IndexBRight", 91 },
			{ "PinkyBLeft", 92 },
			{ "PinkyBRight", 93 },
			{ "RingBLeft", 94 },
			{ "RingBRight", 95 },
			{ "MiddleBLeft", 96 },
			{ "MiddleBRight", 97 },
			{ "ThumbBLeft", 98 },
			{ "ThumbBRight", 99 },
			{ "TailA", 100 },
			{ "TailB", 101 },
			{ "TailC", 102 },
			{ "TailD", 103 },
			{ "TailE", 104 },
		};

		/// <summary>
		/// Maps the CMTool bone names to their index within the Skeleton Head Bones transform array.
		/// </summary>
		public static readonly Dictionary<string, int> HeadBoneIndexLookup = new Dictionary<string, int>()
		{
			{ "RootHead", 0 },
			{ "Jaw", 1 },
			{ "EyelidLowerLeft", 2 },
			{ "EyelidLowerRight", 3 },
			{ "EyeLeft", 4 },
			{ "EyeRight", 5 },
			{ "Nose", 6 },
			{ "CheekLeft", 7 },
			{ "HrothWhiskersLeft", 7 },
			{ "CheekRight", 8 },
			{ "HrothWhiskersRight", 8 },
			{ "LipsLeft", 9 },
			{ "HrothEyebrowLeft", 9 },
			{ "HrothEyebrowRight", 10 },
			{ "LipsRight", 10 },
			{ "EyebrowLeft", 11 },
			{ "HrothBridge", 11 },
			{ "EyebrowRight", 12 },
			{ "HrothBrowLeft", 12 },
			{ "Bridge", 13 },
			{ "HrothBrowRight", 13 },
			{ "BrowLeft", 14 },
			{ "HrothJawUpper", 14 },
			{ "BrowRight", 15 },
			{ "HrothLipUpper", 15 },
			{ "LipUpperA", 16 },
			{ "HrothEyelidUpperLeft", 16 },
			{ "HrothEyelidUpperRight", 17 },
			{ "EyelidUpperLeft", 17 },
			{ "EyelidUpperRight", 18 },
			{ "HrothLipsLeft", 18 },
			{ "LipLowerA", 19 },
			{ "HrothLipsRight", 19 },
			{ "VieraEar01ALeft", 19 },
			{ "LipUpperB", 20 },
			{ "HrothLipUpperLeft", 20 },
			{ "VieraEar01ARight", 20 },
			{ "LipLowerB", 21 },
			{ "HrothLipUpperRight", 21 },
			{ "VieraEar02ALeft", 21 },
			{ "VieraEar02ARight", 22 },
			{ "HrothLipLower", 22 },
			{ "VieraEar03ALeft", 23 },
			{ "VieraEar03ARight", 24 },
			{ "VieraEar04ALeft", 25 },
			{ "VieraEar04ARight", 26 },
			{ "VieraLipLowerA", 27 },
			{ "VieraLipUpperB", 28 },
			{ "VieraEar01BLeft", 29 },
			{ "VieraEar01BRight", 30 },
			{ "VieraEar02BLeft", 31 },
			{ "VieraEar02BRight", 32 },
			{ "VieraEar03BLeft", 33 },
			{ "VieraEar03BRight", 34 },
			{ "VieraEar04BLeft", 35 },
			{ "VieraEar04BRight", 36 },
			{ "VieraLipLowerB", 37 },
		};

		/// <summary>
		/// Provides a mapping of compatible head bones that can be used to convet poses from one race to another.
		/// </summary>
		public static readonly Dictionary<string, string> HrothHeadBoneNameMap = new Dictionary<string, string>()
		{
			{ "HrothBridge", "Bridge" },
			{ "HrothBrowLeft", "BrowLeft" },
			{ "HrothBrowRight", "BrowRight" },
			{ "HrothLipUpper", "LipUpperA" },
			{ "HrothEyelidUpperLeft", "EyelidUpperLeft" },
			{ "HrothEyelidUpperRight", "EyelidUpperRight" },
			{ "HrothLipsLeft", "LipsLeft" },
			{ "HrothLipsRight", "LipsRight" },
			{ "HrothLipLower", "LipLowerA" },
		};

		/// <summary>
		/// Provides a mapping of compatible head bones that can be used to convet poses from one race to another.
		/// </summary>
		public static readonly Dictionary<string, string> VieraHeadBoneNameMap = new Dictionary<string, string>()
		{
			{ "VieraLipLowerA", "LipLowerA" },
			{ "VieraLipUpperB", "LipUpperB" },
			{ "VieraLipLowerB", "LipLowerB" },
		};

		/// <summary>
		/// Maps the CMTool bone names to their index within the Skeleton Hair Bones transform array.
		/// </summary>
		public static readonly Dictionary<string, int> HairBoneIndexLookup = new Dictionary<string, int>()
		{
			{ "ExRootHair", 0 },
			{ "ExHairA", 1 },
			{ "ExHairB", 2 },
			{ "ExHairC", 3 },
			{ "ExHairD", 4 },
			{ "ExHairE", 5 },
			{ "ExHairF", 6 },
			{ "ExHairG", 7 },
			{ "ExHairH", 8 },
			{ "ExHairI", 9 },
			{ "ExHairJ", 10 },
			{ "ExHairK", 11 },
			{ "ExHairL", 12 },
		};

		/// <summary>
		/// Maps the CMTool bone names to their index within the Skeleton Met Bones transform array.
		/// </summary>
		public static readonly Dictionary<string, int> MetBoneIndexLookup = new Dictionary<string, int>()
		{
			{ "ExRootMet", 0 },
			{ "ExMetA", 1 },
			{ "ExMetB", 2 },
			{ "ExMetC", 3 },
			{ "ExMetD", 4 },
			{ "ExMetE", 5 },
			{ "ExMetF", 6 },
			{ "ExMetG", 7 },
			{ "ExMetH", 8 },
			{ "ExMetI", 9 },
			{ "ExMetJ", 10 },
			{ "ExMetK", 11 },
			{ "ExMetL", 12 },
			{ "ExMetM", 13 },
			{ "ExMetN", 14 },
			{ "ExMetO", 15 },
			{ "ExMetP", 16 },
			{ "ExMetQ", 17 },
			{ "ExMetR", 18 },
		};

		/// <summary>
		/// Maps the CMTool bone names to their index within the Skeleton Top Bones transform array.
		/// </summary>
		public static readonly Dictionary<string, int> TopBoneIndexLookup = new Dictionary<string, int>()
		{
			{ "ExRootTop", 0 },
			{ "ExTopA", 1 },
			{ "ExTopB", 2 },
			{ "ExTopC", 3 },
			{ "ExTopD", 4 },
			{ "ExTopE", 5 },
			{ "ExTopF", 6 },
			{ "ExTopG", 7 },
			{ "ExTopH", 8 },
			{ "ExTopI", 9 },
		};

		/// <summary>
		/// The parenting array for player body skeletons.
		/// </summary>
		// This data _should_ exist in memory somewhre for _all_ possible skeletons.
		// we dont know where, though!
		public static readonly List<int> PlayerBodyParents = new List<int>()
		{
			-1, -1, -1, 0, 0, 3, 3, 9, 10, 11, 11, 4, 3, 3, 3, 3, 3, 3, 29, 30, 11, 11, 11, 12, 13, 14, 15, 16, 17, 5, 6, 9, 10, 22, 22, 22, 23, 24, 25, 26, 27, 28, 18, 19, 29, 30, 33, 34, 35, 47, 48, -1, 44, 45, 46, 46, 46, 46, 46, 47, 48, 47, 48, 54, 74, 75, 59, 60, 57, 58, 59, 60, 59, 60, 59, 60, 64, 65, 64, 65, 64, 65, 64, 65, 64, 65, 64, 65, 68, 69, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 3, 100, 101, 102, 103,
		};
	}
}
