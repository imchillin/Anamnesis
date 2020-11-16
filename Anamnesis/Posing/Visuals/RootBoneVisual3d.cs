// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Posing.Visuals
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Windows.Media.Media3D;
	using Anamnesis.Memory;
	using Anamnesis.PoseModule;

	public class RootBoneVisual3d : BoneVisual3d
	{
		public RootBoneVisual3d(ActorViewModel actor, SkeletonVisual3d skeleton)
			: base((TransformViewModel)actor.ModelObject!.Transform!, skeleton)
		{
			this.ReadTransform();
		}

		public override string? TooltipKey => "Pose_CharacterRoot";

		public override void ReadTransform()
		{
			this.Position = this.ViewModel.Position;
			this.Scale = this.ViewModel.Scale;
			this.Rotation = this.ViewModel.Rotation;
		}

		public override void WriteTransform(ModelVisual3D root, bool writeChildren = true)
		{
			this.ViewModel.Position = this.Position;
			this.ViewModel.Scale = this.Scale;
			this.ViewModel.Rotation = this.Rotation;
		}
	}
}
