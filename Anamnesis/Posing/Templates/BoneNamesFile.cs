// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Posing.Templates
{
	using System.Collections.Generic;
	using Anamnesis.Memory;

	public class BoneNamesFile
	{
		public int Depth = 0;

		public int ModelType { get; set; } = 0;
		public Appearance.Races? Race { get; set; }
		public string? BasedOn { get; set; }

		public Dictionary<string, string>? BoneNames { get; set; }

		public bool IsValid(SkeletonViewModel self, ActorViewModel actor)
		{
			if (actor.ModelType != this.ModelType)
				return false;

			if (this.Race != null)
			{
				if (actor.Customize?.Race != this.Race)
					return false;
			}

			return true;
		}

		public void CopyBaseValues(BoneNamesFile from)
		{
			this.Depth = from.Depth + 1;

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
		}
	}
}
