// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.PoseModule
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Input;
	using System.Windows.Media.Media3D;
	using Anamnesis.GUI.Dialogs;
	using Anamnesis.Memory;
	using Anamnesis.PoseModule.Views;
	using Anamnesis.Posing;
	using Anamnesis.Posing.Extensions;
	using Anamnesis.Posing.Templates;
	using Anamnesis.Services;
	using PropertyChanged;
	using SimpleLog;
	using AnQuaternion = Anamnesis.Memory.Quaternion;

	[AddINotifyPropertyChangedInterface]
	public class SkeletonVisual3d : ModelVisual3D, INotifyPropertyChanged
	{
		public List<BoneVisual3d> SelectedBones = new List<BoneVisual3d>();
		public HashSet<BoneVisual3d> HoverBones = new HashSet<BoneVisual3d>();

		private static readonly Logger Log = SimpleLog.Log.GetLogger("Skeleton");

		public SkeletonVisual3d(ActorViewModel actor)
		{
			this.Actor = actor;
			Task.Run(this.GenerateBones);
			Task.Run(this.WriteSkeletonThread);
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		public enum SelectMode
		{
			Override,
			Add,
			Toggle,
		}

		public bool Generating { get; set; } = false;
		public bool LinkEyes { get; set; } = true;
		public bool FlipSides { get; set; } = false;
		public ActorViewModel Actor { get; private set; }
		public int SelectedCount => this.SelectedBones.Count;

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

		public ObservableCollection<BoneVisual3d> Bones { get; private set; } = new ObservableCollection<BoneVisual3d>();

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
			this.Bones.Clear();
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
			}

			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonVisual3d.CurrentBone)));
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonVisual3d.HasSelection)));
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonVisual3d.SelectedCount)));
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
				}
			}

			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonVisual3d.CurrentBone)));
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonVisual3d.HasSelection)));
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonVisual3d.SelectedCount)));
		}

		public void ClearSelection()
		{
			this.SelectedBones.Clear();

			Application.Current?.Dispatcher.Invoke(() =>
			{
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonVisual3d.CurrentBone)));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonVisual3d.HasSelection)));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonVisual3d.SelectedCount)));
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
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonVisual3d.SelectedCount)));
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

			if (this.FlipSides)
			{
				name = name.Replace("Left", "Temp");
				name = name.Replace("Right", "Left");
				name = name.Replace("Temp", "Right");
			}

			foreach (BoneVisual3d bone in this.Bones)
			{
				if (bone.BoneName == name)
				{
					return bone;
				}
			}

			return null;
		}

		public void ReadTranforms()
		{
			if (this.Bones == null)
				return;

			foreach (BoneVisual3d bone in this.Bones)
			{
				bone.ReadTransform();
			}
		}

		private async Task GenerateBones()
		{
			this.Generating = true;

			await Dispatch.MainThread();

			this.Bones.Clear();

			if (this.Actor?.ModelObject?.Skeleton?.Skeleton == null)
				return;

			SkeletonViewModel skeletonVm = this.Actor.ModelObject.Skeleton.Skeleton;

			////TemplateSkeleton template = skeletonVm.GetTemplate(this.Actor);
			await this.Generate(skeletonVm);

			// Map eyes together if they exist
			BoneVisual3d? lEye = this.GetBone("EyeLeft");
			BoneVisual3d? rEye = this.GetBone("EyeRight");
			if (lEye != null && rEye != null)
			{
				lEye.LinkedEye = rEye;
				rEye.LinkedEye = lEye;
			}

			foreach (BoneVisual3d bone in this.Bones)
			{
				bone.ReadTransform();
			}

			foreach (BoneVisual3d bone in this.Bones)
			{
				bone.ReadTransform();
			}

			this.Generating = false;
		}

		private ModelVisual3D GetVisual(string? name)
		{
			if (name == null)
				return this;

			foreach (BoneVisual3d bone in this.Bones)
			{
				if (bone.BoneName == name)
				{
					return bone;
				}
			}

			return this;
		}

		private async Task Generate(SkeletonViewModel memory)
		{
			SkeletonFile? skeletonFile = memory.GetSkeletonFile(this.Actor);

			bool autoSkeleton = false;

			if ((skeletonFile == null || skeletonFile.Parenting == null) && GposeService.Instance.IsGpose)
			{
				string message = LocalizationService.GetStringFormatted("Pose_GenerateSkeleton", this.Actor.DisplayName);
				bool? result = await GenericDialog.Show(message, LocalizationService.GetString("Pose_GenerateSkeletonTitle"), MessageBoxButton.YesNo);
				autoSkeleton = result == true;
			}

			// Get all bones
			this.Bones.Clear();
			this.GetBones(memory.Body, "Body");
			this.GetBones(memory.Head, "Head");
			this.GetBones(memory.Hair, "Hair");
			this.GetBones(memory.Met, "Met");
			this.GetBones(memory.Top, "Top");

			if (skeletonFile != null && skeletonFile.BoneNames != null)
			{
				foreach (BoneVisual3d bone in this.Bones)
				{
					string? newName;
					if (skeletonFile.BoneNames.TryGetValue(bone.BoneName, out newName))
					{
						bone.BoneName = newName;
					}
				}
			}

			if (autoSkeleton)
			{
				try
				{
					// gnerate parenting
					await ParentingUtility.ParentBones(this, this.Bones);
				}
				catch (Exception ex)
				{
					Log.Write(Severity.Error, new Exception("Failed to generate skeleton file.", ex));
					return;
				}

				if (skeletonFile == null)
				{
					skeletonFile = new SkeletonFile();
					skeletonFile.ModelType = this.Actor.ModelType;
					skeletonFile.Race = this.Actor.Customize?.Race;
				}

				skeletonFile.Parenting = new Dictionary<string, string>();
				foreach (BoneVisual3d bone in this.Bones)
				{
					if (bone.Parent == null)
						continue;

					skeletonFile.Parenting.Add(bone.BoneName, bone.Parent.BoneName);
				}

				PoseService.SaveTemplate(skeletonFile);
			}
			else if (skeletonFile != null && skeletonFile.Parenting != null)
			{
				// parenting from file
				foreach (BoneVisual3d bone in this.Bones)
				{
					string? parentBoneName;
					if (skeletonFile.Parenting.TryGetValue(bone.BoneName, out parentBoneName))
					{
						bone.Parent = this.GetBone(parentBoneName);

						if (bone.Parent == null)
						{
							throw new Exception($"Failed to find target parent bone: {parentBoneName}");
						}
					}
					else
					{
						this.Children.Add(bone);
					}
				}
			}
			else
			{
				// no parenting...
				PoseService.Instance.EnableParenting = false;

				foreach (BoneVisual3d bone in this.Bones)
				{
					this.Children.Add(bone);
				}
			}
		}

		private void GetBones(BonesViewModel? vm, string name)
		{
			if (vm == null)
				return;

			for (int i = 0; i < vm.Transforms.Count; i++)
			{
				TransformViewModel? transform = vm.Transforms[i];
				string boneName = name + "_" + i;
				BoneVisual3d bone = new BoneVisual3d(transform, this, boneName);
				this.Bones.Add(bone);
			}
		}

		private async Task WriteSkeletonThread()
		{
			while (Application.Current != null && this.Bones != null)
			{
				await Dispatch.MainThread();

				if (this.CurrentBone != null && PoseService.Instance.IsEnabled)
				{
					try
					{
						this.CurrentBone.WriteTransform(this);
					}
					catch (Exception ex)
					{
						Log.Write(Severity.Error, new Exception($"Failed to write bone transform: {this.CurrentBone.BoneName}", ex));
						this.ClearSelection();
					}
				}

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
