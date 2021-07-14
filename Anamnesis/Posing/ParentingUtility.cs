// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Posing
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Anamnesis.Memory;
	using Anamnesis.PoseModule;
	using Serilog;
	using XivToolsWpf;

	public static class ParentingUtility
	{
		public static async Task ParentBones(SkeletonVisual3d root, IEnumerable<BoneVisual3d> bones)
		{
			PoseService.Instance.IsEnabled = true;
			PoseService.Instance.EnableParenting = false;

			try
			{
				foreach (BoneVisual3d bone in bones)
				{
					root.Children.Add(bone);
					bone.ReadTransform();
				}

				foreach (BoneVisual3d bone in bones)
				{
					await ParentBone(root, bones, bone);
				}
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				PoseService.Instance.IsEnabled = false;
			}
		}

		private static bool IsChildOf(BoneVisual3d childBone, BoneVisual3d parentBone)
		{
			if (childBone.Parent == null)
				return false;

			if (childBone.Parent == parentBone)
				return true;

			return IsChildOf(childBone.Parent, parentBone);
		}

		private static async Task ParentBone(SkeletonVisual3d root, IEnumerable<BoneVisual3d> bones, BoneVisual3d bone)
		{
			// Must wait for ffxiv to update at least one frame
			await Task.Delay(75);
			await Dispatch.MainThread();

			// Get the positions of all bones
			Dictionary<BoneVisual3d, Vector> initialBonePositions = new Dictionary<BoneVisual3d, Vector>();
			foreach (BoneVisual3d otherBone in bones)
			{
				otherBone.ReadTransform();
				initialBonePositions.Add(otherBone, otherBone.Position);
			}

			// rotate the test bone
			Quaternion oldRot = bone.Rotation;
			bone.Rotation *= Quaternion.FromEuler(new Vector(0, 90, 0));
			bone.WriteTransform(root, false);
			bone.ViewModel.WriteToMemory(true);

			// Must wait for ffxiv to update at least one frame
			await Task.Delay(75);
			await Dispatch.MainThread();

			// See if any bones moved as a result of the test bone rotation
			foreach (BoneVisual3d otherBone in bones)
			{
				if (otherBone == bone)
					continue;

				otherBone.ReadTransform();

				// If this bone has moved, then it is a child of the test bone.
				if (initialBonePositions[otherBone] != otherBone.Position)
				{
					//// If this bone is already a child of the testbones hierarchy, dont move it.
					if (IsChildOf(otherBone, bone))
						continue;

					if (IsChildOf(bone, otherBone))
					{
						Log.Warning("Bone that is parent of test bone was moved.");
						continue;
						////throw new Exception("Bone that is parent of test bone was moved.");
					}

					otherBone.Parent = bone;
				}
			}

			// Restore the test bone rotation
			bone.Rotation = oldRot;
			bone.WriteTransform(root, false);
			bone.ViewModel.WriteToMemory(true);

			// restore all initial bone positions
			foreach (BoneVisual3d otherBone in bones)
			{
				otherBone.ReadTransform();
				if (initialBonePositions[otherBone] != otherBone.Position)
				{
					otherBone.Position = initialBonePositions[otherBone];
					otherBone.WriteTransform(root, false);
				}
			}
		}
	}
}
