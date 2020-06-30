// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Reflection;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Media.Media3D;
	using Anamnesis;
	using Anamnesis.Offsets;
	using ConceptMatrix;
	using ConceptMatrix.ThreeD;

	using CmQuaternion = Anamnesis.Quaternion;
	using CmTransform = Anamnesis.Transform;
	using CmVector = Anamnesis.Vector;

	public class SkeletonViewModel : INotifyPropertyChanged
	{
		public ModelVisual3D Root;

		private Actor actor;
		private IMemory<CmQuaternion> rootRotationMem;
		private IMemory<bool> animatingMem;

		private Dictionary<string, Bone> bones;
		private Bone currentBone;

		private IMemory<Appearance> appearanceMem;

		public SkeletonViewModel()
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				this.Root = new ModelVisual3D();
			});

			PoseService poseService = Services.Get<PoseService>();
			poseService.EnabledChanged += this.OnPoseServiceEnabledChanged;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public bool FlipSides { get; set; } = false;
		public bool ParentingEnabled { get; set; } = true;
		public bool CanPose { get; set; }

		public Bone CurrentBone
		{
			get
			{
				return this.currentBone;
			}

			set
			{
				if (Application.Current == null)
					return;

				// Ensure we have written any pending rotations before changing bone targets
				if (this.currentBone != null)
				{
					Application.Current.Dispatcher.Invoke(() =>
					{
						this.currentBone.WriteTransform(this.Root);
					});
				}

				this.currentBone = value;

				if (this.currentBone != null)
				{
					////this.currentBone.ReadTransform();
					new Thread(new ThreadStart(this.PollChanges)).Start();
				}
			}
		}

		public Bone MouseOverBone { get; set; }

		public Appearance.Races Race
		{
			get
			{
				if (this.appearanceMem == null)
					return Appearance.Races.Hyur;

				return this.appearanceMem.Value.Race;
			}
		}

		public bool HasTail
		{
			get
			{
				return this.Race == Appearance.Races.Miqote || this.Race == Appearance.Races.AuRa || this.Race == Appearance.Races.Hrothgar;
			}
		}

		public bool IsViera
		{
			get
			{
				return this.Race == Appearance.Races.Viera;
			}
		}

		public bool IsVieraEars01
		{
			get
			{
				return this.IsViera && this.appearanceMem.Value.TailEarsType <= 1;
			}
		}

		public bool IsVieraEars02
		{
			get
			{
				return this.IsViera && this.appearanceMem.Value.TailEarsType == 2;
			}
		}

		public bool IsVieraEars03
		{
			get
			{
				return this.IsViera && this.appearanceMem.Value.TailEarsType == 3;
			}
		}

		public bool IsVieraEars04
		{
			get
			{
				return this.IsViera && this.appearanceMem.Value.TailEarsType == 4;
			}
		}

		public bool IsHrothgar
		{
			get
			{
				return this.Race == Appearance.Races.Hrothgar;
			}
		}

		public bool HasTailOrEars
		{
			get
			{
				return this.IsViera || this.HasTail;
			}
		}

		public IEnumerable<Bone> Bones
		{
			get
			{
				if (this.bones == null)
					return null;

				return this.bones.Values;
			}
		}

		public CmQuaternion RootRotation
		{
			get
			{
				return this.rootRotationMem.Value;
			}
		}

		public static string GetBoneName(string name, bool flip)
		{
			if (flip)
			{
				// flip left and right side bones
				if (name.Contains("Left"))
					return name.Replace("Left", "Right");

				if (name.Contains("Right"))
					return name.Replace("Right", "Left");
			}

			return name;
		}

		public async Task Initialize(Actor actor)
		{
			if (this.actor == actor)
				return;

			this.actor = actor;

			this.Clear();

			this.appearanceMem = actor.GetMemory(Offsets.Main.ActorAppearance);
			this.rootRotationMem = actor.GetMemory(Offsets.Main.Rotation);
			this.rootRotationMem.ValueChanged += this.RootRotationMem_ValueChanged;
			this.animatingMem = actor.GetMemory(Offsets.Main.Animating);
			this.animatingMem.ValueChanged += this.OnAnimatingChanged;

			await Application.Current.Dispatcher.InvokeAsync(async () =>
			{
				this.CanPose = !this.animatingMem.Value;
				await this.GenerateBones(actor);
			});

			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonViewModel.Bones)));
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonViewModel.HasTail)));
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonViewModel.HasTailOrEars)));
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonViewModel.IsHrothgar)));
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonViewModel.IsViera)));
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonViewModel.IsVieraEars01)));
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonViewModel.IsVieraEars02)));
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonViewModel.IsVieraEars03)));
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonViewModel.IsVieraEars04)));
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkeletonViewModel.Race)));

			await Application.Current.Dispatcher.InvokeAsync(this.RefreshBones);

			this.RootRotationMem_ValueChanged(null, null);
		}

		public void RefreshBones()
		{
			foreach (Bone bone in this.bones.Values)
			{
				bone.ReadTransform();
			}

			// Since we overwrite teh games skeleton view, we need to set the customization scaling.
			// we should move this somewhere else if we add support for non player actors.
			Appearance appearance = this.actor.GetValue(Offsets.Main.ActorAppearance);

			if (this.bones.ContainsKey("BreastLeft") && this.bones.ContainsKey("BreastRight"))
			{
				float bustScale = ((appearance.Bust - 50) / 50) * 0.1f;
				this.bones["BreastLeft"].Scale += new CmVector(bustScale, bustScale, bustScale);
				this.bones["BreastRight"].Scale += new CmVector(bustScale, bustScale, bustScale);
			}

			if (appearance.Race == Appearance.Races.Viera)
			{
				float earScale = ((100 - appearance.EarMuscleTailSize) / 100.0f) * 0.2f;
				CmVector earScaleV = new CmVector(earScale, earScale, earScale);
				this.bones["Ear01ALeft"].Scale -= earScaleV;
				this.bones["Ear01ARight"].Scale -= earScaleV;
				this.bones["Ear02ALeft"].Scale -= earScaleV;
				this.bones["Ear02ARight"].Scale -= earScaleV;
				this.bones["Ear03ALeft"].Scale -= earScaleV;
				this.bones["Ear03ARight"].Scale -= earScaleV;
				this.bones["Ear04ALeft"].Scale -= earScaleV;
				this.bones["Ear04ARight"].Scale -= earScaleV;
			}

			if (appearance.Race == Appearance.Races.Miqote)
			{
				float tailScale = ((100 - appearance.EarMuscleTailSize) / 100.0f) * 0.15f;
				CmVector tailScaleV = new CmVector(tailScale, tailScale, tailScale);
				this.bones["TailA"].Scale -= tailScaleV;
				this.bones["TailB"].Scale -= tailScaleV;
			}

			foreach (Bone bone in this.bones.Values)
			{
				bone.WriteTransform(this.Root);
			}
		}

		public void Clear()
		{
			this.appearanceMem?.Dispose();
			this.rootRotationMem?.Dispose();

			this.bones?.Clear();
		}

		public bool GetIsBoneSelected(Bone bone)
		{
			return this.CurrentBone == bone;
		}

		public bool GetIsBoneParentsSelected(Bone bone)
		{
			if (this.GetIsBoneSelected(bone))
				return true;

			if (bone.Parent != null)
			{
				return this.GetIsBoneParentsSelected(bone.Parent);
			}

			return false;
		}

		public bool GetIsBoneHovered(Bone bone)
		{
			return this.MouseOverBone == bone;
		}

		public bool GetIsBoneParentsHovered(Bone bone)
		{
			if (this.GetIsBoneHovered(bone))
				return true;

			if (bone.Parent != null)
			{
				return this.GetIsBoneParentsHovered(bone.Parent);
			}

			return false;
		}

		public Bone GetBone(string name)
		{
			if (this.bones == null)
				throw new Exception("Bones not generated");

			if (!this.bones.ContainsKey(name))
				return null;

			return this.bones[name];
		}

		// gets all bones defined in BonesOffsets.
		private async Task GenerateBones(Actor actor)
		{
			if (this.bones != null)
			{
				foreach (Bone bone in this.bones.Values)
				{
					bone.Dispose();
				}
			}

			this.Root.Children.Clear();

			this.bones = new Dictionary<string, Bone>();

			if (actor == null)
				return;

			if (this.Race == 0)
				return;

			SkeletonService skeletonService = Services.Get<SkeletonService>();
			Dictionary<string, SkeletonService.Bone> boneDefs = await skeletonService.Load(this.Race);

			// Find all ExHair, ExMet, and ExTop bones, and disable any that are outside the bounds
			// of the current characters actual skeleton.
			// ex-  bones are numbered starting from 1 (there is no ExHair0!)
			byte exHairCount = actor.GetValue(Offsets.Main.ExHairCount);
			byte exMetCount = actor.GetValue(Offsets.Main.ExMetCount);
			byte exTopCount = actor.GetValue(Offsets.Main.ExTopCount);

			// Once to load all bones
			foreach ((string name, SkeletonService.Bone boneDef) in boneDefs)
			{
				if (this.bones.ContainsKey(name))
					throw new Exception("Duplicate bone: \"" + name + "\"");

				if (name.StartsWith("ExHair"))
				{
					byte num = byte.Parse(name.Replace("ExHair", string.Empty));
					if (num >= exHairCount)
					{
						continue;
					}
				}
				else if (name.StartsWith("ExMet"))
				{
					byte num = byte.Parse(name.Replace("ExMet", string.Empty));
					if (num >= exMetCount)
					{
						continue;
					}
				}
				else if (name.StartsWith("ExTop"))
				{
					byte num = byte.Parse(name.Replace("ExTop", string.Empty));
					if (num >= exTopCount)
					{
						continue;
					}
				}
				else if (name == "TailE")
				{
					continue;
				}

				try
				{
					IMemory<CmTransform> transMem = actor.GetMemory(boneDef.Offsets);
					this.bones[name] = new Bone(this, name, transMem, boneDef);
					this.Root.Children.Add(this.bones[name]);
				}
				catch (Exception ex)
				{
					Log.Write("Failed to create bone View Model for bone: " + name + " - " + ex.Message);
					////throw new Exception("Failed to create bone View Model for bone: " + name, ex);
				}
			}

			// Again to set parenting
			foreach ((string name, SkeletonService.Bone boneDef) in boneDefs)
			{
				if (boneDef.Parent != null)
				{
					this.ParentBone(boneDef.Parent, name);
				}
			}

			this.GetBone("Root").IsEnabled = false;

			foreach (Bone bone in this.bones.Values)
			{
				bone.ReadTransform();
			}
		}

		private void ParentBone(string parentName, string childName)
		{
			Bone parent = this.GetBone(parentName);
			Bone child = this.GetBone(childName);

			if (parent == null)
				return;

			if (child == null)
				return;

			if (parent.Children.Contains(child) || child.Parent == parent)
			{
				Console.WriteLine("Duplicate parenting: " + parentName + " - " + childName);
				return;
			}

			if (child.Parent != null)
				throw new Exception("Attempt to parent bone: " + childName + " to multiple parents: " + parentName + " and " + this.bones[childName].Parent.BoneName);

			if (this.Root.Children.Contains(child))
				this.Root.Children.Remove(child);

			parent.Children.Add(child);
			child.Parent = parent;
		}

		private void PollChanges()
		{
			Bone bone = this.CurrentBone;

			try
			{
				while (bone == this.currentBone && Application.Current != null)
				{
					Thread.Sleep(32);

					if (this.CurrentBone == null)
						continue;

					if (Application.Current == null)
						continue;

					Application.Current.Dispatcher.Invoke(() =>
					{
						if (this.CurrentBone == null)
							return;

						this.CurrentBone.WriteTransform(this.Root);
					});
				}
			}
			catch (TaskCanceledException)
			{
			}
		}

		private void RootRotationMem_ValueChanged(object sender, object value)
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				System.Windows.Media.Media3D.Quaternion rot = this.RootRotation.ToMedia3DQuaternion();
				RotateTransform3D trans = new RotateTransform3D(new QuaternionRotation3D(rot));
				this.Root.Transform = trans;
			});
		}

		[PropertyChanged.SuppressPropertyChangedWarnings]
		private void OnPoseServiceEnabledChanged(bool value)
		{
			if (value)
			{
				this.RefreshBones();
			}
		}

		[PropertyChanged.SuppressPropertyChangedWarnings]
		private void OnAnimatingChanged(object sender, object value)
		{
			this.CanPose = !this.animatingMem.Value;
		}
	}
}
