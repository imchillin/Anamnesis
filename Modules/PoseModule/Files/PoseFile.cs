// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.PoseModule
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Anamnesis.Memory;
	using Anamnesis.Services;

	public class PoseFile : FileBase
	{
		public static readonly FileType FileType = new FileType(@"pose", "Anamnesis Pose File", typeof(PoseFile), true, "Poses");

		[Flags]
		public enum Groups
		{
			None = 0,

			Body = 1,
			Head = 2,
			Hair = 4,
			Met = 8,
			Top = 16,

			All = Body | Head | Hair | Met | Top,
		}

		public override FileType Type => FileType;

		public Configuration Config { get; set; } = new Configuration();

		public List<Bone> Body { get; set; }
		public List<Bone> Head { get; set; }
		public List<Bone> Hair { get; set; }
		public List<Bone> Met { get; set; }
		public List<Bone> Top { get; set; }

		public void WriteToFile(ActorViewModel actor, Configuration config)
		{
			this.Config = config;

			SkeletonViewModel skeleton = actor?.Model?.Skeleton?.Skeleton;

			if (skeleton == null)
				throw new Exception("No skeleton in actor");

			if (config.Groups.HasFlag(Groups.Body))
				this.Body = this.WriteToFile(skeleton.Body);

			if (config.Groups.HasFlag(Groups.Head))
				this.Head = this.WriteToFile(skeleton.Head);

			if (config.Groups.HasFlag(Groups.Hair))
				this.Hair = this.WriteToFile(skeleton.Hair);

			if (config.Groups.HasFlag(Groups.Met))
				this.Met = this.WriteToFile(skeleton.Met);

			if (config.Groups.HasFlag(Groups.Top))
				this.Top = this.WriteToFile(skeleton.Top);

			Log.Write("Saved skeleton to file");
		}

		public async Task ReadFromFile(ActorViewModel actor, Configuration config)
		{
			SkeletonViewModel skeleton = actor?.Model?.Skeleton?.Skeleton;

			if (skeleton == null)
				throw new Exception("No skeleton in actor");

			PoseService.Instance.SetEnabled(true);
			await Task.Delay(100);

			// don't freeze positions if we aren't writing any
			PoseService.Instance.FreezePositions = this.Config.IncludePosition;

			if (config.Groups.HasFlag(Groups.Body))
				this.ReadFromFile(this.Body, skeleton.Body, config);

			if (config.Groups.HasFlag(Groups.Head))
				this.ReadFromFile(this.Head, skeleton.Head, config);

			if (config.Groups.HasFlag(Groups.Hair))
				this.ReadFromFile(this.Hair, skeleton.Hair, config);

			if (config.Groups.HasFlag(Groups.Met))
				this.ReadFromFile(this.Met, skeleton.Met, config);

			if (config.Groups.HasFlag(Groups.Top))
				this.ReadFromFile(this.Top, skeleton.Top, config);

			await Task.Delay(100);
			PoseService.Instance.FreezePositions = true;

			////skeleton.RefreshBones();
		}

		private List<Bone> WriteToFile(BonesViewModel bones)
		{
			if (bones == null)
				return null;

			if (bones.Count <= 0 || bones.Count > 512)
				return null;

			if (bones.Transforms == null || bones.Transforms.Count != bones.Count)
				throw new Exception("Bone transform array does not match expected size");

			List<Bone> transforms = new List<Bone>();
			foreach (TransformViewModel bone in bones.Transforms)
			{
				Transform trans = (Transform)bone.GetModel();
				transforms.Add(new Bone(trans));
			}

			return transforms;
		}

		private void ReadFromFile(List<Bone> transforms, BonesViewModel bones, Configuration config)
		{
			if (bones == null)
				return;

			if (transforms.Count <= 0)
				return;

			////if (transforms.Count != bones.Count)
			////	throw new Exception("Saved pose bone count does not match target skeleton bone count");

			int count = Math.Min(transforms.Count, bones.Count);

			for (int i = 0; i < count; i++)
			{
				if (config.IncludePosition && this.Config.IncludePosition && transforms[i].Position != Vector.Zero)
					bones.Transforms[i].Position = transforms[i].Position;

				if (config.IncludeRotation && this.Config.IncludeRotation)
					bones.Transforms[i].Rotation = transforms[i].Rotation;

				if (config.IncludeScale && this.Config.IncludeScale && transforms[i].Scale != Vector.Zero)
					bones.Transforms[i].Scale = transforms[i].Scale;
			}
		}

		public class Configuration
		{
			public PoseFile.Groups Groups { get; set; } = Groups.All;
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
