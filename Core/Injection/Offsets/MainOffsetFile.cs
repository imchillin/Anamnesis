// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Injection.Offsets
{
	public class MainOffsetFile
	{
		public ActorTableOffset ActorTable { get; set; }
		public ActorTableOffset GposeActorTable { get; set; }
		public BaseOffset Gpose { get; set; }
		public BaseOffset<bool> GposeCheck { get; set; }
		public BaseOffset<ushort> GposeCheck2 { get; set; }
		public BaseOffset Target { get; set; }
		public BaseOffset CameraOffset { get; set; }
		public BaseOffset Time { get; set; }
		public BaseOffset WeatherOffset { get; set; }
		public BaseOffset TerritoryOFfset { get; set; }
		public BaseOffset GposeFilters { get; set; }
		public BaseOffset MusicOffset { get; set; }

		public FlagOffset Skeleton1Flag { get; set; }
		public FlagOffset Skeleton2Flag { get; set; }
		public FlagOffset Skeleton3Flag { get; set; }
		public FlagOffset Skeleton4flag { get; set; }
		public FlagOffset Skeleton5Flag { get; set; }
		public FlagOffset Skeleton6Flag { get; set; }
		public FlagOffset Physics1Flag { get; set; }
		public FlagOffset Physics2Flag { get; set; }
		public FlagOffset Physics3Flag { get; set; }

		public Offset RenderOffset1 { get; set; }
		public Offset RenderOffset2 { get; set; }

		public Offset<string> Name { get; set; }
		public Offset<string> ActorID { get; set; }
		public Offset<ActorTypes> ActorType { get; set; }
		public Offset<byte> ActorRender { get; set; }

		public Offset<Vector> Camera { get; set; }

		public Offset<Appearance> ActorAppearance { get; set; }
		public Offset FCTag { get; set; }
		public Offset Title { get; set; }
		public Offset ModelChara { get; set; }
		public Offset ActorVoice { get; set; }
		public Offset StatusEffect { get; set; }

		public Offset Transparency { get; set; }
		public Offset<Weapon> MainHand { get; set; }
		public Offset<Weapon> OffHand { get; set; }
		public Offset<Equipment> ActorEquipment { get; set; }

		public Offset ForceAnimation { get; set; }
		public Offset IdleAnimation { get; set; }
		public Offset AnimationSpeed1 { get; set; }
		public Offset AnimationSpeed2 { get; set; }
		public Offset FreezeFacial { get; set; }

		public Offset DataPath { get; set; }
		public Offset DataHead { get; set; }

		public Offset<Vector> Position { get; set; }
		public Offset<Quaternion> Rotation { get; set; }
		public Offset Height { get; set; }
		public Offset Wetness { get; set; }
		public Offset Drenched { get; set; }

		public Offset<Vector> BustScale { get; set; }
		public Offset UniqueFeatureScale { get; set; }
		public Offset MuscleTone { get; set; }
		public Offset<Vector> Scale { get; set; }

		public Offset<Vector> MainHandScale { get; set; }
		public Offset<Color> MainHandColor { get; set; }
		public Offset<Vector> OffhandScale { get; set; }
		public Offset<Color> OffhandColor { get; set; }

		// Actor's RGB Values
		public Offset<Color> SkinColor { get; set; }
		public Offset<Color> SkinGloss { get; set; }
		public Offset<Color> HairColor { get; set; }
		public Offset<Color> HairGloss { get; set; }
		public Offset<Color> HairHiglight { get; set; }
		public Offset<Color> LeftEyeColor { get; set; }
		public Offset<Color> RightEyeColor { get; set; }
		public Offset<Color> MouthColor { get; set; }
		public Offset<float> MouthGloss { get; set; }
		public Offset<Color> LimbalColor { get; set; }

		public Offset<Vector> CameraView { get; set; }
		public Offset CameraCurrentZoom { get; set; }
		public Offset CameraMinZoom { get; set; }
		public Offset CameraMaxZoom { get; set; }
		public Offset FOVCurrent { get; set; }
		public Offset FOVCurrent2 { get; set; }
		public Offset FOV2 { get; set; }
		public Offset<float> CameraAngleX { get; set; }
		public Offset<float> CameraAngleY { get; set; }
		public Offset CameraYMin { get; set; }
		public Offset CameraYMax { get; set; }
		public Offset<float> CameraRotation { get; set; }
		public Offset CameraUpDown { get; set; }
		public Offset CameraPanX { get; set; }
		public Offset CameraPanY { get; set; }

		public Offset TimeControl { get; set; }
		public Offset Territory { get; set; }
		public Offset Weather { get; set; }
		public Offset ForceWeather { get; set; }
		public Offset Music { get; set; }
		public Offset Music2 { get; set; }

		public Offset GposeFilterEnable { get; set; }
		public Offset GposeFilterTable { get; set; }
	}
}
