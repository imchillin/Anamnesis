// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Posing.Templates
{
	using System.Collections.Generic;
	using Anamnesis.Memory;

	public class SkeletonFile
	{
		public int Depth = 0;

		public int ModelType { get; set; } = 0;
		public Appearance.Races? Race { get; set; }
		public Appearance.Ages? Age { get; set; }
		public string? BasedOn { get; set; }

		public Dictionary<string, string>? BoneNames { get; set; }
		public Dictionary<string, string>? Parenting { get; set; }

		public bool IsValid(SkeletonViewModel self, ActorViewModel actor)
		{
			if (actor.ModelType != this.ModelType)
				return false;

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

			if (this.BoneNames == null)
				this.BoneNames = new Dictionary<string, string>();

			foreach ((string key, string name) in from.BoneNames)
			{
				if (this.BoneNames.ContainsKey(key))
					continue;

				this.BoneNames.Add(key, name);
			}

			if (this.Parenting == null)
			{
				this.Parenting = from.Parenting;
			}
		}
	}
}
