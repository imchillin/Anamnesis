// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.Offsets
{
	using System;
	using ConceptMatrix.Injection;
	using ConceptMatrix.Injection.Offsets;

	[Serializable]
	public static class Offsets
	{
		public static ActorTableOffset ActorTable = 0x1C65570;
		public static ActorTableOffset GposeActorTable = 0x1C67000;
		public static BaseOffset Gpose = 0x1C64168;
		public static BaseOffset<bool> GposeCheck = 0x1CB4C9A;
		public static BaseOffset<ushort> GposeCheck2 = 0x1C67D50;
		public static BaseOffset Target = 0x1C641D0;
		public static BaseOffset CameraOffset = 0x1C63F80;
		public static BaseOffset Time = 0x1C44AF8;
		public static BaseOffset WeatherOffset = 0x1C1B858;
		public static BaseOffset TerritoryOFfset = 0x1C42430;
		public static BaseOffset GposeFilters = 0x1C42BB8;
		public static BaseOffset MusicOffset = 0x1C81CA8;
		public static FlagOffset SkeletonOffset1 = new FlagOffset(0x1382290, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 }, new byte[] { 0x41, 0x0F, 0x29, 0x5C, 0x12, 0x10 }); // Base
		public static FlagOffset SkeletonOffset2 = new FlagOffset(0x13833BD, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 }, new byte[] { 0x43, 0x0F, 0x29, 0x5C, 0x18, 0x10 });
		public static FlagOffset SkeletonOffset3 = new FlagOffset(0x1391024, new byte[] { 0x90, 0x90, 0x90, 0x90 }, new byte[] { 0x0F, 0x29, 0x5E, 0x10 });
		public static FlagOffset SkeletonOffset4 = new FlagOffset(0x13822A0, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 }, new byte[] { 0x41, 0x0F, 0x29, 0x44, 0x12, 0x20 }); // Scale
		public static FlagOffset SkeletonOffset5 = new FlagOffset(0x138221B, new byte[] { }, new byte[] { }); // Position
		public static FlagOffset SkeletonOffset6 = new FlagOffset(0x13833CD, new byte[] { }, new byte[] { 0x43, 0x0F, 0x29, 0x44, 0x18, 0x20 }); // Scale 2

		public static FlagOffset PhysicsOffset1 = new FlagOffset(0x37AA48, new byte[] { 0x90, 0x90, 0x90, 0x90 }, new byte[] { 0x0F, 0x29, 0x48, 0x10 });
		public static FlagOffset PhysicsOffset2 = new FlagOffset(0x37AA3F, new byte[] { 0x90, 0x90, 0x90 }, new byte[] { 0x0F, 0x29, 0x00 });
		public static FlagOffset PhysicsOffset3 = new FlagOffset(0x37AA52, new byte[] { 0x90, 0x90, 0x90, 0x90 }, new byte[] { 0x0F, 0x29, 0x40, 0x20 });

		public static Offset RenderOffset1 = 0x431330;
		public static Offset RenderOffset2 = 0x431326;

		public static Offset<string> Name = 0x30;
		public static Offset<string> ActorID = 0x34;
		public static Offset<ActorTypes> ActorType = 0x8C;
		public static Offset<byte> ActorRender = 0x104;

		public static Offset<Vector> Camera = 0xA0;

		public static Offset<Appearance> ActorAppearance = 0x17B8; // Starting postion is Race Address Offset
		public static Offset FCTag = 0x17D2;
		public static Offset Title = 0x18B2;
		public static Offset ModelChara = 0x1888;
		public static Offset ActorVoice = 0x18B8;
		public static Offset StatusEffect = 0x1C1C;

		public static Offset Transparency = 0x1704;
		public static Offset<Weapon> MainHand = 0x1450;
		public static Offset<Weapon> OffHand = 0x14B8;
		public static Offset<Equipment> ActorEquipment = 0x1708; // Starting position is Head Piece Address Offset'

		public static Offset ForceAnimation = 0xC60;
		public static Offset IdleAnimation = 0x18BE;
		public static Offset AnimationSpeed1 = 0xCD4;
		public static Offset AnimationSpeed2 = 0xCD8;
		public static Offset FreezeFacial = 0xCDC;

		public static Offset DataPath = new[] { 0xF0, 0X938 };
		public static Offset DataHead = new[] { 0xF0, 0X93C };

		public static Offset<Vector> Position = new[] { 0xF0, 0X50 };
		public static Offset Rotation = new[] { 0xF0, 0X60 };
		public static Offset Height = new[] { 0xF0, 0X26C };
		public static Offset Wetness = new[] { 0xF0, 0X2B0 };
		public static Offset Drenched = new[] { 0xF0, 0X2BC };

		public static Offset<Vector> BustScale = new[] { 0xF0, 0X148, 0x68 };
		public static Offset UniqueFeatureScale = new[] { 0xF0, 0X148, 0x74 }; // Tail & Ears.
		public static Offset MuscleTone = new[] { 0xF0, 0X240, 0x28, 0x20, 0x0C };
		public static Offset<Vector> Scale = new[] { 0xF0, 0x70 };

		public static Offset<Vector> MainHandScale = new[] { 0xF0, 0x30, 0x70 };
		public static Offset<Color> MainHandColor = new[] { 0xF0, 0x30, 0x258 };
		public static Offset<Vector> OffhandScale = new[] { 0xF0, 0x30, 0x28, 0x70 };
		public static Offset<Color> OffhandColor = new[] { 0xF0, 0x30, 0x28, 0x258 };

		// Actor's RGB Values
		public static Offset<Color> SkinColor = new[] { 0xF0, 0x240, 0x28, 0x20, 0x00 };
		public static Offset<Color> SkinGloss = new[] { 0xF0, 0x240, 0x28, 0x20, 0x10 };
		public static Offset<Color> HairColor = new[] { 0xF0, 0x240, 0x28, 0x20, 0x30 };
		public static Offset<Color> HairGloss = new[] { 0xF0, 0x240, 0x28, 0x20, 0x40 };
		public static Offset<Color> HairHiglight = new[] { 0xF0, 0x240, 0x28, 0x20, 0x50 };
		public static Offset<Color> LeftEyeColor = new[] { 0xF0, 0x240, 0x28, 0x20, 0x60 };
		public static Offset<Color> RightEyeColor = new[] { 0xF0, 0x240, 0x28, 0x20, 0x70 };
		public static Offset<Color> MouthColor = new[] { 0xF0, 0x240, 0x28, 0x20, 0x20 };
		public static Offset<float> MouthGloss = new[] { 0xF0, 0x240, 0x28, 0x20, 0x2C };
		public static Offset<Color> LimbalColor = new[] { 0xF0, 0x240, 0x28, 0x20, 0x80 };

		public static Offset<Vector> CameraView = 0x180;
		public static Offset CameraCurrentZoom = 0x114;
		public static Offset CameraMinZoom = 0x118;
		public static Offset CameraMaxZoom = 0x11c;
		public static Offset FOVCurrent = 0x120;
		public static Offset FOVCurrent2 = 0x124;
		public static Offset FOV2 = 0x12c;
		public static Offset<float> CameraAngleX = 0x130;
		public static Offset<float> CameraAngleY = 0x134;
		public static Offset CameraYMin = 0x14C;
		public static Offset CameraYMax = 0x148;
		public static Offset<float> CameraRotation = 0x164;
		public static Offset CameraUpDown = 0x218;
		public static Offset CameraPanX = 0x150;
		public static Offset CameraPanY = 0x154;

		public static Offset TimeControl = new[] { 0x10, 0X08, 0x28, 0x80 };
		public static Offset Territory = new[] { 0x00, 0X134C };
		public static Offset Weather = 0x20;
		public static Offset ForceWeather = 0x26; // From GposeFilters
		public static Offset Music = new[] { 0xC0, 0X114 };
		public static Offset Music2 = new[] { 0xC0, 0X116 };

		public static Offset GposeFilterEnable = 0x37B;
		public static Offset GposeFilterTable = 0x318;
	}
}
