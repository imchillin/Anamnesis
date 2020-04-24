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
	using ConceptMatrix;
	using ConceptMatrix.ThreeD;

	using Vector = ConceptMatrix.Vector;

	public class SkeletonViewModel : INotifyPropertyChanged
	{
		private static IInjectionService injection;

		private IMemory<Flag> skel1Mem;
		private IMemory<Flag> skel2Mem;
		private IMemory<Flag> skel3Mem;
		private IMemory<Flag> phys1Mem;
		private IMemory<Flag> phys2Mem;

		private Dictionary<string, Bone> bones;
		private Bone currentBone;
		private bool enabled;

		private IMemory<Appearance> appearance;

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
					this.skel1Mem.Value = Flag.Enabled;
					this.skel2Mem.Value = Flag.Enabled;
					this.skel3Mem.Value = Flag.Enabled;
					this.phys1Mem.Value = Flag.Enabled;
					this.phys2Mem.Value = Flag.Enabled;

					// Poll changes thread
					new Thread(new ThreadStart(this.PollChanges)).Start();

					// Watch camera thread
					new Thread(new ThreadStart(this.WatchCamera)).Start();
				}
				else
				{
					this.skel1Mem.Value = Flag.Disabled;
					this.skel2Mem.Value = Flag.Disabled;
					this.skel3Mem.Value = Flag.Disabled;
					this.phys1Mem.Value = Flag.Disabled;
					this.phys2Mem.Value = Flag.Disabled;
				}
			}
		}

		public bool FlipSides
		{
			get;
			set;
		}

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
					this.currentBone.ApplyTransform();

				this.currentBone = value;
			}
		}

		public Bone MouseOverBone { get; set; }

		public Quaternion CameraRotation
		{
			get;
			set;
		}

		public Appearance.Races Race
		{
			get
			{
				return this.appearance.Value.Race;
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
				return this.IsViera && this.appearance.Value.TailEarsType <= 1;
			}
		}

		public bool IsVieraEars02
		{
			get
			{
				return this.IsViera && this.appearance.Value.TailEarsType == 2;
			}
		}

		public bool IsVieraEars03
		{
			get
			{
				return this.IsViera && this.appearance.Value.TailEarsType == 3;
			}
		}

		public bool IsVieraEars04
		{
			get
			{
				return this.IsViera && this.appearance.Value.TailEarsType == 4;
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
			injection = Services.Get<IInjectionService>();

			this.skel1Mem = Offsets.Main.Skeleton1Flag.GetMemory();
			this.skel2Mem = Offsets.Main.Skeleton2Flag.GetMemory();
			this.skel3Mem = Offsets.Main.Skeleton3Flag.GetMemory();
			this.phys1Mem = Offsets.Main.Physics1Flag.GetMemory();
			this.phys2Mem = Offsets.Main.Physics2Flag.GetMemory();

			this.appearance = selection.BaseAddress.GetMemory(Offsets.Main.ActorAppearance);

			await this.GenerateBones(selection);
		}

		public void Clear()
		{
			this.skel1Mem?.Dispose();
			this.skel2Mem?.Dispose();
			this.skel3Mem?.Dispose();
			this.phys1Mem?.Dispose();
			this.phys2Mem?.Dispose();

			this.appearance?.Dispose();

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
					IMemory<Transform> transMem = selection.BaseAddress.GetMemory(boneDef.Offsets);
					this.bones[name] = new Bone(name, transMem);
				}
				catch (Exception ex)
				{
					throw new Exception("Failed to create bone View Model for bone: " + name, ex);
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
		}

		private void ParentBone(string parentName, string childName)
		{
			Bone parent = this.GetBone(parentName);
			Bone child = this.GetBone(childName);

			if (parent.Children.Contains(child) || child.Parent == parent)
			{
				Console.WriteLine("Duplicate parenting: " + parentName + " - " + childName);
				return;
			}

			if (child.Parent != null)
				throw new Exception("Attempt to parent bone: " + childName + " to multiple parents: " + parentName + " and " + this.bones[childName].Parent.BoneName);

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

				this.CurrentBone.ApplyTransform();
			}

			this.IsEnabled = false;
		}

		private void WatchCamera()
		{
			IMemory<float> camX = Offsets.Main.CameraOffset.GetMemory(Offsets.Main.CameraAngleX);
			IMemory<float> camY = Offsets.Main.CameraOffset.GetMemory(Offsets.Main.CameraAngleY);
			IMemory<float> camZ = Offsets.Main.CameraOffset.GetMemory(Offsets.Main.CameraRotation);

			while (this.IsEnabled && Application.Current != null)
			{
				Vector camEuler = default;

				// It's weird that these would be in radians. maybe the memory is actually a quaternion?
				// TODO: investigate.
				camEuler.Y = (float)MathUtils.RadiansToDegrees((double)camX.Value);
				camEuler.Z = (float)-MathUtils.RadiansToDegrees((double)camY.Value);
				camEuler.X = (float)MathUtils.RadiansToDegrees((double)camZ.Value);

				try
				{
					Application.Current.Dispatcher.Invoke(() =>
					{
						this.CameraRotation = Quaternion.FromEuler(camEuler);
					});
				}
				catch (Exception)
				{
				}

				Thread.Sleep(32);
			}
		}
	}
}
