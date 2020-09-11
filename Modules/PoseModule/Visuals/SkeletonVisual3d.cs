// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.PoseModule
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Text;
	using Anamnesis.Memory;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class SkeletonVisual3d : INotifyPropertyChanged
	{
		public SkeletonVisual3d(ActorViewModel actor)
		{
			this.Actor = actor;

			this.GenerateBones();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public ActorViewModel Actor { get; private set; }
		public BoneVisual3d MouseOverBone { get; set; }
		public BoneVisual3d CurrentBone { get; set; }

		public List<BoneVisual3d> RootBones { get; } = new List<BoneVisual3d>();
		public List<BoneVisual3d> Bones { get; private set; }

		public bool HasTail => this.Actor?.Customize?.Race == Appearance.Races.Miqote
			|| this.Actor?.Customize?.Race == Appearance.Races.AuRa
			|| this.Actor?.Customize?.Race == Appearance.Races.Hrothgar;

		public bool IsViera => this.Actor?.Customize?.Race == Appearance.Races.Viera;
		public bool IsVieraEars01 => this.IsViera && this.Actor?.Customize?.TailEarsType <= 1;
		public bool IsVieraEars02 => this.IsViera && this.Actor?.Customize?.TailEarsType == 2;
		public bool IsVieraEars03 => this.IsViera && this.Actor?.Customize?.TailEarsType == 3;
		public bool IsVieraEars04 => this.IsViera && this.Actor?.Customize?.TailEarsType == 4;
		public bool IsHrothgar => this.Actor?.Customize?.Race == Appearance.Races.Hrothgar;
		public bool HasTailOrEars => this.IsViera || this.HasTail;

		public Quaternion RootRotation
		{
			get
			{
				return this.Actor?.Model?.Transform?.Rotation ?? Quaternion.Identity;
			}
		}

		public bool GetIsBoneHovered(BoneVisual3d bone)
		{
			return false;
		}

		public bool GetIsBoneSelected(BoneVisual3d bone)
		{
			return false;
		}

		public bool GetIsBoneParentsSelected(BoneVisual3d bone)
		{
			return false;
		}

		public bool GetIsBoneParentsHovered(BoneVisual3d bone)
		{
			return false;
		}

		public BoneVisual3d GetBone(string name)
		{
			return null;
		}

		private void GenerateBones()
		{
			this.Bones = new List<BoneVisual3d>();

			// only show actors that have a body
			if (this.Actor?.Model?.Skeleton?.Skeleton?.Body == null)
				return;

			SkeletonViewModel skeletonVm = this.Actor.Model.Skeleton.Skeleton;

			foreach (TransformViewModel boneTrans in skeletonVm.Body.Transforms)
			{
				BoneVisual3d bone = new BoneVisual3d(boneTrans, this);
				this.Bones.Add(bone);

				this.RootBones.Add(bone);
			}
		}
	}
}
