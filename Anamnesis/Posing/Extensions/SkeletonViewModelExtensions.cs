// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Posing.Extensions
{
	using System.Collections.Generic;
	using Anamnesis.Memory;
	using Anamnesis.PoseModule;
	using Anamnesis.Posing.Templates;

	public static class SkeletonViewModelExtensions
	{
		public static SkeletonFile? GetSkeletonFile(this SkeletonViewModel self, ActorViewModel actor)
		{
			int maxDepth = int.MinValue;
			SkeletonFile? maxSkel = null;

			foreach (SkeletonFile template in PoseService.BoneNameFiles)
			{
				if (template.IsValid(self, actor))
				{
					if (template.Depth > maxDepth)
					{
						maxDepth = template.Depth;
						maxSkel = template;
					}
				}
			}

			if (maxSkel != null)
				return maxSkel;

			return null;
		}
	}
}
