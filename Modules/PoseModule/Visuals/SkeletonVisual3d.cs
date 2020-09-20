// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.PoseModule
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Media.Media3D;
	using Anamnesis.Memory;
	using PropertyChanged;

	using AnQuaternion = Anamnesis.Memory.Quaternion;

	[AddINotifyPropertyChangedInterface]
	public class SkeletonVisual3d : ModelVisual3D, INotifyPropertyChanged
	{
		private BoneVisual3d currentBone;

		public SkeletonVisual3d(ActorViewModel actor)
		{
			this.Actor = actor;

			this.GenerateBones();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public ActorViewModel Actor { get; private set; }
		public BoneVisual3d MouseOverBone { get; set; }

		public BoneVisual3d CurrentBone
		{
			get
			{
				return this.currentBone;
			}
			set
			{
				this.currentBone = value;
				Task.Run(this.WriteSkeletonThread);
			}
		}

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

		public AnQuaternion RootRotation
		{
			get
			{
				return this.Actor?.Model?.Transform?.Rotation ?? AnQuaternion.Identity;
			}
		}

		public bool GetIsBoneHovered(BoneVisual3d bone)
		{
			return bone == this.MouseOverBone;
		}

		public bool GetIsBoneSelected(BoneVisual3d bone)
		{
			return bone == this.CurrentBone;
		}

		public bool GetIsBoneParentsSelected(BoneVisual3d bone)
		{
			while (bone != null)
			{
				if (bone == this.CurrentBone)
					return true;

				bone = bone.Parent;
			}

			return false;
		}

		public bool GetIsBoneParentsHovered(BoneVisual3d bone)
		{
			while (bone != null)
			{
				if (bone == this.MouseOverBone)
					return true;

				bone = bone.Parent;
			}

			return false;
		}

		public BoneVisual3d GetBone(string name)
		{
			// only show actors that have a body
			if (this.Actor?.Model?.Skeleton?.Skeleton?.Body == null)
				return null;

			if (this.Actor.ModelType != 0)
				return null;

			TransformViewModel transform = this.Actor.Model.Skeleton.Skeleton.GetBone(name);

			if (transform == null)
				return null;

			foreach (BoneVisual3d bone in this.Bones)
			{
				if (bone.ViewModel == transform)
				{
					return bone;
				}
			}

			return null;
		}

		public void ReadTranforms()
		{
			foreach (BoneVisual3d bone in this.Bones)
			{
				bone.ReadTransform();
			}
		}

		private void GenerateBones()
		{
			this.Bones = new List<BoneVisual3d>();

			// only show actors that have a body
			if (this.Actor?.Model?.Skeleton?.Skeleton?.Body == null)
				return;

			SkeletonViewModel skeletonVm = this.Actor.Model.Skeleton.Skeleton;

			// Body bones
			List<BoneVisual3d> bodyBones = new List<BoneVisual3d>();
			for (int i = 0; i < skeletonVm.Body.Count; i++)
			{
				BoneVisual3d bone = new BoneVisual3d(skeletonVm.Body.Transforms[i], this);
				bodyBones.Add(bone);
				this.Bones.Add(bone);
				////this.RootBones.Add(bone);
			}

			for (int i = 0; i < skeletonVm.Body.Count; i++)
			{
				int parent = SkeletonUtility.PlayerBodyParents[i];
				if (parent == -1)
				{
					this.Children.Add(bodyBones[i]);
				}
				else
				{
					bodyBones[i].Parent = bodyBones[parent];
				}
			}

			int headBoneIndex = SkeletonUtility.BodyBoneIndexLookup["Head"];

			BoneVisual3d headRoot = null;
			if (headBoneIndex < bodyBones.Count)
				headRoot = bodyBones[headBoneIndex];

			if (skeletonVm.Head != null)
			{
				foreach (TransformViewModel boneTrans in skeletonVm.Head.Transforms)
				{
					BoneVisual3d bone = new BoneVisual3d(boneTrans, this);
					this.Bones.Add(bone);
					bone.Parent = headRoot;

					if (headRoot == null)
					{
						this.Children.Add(bone);
					}
				}
			}

			if (skeletonVm.Hair != null)
			{
				foreach (TransformViewModel boneTrans in skeletonVm.Hair.Transforms)
				{
					BoneVisual3d bone = new BoneVisual3d(boneTrans, this);
					this.Bones.Add(bone);
					bone.Parent = headRoot;

					if (headRoot == null)
					{
						this.Children.Add(bone);
					}
				}
			}

			if (skeletonVm.Met != null)
			{
				foreach (TransformViewModel boneTrans in skeletonVm.Met.Transforms)
				{
					BoneVisual3d bone = new BoneVisual3d(boneTrans, this);
					this.Bones.Add(bone);
					this.Children.Add(bone);
				}
			}

			if (skeletonVm.Top != null)
			{
				foreach (TransformViewModel boneTrans in skeletonVm.Top.Transforms)
				{
					BoneVisual3d bone = new BoneVisual3d(boneTrans, this);
					this.Bones.Add(bone);
					this.Children.Add(bone);
				}
			}

			foreach (BoneVisual3d bone in this.Bones)
			{
				bone.ReadTransform();
			}

			foreach (BoneVisual3d bone in this.Bones)
			{
				bone.ReadTransform();
			}
		}

		private async Task WriteSkeletonThread()
		{
			while (Application.Current != null && this.CurrentBone != null && PoseService.Instance.IsEnabled)
			{
				Application.Current.Dispatcher.Invoke(() =>
				{
					this.CurrentBone.WriteTransform(this);
				});

				// up to 60 times a second
				await Task.Delay(16);
			}
		}
	}
}
