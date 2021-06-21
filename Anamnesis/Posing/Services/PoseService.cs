// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.PoseModule
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading.Tasks;
	using Anamnesis.Core.Memory;
	using Anamnesis.Memory;
	using Anamnesis.Posing.Templates;
	using Anamnesis.Serialization;
	using Anamnesis.Services;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class PoseService : ServiceBase<PoseService>
	{
		public static List<SkeletonFile> BoneNameFiles = new List<SkeletonFile>();
		private static Dictionary<ActorViewModel, SkeletonVisual3d> actorSkeletons = new Dictionary<ActorViewModel, SkeletonVisual3d>();

		private NopHookViewModel? freezeRot1;
		private NopHookViewModel? freezeRot2;
		private NopHookViewModel? freezeRot3;
		private NopHookViewModel? freezeScale1;
		private NopHookViewModel? freezePosition;
		private NopHookViewModel? freezePosition2;
		private NopHookViewModel? freeseScale2;
		private NopHookViewModel? freezePhysics1;
		private NopHookViewModel? freezePhysics2;
		private NopHookViewModel? freezePhysics3;

		private bool isEnabled;

		public delegate void PoseEvent(bool value);

		public static event PoseEvent? EnabledChanged;

		public bool IsEnabled
		{
			get
			{
				return this.isEnabled;
			}

			set
			{
				if (this.IsEnabled == value)
					return;

				this.SetEnabled(value);
			}
		}

		public bool FreezePhysics
		{
			get
			{
				return this.freezePhysics1?.Enabled ?? false;
			}
			set
			{
				this.freezePhysics1?.SetEnabled(value);
				this.freezePhysics2?.SetEnabled(value);
			}
		}

		public bool FreezePositions
		{
			get
			{
				return this.freezePosition?.Enabled ?? false;
			}
			set
			{
				this.freezePosition?.SetEnabled(value);
				this.freezePosition2?.SetEnabled(value);
			}
		}

		public bool FreezeScale
		{
			get
			{
				return this.freezeScale1?.Enabled ?? false;
			}
			set
			{
				this.freezeScale1?.SetEnabled(value);
				this.freezePhysics3?.SetEnabled(value);
				this.freeseScale2?.SetEnabled(value);
			}
		}

		public bool FreezeRotation
		{
			get
			{
				return this.freezeRot1?.Enabled ?? false;
			}
			set
			{
				this.freezeRot1?.SetEnabled(value);
				this.freezeRot2?.SetEnabled(value);
				this.freezeRot3?.SetEnabled(value);
			}
		}

		public bool EnableParenting { get; set; } = true;

		public bool CanEdit { get; set; }

		public static void SaveTemplate(SkeletonFile skeleton)
		{
			string name = "Generated_";

			if (skeleton.ModelTypes != null)
			{
				for (int i = 0; i < skeleton.ModelTypes.Count; i++)
				{
					if (i > 0)
						name += "_";

					name += skeleton.ModelTypes[i];
				}
			}

			if (skeleton.Race != null)
				name += "_" + skeleton.Race;

			if (skeleton.Age != null)
				name += "_" + skeleton.Age;

			SerializerService.SerializeFile("Data/Skeletons/" + name + ".json", skeleton);
			BoneNameFiles.Add(skeleton);
		}

		public static async Task<SkeletonVisual3d> GetVisual(ActorViewModel actor)
		{
			SkeletonVisual3d skeleton;

			if (actorSkeletons.ContainsKey(actor))
			{
				skeleton = actorSkeletons[actor];
				skeleton.Clear();
				actorSkeletons.Remove(actor);
			}

			// TODO: Why does a new skeleton work, but clearing an old one gives us "not a child of the specified visual" when writing?
			////else
			{
				skeleton = new SkeletonVisual3d(actor);
				actorSkeletons.Add(actor, skeleton);
			}

			skeleton.Clear();
			await skeleton.GenerateBones();

			return skeleton;
		}

		public override async Task Initialize()
		{
			await base.Initialize();

			this.freezePosition = new NopHookViewModel(AddressService.SkeletonFreezePosition, 5);
			this.freezePosition2 = new NopHookViewModel(AddressService.SkeletonFreezePosition2, 5);
			this.freezeRot1 = new NopHookViewModel(AddressService.SkeletonFreezeRotation, 6);
			this.freezeRot2 = new NopHookViewModel(AddressService.SkeletonFreezeRotation2, 6);
			this.freezeRot3 = new NopHookViewModel(AddressService.SkeletonFreezeRotation3, 4);
			this.freezeScale1 = new NopHookViewModel(AddressService.SkeletonFreezeScale, 6);
			this.freeseScale2 = new NopHookViewModel(AddressService.SkeletonFreezeScale2, 6);
			this.freezePhysics1 = new NopHookViewModel(AddressService.SkeletonFreezePhysics, 4);
			this.freezePhysics2 = new NopHookViewModel(AddressService.SkeletonFreezePhysics2, 3);
			this.freezePhysics3 = new NopHookViewModel(AddressService.SkeletonFreezePhysics3, 4);

			GposeService.GposeStateChanging += this.OnGposeStateChanging;

			string[] templates = Directory.GetFiles("Data/Skeletons/", "*.json");
			foreach (string templatePath in templates)
			{
				this.Load(templatePath);
			}
		}

		public override async Task Shutdown()
		{
			await base.Shutdown();
			this.SetEnabled(false);
		}

		public void SetEnabled(bool enabled)
		{
			// Don't try to enable posing unless we are in gpose
			if (enabled && !GposeService.Instance.IsGpose)
				throw new Exception("Attempt to enable posing outside of gpose");

			if (this.isEnabled == enabled)
				return;

			this.isEnabled = enabled;

			this.FreezePositions = false;
			this.FreezeScale = false;
			this.EnableParenting = true;
			this.FreezePhysics = enabled;
			this.FreezeRotation = enabled;

			EnabledChanged?.Invoke(enabled);

			this.RaisePropertyChanged(nameof(this.IsEnabled));
		}

		private void OnGposeStateChanging()
		{
			this.SetEnabled(false);
		}

		private SkeletonFile Load(string path)
		{
			SkeletonFile template = SerializerService.DeserializeFile<SkeletonFile>(path);
			template.Path = path;
			BoneNameFiles.Add(template);

			if (template.BasedOn != null)
			{
				SkeletonFile baseTemplate = this.Load("Data/Skeletons/" + template.BasedOn);
				template.CopyBaseValues(baseTemplate);
			}

			// Validate that all bone names are unique
			if (template.BoneNames != null)
			{
				HashSet<string> boneNames = new HashSet<string>();

				foreach ((string orignal, string name) in template.BoneNames)
				{
					if (boneNames.Contains(name))
						throw new Exception($"Duplicate bone name: {name} in skeleton file: {path}");

					boneNames.Add(name);
				}
			}

			return template;
		}
	}
}
