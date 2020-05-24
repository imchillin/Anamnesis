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
	using ConceptMatrix;
	using ConceptMatrix.ThreeD;

	using CmQuaternion = ConceptMatrix.Quaternion;
	using CmTransform = ConceptMatrix.Transform;
	using CmVector = ConceptMatrix.Vector;

	public class SkeletonViewModel : INotifyPropertyChanged
	{
		public ModelVisual3D Root;

		private IMemory<Flag> skel1Mem;
		private IMemory<Flag> skel2Mem;
		private IMemory<Flag> skel3Mem;
		private IMemory<Flag> skel4Mem;
		private IMemory<Flag> skel5Mem;
		private IMemory<Flag> skel6Mem;

		private IMemory<Flag> phys1Mem;
		private IMemory<Flag> phys2Mem;
		private IMemory<Flag> phys3Mem;

		private IMemory<CmQuaternion> rootRotationMem;

		private Dictionary<string, Bone> bones;
		private Bone currentBone;
		private bool enabled;
		private bool freezePhysics;

		private IMemory<Appearance> appearanceMem;

		public SkeletonViewModel()
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				this.Root = new ModelVisual3D();
			});
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public bool IsEnabled
		{
			get
			{
				return this.enabled;
			}

			set
			{
				this.CurrentBone = null;
				this.enabled = value;

				if (this.enabled)
				{
					// rotations
					this.skel1Mem.Value = Flag.Enabled;
					this.skel2Mem.Value = Flag.Enabled;
					this.skel3Mem.Value = Flag.Enabled;

					// scale
					this.skel4Mem.Value = Flag.Enabled;
					this.skel6Mem.Value = Flag.Enabled;

					// Positions
					this.skel5Mem.Value = Flag.Enabled;

					// Poll changes thread
					new Thread(new ThreadStart(this.PollChanges)).Start();
				}
				else
				{
					// rotations
					this.skel1Mem.Value = Flag.Disabled;
					this.skel2Mem.Value = Flag.Disabled;
					this.skel3Mem.Value = Flag.Disabled;

					// scale
					this.skel4Mem.Value = Flag.Disabled;
					this.skel6Mem.Value = Flag.Disabled;

					// Positions
					this.skel5Mem.Value = Flag.Disabled;
				}

				this.FreezePhysics = value;
			}
		}

		public bool FreezePhysics
		{
			get
			{
				return this.freezePhysics;
			}

			set
			{
				this.freezePhysics = value;

				if (this.freezePhysics)
				{
					this.phys1Mem.Value = Flag.Enabled;
					this.phys2Mem.Value = Flag.Enabled;
					this.phys3Mem.Value = Flag.Enabled;
				}
				else
				{
					this.phys1Mem.Value = Flag.Disabled;
					this.phys2Mem.Value = Flag.Disabled;
					this.phys3Mem.Value = Flag.Disabled;
				}
			}
		}

		public bool FlipSides { get; set; } = false;
		public bool ParentingEnabled { get; set; } = true;

		public Bone CurrentBone
		{
			get
			{
				return this.currentBone;
			}

			set
			{
				if (!this.IsEnabled)
					return;

				// Ensure we have written any pending rotations before changing bone targets
				if (this.currentBone != null)
					this.currentBone.WriteTransform(this.Root);

				this.currentBone = value;
				this.currentBone?.ReadTransform();
			}
		}

		public Bone MouseOverBone { get; set; }

		public Appearance.Races Race
		{
			get
			{
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

		public async Task Initialize(Selection selection)
		{
			this.skel1Mem = Offsets.Main.Skeleton1Flag.GetMemory();
			this.skel2Mem = Offsets.Main.Skeleton2Flag.GetMemory();
			this.skel3Mem = Offsets.Main.Skeleton3Flag.GetMemory();
			this.skel4Mem = Offsets.Main.Skeleton4flag.GetMemory();
			this.skel5Mem = Offsets.Main.Skeleton5Flag.GetMemory();
			this.skel6Mem = Offsets.Main.Skeleton6Flag.GetMemory();
			this.phys1Mem = Offsets.Main.Physics1Flag.GetMemory();
			this.phys2Mem = Offsets.Main.Physics2Flag.GetMemory();
			this.phys3Mem = Offsets.Main.Physics3Flag.GetMemory();

			this.appearanceMem = selection.BaseAddress.GetMemory(Offsets.Main.ActorAppearance);
			this.rootRotationMem = selection.BaseAddress.GetMemory(Offsets.Main.Rotation);

			await this.GenerateBones(selection);

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
		}

		public void Clear()
		{
			this.IsEnabled = false;

			this.skel1Mem?.Dispose();
			this.skel2Mem?.Dispose();
			this.skel3Mem?.Dispose();
			this.skel4Mem?.Dispose();
			this.skel5Mem?.Dispose();
			this.skel6Mem?.Dispose();
			this.phys1Mem?.Dispose();
			this.phys2Mem?.Dispose();
			this.phys3Mem?.Dispose();

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
		private async Task GenerateBones(Selection selection)
		{
			if (this.bones != null)
			{
				foreach (Bone bone in this.bones.Values)
				{
					bone.Dispose();
				}
			}

			this.bones = new Dictionary<string, Bone>();

			if (selection == null)
				return;

			if (this.Race == 0)
				return;

			SkeletonService skeletonService = Services.Get<SkeletonService>();
			Dictionary<string, SkeletonService.Bone> boneDefs = await skeletonService.Load(this.Race);

			// Once to load all bones
			foreach ((string name, SkeletonService.Bone boneDef) in boneDefs)
			{
				if (this.bones.ContainsKey(name))
					throw new Exception("Duplicate bone: \"" + name + "\"");

				try
				{
					IMemory<CmTransform> transMem = selection.BaseAddress.GetMemory(boneDef.Offsets);
					transMem.Name = "Bone_" + name;
					this.bones[name] = new Bone(name, transMem, boneDef);
					this.Root.Children.Add(this.bones[name]);
				}
				catch (Exception ex)
				{
					Log.Write("Failed to create bone View Model for bone: " + name + " - " + ex.Message);
					////throw new Exception("Failed to create bone View Model for bone: " + name, ex);
				}
			}

			// Find all ExHair, ExMet, and ExTop bones, and disable any that are outside the bounds
			// of the current characters actual skeleton.
			// ex-  bones are numbered starting from 1 (there is no ExHair0!)
			byte exHairCount = selection.BaseAddress.GetValue(Offsets.Main.ExHairCount);
			byte exMetCount = selection.BaseAddress.GetValue(Offsets.Main.ExMetCount);
			byte exTopCount = selection.BaseAddress.GetValue(Offsets.Main.ExTopCount);

			foreach (string boneName in boneDefs.Keys)
			{
				if (!this.bones.ContainsKey(boneName))
					continue;

				if (boneName.StartsWith("ExHair"))
				{
					byte num = byte.Parse(boneName.Replace("ExHair", string.Empty));
					if (num > exHairCount)
					{
						this.bones[boneName].IsEnabled = false;
					}
				}
				else if (boneName.StartsWith("ExMet"))
				{
					byte num = byte.Parse(boneName.Replace("ExMet", string.Empty));
					if (num > exMetCount)
					{
						this.bones[boneName].IsEnabled = false;
					}
				}
				else if (boneName.StartsWith("ExTop"))
				{
					byte num = byte.Parse(boneName.Replace("ExTop", string.Empty));
					if (num > exTopCount)
					{
						this.bones[boneName].IsEnabled = false;
					}
				}
				else if (boneName == "TailE")
				{
					this.bones[boneName].IsEnabled = false;
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

			foreach (Bone gizmo in this.bones.Values)
			{
				gizmo.ReadTransform();
			}

			/*foreach (BoneGizmo gizmo in this.gizmoLookup.Values)
			{
				gizmo.WriteTransform(this.root);
			}*/
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
			while (this.IsEnabled && Application.Current != null)
			{
				Thread.Sleep(32);

				if (!this.IsEnabled)
					continue;

				if (this.CurrentBone == null)
					continue;

				if (Application.Current == null)
					return;

				Application.Current.Dispatcher.Invoke(() =>
				{
					if (this.CurrentBone == null)
						return;

					this.CurrentBone.WriteTransform(this.Root);
				});
			}

			this.IsEnabled = false;
		}
	}
}
