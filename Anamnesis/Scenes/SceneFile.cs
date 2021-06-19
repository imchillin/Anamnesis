// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Scenes
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows;
	using Anamnesis.Files;
	using Anamnesis.Files.Infos;
	using Anamnesis.GUI.Dialogs;
	using Anamnesis.Memory;
	using Anamnesis.PoseModule;
	using Anamnesis.Services;
	using Serilog;

	using Quaternion = Anamnesis.Memory.Quaternion;
	using Vector = Anamnesis.Memory.Vector;

	#pragma warning disable SA1402, SA1649
	public class SceneFileInfo : JsonFileInfoBase<SceneFile>
	{
		public override string Extension => "scene";
		public override string Name => "Anamnesis Scene File";
		public override IFileSource[] FileSources => new[] { new LocalFileSource("Local Files", SettingsService.Current.DefaultSceneDirectory) };
	}

	public class SceneFile : FileBase
	{
		public uint TerritoryId { get; set; }
		public string? TerritoryName { get; set; }

		public Vector RootPosition { get; set; }

		public long? TimeOfDay { get; set; }
		public byte? DayOfMonth { get; set; }
		public ushort? Weather { get; set; }

		public bool? UnlimitCamera { get; set; }
		public Vector2D? CameraAngle { get; set; }
		public float? CameraYMin { get; set; }
		public float? CameraYMax { get; set; }
		public Vector2D? CameraPan { get; set; }
		public float? CameraRotation { get; set; }
		public float? CameraZoom { get; set; }
		public float? CameraMinZoom { get; set; }
		public float? CameraMaxZoom { get; set; }
		public float? CameraFieldOfView { get; set; }
		public Vector? CameraPosition { get; set; }

		public List<SceneActor>? Actors { get; set; }

		private static ILogger Log => Serilog.Log.ForContext<SceneFile>();

		public async Task WriteToFile(Configuration config)
		{
			this.TerritoryId = TerritoryService.Instance.CurrentTerritoryId;
			this.TerritoryName = TerritoryService.Instance.CurrentTerritoryName;

			if (config.IncludeTime)
			{
				this.TimeOfDay = TimeService.Instance.TimeOfDay;
				this.DayOfMonth = TimeService.Instance.DayOfMonth;
			}

			if (config.IncludeWeather)
			{
				this.Weather = TerritoryService.Instance.CurrentWeatherId;
			}

			Dictionary<TargetService.ActorTableActor, SceneActor>? actors = null;
			if (config.IncludeActors)
			{
				actors = await ActorNamer.GetActors(TargetService.Instance.PinnedActors);

				foreach ((TargetService.ActorTableActor tableActor, SceneActor sceneActor) in actors)
				{
					ActorViewModel? actor = tableActor.GetViewModel();

					if (actor == null)
						continue;

					if (this.RootPosition == Vector.Zero && actor.ModelObject?.Transform != null)
					{
						this.RootPosition = actor.ModelObject.Transform.Position;
					}
				}
			}

			if (config.IncludeCamera && CameraService.Instance.Camera != null)
			{
				this.UnlimitCamera = CameraService.Instance.DelimitCamera;
				this.CameraAngle = CameraService.Instance.Camera.Angle;
				this.CameraYMin = CameraService.Instance.Camera.YMin;
				this.CameraYMax = CameraService.Instance.Camera.YMax;
				this.CameraPan = CameraService.Instance.Camera.Pan;
				this.CameraRotation = CameraService.Instance.Camera.Rotation;
				this.CameraZoom = CameraService.Instance.Camera.Zoom;
				this.CameraMinZoom = CameraService.Instance.Camera.MinZoom;
				this.CameraMaxZoom = CameraService.Instance.Camera.MaxZoom;
				this.CameraFieldOfView = CameraService.Instance.Camera.FieldOfView;
				this.CameraPosition = CameraService.Instance.CameraPosition;
			}

			if (config.IncludeActors && actors != null)
			{
				this.Actors = new List<SceneActor>();

				foreach ((TargetService.ActorTableActor tableActor, SceneActor sceneActor) in actors)
				{
					ActorViewModel? vm = tableActor.GetViewModel();

					if (vm == null)
						continue;

					throw new NotImplementedException();

					/*SkeletonVisual3d skeleton;

					sceneActor.FromActor(vm, this.RootPosition, config);
					this.Actors.Add(sceneActor);*/
				}
			}
		}

		public async Task Apply(Configuration config)
		{
			Dictionary<SceneActor, TargetService.ActorTableActor>? lookup = null;

			if (config.IncludeActors && this.Actors != null)
				lookup = await ActorSelector.GetActors(this.Actors);

			if (TerritoryService.Instance.CurrentTerritoryId != this.TerritoryId)
			{
				this.RootPosition = Vector.Zero;

				if (lookup != null && lookup.Count > 0)
				{
					// Use the position of the first valid actor as the root position
					foreach ((SceneActor scene, TargetService.ActorTableActor actor) in lookup)
					{
						ActorViewModel? actorVm = actor.GetViewModel();

						if (actorVm == null)
							continue;

						if (this.RootPosition == Vector.Zero && actorVm.ModelObject?.Transform != null)
						{
							this.RootPosition = actorVm.ModelObject.Transform.Position;
						}
					}
				}
				else
				{
					// No actors to fall back on means no positions
					config.IncludeActorPosition = false;
					config.IncludeCamera = false;
				}

				// TODO: Check to see if the desired weather is available for this map
				config.IncludeWeather = false;
			}

			if (config.IncludeTime && this.TimeOfDay != null && this.DayOfMonth != null)
			{
				TimeService.Instance.Freeze = true;
				TimeService.Instance.TimeOfDay = (int)this.TimeOfDay;
				TimeService.Instance.DayOfMonth = (byte)this.DayOfMonth;
			}

			if (config.IncludeWeather && this.Weather != null)
			{
				TerritoryService.Instance.CurrentWeatherId = (ushort)this.Weather;
			}

			if (lookup != null && lookup.Count != 0)
			{
				if (await GposeService.LeaveGpose("You must not be in Gpose to apply actor appearances"))
				{
					foreach ((SceneActor scene, TargetService.ActorTableActor actor) in lookup)
					{
						ActorViewModel? actorVm = actor.GetViewModel();

						if (actorVm == null)
							continue;

						await scene.ApplyOverworld(actorVm, config);
					}
				}

				if (await GposeService.EnterGpose("You must be in Gpose to apply actor poses"))
				{
					foreach ((SceneActor scene, TargetService.ActorTableActor actor) in lookup)
					{
						ActorViewModel? actorVm = actor.GetViewModel();

						if (actorVm == null)
							continue;

						await scene.ApplyGpose(actorVm, this.RootPosition, config);
					}
				}
			}

			if (await GposeService.EnterGpose("You must be in Gpose to apply camera settings"))
			{
				if (config.IncludeCamera && CameraService.Instance.Camera != null)
				{
					if (this.UnlimitCamera != null)
						CameraService.Instance.DelimitCamera = (bool)this.UnlimitCamera;

					if (this.CameraAngle != null)
						CameraService.Instance.Camera.Angle = (Vector2D)this.CameraAngle;

					if (this.CameraYMin != null)
						CameraService.Instance.Camera.YMin = (float)this.CameraYMin;

					if (this.CameraYMax != null)
						CameraService.Instance.Camera.YMax = (float)this.CameraYMax;

					if (this.CameraPan != null)
						CameraService.Instance.Camera.Pan = (Vector2D)this.CameraPan;

					if (this.CameraRotation != null)
						CameraService.Instance.Camera.Rotation = (float)this.CameraRotation;

					if (this.CameraZoom != null)
						CameraService.Instance.Camera.Zoom = (float)this.CameraZoom;

					if (this.CameraMinZoom != null)
						CameraService.Instance.Camera.MinZoom = (float)this.CameraMinZoom;

					if (this.CameraMaxZoom != null)
						CameraService.Instance.Camera.MaxZoom = (float)this.CameraMaxZoom;

					if (this.CameraFieldOfView != null)
						CameraService.Instance.Camera.FieldOfView = (float)this.CameraFieldOfView;

					if (this.CameraPosition != null)
					{
						CameraService.Instance.CameraPosition = (Vector)this.CameraPosition;
					}
				}
			}
		}

		[Serializable]
		public class SceneActor
		{
			public string? Identifier { get; set; }
			public Vector? Position { get; set; }
			public Quaternion? Rotation { get; set; }
			public Vector? Scale { get; set; }

			public PoseFile? Pose { get; set; }
			public CharacterFile? Character { get; set; }

			public void FromActor(ActorViewModel actor, SkeletonVisual3d skeleton, Vector rootPos, Configuration config)
			{
				if (actor.ModelObject?.Transform != null)
				{
					if (config.IncludeActorPosition)
						this.Position = rootPos - actor.ModelObject.Transform.Position;

					if (config.IncludeActorRotation)
						this.Rotation = actor.ModelObject.Transform.Rotation;

					if (config.IncludeActorScale)
						this.Scale = actor.ModelObject.Transform.Scale;
				}

				this.Pose = new PoseFile();
				this.Pose.WriteToFile(actor, skeleton, config.Pose, false);

				this.Character = new CharacterFile();
				this.Character.WriteToFile(actor, config.Character);
			}

			public async Task ApplyOverworld(ActorViewModel actor, Configuration config)
			{
				if (this.Character != null)
				{
					await this.Character.Apply(actor, config.Character);
				}
			}

			public Task ApplyGpose(ActorViewModel actor, Vector rootPos, Configuration config)
			{
				if (actor.ModelObject?.Transform != null)
				{
					if (this.Position != null)
						actor.ModelObject.Transform.Position = rootPos + (Vector)this.Position;

					if (this.Rotation != null)
						actor.ModelObject.Transform.Rotation = (Quaternion)this.Rotation;

					if (this.Scale != null)
					{
						actor.ModelObject.Transform.Scale = (Vector)this.Scale;
					}
				}

				if (this.Pose != null)
				{
					throw new NotImplementedException();
					////await this.Pose.Apply(actor, config.Pose);
				}

				return Task.CompletedTask;
			}
		}

		public class Configuration
		{
			public bool IncludeTime { get; set; } = true;
			public bool IncludeWeather { get; set; } = true;
			public bool IncludeCamera { get; set; } = true;
			public bool IncludePose { get; set; } = true;
			public PoseFile.Configuration Pose { get; set; } = new PoseFile.Configuration();
			public bool IncludeCharacter { get; set; } = true;
			public CharacterFile.SaveModes Character { get; set; } = CharacterFile.SaveModes.All;
			public bool IncludeActorRotation { get; set; } = true;
			public bool IncludeActorPosition { get; set; } = true;
			public bool IncludeActorScale { get; set; } = true;

			public bool IncludeActors
			{
				get
				{
					return this.IncludePose ||
						this.IncludeCharacter ||
						this.IncludeActorPosition ||
						this.IncludeActorRotation ||
						this.IncludeActorScale;
				}

				set
				{
					this.IncludePose = value;
					this.IncludeCharacter = value;
					this.IncludeActorPosition = value;
					this.IncludeActorRotation = value;
					this.IncludeActorScale = value;
				}
			}
		}
	}
}
