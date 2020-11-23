// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Posing.Templates
{
	using System.Collections.Generic;
	using Anamnesis.Memory;

	public class TemplateSkeleton
	{
		public int Depth = 0;

		public int ModelType { get; set; } = 0;
		public Appearance.Races? Race { get; set; }
		public string? BasedOn { get; set; }

		public Dictionary<string, TemplateBone>? Body { get; set; }
		public string? HeadRoot { get; set; }
		public Dictionary<string, TemplateBone>? Head { get; set; }
		public string? HairRoot { get; set; }
		public Dictionary<string, TemplateBone>? Hair { get; set; }
		public string? MetRoot { get; set; }
		public Dictionary<string, TemplateBone>? Met { get; set; }
		public string? TopRoot { get; set; }
		public Dictionary<string, TemplateBone>? Top { get; set; }

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

		public void CopyBaseValues(TemplateSkeleton from)
		{
			this.Depth = from.Depth + 1;
			this.Body = this.Body ?? from.Body;
			this.HeadRoot = this.HeadRoot ?? from.HeadRoot;
			this.Head = this.Head ?? from.Head;
			this.HairRoot = this.HairRoot ?? from.HairRoot;
			this.Hair = this.Hair ?? from.Hair;
			this.MetRoot = this.MetRoot ?? from.MetRoot;
			this.Met = this.Met ?? from.Met;
			this.TopRoot = this.TopRoot ?? from.TopRoot;
			this.Top = this.Top ?? from.Top;
		}
	}
}
