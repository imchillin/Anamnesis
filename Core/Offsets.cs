// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix
{
	using System;
	using ConceptMatrix.Injection.Offsets;

	[Serializable]
	public static class Offsets
	{
		public static MainOffsetFile Main;

		static Offsets()
		{
			Main = new MainOffsetFile();

			Main.ActorTable = 0x1C65570;
			Main.GposeActorTable = 0x1C67000;
			Main.Gpose = 0x1C64168;
			Main.GposeCheck = 0x1CB4C9A;
			Main.GposeCheck2 = 0x1C67D50;
			Main.Target = 0x1C641D0;
			Main.CameraOffset = 0x1C63F80;
			Main.Time = 0x1C44AF8;
			Main.WeatherOffset = 0x1C1B858;
			Main.TerritoryOFfset = 0x1C42430;
			Main.GposeFilters = 0x1C42BB8;
			Main.MusicOffset = 0x1C81CA8;

			Main.Skeleton1Flag = new FlagOffset(0x1382290, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 }, new byte[] { 0x41, 0x0F, 0x29, 0x5C, 0x12, 0x10 }); // Base
			Main.Skeleton2Flag = new FlagOffset(0x13833BD, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 }, new byte[] { 0x43, 0x0F, 0x29, 0x5C, 0x18, 0x10 });
			Main.Skeleton3Flag = new FlagOffset(0x1391024, new byte[] { 0x90, 0x90, 0x90, 0x90 }, new byte[] { 0x0F, 0x29, 0x5E, 0x10 });
			Main.Skeleton4flag = new FlagOffset(0x13822A0, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 }, new byte[] { 0x41, 0x0F, 0x29, 0x44, 0x12, 0x20 }); // Scale
			Main.Skeleton5Flag = new FlagOffset(0x138221B, new byte[] { }, new byte[] { }); // Position
			Main.Skeleton6Flag = new FlagOffset(0x13833CD, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 }, new byte[] { 0x43, 0x0F, 0x29, 0x44, 0x18, 0x20 }); // Scale 2
			Main.Physics1Flag = new FlagOffset(0x37AA48, new byte[] { 0x90, 0x90, 0x90, 0x90 }, new byte[] { 0x0F, 0x29, 0x48, 0x10 });
			Main.Physics2Flag = new FlagOffset(0x37AA3F, new byte[] { 0x90, 0x90, 0x90 }, new byte[] { 0x0F, 0x29, 0x00 });
			Main.Physics3Flag = new FlagOffset(0x37AA52, new byte[] { 0x90, 0x90, 0x90, 0x90 }, new byte[] { 0x0F, 0x29, 0x40, 0x20 });

			Main.RenderOffset1 = 0x431330;
			Main.RenderOffset2 = 0x431326;

			Main.Name = 0x30;
			Main.ActorID = 0x34;
			Main.ActorType = 0x8C;
			Main.ActorRender = 0x104;

			Main.Camera = 0xA0;

			Main.ActorAppearance = 0x17B8; // Starting postion is Race Address Offset
			Main.FCTag = 0x17D2;
			Main.Title = 0x18B2;
			Main.ModelChara = 0x1888;
			Main.ActorVoice = 0x18B8;
			Main.StatusEffect = 0x1C1C;

			Main.Transparency = 0x1704;
			Main.MainHand = 0x1450;
			Main.OffHand = 0x14B8;
			Main.ActorEquipment = 0x1708; // Starting position is Head Piece Address Offset'

			Main.ForceAnimation = 0xC60;
			Main.IdleAnimation = 0x18BE;
			Main.AnimationSpeed1 = 0xCD4;
			Main.AnimationSpeed2 = 0xCD8;
			Main.FreezeFacial = 0xCDC;

			Main.DataPath = new[] { 0xF0, 0X938 };
			Main.DataHead = new[] { 0xF0, 0X93C };

			Main.Position = new[] { 0xF0, 0X50 };
			Main.Rotation = new[] { 0xF0, 0X60 };
			Main.Height = new[] { 0xF0, 0X26C };
			Main.Wetness = new[] { 0xF0, 0X2B0 };
			Main.Drenched = new[] { 0xF0, 0X2BC };

			Main.BustScale = new[] { 0xF0, 0X148, 0x68 };
			Main.UniqueFeatureScale = new[] { 0xF0, 0X148, 0x74 }; // Tail & Ears.
			Main.MuscleTone = new[] { 0xF0, 0X240, 0x28, 0x20, 0x0C };
			Main.Scale = new[] { 0xF0, 0x70 };

			Main.MainHandScale = new[] { 0xF0, 0x30, 0x70 };
			Main.MainHandColor = new[] { 0xF0, 0x30, 0x258 };
			Main.OffhandScale = new[] { 0xF0, 0x30, 0x28, 0x70 };
			Main.OffhandColor = new[] { 0xF0, 0x30, 0x28, 0x258 };

			Main.SkinColor = new[] { 0xF0, 0x240, 0x28, 0x20, 0x00 };
			Main.SkinGloss = new[] { 0xF0, 0x240, 0x28, 0x20, 0x10 };
			Main.HairColor = new[] { 0xF0, 0x240, 0x28, 0x20, 0x30 };
			Main.HairGloss = new[] { 0xF0, 0x240, 0x28, 0x20, 0x40 };
			Main.HairHiglight = new[] { 0xF0, 0x240, 0x28, 0x20, 0x50 };
			Main.LeftEyeColor = new[] { 0xF0, 0x240, 0x28, 0x20, 0x60 };
			Main.RightEyeColor = new[] { 0xF0, 0x240, 0x28, 0x20, 0x70 };
			Main.MouthColor = new[] { 0xF0, 0x240, 0x28, 0x20, 0x20 };
			Main.MouthGloss = new[] { 0xF0, 0x240, 0x28, 0x20, 0x2C };
			Main.LimbalColor = new[] { 0xF0, 0x240, 0x28, 0x20, 0x80 };

			Main.CameraView = 0x180;
			Main.CameraCurrentZoom = 0x114;
			Main.CameraMinZoom = 0x118;
			Main.CameraMaxZoom = 0x11c;
			Main.FOVCurrent = 0x120;
			Main.FOVCurrent2 = 0x124;
			Main.FOV2 = 0x12c;
			Main.CameraAngleX = 0x130;
			Main.CameraAngleY = 0x134;
			Main.CameraYMin = 0x14C;
			Main.CameraYMax = 0x148;
			Main.CameraRotation = 0x164;
			Main.CameraUpDown = 0x218;
			Main.CameraPanX = 0x150;
			Main.CameraPanY = 0x154;

			Main.TimeControl = new[] { 0x10, 0X08, 0x28, 0x80 };
			Main.Territory = new[] { 0x00, 0X134C };
			Main.Weather = 0x20;
			Main.ForceWeather = 0x26; // From GposeFilters
			Main.Music = new[] { 0xC0, 0X114 };
			Main.Music2 = new[] { 0xC0, 0X116 };

			Main.GposeFilterEnable = 0x37B;
			Main.GposeFilterTable = 0x318;

			Main.ExHairCount = new[] { 0xF0, 0xA0, 0x68, 0x4C0, 0x10 };
			Main.ExMetCount = new[] { 0xF0, 0xA0, 0x68, 0x680, 0x10 };
			Main.ExTopCount = new[] { 0xF0, 0xA0, 0x68, 0x840, 0x10 };
		}
	}
}
