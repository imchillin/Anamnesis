// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Posing.Extensions
{
	using Anamnesis.Memory;
	using Anamnesis.PoseModule;
	using Anamnesis.Posing.Templates;

	public static class SkeletonViewModelExtensions
	{
		public static SkeletonTemplateFile? GetSkeletonTemplate(this SkeletonViewModel self, ActorViewModel actor)
		{
			SkeletonTemplateFile? skel = GetSkeletonTemplate(self, actor, false);

			if (skel == null)
				skel = GetSkeletonTemplate(self, actor, true);

			return skel;
		}

		private static SkeletonTemplateFile? GetSkeletonTemplate(this SkeletonViewModel self, ActorViewModel actor, bool includeGenerated)
		{
			int maxDepth = int.MinValue;
			SkeletonTemplateFile? maxSkel = null;

			foreach (SkeletonTemplateFile template in PoseService.SkeletonTemplates)
			{
				if (template.IsGeneratedParenting != includeGenerated)
					continue;

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
