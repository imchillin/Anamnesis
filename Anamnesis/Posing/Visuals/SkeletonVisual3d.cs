// © Anamnesis.
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
	using Serilog;
	using XivToolsWpf;
	using AnQuaternion = Anamnesis.Memory.Quaternion;

	[AddINotifyPropertyChangedInterface]
	public class SkeletonVisual3d : ModelVisual3D, INotifyPropertyChanged
	{
		public List<BoneVisual3d> SelectedBones = new List<BoneVisual3d>();
		public HashSet<BoneVisual3d> HoverBones = new HashSet<BoneVisual3d>();

		private readonly QuaternionRotation3D rootRotation;

		public SkeletonVisual3d(ActorViewModel actor)
		{
			this.Actor = actor;
			////Task.Run(this.GenerateBones);
			Task.Run(this.WriteSkeletonThread);

			if (actor.ModelObject?.Transform != null)
				actor.ModelObject.Transform.PropertyChanged += this.OnTransformPropertyChanged;

			this.rootRotation = new QuaternionRotation3D();
			this.Transform = new RotateTransform3D(this.rootRotation);
			this.OnTransformPropertyChanged(null, null);
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
		public ActorViewModel Actor { get; private set; }
		public SkeletonFile? File { get; private set; }
		public int SelectedCount => this.SelectedBones.Count;
		public bool CanEditBone => this.SelectedBones.Count == 1;
		public bool HasSelection => this.SelectedBones.Count > 0;
		public bool HasHover => this.HoverBones.Count > 0;

		public bool FlipSides
		{
			get => SettingsService.Current.FlipPoseGuiSides;
			set => SettingsService.Current.FlipPoseGuiSides = value;
		}

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
		}

		public ObservableCollection<BoneVisual3d> Bones { get; private set; } = new ObservableCollection<BoneVisual3d>();

		public bool HasTail => this.Actor?.Customize?.Race == Customize.Races.Miqote
			|| this.Actor?.Customize?.Race == Customize.Races.AuRa
			|| this.Actor?.Customize?.Race == Customize.Races.Hrothgar;

		public bool IsCustomFace => this.IsMiqote || this.IsHrothgar;
		public bool IsMiqote => this.Actor?.Customize?.Race == Customize.Races.Miqote;
		public bool IsViera => this.Actor?.Customize?.Race == Customize.Races.Viera;
		public bool IsVieraEars01 => this.IsViera && this.Actor?.Customize?.TailEarsType <= 1;
		public bool IsVieraEars02 => this.IsViera && this.Actor?.Customize?.TailEarsType == 2;
		public bool IsVieraEars03 => this.IsViera && this.Actor?.Customize?.TailEarsType == 3;
		public bool IsVieraEars04 => this.IsViera && this.Actor?.Customize?.TailEarsType == 4;
		public bool IsHrothgar => this.Actor?.Customize?.Race == Customize.Races.Hrothgar;
		public bool HasTailOrEars => this.IsViera || this.HasTail;

		public AnQuaternion RootRotation
		{
			get
			{
				return this.Actor?.ModelObject?.Transform?.Rotation ?? AnQuaternion.Identity;
			}
		}

		private static ILogger Log => Serilog.Log.ForContext<SkeletonVisual3d>();

		public void Clear()
		{
			this.ClearSelection();
			this.HoverBones.Clear();
			this.Bones.Clear();
			this.Children.Clear();
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
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonVisual3d.CanEditBone)));
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
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonVisual3d.CanEditBone)));
		}

		public void ClearSelection()
		{
			this.SelectedBones.Clear();

			Application.Current?.Dispatcher.Invoke(() =>
			{
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonVisual3d.CurrentBone)));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonVisual3d.HasSelection)));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonVisual3d.SelectedCount)));
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonVisual3d.CanEditBone)));
			});
		}

		public void Hover(BoneVisual3d bone, bool hover, bool notify = true)
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

			if (notify)
			{
				this.NotifyHover();
			}
		}

		public void NotifyHover()
		{
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
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonVisual3d.CanEditBone)));
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

			foreach (BoneVisual3d bone in this.Bones)
			{
				if (bone.BoneName == name)
				{
					return bone;
				}

				if (bone.OriginalBoneName == name)
				{
					return bone;
				}
			}

			////Log.Information($"Optional bone not found: {name}");

			return null;
		}

		/// <summary>
		/// Returns true if the entire selection is head + face bones only.
		/// Hacky special check for the loading of expresisons (#365).
		/// </summary>
		public bool GetIsHeadSelection()
		{
			BoneVisual3d? head = this.GetBone("Head");

			if (head == null || !this.GetIsBoneSelected(head))
				return false;

			foreach (BoneVisual3d? bone in this.SelectedBones)
			{
				if (bone == head)
					continue;

				if (bone.HasParent(head))
					continue;

				return false;
			}

			return true;
		}

		/// <summary>
		/// Returns true if the entire selection is head + face bones only.
		/// Hacky special check for the loading of expresisons (#365).
		/// </summary>
		public void SelectHead()
		{
			this.ClearSelection();

			BoneVisual3d? headBone = this.GetBone("Head");
			if (headBone == null)
				return;

			List<BoneVisual3d> headBones = new List<BoneVisual3d>();

			headBones.Add(headBone);

			foreach (BoneVisual3d bone in this.Bones)
			{
				if (bone.OriginalBoneName.StartsWith("Head_"))
				{
					headBones.Add(bone);
				}
			}

			this.Select(headBones, SkeletonVisual3d.SelectMode.Add);
		}

		public void Reselect()
		{
			List<BoneVisual3d> selection = new List<BoneVisual3d>(this.SelectedBones);
			this.ClearSelection();
			this.Select(selection);
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

		public async Task GenerateBones(bool forceGenerateParenting = false)
		{
			this.Generating = true;

			this.ClearSelection();

			try
			{
				await Dispatch.MainThread();

				if (!GposeService.Instance.IsGpose)
				{
					this.Generating = false;
					return;
				}

				this.Bones.Clear();
				this.Children.Clear();

				if (this.Actor?.ModelObject?.Skeleton?.Skeleton == null)
					return;

				SkeletonViewModel skeletonVm = this.Actor.ModelObject.Skeleton.Skeleton;

				////TemplateSkeleton template = skeletonVm.GetTemplate(this.Actor);
				await this.Generate(skeletonVm, forceGenerateParenting);

				if (!GposeService.Instance.IsGpose)
				{
					this.Generating = false;
					return;
				}

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
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				this.Generating = false;
			}
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

		private async Task Generate(SkeletonViewModel memory, bool forceGenerateParenting = false)
		{
			this.File = memory.GetSkeletonFile(this.Actor);
			Log.Information($"Loaded skeleton file: {this.File?.Path}");

			bool autoSkeleton = false;

			if ((forceGenerateParenting || this.File == null || this.File.Parenting == null) && GposeService.Instance.IsGpose)
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

			if (this.File != null && this.File.BoneNames != null)
			{
				foreach (BoneVisual3d bone in this.Bones)
				{
					string? newName;
					if (this.File.BoneNames.TryGetValue(bone.BoneName, out newName))
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
					Log.Error(ex, "Failed to generate skeleton file.");
					return;
				}

				if (this.File == null)
				{
					this.File = new SkeletonFile();
					this.File.ModelTypes = new List<int>();
					this.File.ModelTypes.Add(this.Actor.ModelType);
					this.File.Race = this.Actor.Customize?.Race;
					this.File.Age = this.Actor.Customize?.Age;
				}

				this.File.IsGeneratedParenting = true;
				this.File.Parenting = new Dictionary<string, string>();
				foreach (BoneVisual3d bone in this.Bones)
				{
					if (bone.Parent == null)
						continue;

					this.File.Parenting.Add(bone.BoneName, bone.Parent.BoneName);
				}

				PoseService.SaveTemplate(this.File);
			}
			else if (this.File != null && this.File.Parenting != null)
			{
				// parenting from file
				foreach (BoneVisual3d bone in this.Bones)
				{
					string? parentBoneName;
					if (this.File.Parenting.TryGetValue(bone.BoneName, out parentBoneName) || this.File.Parenting.TryGetValue(bone.OriginalBoneName, out parentBoneName))
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
				TransformPtrViewModel? transform = vm.Transforms[i];
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
						Log.Error(ex, $"Failed to write bone transform: {this.CurrentBone.BoneName}");
						this.ClearSelection();
					}
				}

				// up to 60 times a second
				await Task.Delay(16);
			}
		}

		private async void OnTransformPropertyChanged(object? sender, PropertyChangedEventArgs? e)
		{
			await Dispatch.MainThread();

			if (Application.Current == null)
				return;

			this.rootRotation.Quaternion = this.RootRotation.ToMedia3DQuaternion();
		}
	}

	#pragma warning disable SA1201
	public interface IBone
	{
		BoneVisual3d? Visual { get; }
	}
}