// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Posing.Extensions
{
	using Anamnesis.Memory;
	using Anamnesis.PoseModule;
	using Anamnesis.Posing.Templates;

	public static class SkeletonViewModelExtensions
	{
		public static TemplateSkeleton GetTemplate(this SkeletonViewModel self, ActorViewModel actor)
		{
			int maxDepth = int.MinValue;
			TemplateSkeleton? maxSkel = null;

			foreach (TemplateSkeleton template in PoseService.Templates)
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

			// No template...
			return new TemplateSkeleton();
		}
	}
}
