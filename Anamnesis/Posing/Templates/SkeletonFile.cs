// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Posing.Templates
{
	using System.Collections.Generic;
	using Anamnesis.Memory;

	public class SkeletonFile
	{
		public int Depth = 0;
		public string Path = string.Empty;

		public bool IsGeneratedParenting { get; set; } = false;

		public List<int>? ModelTypes { get; set; }
		public Customize.Races? Race { get; set; }
		public Customize.Ages? Age { get; set; }
		public bool AllowPoseGui { get; set; } = false;
		public bool AllowPoseMatrix { get; set; } = false;
		public string? BasedOn { get; set; }

		public Dictionary<string, string>? BoneNames { get; set; }
		public Dictionary<string, string>? Parenting { get; set; }

		public bool IsValid(SkeletonViewModel self, ActorViewModel actor)
		{
			if (this.ModelTypes != null)
			{
				if (!this.ModelTypes.Contains(actor.ModelType))
				{
					return false;
				}
			}

			if (this.Race != null)
			{
				if (actor.Customize?.Race != this.Race)
				{
					return false;
				}
			}

			if (this.Age != null)
			{
				if (actor.Customize?.Age != this.Age)
				{
					return false;
				}
			}

			return true;
		}

		public void CopyBaseValues(SkeletonFile from)
		{
			this.Depth = from.Depth + 1;

			if (this.Age == null)
				this.Age = from.Age;

			if (from.BoneNames == null)
				return;

			if (this.ModelTypes == null)
				this.ModelTypes = from.ModelTypes;

			if (this.BoneNames == null)
				this.BoneNames = new Dictionary<string, string>();

			foreach ((string key, string name) in from.BoneNames)
			{
				if (this.BoneNames.ContainsKey(key))
					continue;

				this.BoneNames.Add(key, name);
			}

			if (from.Parenting != null)
			{
				if (this.Parenting == null)
					this.Parenting = new Dictionary<string, string>();

				foreach ((string bone, string parent) in from.Parenting)
				{
					if (this.Parenting.ContainsKey(bone))
						continue;

					this.Parenting.Add(bone, parent);
				}
			}
		}
	}
}
