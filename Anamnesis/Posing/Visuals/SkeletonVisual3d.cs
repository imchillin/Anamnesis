// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.PoseModule
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Input;
	using System.Windows.Media.Media3D;
	using Anamnesis.Memory;
	using Anamnesis.Posing;
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

		public SkeletonVisual3d(ActorMemory actor)
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

		public bool LinkEyes { get; set; } = true;
		public ActorMemory Actor { get; private set; }
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

		public bool HasTail => this.Actor?.Customize?.Race == ActorCustomizeMemory.Races.Miqote
			|| this.Actor?.Customize?.Race == ActorCustomizeMemory.Races.AuRa
			|| this.Actor?.Customize?.Race == ActorCustomizeMemory.Races.Hrothgar;

		public bool IsCustomFace => this.IsMiqote || this.IsHrothgar;
		public bool IsMiqote => this.Actor?.Customize?.Race == ActorCustomizeMemory.Races.Miqote;
		public bool IsViera => this.Actor?.Customize?.Race == ActorCustomizeMemory.Races.Viera;
		public bool IsVieraEars01 => this.IsViera && this.Actor?.Customize?.TailEarsType <= 1;
		public bool IsVieraEars02 => this.IsViera && this.Actor?.Customize?.TailEarsType == 2;
		public bool IsVieraEars03 => this.IsViera && this.Actor?.Customize?.TailEarsType == 3;
		public bool IsVieraEars04 => this.IsViera && this.Actor?.Customize?.TailEarsType == 4;
		public bool IsHrothgar => this.Actor?.Customize?.Race == ActorCustomizeMemory.Races.Hrothgar;
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
			if (this.Actor?.ModelObject?.Skeleton == null)
				return null;

			// only show actors that have atleast one partial skeleton
			if (this.Actor.ModelObject.Skeleton.Count <= 0)
				return null;

			string? modernName = LegacyBoneNameConverter.GetModernName(name);
			if (modernName != null)
				name = modernName;

			BoneVisual3d? best = null;
			foreach (BoneVisual3d bone in this.Bones)
			{
				if (bone.BoneName == name)
				{
					if (best != null)
						Log.Warning("Multiple bones with the same name!");

					best = bone;
				}
			}

			////Log.Information($"Optional bone not found: {name}");

			return best;
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

			throw new NotImplementedException();

			/*BoneVisual3d? headBone = this.GetBone("Head");
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

			this.Select(headBones, SkeletonVisual3d.SelectMode.Add);*/
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
				if (this.GetIsBoneSelected(bone))
					continue;

				bone.ViewModel.Tick();
				bone.ReadTransform();
			}
		}

		public async Task GenerateBones()
		{
			await Dispatch.MainThread();

			this.ClearSelection();

			try
			{
				await Dispatch.MainThread();

				if (!GposeService.Instance.IsGpose)
					return;

				this.Bones.Clear();
				this.Children.Clear();

				if (this.Actor?.ModelObject?.Skeleton == null)
					return;

				SkeletonMemory skeletonVm = this.Actor.ModelObject.Skeleton;

				////TemplateSkeleton template = skeletonVm.GetTemplate(this.Actor);
				this.Generate(skeletonVm);

				if (!GposeService.Instance.IsGpose)
					return;

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
		}

		private void Generate(SkeletonMemory memory)
		{
			// Get all bones
			this.Bones.Clear();

			for (int partialSkeletonIndex = 0; partialSkeletonIndex < memory.Count; partialSkeletonIndex++)
			{
				PartialSkeletonMemory partialSkeleton = memory[partialSkeletonIndex];

				HkaPoseMemory? bestHkaPose = partialSkeleton.Pose1;

				if (bestHkaPose == null || bestHkaPose.Skeleton?.Bones == null || bestHkaPose.Skeleton?.ParentIndices == null || bestHkaPose.Transforms == null)
				{
					Log.Warning("Failed to find best HkaSkeleton for partial skeleton");
					continue;
				}

				int count = bestHkaPose.Transforms.Count;

				List<BoneVisual3d> bones = new List<BoneVisual3d>();

				// Load all bones first
				for (int boneIndex = 0; boneIndex < count; boneIndex++)
				{
					string name = bestHkaPose.Skeleton.Bones[boneIndex].Name.ToString();
					TransformMemory? transform = bestHkaPose.Transforms[boneIndex];
					bones.Add(new BoneVisual3d(transform, this, name));
				}

				CheatBoneNames(partialSkeletonIndex, bones);

				// Set parents now all the bones are loaded
				for (int boneIndex = 0; boneIndex < count; boneIndex++)
				{
					int parentIndex = bestHkaPose.Skeleton.ParentIndices[boneIndex];

					if (parentIndex < 0)
					{
						// this bone has no parent, is root.
						this.Children.Add(bones[boneIndex]);
					}
					else
					{
						bones[boneIndex].Parent = bones[parentIndex];
					}
				}

				// push the results into the bone list
				foreach (BoneVisual3d bone in bones)
				{
					this.Bones.Add(bone);
				}
			}
		}

#pragma warning disable

		private static Dictionary<string, string> BodyBoneNames = new Dictionary<string, string>
		{
			{ "Body_0", "Root" },
			{ "Body_1", "Abdomen" },
			{ "Body_2", "Throw" },
			{ "Body_3", "Waist" },
			{ "Body_4", "SpineA" },
			{ "Body_5", "LegLeft" },
			{ "Body_6", "LegRight" },
			{ "Body_7", "HolsterLeft" },
			{ "Body_8", "HolsterRight" },
			{ "Body_9", "SheatheLeft" },
			{ "Body_10", "SheatheRight" },
			{ "Body_11", "SpineB" },
			{ "Body_12", "ClothBackALeft" },
			{ "Body_13", "ClothBackARight" },
			{ "Body_14", "ClothFrontALeft" },
			{ "Body_15", "ClothFrontARight" },
			{ "Body_16", "ClothSideALeft" },
			{ "Body_17", "ClothSideARight" },
			{ "Body_18", "KneeLeft" },
			{ "Body_19", "KneeRight" },
			{ "Body_20", "BreastLeft" },
			{ "Body_21", "BreastRight" },
			{ "Body_22", "SpineC" },
			{ "Body_23", "ClothBackBLeft" },
			{ "Body_24", "ClothBackBRight" },
			{ "Body_25", "ClothFrontBLeft" },
			{ "Body_26", "ClothFrontBRight" },
			{ "Body_27", "ClothSideBLeft" },
			{ "Body_28", "ClothSideBRight" },
			{ "Body_29", "CalfLeft" },
			{ "Body_30", "CalfRight" },
			{ "Body_31", "ScabbardLeft" },
			{ "Body_32", "ScabbardRight" },
			{ "Body_33", "Neck" },
			{ "Body_34", "ClavicleLeft" },
			{ "Body_35", "ClavicleRight" },
			{ "Body_36", "ClothBackCLeft" },
			{ "Body_37", "ClothBackCRight" },
			{ "Body_38", "ClothFrontCLeft" },
			{ "Body_39", "ClothFrontCRight" },
			{ "Body_40", "ClothSideCLeft" },
			{ "Body_41", "ClothSideCRight" },
			{ "Body_42", "PoleynLeft" },
			{ "Body_43", "PoleynRight" },
			{ "Body_44", "FootLeft" },
			{ "Body_45", "FootRight" },
			{ "Body_46", "Head" },
			{ "Body_47", "ArmLeft" },
			{ "Body_48", "ArmRight" },
			{ "Body_49", "PauldronLeft" },
			{ "Body_50", "PauldronRight" },
			{ "Body_51", "Unknown00" },
			{ "Body_52", "ToesLeft" },
			{ "Body_53", "ToesRight" },
			{ "Body_54", "HairA" },
			{ "Body_55", "HairFrontLeft" },
			{ "Body_56", "HairFrontRight" },
			{ "Body_57", "EarLeft" },
			{ "Body_58", "EarRight" },
			{ "Body_59", "ForearmLeft" },
			{ "Body_60", "ForearmRight" },
			{ "Body_61", "ShoulderLeft" },
			{ "Body_62", "ShoulderRight" },
			{ "Body_63", "HairB" },
			{ "Body_64", "HandLeft" },
			{ "Body_65", "HandRight" },
			{ "Body_66", "ShieldLeft" },
			{ "Body_67", "ShieldRight" },
			{ "Body_68", "EarringALeft" },
			{ "Body_69", "EarringARight" },
			{ "Body_70", "ElbowLeft" },
			{ "Body_71", "ElbowRight" },
			{ "Body_72", "CouterLeft" },
			{ "Body_73", "CouterRight" },
			{ "Body_74", "WristLeft" },
			{ "Body_75", "WristRight" },
			{ "Body_76", "IndexALeft" },
			{ "Body_77", "IndexARight" },
			{ "Body_78", "PinkyALeft" },
			{ "Body_79", "PinkyARight" },
			{ "Body_80", "RingALeft" },
			{ "Body_81", "RingARight" },
			{ "Body_82", "MiddleALeft" },
			{ "Body_83", "MiddleARight" },
			{ "Body_84", "ThumbALeft" },
			{ "Body_85", "ThumbARight" },
			{ "Body_86", "WeaponLeft" },
			{ "Body_87", "WeaponRight" },
			{ "Body_88", "EarringBLeft" },
			{ "Body_89", "EarringBRight" },
			{ "Body_90", "IndexBLeft" },
			{ "Body_91", "IndexBRight" },
			{ "Body_92", "PinkyBLeft" },
			{ "Body_93", "PinkyBRight" },
			{ "Body_94", "RingBLeft" },
			{ "Body_95", "RingBRight" },
			{ "Body_96", "MiddleBLeft" },
			{ "Body_97", "MiddleBRight" },
			{ "Body_98", "ThumbBLeft" },
			{ "Body_99", "ThumbBRight" },
			{ "Body_100", "TailA" },
			{ "Body_101", "TailB" },
			{ "Body_102", "TailC" },
			{ "Body_103", "TailD" },
			{ "Body_104", "TailE" },
			{ "Head_0", "RootHead" },
			{ "Head_1", "Jaw" },
			{ "Head_2", "EyelidLowerLeft" },
			{ "Head_3", "EyelidLowerRight" },
			{ "Head_4", "EyeLeft" },
			{ "Head_5", "EyeRight" },
			{ "Head_6", "Nose" },
			{ "Head_7", "CheekLeft" },
			{ "Head_8", "CheekRight" },
			{ "Head_9", "LipsLeft" },
			{ "Head_10", "LipsRight" },
			{ "Head_11", "EyebrowLeft" },
			{ "Head_12", "EyebrowRight" },
			{ "Head_13", "Bridge" },
			{ "Head_14", "BrowLeft" },
			{ "Head_15", "BrowRight" },
			{ "Head_16", "LipUpperA" },
			{ "Head_17", "EyelidUpperLeft" },
			{ "Head_18", "EyelidUpperRight" },
			{ "Head_19", "LipLowerA" },
			{ "Head_20", "LipUpperB" },
			{ "Head_21", "LipLowerB" },
		};

		private static void CheatBoneNames(int partIndex, List<BoneVisual3d> bones)
		{
			string cat = partIndex switch
			{
				0 => "Body",
				1 => "Head",
				2 => "Hair",
				3 => "Met",
				4 => "Top",
			};

			for (int i = 0; i < bones.Count; i++)
			{
				LogNewBoneLoc(cat, i, bones);
			}
		}

		private static void LogNewBoneLoc(string cat, int i, List<BoneVisual3d> bones)
		{
			string oldKeyName = $"{cat}_{i}";
			string newKeyName = bones[i].BoneName;
			string locName = oldKeyName;

			if (BodyBoneNames.ContainsKey(oldKeyName))
				locName = BodyBoneNames[oldKeyName];

			string oldLocValue = LocalizationService.GetString($"Pose_{locName}", true);
			string newLocValue = LocalizationService.GetString($"Pose_{newKeyName}", true);

			if (string.IsNullOrEmpty(oldLocValue))
				return;

			if (!string.IsNullOrEmpty(newLocValue))
			{
				if (oldLocValue != newLocValue)
				{
					Log.Warning($"{newLocValue} != {oldLocValue}");
				}
			}
			else
			{
				Log.Information($"\"Pose_{newKeyName}\": \"{oldLocValue}\",");
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