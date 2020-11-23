// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Files
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Anamnesis.Files.Infos;
	using Anamnesis.Files.Types;
	using Anamnesis.Memory;
	using Anamnesis.PoseModule;
	using Anamnesis.Services;

	#pragma warning disable SA1402, SA1649
	public class PoseFileInfo : JsonFileInfoBase<PoseFile>
	{
		public override string Extension => "pose";
		public override string Name => "Anamnesis Pose File";
		public override IFileSource FileSource => new LocalFileSource("Local Files", SettingsService.Current.DefaultPoseDirectory);
	}

	public class PoseFile : FileBase
	{
		public Appearance.Races? Race { get; set; }
		public Configuration Config { get; set; } = new Configuration();

		public Dictionary<string, Bone?>? Body { get; set; }
		public Dictionary<string, Bone?>? Hair { get; set; }
		public Dictionary<string, Bone?>? Met { get; set; }
		public Dictionary<string, Bone?>? Top { get; set; }
		public Dictionary<string, Bone?>? Head { get; set; }

		public void WriteToFile(ActorViewModel actor, Configuration config)
		{
			this.Config = config;
			this.Race = actor?.Customize?.Race;

			SkeletonViewModel? skeleton = actor?.ModelObject?.Skeleton?.Skeleton;

			/*if (skeleton == null)
				throw new Exception("No skeleton in actor");

			if (skeleton.Body != null)
				this.Body = this.WriteToFile(skeleton.Body);

			if (skeleton.Head != null)
				this.Head = this.WriteToFile(skeleton.Head);

			if (skeleton.Hair != null)
				this.Hair = this.WriteToFile(skeleton.Hair);

			if (skeleton.Met != null)
				this.Met = this.WriteToFile(skeleton.Met);

			if (skeleton.Top != null)
				this.Top = this.WriteToFile(skeleton.Top);*/

			Log.Write("Saved skeleton to file");
		}

		public async Task Apply(ActorViewModel actor, Configuration config)
		{
			SkeletonViewModel? skeleton = actor?.ModelObject?.Skeleton?.Skeleton;

			if (skeleton == null)
				throw new Exception("No skeleton in actor");

			PoseService.Instance.SetEnabled(true);
			PoseService.Instance.CanEdit = false;
			await Task.Delay(100);

			// don't freeze positions if we aren't writing any
			PoseService.Instance.FreezePositions = this.Config.IncludePosition && config.IncludePosition;

			/*if (skeleton.Body != null)
				this.ReadFromFile(this.Body, skeleton.Body, config);

			if (skeleton.Head != null)
				this.ReadHeadFromFile(actor, config);

			if (skeleton.Hair != null)
				this.ReadFromFile(this.Hair, skeleton.Hair, config);

			if (skeleton.Met != null)
				this.ReadFromFile(this.Met, skeleton.Met, config);

			if (skeleton.Top != null)
				this.ReadFromFile(this.Top, skeleton.Top, config);*/

			await Task.Delay(100);
			PoseService.Instance.FreezePositions = true;

			await skeleton.ReadFromMemoryAsync();
			PoseService.Instance.CanEdit = true;
		}

		/*private Dictionary<string, Bone?>? WriteToFile(BonesViewModel bones)
		{
			if (bones == null)
				return null;

			if (bones.Count <= 0 || bones.Count > 512)
				return null;

			if (bones.Transforms == null || bones.Transforms.Count != bones.Count)
				throw new Exception("Bone transform array does not match expected size");

			Dictionary<string, Bone?>? transforms = new Dictionary<string, Bone?>?(bones.Transforms.Count);
			for (int i = 0; i < bones.Transforms.Count; i++)
			{
				Transform? trans = bones.Transforms[i].Model;

				if (trans == null)
					throw new Exception("Bone is missing transform");

				transforms.Add(new Bone(trans.Value));
			}

			return transforms;
		}

		private void ReadFromFile(List<Bone?>? transforms, BonesViewModel bones, Configuration config)
		{
			if (bones == null)
				return;

			if (transforms == null || transforms.Count <= 0)
				return;

			////if (transforms.Count != bones.Count)
			////throw new Exception("Saved pose bone count does not match target skeleton bone count");

			int count = Math.Min(transforms.Count, bones.Count);

			for (int i = 0; i < count; i++)
			{
				Bone? bone = transforms[i];

				if (bone == null)
					continue;

				if (config.IncludePosition && this.Config.IncludePosition && bone.Position != Vector.Zero)
					bones.Transforms[i].Position = bone.Position;

				if (config.IncludeRotation && this.Config.IncludeRotation)
					bones.Transforms[i].Rotation = bone.Rotation;

				if (config.IncludeScale && this.Config.IncludeScale && bone.Scale != Vector.Zero)
					bones.Transforms[i].Scale = bone.Scale;
			}
		}*/

		public class Configuration
		{
			public List<BoneVisual3d>? Bones;
			public bool IncludeRotation { get; set; } = true;
			public bool IncludePosition { get; set; } = true;
			public bool IncludeScale { get; set; } = true;
		}

		[Serializable]
		public class Bone
		{
			public Bone()
			{
			}

			public Bone(Transform trans)
			{
				this.Position = trans.Position;
				this.Rotation = trans.Rotation;
				this.Scale = trans.Scale;
			}

			public Vector Position { get; set; }
			public Quaternion Rotation { get; set; }
			public Vector Scale { get; set; }
		}
	}
}
