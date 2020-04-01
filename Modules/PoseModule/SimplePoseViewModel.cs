// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Globalization;
	using System.Reflection;
	using System.Threading;
	using System.Windows;
	using System.Windows.Media.Media3D;
	using ConceptMatrix;
	using ConceptMatrix.Services;
	using ConceptMatrix.ThreeD;
	using PropertyChanged;

	public class SimplePoseViewModel : INotifyPropertyChanged
	{
		private static IInjectionService injection;

		private Dictionary<string, Bone> bones;
		private Bone currentBone;
		private bool enabled;

		private IMemory<int> race;
		private IMemory<int> tailType;

		public SimplePoseViewModel(Selection selection)
		{
			injection = Module.Services.Get<IInjectionService>();

			this.race = injection.GetMemory<int>(selection.BaseAddress, injection.Offsets.Character.Race);
			this.tailType = injection.GetMemory<int>(selection.BaseAddress, injection.Offsets.Character.TailType);

			this.GenerateBones();
		}

		#pragma warning disable CS0067
		public event PropertyChangedEventHandler PropertyChanged;
		#pragma warning restore

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

				IMemory<byte[]> skeleton = injection.GetMemory<byte[]>(Offsets.BaseAddresses.Skeleton);
				IMemory<byte[]> skeleton2 = injection.GetMemory<byte[]>(Offsets.BaseAddresses.Skeleton2);
				IMemory<byte[]> skeleton3 = injection.GetMemory<byte[]>(Offsets.BaseAddresses.Skeleton3);
				IMemory<byte[]> physics = injection.GetMemory<byte[]>(Offsets.BaseAddresses.Physics);
				IMemory<byte[]> physics2 = injection.GetMemory<byte[]>(Offsets.BaseAddresses.Physics2);

				if (this.enabled)
				{
					skeleton.Set(new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
					skeleton2.Set(new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
					skeleton3.Set(new byte[] { 0x90, 0x90, 0x90, 0x90 });
					physics.Set(new byte[] { 0x90, 0x90, 0x90, 0x90 });
					physics2.Set(new byte[] { 0x90, 0x90, 0x90 });

					// Poll changes thread
					new Thread(new ThreadStart(this.PollChanges)).Start();

					// Watch camera thread
					new Thread(new ThreadStart(this.WatchCamera)).Start();
				}
				else
				{
					skeleton.Set(new byte[] { 0x41, 0x0F, 0x29, 0x5C, 0x12, 0x10 });
					skeleton2.Set(new byte[] { 0x43, 0x0F, 0x29, 0x5C, 0x18, 0x10 });
					skeleton3.Set(new byte[] { 0x0F, 0x29, 0x5E, 0x10 });
					physics.Set(new byte[] { 0x0F, 0x29, 0x48, 0x10 });
					physics2.Set(new byte[] { 0x0F, 0x29, 0x00 });
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
					this.currentBone.SetRotation();

				// Ensure we have the correct rotation for a bone before we edit it
				if (value != null)
					value.GetRotation();

				this.currentBone = value;
			}
		}

		public Bone MouseOverBone { get; set; }

		public Quaternion CameraRotation
		{
			get;
			set;
		}

		public bool HasTail
		{
			get
			{
				return this.race.Get() == 4 || this.race.Get() == 6 || this.race.Get() == 7;
			}
		}

		public bool IsViera
		{
			get
			{
				return this.race.Get() == 8;
			}
		}

		public bool IsVieraEars01
		{
			get
			{
				return this.race.Get() == 8 && this.tailType.Get() <= 1;
			}
		}

		public bool IsVieraEars02
		{
			get
			{
				return this.race.Get() == 8 && this.tailType.Get() == 2;
			}
		}

		public bool IsVieraEars03
		{
			get
			{
				return this.race.Get() == 8 && this.tailType.Get() == 3;
			}
		}

		public bool IsVieraEars04
		{
			get
			{
				return this.race.Get() == 8 && this.tailType.Get() == 4;
			}
		}

		public bool IsHrothgar
		{
			get
			{
				return this.race.Get() == 7;
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
				throw new Exception("Unable to locate bone: \"" + name + "\"");

			return this.bones[name];
		}

		// gets all bones defined in BonesOffsets.
		private void GenerateBones()
		{
			this.bones = new Dictionary<string, Bone>();
			Offsets.Character c = injection.Offsets.Character;
			PropertyInfo[] boneProperties = typeof(Offsets.Bones).GetProperties();
			foreach (PropertyInfo boneProperty in boneProperties)
			{
				if (!boneProperty.Name.Contains("_X"))
					continue;

				string boneName = boneProperty.Name.Replace("_X", string.Empty);

				if (this.bones.ContainsKey(boneName))
					throw new Exception("Duplicate bone: \"" + boneName + "\"");

				this.bones[boneName] = new Bone(boneName);

				// bit of a hack...
				if (boneName.Contains(@"Hroth"))
					this.bones[boneName].IsEnabled = this.IsHrothgar;

				if (boneName.Contains(@"Viera"))
					this.bones[boneName].IsEnabled = this.IsViera;

				if (boneName.Contains(@"Tail"))
					this.bones[boneName].IsEnabled = this.HasTail;
			}

			// special case for Viera lips
			// disable lip bones if Viera, as they have their own set of lip bones...
			this.GetBone("LipLowerA").IsEnabled = !this.IsViera;
			this.GetBone("LipUpperB").IsEnabled = !this.IsViera;
			this.GetBone("LipLowerB").IsEnabled = !this.IsViera;

			// now that we have all the bones, we make a hierarchy
			// torso tree
			this.ParentBone("Root", "SpineA");
			this.ParentBone("SpineA", "SpineB");
			this.ParentBone("SpineB", "SpineC");
			this.ParentBone("SpineC", "Neck");
			this.ParentBone("Neck", "Head");
			this.ParentBone("SpineB", "BreastLeft");
			this.ParentBone("SpineB", "BreastRight");

			// clothes tree
			this.ParentBone("Waist", "ClothBackALeft");
			this.ParentBone("ClothBackALeft", "ClothBackBLeft");
			this.ParentBone("ClothBackBLeft", "ClothBackCLeft");
			this.ParentBone("Waist", "ClothBackARight");
			this.ParentBone("ClothBackARight", "ClothBackBRight");
			this.ParentBone("ClothBackBRight", "ClothBackCRight");
			this.ParentBone("Waist", "ClothSideALeft");
			this.ParentBone("ClothSideALeft", "ClothSideBLeft");
			this.ParentBone("ClothSideBLeft", "ClothSideCLeft");
			this.ParentBone("Waist", "ClothSideARight");
			this.ParentBone("ClothSideARight", "ClothSideBRight");
			this.ParentBone("ClothSideBRight", "ClothSideCRight");
			this.ParentBone("Waist", "ClothFrontALeft");
			this.ParentBone("ClothFrontALeft", "ClothFrontBLeft");
			this.ParentBone("ClothFrontBLeft", "ClothFrontCLeft");
			this.ParentBone("Waist", "ClothFrontARight");
			this.ParentBone("ClothFrontARight", "ClothFrontBRight");
			this.ParentBone("ClothFrontBRight", "ClothFrontCRight");

			// Facebone (middy) tree
			this.ParentBone("Head", "Nose");
			this.ParentBone("Head", "Jaw");
			this.ParentBone("Head", "EyelidLowerLeft");
			this.ParentBone("Head", "EyelidLowerRight");
			this.ParentBone("Head", "EyeLeft");
			this.ParentBone("Head", "EyeRight");
			this.ParentBone("Head", "EarLeft");
			this.ParentBone("EarLeft", "EarringALeft");
			this.ParentBone("EarringALeft", "EarringBLeft");
			this.ParentBone("Head", "EarRight");
			this.ParentBone("EarRight", "EarringARight");
			this.ParentBone("EarringARight", "EarringBRight");
			this.ParentBone("Head", "HairFrontLeft");
			this.ParentBone("Head", "HairFrontRight");
			this.ParentBone("Head", "HairA");
			this.ParentBone("HairA", "HairB");
			this.ParentBone("Head", "CheekLeft");
			this.ParentBone("Head", "CheekRight");
			this.ParentBone("Head", "LipsLeft");
			this.ParentBone("Head", "LipsRight");
			this.ParentBone("Head", "EyebrowLeft");
			this.ParentBone("Head", "EyebrowRight");
			this.ParentBone("Head", "Bridge");
			this.ParentBone("Head", "BrowLeft");
			this.ParentBone("Head", "BrowRight");
			this.ParentBone("Head", "LipUpperA");
			this.ParentBone("Head", "EyelidUpperLeft");
			this.ParentBone("Head", "EyelidUpperRight");
			this.ParentBone("Jaw", "LipLowerA");
			this.ParentBone("Head", "LipUpperB");
			this.ParentBone("LipLowerA", "LipLowerB");

			this.ParentBone("Head", "ExHairA");
			this.ParentBone("Head", "ExHairB");
			this.ParentBone("Head", "ExHairC");
			this.ParentBone("Head", "ExHairD");
			this.ParentBone("Head", "ExHairE");
			this.ParentBone("Head", "ExHairF");
			this.ParentBone("Head", "ExHairG");
			this.ParentBone("Head", "ExHairH");
			this.ParentBone("Head", "ExHairI");
			this.ParentBone("Head", "ExHairJ");
			this.ParentBone("Head", "ExHairK");
			this.ParentBone("Head", "ExHairL");

			// Facebone hroth tree
			this.ParentBone("Head", "HrothEyebrowLeft");
			this.ParentBone("Head", "HrothEyebrowRight");
			this.ParentBone("Head", "HrothBridge");
			this.ParentBone("Head", "HrothBrowLeft");
			this.ParentBone("Head", "HrothBrowRight");
			this.ParentBone("Head", "HrothJawUpper");
			this.ParentBone("Head", "HrothLipUpper");
			this.ParentBone("Head", "HrothEyelidUpperLeft");
			this.ParentBone("Head", "HrothEyelidUpperRight");
			this.ParentBone("Head", "HrothLipsLeft");
			this.ParentBone("Head", "HrothLipsRight");
			this.ParentBone("Head", "HrothLipUpperLeft");
			this.ParentBone("Head", "HrothLipUpperRight");
			this.ParentBone("Head", "HrothLipLower");
			this.ParentBone("Head", "HrothWhiskersLeft");
			this.ParentBone("Head", "HrothWhiskersRight");

			// Facebone Viera tree
			this.ParentBone("Jaw", "VieraLipLowerA");
			this.ParentBone("Jaw", "VieraLipLowerB");
			this.ParentBone("Head", "VieraLipUpperB");
			this.ParentBone("Head", "VieraEar01ALeft");
			this.ParentBone("Head", "VieraEar02ALeft");
			this.ParentBone("Head", "VieraEar03ALeft");
			this.ParentBone("Head", "VieraEar04ALeft");
			this.ParentBone("Head", "VieraEar01ARight");
			this.ParentBone("Head", "VieraEar02ARight");
			this.ParentBone("Head", "VieraEar03ARight");
			this.ParentBone("Head", "VieraEar04ARight");
			this.ParentBone("VieraEar01ALeft", "VieraEar01BLeft");
			this.ParentBone("VieraEar02ALeft", "VieraEar02BLeft");
			this.ParentBone("VieraEar03ALeft", "VieraEar03BLeft");
			this.ParentBone("VieraEar04ALeft", "VieraEar04BLeft");
			this.ParentBone("VieraEar01ARight", "VieraEar01BRight");
			this.ParentBone("VieraEar02ARight", "VieraEar02BRight");
			this.ParentBone("VieraEar03ARight", "VieraEar03BRight");
			this.ParentBone("VieraEar04ARight", "VieraEar04BRight");

			// armbone tree
			this.ParentBone("SpineC", "ClavicleLeft");
			this.ParentBone("ClavicleLeft", "ArmLeft");
			this.ParentBone("ArmLeft", "ShoulderLeft");
			this.ParentBone("ArmLeft", "PauldronLeft");
			this.ParentBone("ArmLeft", "ForearmLeft");
			this.ParentBone("ForearmLeft", "ElbowLeft");
			this.ParentBone("ForearmLeft", "WristLeft");
			this.ParentBone("ForearmLeft", "ShieldLeft");
			this.ParentBone("ForearmLeft", "CouterLeft");
			this.ParentBone("ForearmLeft", "WristLeft");
			this.ParentBone("HandLeft", "WeaponLeft");
			this.ParentBone("HandLeft", "ThumbALeft");
			this.ParentBone("ThumbALeft", "ThumbBLeft");
			this.ParentBone("WristLeft", "HandLeft");
			this.ParentBone("HandLeft", "IndexALeft");
			this.ParentBone("IndexALeft", "IndexBLeft");
			this.ParentBone("HandLeft", "MiddleALeft");
			this.ParentBone("MiddleALeft", "MiddleBLeft");
			this.ParentBone("HandLeft", "RingALeft");
			this.ParentBone("RingALeft", "RingBLeft");
			this.ParentBone("HandLeft", "PinkyALeft");
			this.ParentBone("PinkyALeft", "PinkyBLeft");

			this.ParentBone("SpineC", "ClavicleRight");
			this.ParentBone("ClavicleRight", "ArmRight");
			this.ParentBone("ArmRight", "ShoulderRight");
			this.ParentBone("ArmRight", "PauldronRight");
			this.ParentBone("ArmRight", "ForearmRight");
			this.ParentBone("ForearmRight", "ElbowRight");
			this.ParentBone("ForearmRight", "WristRight");
			this.ParentBone("ForearmRight", "ShieldRight");
			this.ParentBone("ForearmRight", "CouterRight");
			this.ParentBone("ForearmRight", "WristRight");
			this.ParentBone("WristRight", "HandRight");
			this.ParentBone("HandRight", "WeaponRight");
			this.ParentBone("HandRight", "ThumbARight");
			this.ParentBone("ThumbARight", "ThumbBRight");
			this.ParentBone("HandRight", "IndexARight");
			this.ParentBone("IndexARight", "IndexBRight");
			this.ParentBone("HandRight", "MiddleARight");
			this.ParentBone("MiddleARight", "MiddleBRight");
			this.ParentBone("HandRight", "RingARight");
			this.ParentBone("RingARight", "RingBRight");
			this.ParentBone("HandRight", "PinkyARight");
			this.ParentBone("PinkyARight", "PinkyBRight");

			// lower half bones tree
			this.ParentBone("Root", "Waist");
			this.ParentBone("Waist", "LegLeft");
			this.ParentBone("CalfLeft", "KneeLeft");
			this.ParentBone("KneeLeft", "PoleynLeft");
			this.ParentBone("LegLeft", "CalfLeft");
			this.ParentBone("CalfLeft", "FootLeft");
			this.ParentBone("FootLeft", "ToesLeft");
			this.ParentBone("Waist", "LegRight");
			this.ParentBone("CalfRight", "KneeRight");
			this.ParentBone("KneeRight", "PoleynRight");
			this.ParentBone("LegRight", "CalfRight");
			this.ParentBone("CalfRight", "FootRight");
			this.ParentBone("FootRight", "ToesRight");

			this.ParentBone("SpineB", "SheatheLeft");
			this.ParentBone("SpineB", "SheatheRight");
			this.ParentBone("SheatheLeft", "HolsterLeft");
			this.ParentBone("SheatheRight", "HolsterRight");
			this.ParentBone("SheatheLeft", "ScabbardLeft");
			this.ParentBone("SheatheRight", "ScabbardRight");

			// tail bones tree
			this.ParentBone("Waist", "TailA");
			this.ParentBone("TailA", "TailB");
			this.ParentBone("TailB", "TailC");
			this.ParentBone("TailC", "TailD");
			this.ParentBone("TailD", "TailE");
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
			while (this.IsEnabled)
			{
				Thread.Sleep(32);

				if (!this.IsEnabled)
					continue;

				if (this.CurrentBone == null)
					continue;

				this.CurrentBone.SetRotation();
			}
		}

		private void WatchCamera()
		{
			IMemory<float> camX = injection.GetMemory<float>(Offsets.BaseAddresses.Camera, injection.Offsets.Character.CamAngleX);
			IMemory<float> camY = injection.GetMemory<float>(Offsets.BaseAddresses.Camera, injection.Offsets.Character.CamAngleY);
			IMemory<float> camZ = injection.GetMemory<float>(Offsets.BaseAddresses.Camera, injection.Offsets.Character.CameraHeight2);

			while (this.IsEnabled && Application.Current != null)
			{
				Vector3D camEuler = default(Vector3D);

				camEuler.Y = MathUtils.RadiansToDegrees(camX.Get());
				camEuler.Z = -MathUtils.RadiansToDegrees(camY.Get());
				camEuler.X = MathUtils.RadiansToDegrees(camZ.Get());

				try
				{
					Application.Current.Dispatcher.Invoke(() =>
					{
						this.CameraRotation = camEuler.ToQuaternion();
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
