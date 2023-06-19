// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.Json.Serialization;
using Anamnesis.Memory;

public class CmToolPoseFile : JsonFileBase
{
	[JsonIgnore] public override string FileExtension => ".cmp";
	[JsonIgnore] public override string TypeName => "CMTool pose";

	/*public string Description { get; set; }
	public string DateCreated { get; set; }
	public string CMPVersion { get; set; }
	public string Clan { get; set; }
	public string Body { get; set; }*/

	public string? Race { get; set; }

	public string? Root { get; set; }
	public string? Abdomen { get; set; }
	public string? Throw { get; set; }
	public string? Waist { get; set; }
	public string? SpineA { get; set; }
	public string? LegLeft { get; set; }
	public string? LegRight { get; set; }
	public string? HolsterLeft { get; set; }
	public string? HolsterRight { get; set; }
	public string? SheatheLeft { get; set; }
	public string? SheatheRight { get; set; }
	public string? SpineB { get; set; }
	public string? ClothBackALeft { get; set; }
	public string? ClothBackARight { get; set; }
	public string? ClothFrontALeft { get; set; }
	public string? ClothFrontARight { get; set; }
	public string? ClothSideALeft { get; set; }
	public string? ClothSideARight { get; set; }
	public string? KneeLeft { get; set; }
	public string? KneeRight { get; set; }
	public string? BreastLeft { get; set; }
	public string? BreastRight { get; set; }
	public string? SpineC { get; set; }
	public string? ClothBackBLeft { get; set; }
	public string? ClothBackBRight { get; set; }
	public string? ClothFrontBLeft { get; set; }
	public string? ClothFrontBRight { get; set; }
	public string? ClothSideBLeft { get; set; }
	public string? ClothSideBRight { get; set; }
	public string? CalfLeft { get; set; }
	public string? CalfRight { get; set; }
	public string? ScabbardLeft { get; set; }
	public string? ScabbardRight { get; set; }
	public string? Neck { get; set; }
	public string? ClavicleLeft { get; set; }
	public string? ClavicleRight { get; set; }
	public string? ClothBackCLeft { get; set; }
	public string? ClothBackCRight { get; set; }
	public string? ClothFrontCLeft { get; set; }
	public string? ClothFrontCRight { get; set; }
	public string? ClothSideCLeft { get; set; }
	public string? ClothSideCRight { get; set; }
	public string? PoleynLeft { get; set; }
	public string? PoleynRight { get; set; }
	public string? FootLeft { get; set; }
	public string? FootRight { get; set; }
	public string? Head { get; set; }
	public string? ArmLeft { get; set; }
	public string? ArmRight { get; set; }
	public string? PauldronLeft { get; set; }
	public string? PauldronRight { get; set; }
	public string? Unknown00 { get; set; }
	public string? ToesLeft { get; set; }
	public string? ToesRight { get; set; }
	public string? HairA { get; set; }
	public string? HairFrontLeft { get; set; }
	public string? HairFrontRight { get; set; }
	public string? EarLeft { get; set; }
	public string? EarRight { get; set; }
	public string? ForearmLeft { get; set; }
	public string? ForearmRight { get; set; }
	public string? ShoulderLeft { get; set; }
	public string? ShoulderRight { get; set; }
	public string? HairB { get; set; }
	public string? HandLeft { get; set; }
	public string? HandRight { get; set; }
	public string? ShieldLeft { get; set; }
	public string? ShieldRight { get; set; }
	public string? EarringALeft { get; set; }
	public string? EarringARight { get; set; }
	public string? ElbowLeft { get; set; }
	public string? ElbowRight { get; set; }
	public string? CouterLeft { get; set; }
	public string? CouterRight { get; set; }
	public string? WristLeft { get; set; }
	public string? WristRight { get; set; }
	public string? IndexALeft { get; set; }
	public string? IndexARight { get; set; }
	public string? PinkyALeft { get; set; }
	public string? PinkyARight { get; set; }
	public string? RingALeft { get; set; }
	public string? RingARight { get; set; }
	public string? MiddleALeft { get; set; }
	public string? MiddleARight { get; set; }
	public string? ThumbALeft { get; set; }
	public string? ThumbARight { get; set; }
	public string? WeaponLeft { get; set; }
	public string? WeaponRight { get; set; }
	public string? EarringBLeft { get; set; }
	public string? EarringBRight { get; set; }
	public string? IndexBLeft { get; set; }
	public string? IndexBRight { get; set; }
	public string? PinkyBLeft { get; set; }
	public string? PinkyBRight { get; set; }
	public string? RingBLeft { get; set; }
	public string? RingBRight { get; set; }
	public string? MiddleBLeft { get; set; }
	public string? MiddleBRight { get; set; }
	public string? ThumbBLeft { get; set; }
	public string? ThumbBRight { get; set; }
	public string? TailA { get; set; }
	public string? TailB { get; set; }
	public string? TailC { get; set; }
	public string? TailD { get; set; }
	public string? TailE { get; set; }
	public string? RootHead { get; set; }
	public string? Jaw { get; set; }
	public string? EyelidLowerLeft { get; set; }
	public string? EyelidLowerRight { get; set; }
	public string? EyeLeft { get; set; }
	public string? EyeRight { get; set; }
	public string? Nose { get; set; }
	public string? CheekLeft { get; set; }
	public string? CheekRight { get; set; }
	public string? LipsLeft { get; set; }
	public string? LipsRight { get; set; }
	public string? EyebrowLeft { get; set; }
	public string? EyebrowRight { get; set; }
	public string? Bridge { get; set; }
	public string? BrowLeft { get; set; }
	public string? BrowRight { get; set; }
	public string? LipUpperA { get; set; }
	public string? EyelidUpperLeft { get; set; }
	public string? EyelidUpperRight { get; set; }
	public string? LipLowerA { get; set; }
	public string? LipUpperB { get; set; }
	public string? LipLowerB { get; set; }
	public string? HrothWhiskersLeft { get; set; }
	public string? HrothWhiskersRight { get; set; }
	public string? HrothEyebrowLeft { get; set; }
	public string? HrothEyebrowRight { get; set; }
	public string? HrothBridge { get; set; }
	public string? HrothBrowLeft { get; set; }
	public string? HrothBrowRight { get; set; }
	public string? HrothJawUpper { get; set; }
	public string? HrothLipUpper { get; set; }
	public string? HrothEyelidUpperLeft { get; set; }
	public string? HrothEyelidUpperRight { get; set; }
	public string? HrothLipsLeft { get; set; }
	public string? HrothLipsRight { get; set; }
	public string? HrothLipUpperLeft { get; set; }
	public string? HrothLipUpperRight { get; set; }
	public string? HrothLipLower { get; set; }
	public string? VieraEar01ALeft { get; set; }
	public string? VieraEar01ARight { get; set; }
	public string? VieraEar02ALeft { get; set; }
	public string? VieraEar02ARight { get; set; }
	public string? VieraEar03ALeft { get; set; }
	public string? VieraEar03ARight { get; set; }
	public string? VieraEar04ALeft { get; set; }
	public string? VieraEar04ARight { get; set; }
	public string? VieraLipLowerA { get; set; }
	public string? VieraLipUpperB { get; set; }
	public string? VieraEar01BLeft { get; set; }
	public string? VieraEar01BRight { get; set; }
	public string? VieraEar02BLeft { get; set; }
	public string? VieraEar02BRight { get; set; }
	public string? VieraEar03BLeft { get; set; }
	public string? VieraEar03BRight { get; set; }
	public string? VieraEar04BLeft { get; set; }
	public string? VieraEar04BRight { get; set; }
	public string? VieraLipLowerB { get; set; }
	public string? ExRootHair { get; set; }
	public string? ExHairA { get; set; }
	public string? ExHairB { get; set; }
	public string? ExHairC { get; set; }
	public string? ExHairD { get; set; }
	public string? ExHairE { get; set; }
	public string? ExHairF { get; set; }
	public string? ExHairG { get; set; }
	public string? ExHairH { get; set; }
	public string? ExHairI { get; set; }
	public string? ExHairJ { get; set; }
	public string? ExHairK { get; set; }
	public string? ExHairL { get; set; }
	public string? ExRootMet { get; set; }
	public string? ExMetA { get; set; }
	public string? ExMetB { get; set; }
	public string? ExMetC { get; set; }
	public string? ExMetD { get; set; }
	public string? ExMetE { get; set; }
	public string? ExMetF { get; set; }
	public string? ExMetG { get; set; }
	public string? ExMetH { get; set; }
	public string? ExMetI { get; set; }
	public string? ExMetJ { get; set; }
	public string? ExMetK { get; set; }
	public string? ExMetL { get; set; }
	public string? ExMetM { get; set; }
	public string? ExMetN { get; set; }
	public string? ExMetO { get; set; }
	public string? ExMetP { get; set; }
	public string? ExMetQ { get; set; }
	public string? ExMetR { get; set; }
	public string? ExRootTop { get; set; }
	public string? ExTopA { get; set; }
	public string? ExTopB { get; set; }
	public string? ExTopC { get; set; }
	public string? ExTopD { get; set; }
	public string? ExTopE { get; set; }
	public string? ExTopF { get; set; }
	public string? ExTopG { get; set; }
	public string? ExTopH { get; set; }
	public string? ExTopI { get; set; }

	public string? RootSize { get; set; }
	public string? AbdomenSize { get; set; }
	public string? ThrowSize { get; set; }
	public string? WaistSize { get; set; }
	public string? SpineASize { get; set; }
	public string? LegLeftSize { get; set; }
	public string? LegRightSize { get; set; }
	public string? HolsterLeftSize { get; set; }
	public string? HolsterRightSize { get; set; }
	public string? SheatheLeftSize { get; set; }
	public string? SheatheRightSize { get; set; }
	public string? SpineBSize { get; set; }
	public string? ClothBackALeftSize { get; set; }
	public string? ClothBackARightSize { get; set; }
	public string? ClothFrontALeftSize { get; set; }
	public string? ClothFrontARightSize { get; set; }
	public string? ClothSideALefSizet { get; set; }
	public string? ClothSideARightSize { get; set; }
	public string? KneeLeftSize { get; set; }
	public string? KneeRightSize { get; set; }
	public string? BreastLeftSize { get; set; }
	public string? BreastRightSize { get; set; }
	public string? SpineCSize { get; set; }
	public string? ClothBackBLeftSize { get; set; }
	public string? ClothBackBRightSize { get; set; }
	public string? ClothFrontBLeftSize { get; set; }
	public string? ClothFrontBRightSize { get; set; }
	public string? ClothSideBLeftSize { get; set; }
	public string? ClothSideBRightSize { get; set; }
	public string? CalfLeftSize { get; set; }
	public string? CalfRightSize { get; set; }
	public string? ScabbardLeftSize { get; set; }
	public string? ScabbardRightSize { get; set; }
	public string? NeckSize { get; set; }
	public string? ClavicleLeftSize { get; set; }
	public string? ClavicleRightSize { get; set; }
	public string? ClothBackCLeftSize { get; set; }
	public string? ClothBackCRightSize { get; set; }
	public string? ClothFrontCLeftSize { get; set; }
	public string? ClothFrontCRightSize { get; set; }
	public string? ClothSideCLeftSize { get; set; }
	public string? ClothSideCRightSize { get; set; }
	public string? PoleynLeftSize { get; set; }
	public string? PoleynRightSize { get; set; }
	public string? FootLeftSize { get; set; }
	public string? FootRightSize { get; set; }
	public string? HeadSize { get; set; }
	public string? ArmLeftSize { get; set; }
	public string? ArmRightSize { get; set; }
	public string? PauldronLeftSize { get; set; }
	public string? PauldronRightSize { get; set; }
	public string? Unknown00Size { get; set; }
	public string? ToesLeftSize { get; set; }
	public string? ToesRightSize { get; set; }
	public string? HairASize { get; set; }
	public string? HairFrontLeftSize { get; set; }
	public string? HairFrontRightSize { get; set; }
	public string? EarLeftSize { get; set; }
	public string? EarRightSize { get; set; }
	public string? ForearmLeftSize { get; set; }
	public string? ForearmRightSize { get; set; }
	public string? ShoulderLeftSize { get; set; }
	public string? ShoulderRightSize { get; set; }
	public string? HairBSize { get; set; }
	public string? HandLeftSize { get; set; }
	public string? HandRightSize { get; set; }
	public string? ShieldLeftSize { get; set; }
	public string? ShieldRightSize { get; set; }
	public string? EarringALeftSize { get; set; }
	public string? EarringARightSize { get; set; }
	public string? ElbowLeftSize { get; set; }
	public string? ElbowRightSize { get; set; }
	public string? CouterLeftSize { get; set; }
	public string? CouterRightSize { get; set; }
	public string? WristLeftSize { get; set; }
	public string? WristRightSize { get; set; }
	public string? IndexALeftSize { get; set; }
	public string? IndexARightSize { get; set; }
	public string? PinkyALeftSize { get; set; }
	public string? PinkyARightSize { get; set; }
	public string? RingALeftSize { get; set; }
	public string? RingARightSize { get; set; }
	public string? MiddleALeftSize { get; set; }
	public string? MiddleARightSize { get; set; }
	public string? ThumbALeftSize { get; set; }
	public string? ThumbARightSize { get; set; }
	public string? WeaponLeftSize { get; set; }
	public string? WeaponRightSize { get; set; }
	public string? EarringBLeftSize { get; set; }
	public string? EarringBRightSize { get; set; }
	public string? IndexBLeftSize { get; set; }
	public string? IndexBRightSize { get; set; }
	public string? PinkyBLeftSize { get; set; }
	public string? PinkyBRightSize { get; set; }
	public string? RingBLeftSize { get; set; }
	public string? RingBRightSize { get; set; }
	public string? MiddleBLeftSize { get; set; }
	public string? MiddleBRightSize { get; set; }
	public string? ThumbBLeftSize { get; set; }
	public string? ThumbBRightSize { get; set; }
	public string? TailASize { get; set; }
	public string? TailBSize { get; set; }
	public string? TailCSize { get; set; }
	public string? TailDSize { get; set; }
	public string? TailESize { get; set; }
	public string? RootHeadSize { get; set; }
	public string? JawSize { get; set; }
	public string? EyelidLowerLeftSize { get; set; }
	public string? EyelidLowerRightSize { get; set; }
	public string? EyeLeftSize { get; set; }
	public string? EyeRightSize { get; set; }
	public string? NoseSize { get; set; }
	public string? CheekLeftSize { get; set; }
	public string? CheekRightSize { get; set; }
	public string? LipsLeftSize { get; set; }
	public string? LipsRightSize { get; set; }
	public string? EyebrowLeftSize { get; set; }
	public string? EyebrowRightSize { get; set; }
	public string? BridgeSize { get; set; }
	public string? BrowLeftSize { get; set; }
	public string? BrowRightSize { get; set; }
	public string? LipUpperASize { get; set; }
	public string? EyelidUpperLeftSize { get; set; }
	public string? EyelidUpperRightSize { get; set; }
	public string? LipLowerASize { get; set; }
	public string? LipUpperBSize { get; set; }
	public string? LipLowerBSize { get; set; }
	public string? HrothWhiskersLeftSize { get; set; }
	public string? HrothWhiskersRightSize { get; set; }
	public string? HrothEyebrowLeftSize { get; set; }
	public string? HrothEyebrowRightSize { get; set; }
	public string? HrothBridgeSize { get; set; }
	public string? HrothBrowLeftSize { get; set; }
	public string? HrothBrowRightSize { get; set; }
	public string? HrothJawUpperSize { get; set; }
	public string? HrothLipUpperSize { get; set; }
	public string? HrothEyelidUpperLeftSize { get; set; }
	public string? HrothEyelidUpperRightSize { get; set; }
	public string? HrothLipsLeftSize { get; set; }
	public string? HrothLipsRightSize { get; set; }
	public string? HrothLipUpperLeftSize { get; set; }
	public string? HrothLipUpperRightSize { get; set; }
	public string? HrothLipLowerSize { get; set; }
	public string? VieraEar01ALeftSize { get; set; }
	public string? VieraEar01ARightSize { get; set; }
	public string? VieraEar02ALeftSize { get; set; }
	public string? VieraEar02ARightSize { get; set; }
	public string? VieraEar03ALeftSize { get; set; }
	public string? VieraEar03ARightSize { get; set; }
	public string? VieraEar04ALeftSize { get; set; }
	public string? VieraEar04ARightSize { get; set; }
	public string? VieraLipLowerASize { get; set; }
	public string? VieraLipUpperBSize { get; set; }
	public string? VieraEar01BLeftSize { get; set; }
	public string? VieraEar01BRightSize { get; set; }
	public string? VieraEar02BLeftSize { get; set; }
	public string? VieraEar02BRightSize { get; set; }
	public string? VieraEar03BLeftSize { get; set; }
	public string? VieraEar03BRightSize { get; set; }
	public string? VieraEar04BLeftSize { get; set; }
	public string? VieraEar04BRightSize { get; set; }
	public string? VieraLipLowerBSize { get; set; }
	public string? ExRootHairSize { get; set; }
	public string? ExHairASize { get; set; }
	public string? ExHairBSize { get; set; }
	public string? ExHairCSize { get; set; }
	public string? ExHairDSize { get; set; }
	public string? ExHairESize { get; set; }
	public string? ExHairFSize { get; set; }
	public string? ExHairGSize { get; set; }
	public string? ExHairHSize { get; set; }
	public string? ExHairISize { get; set; }
	public string? ExHairJSize { get; set; }
	public string? ExHairKSize { get; set; }
	public string? ExHairLSize { get; set; }
	public string? ExRootMetSize { get; set; }
	public string? ExMetASize { get; set; }
	public string? ExMetBSize { get; set; }
	public string? ExMetCSize { get; set; }
	public string? ExMetDSize { get; set; }
	public string? ExMetESize { get; set; }
	public string? ExMetFSize { get; set; }
	public string? ExMetGSize { get; set; }
	public string? ExMetHSize { get; set; }
	public string? ExMetISize { get; set; }
	public string? ExMetJSize { get; set; }
	public string? ExMetKSize { get; set; }
	public string? ExMetLSize { get; set; }
	public string? ExMetMSize { get; set; }
	public string? ExMetNSize { get; set; }
	public string? ExMetOSize { get; set; }
	public string? ExMetPSize { get; set; }
	public string? ExMetQSize { get; set; }
	public string? ExMetRSize { get; set; }
	public string? ExRootTopSize { get; set; }
	public string? ExTopASize { get; set; }
	public string? ExTopBSize { get; set; }
	public string? ExTopCSize { get; set; }
	public string? ExTopDSize { get; set; }
	public string? ExTopESize { get; set; }
	public string? ExTopFSize { get; set; }
	public string? ExTopGSize { get; set; }
	public string? ExTopHSize { get; set; }
	public string? ExTopISize { get; set; }

	public PoseFile Upgrade(ActorCustomizeMemory.Races race)
	{
		PoseFile file = new PoseFile();
		Type legacyType = this.GetType();

		if (this.Race == null)
			throw new Exception("Legacy pose file has no race specified");

		ActorCustomizeMemory.Races fileRace = (ActorCustomizeMemory.Races)byte.Parse(this.Race);
		file.Bones = new Dictionary<string, PoseFile.Bone?>();

		PropertyInfo[] props = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);
		foreach (PropertyInfo propertyInfo in props)
		{
			string boneName = propertyInfo.Name;

			// ignore properties that are not in the json.
			JsonIgnoreAttribute? ignore = propertyInfo.GetCustomAttribute<JsonIgnoreAttribute>();
			if (ignore != null)
				continue;

			if (boneName == "Race")
				continue;

			if (boneName.EndsWith("Size"))
				continue;

			if (boneName == "Type")
				continue;

			PropertyInfo? rotProp = legacyType.GetProperty(boneName);
			PropertyInfo? scaleProp = legacyType.GetProperty(boneName + "Size");

			if (boneName.StartsWith(@"Hroth") && fileRace != ActorCustomizeMemory.Races.Hrothgar)
				continue;

			if (boneName.StartsWith("Viera") && fileRace != ActorCustomizeMemory.Races.Viera)
				continue;

			boneName = boneName.Replace(@"Hroth", string.Empty);
			boneName = boneName.Replace(@"Viera", string.Empty);

			string? rotString = null;
			string? scaleString = null;

			if (rotProp != null)
				rotString = (string?)rotProp.GetValue(this);

			if (scaleProp != null)
				scaleString = (string?)scaleProp.GetValue(this);

			if (rotString == null && scaleString == null)
				continue;

			PoseFile.Bone bone = StringToBone(rotString, scaleString);

			if (file.Bones.ContainsKey(boneName))
				file.Bones.Remove(boneName);

			file.Bones.Add(boneName, bone);
		}

		return file;
	}

	private static PoseFile.Bone StringToBone(string? rot = null, string? scale = null, string? position = null)
	{
		PoseFile.Bone bone = new PoseFile.Bone();

		if (!string.IsNullOrEmpty(rot) && rot != "null")
		{
			byte[] data = StringToByteArray(rot);

			Quaternion value = default;
			value.X = BitConverter.ToSingle(data, 0);
			value.Y = BitConverter.ToSingle(data, 4);
			value.Z = BitConverter.ToSingle(data, 8);
			value.W = BitConverter.ToSingle(data, 12);
			bone.Rotation = value;
		}

		if (!string.IsNullOrEmpty(scale) && scale != "null")
		{
			byte[] data = StringToByteArray(scale);

			Vector value = default;
			value.X = BitConverter.ToSingle(data, 0);
			value.Y = BitConverter.ToSingle(data, 4);
			value.Z = BitConverter.ToSingle(data, 8);
			bone.Scale = value;
		}

		return bone;
	}

	private static byte[] StringToByteArray(string hex)
	{
		try
		{
			hex = hex.Trim();
			string[] parts = hex.Split(' ');
			byte[] data = new byte[parts.Length];

			for (int i = 0; i < parts.Length; i++)
			{
				data[i] = byte.Parse(parts[i], NumberStyles.HexNumber);
			}

			return data;
		}
		catch (Exception ex)
		{
			throw new Exception($"Failed to parse string: {hex} to byte array", ex);
		}
	}
}
