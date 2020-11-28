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
	using System.Windows.Input;
	using System.Windows.Media.Media3D;
	using Anamnesis.Memory;
	using Anamnesis.PoseModule.Views;
	using Anamnesis.Posing.Extensions;
	using Anamnesis.Posing.Templates;
	using PropertyChanged;

	using AnQuaternion = Anamnesis.Memory.Quaternion;

	[AddINotifyPropertyChangedInterface]
	public class SkeletonVisual3d : ModelVisual3D, INotifyPropertyChanged
	{
		public List<BoneVisual3d> SelectedBones = new List<BoneVisual3d>();
		public HashSet<BoneVisual3d> HoverBones = new HashSet<BoneVisual3d>();

		public SkeletonVisual3d(ActorViewModel actor)
		{
			this.Actor = actor;
			this.GenerateBones();
			Task.Run(this.WriteSkeletonThread);
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		public enum SelectMode
		{
			Override,
			Add,
			Toggle,
		}

		public bool LinkEyes { get; set; } = true;
		public ActorViewModel Actor { get; private set; }

		public BoneVisual3d? CurrentBone
		{
			get
			{
				if (this.SelectedBones.Count <= 0)
					return null;

				return this.SelectedBones[this.SelectedBones.Count - 1];
			}

			set
			{
				throw new NotSupportedException();
			}

			/*set
			{
				this.HoverBones.Clear();

				if (!Keyboard.IsKeyDown(Key.LeftCtrl))
					this.SelectedBones.Clear();

				if (value != null)
					this.Select(value, SelectMode.Add);

				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonVisual3d.HasSelection)));
			}*/
		}

		public bool HasSelection => this.SelectedBones.Count > 0;
		public bool HasHover => this.HoverBones.Count > 0;

		public Dictionary<string, BoneVisual3d>? Bones { get; private set; }

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
				return this.Actor?.ModelObject?.Transform?.Rotation ?? AnQuaternion.Identity;
			}
		}

		public void Clear()
		{
			this.ClearSelection();
			this.HoverBones.Clear();

			if (this.Bones != null)
				this.Bones.Clear();

			this.Bones = null;
		}

		public void Select(IBone bone)
		{
			if (bone.Visual == null)
				return;

			SkeletonVisual3d.SelectMode mode = SkeletonVisual3d.SelectMode.Override;

			if (Keyboard.IsKeyDown(Key.LeftCtrl))
				mode = SkeletonVisual3d.SelectMode.Toggle;

			if (Keyboard.IsKeyDown(Key.LeftShift))
				mode = SkeletonVisual3d.SelectMode.Add;

			if (mode == SelectMode.Override)
				this.SelectedBones.Clear();

			if (this.SelectedBones.Contains(bone.Visual))
			{
				if (mode == SelectMode.Toggle)
				{
					this.SelectedBones.Remove(bone.Visual);
				}
			}
			else
			{
				this.SelectedBones.Add(bone.Visual);

				if (bone.Visual.Template?.RequiresPositions == true)
				{
					PoseService.Instance.LockChildRotation = true;
				}
			}

			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonVisual3d.CurrentBone)));
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonVisual3d.HasSelection)));
		}

		public void Select(IEnumerable<IBone> bones)
		{
			SkeletonVisual3d.SelectMode mode = SkeletonVisual3d.SelectMode.Override;

			if (Keyboard.IsKeyDown(Key.LeftCtrl))
				mode = SkeletonVisual3d.SelectMode.Toggle;

			if (Keyboard.IsKeyDown(Key.LeftShift))
				mode = SkeletonVisual3d.SelectMode.Add;

			if (mode == SelectMode.Override)
			{
				this.SelectedBones.Clear();
				mode = SelectMode.Add;
			}

			foreach (IBone bone in bones)
			{
				if (bone.Visual == null)
					continue;

				if (this.SelectedBones.Contains(bone.Visual))
				{
					if (mode == SelectMode.Toggle)
					{
						this.SelectedBones.Remove(bone.Visual);
					}
				}
				else
				{
					this.SelectedBones.Add(bone.Visual);

					if (bone.Visual.Template?.RequiresPositions == true)
					{
						PoseService.Instance.LockChildRotation = true;
					}
				}
			}

			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonVisual3d.CurrentBone)));
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonVisual3d.HasSelection)));
		}

		public void ClearSelection()
		{
			this.SelectedBones.Clear();

			Application.Current?.Dispatcher.Invoke(() =>
			{
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonVisual3d.CurrentBone)));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonVisual3d.HasSelection)));
			});
		}

		public void Hover(BoneVisual3d bone, bool hover)
		{
			if (this.HoverBones.Contains(bone) && !hover)
			{
				this.HoverBones.Remove(bone);
			}
			else if (!this.HoverBones.Contains(bone) && hover)
			{
				this.HoverBones.Add(bone);
			}
			else
			{
				return;
			}

			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonVisual3d.HasHover)));
		}

		public void Select(List<BoneVisual3d> bones, SelectMode mode)
		{
			if (mode == SelectMode.Override)
				this.SelectedBones.Clear();

			foreach (BoneVisual3d bone in bones)
			{
				if (this.SelectedBones.Contains(bone))
				{
					if (mode == SelectMode.Toggle)
					{
						this.SelectedBones.Remove(bone);
					}
				}
				else
				{
					this.SelectedBones.Add(bone);
				}
			}

			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonVisual3d.CurrentBone)));
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonVisual3d.HasSelection)));
		}

		public bool GetIsBoneHovered(BoneVisual3d bone)
		{
			return this.HoverBones.Contains(bone);
		}

		public bool GetIsBoneSelected(BoneVisual3d bone)
		{
			return this.SelectedBones.Contains(bone);
		}

		public bool GetIsBoneParentsSelected(BoneVisual3d? bone)
		{
			while (bone != null)
			{
				if (this.GetIsBoneSelected(bone))
					return true;

				bone = bone.Parent;
			}

			return false;
		}

		public bool GetIsBoneParentsHovered(BoneVisual3d? bone)
		{
			while (bone != null)
			{
				if (this.GetIsBoneHovered(bone))
					return true;

				bone = bone.Parent;
			}

			return false;
		}

		public BoneVisual3d? GetBone(string name)
		{
			// only show actors that have a body
			if (this.Actor?.ModelObject?.Skeleton?.Skeleton?.Body == null)
				return null;

			if (this.Actor.ModelType != 0)
				return null;

			if (this.Bones != null && this.Bones.ContainsKey(name))
				return this.Bones[name];

			return null;
		}

		public void ReadTranforms()
		{
			if (this.Bones == null)
				return;

			foreach (BoneVisual3d bone in this.Bones.Values)
			{
				bone.ReadTransform();
			}
		}

		private void GenerateBones()
		{
			this.Bones = new Dictionary<string, BoneVisual3d>();

			if (this.Actor?.ModelObject?.Skeleton?.Skeleton == null)
				return;

			SkeletonViewModel skeletonVm = this.Actor.ModelObject.Skeleton.Skeleton;

			TemplateSkeleton template = skeletonVm.GetTemplate(this.Actor);
			this.Generate(template, skeletonVm);

			// Map eyes together if they exist
			BoneVisual3d? lEye = this.GetBone("EyeLeft");
			BoneVisual3d? rEye = this.GetBone("EyeRight");
			if (lEye != null && rEye != null)
			{
				lEye.LinkedEye = rEye;
				rEye.LinkedEye = lEye;
			}

			foreach (BoneVisual3d bone in this.Bones.Values)
			{
				bone.ReadTransform();
			}

			foreach (BoneVisual3d bone in this.Bones.Values)
			{
				bone.ReadTransform();
			}
		}

		private ModelVisual3D GetVisual(string? name)
		{
			if (name == null)
				return this;

			if (this.Bones != null && this.Bones.ContainsKey(name))
				return this.Bones[name];

			return this;
		}

		private void Generate(TemplateSkeleton template, SkeletonViewModel memory)
		{
			this.Generate(template.Body, memory.Body, "Body", this);
			this.Generate(template.Head, memory.Head, "Head", this.GetVisual(template.HeadRoot));
			this.Generate(template.Hair, memory.Hair, "Hair", this.GetVisual(template.HairRoot));
			this.Generate(template.Met, memory.Met, "Met", this.GetVisual(template.MetRoot));
			this.Generate(template.Top, memory.Top, "Top", this.GetVisual(template.TopRoot));
		}

		private void Generate(Dictionary<string, TemplateBone>? template, BonesViewModel? memory, string fallbackName, ModelVisual3D root)
		{
			if (this.Bones == null)
				this.Bones = new Dictionary<string, BoneVisual3d>();

			if (template == null)
				template = new Dictionary<string, TemplateBone>();

			Dictionary<int, string> nameLookup = new Dictionary<int, string>();
			foreach ((string name, TemplateBone templateBone) in template)
			{
				nameLookup.Add(templateBone.Index, name);
			}

			if (memory != null)
			{
				Dictionary<string, BoneVisual3d> newBones = new Dictionary<string, BoneVisual3d>();
				for (int i = 0; i < memory.Transforms.Count; i++)
				{
					TransformViewModel? transform = memory.Transforms[i];
					string name = fallbackName + "_" + i;

					if (nameLookup.ContainsKey(i))
						name = nameLookup[i];

					TemplateBone? boneTemplate = null;

					if (template.ContainsKey(name))
						boneTemplate = template[name];

					BoneVisual3d bone = new BoneVisual3d(transform, this, name, boneTemplate);
					newBones.Add(name, bone);
					this.Bones.Add(name, bone);
				}

				foreach (BoneVisual3d bone in newBones.Values)
				{
					string? parentBoneName = null;

					if (template.ContainsKey(bone.BoneName))
						parentBoneName = template[bone.BoneName].Parent;

					if (parentBoneName != null)
					{
						bone.Parent = newBones[parentBoneName];
					}
					else if (root is BoneVisual3d rootBone)
					{
						bone.Parent = rootBone;
					}
					else
					{
						root.Children.Add(bone);
					}
				}
			}
		}

		private async Task WriteSkeletonThread()
		{
			while (Application.Current != null && this.Bones != null)
			{
				await Dispatch.MainThread();

				if (this.CurrentBone != null && PoseService.Instance.IsEnabled)
					this.CurrentBone?.WriteTransform(this);

				// up to 60 times a second
				await Task.Delay(16);
			}
		}
	}

	#pragma warning disable SA1201
	public interface IBone
	{
		BoneVisual3d? Visual { get; }
	}
}
