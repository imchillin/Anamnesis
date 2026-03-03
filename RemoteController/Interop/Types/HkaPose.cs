// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Interop.Types;

using System;
using System.Runtime.InteropServices;

/// <summary>
/// Represents a Havok animation pose (hkaPose).
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 0x50)]
public unsafe struct HkaPose
{
	/// <summary>
	/// Flags indicating the dirty state of bone transforms.
	/// </summary>
	[Flags]
	public enum BoneFlag : int
	{
		/// <summary>
		/// The local-space transform is dirty and needs recalculation.
		/// </summary>
		LocalDirty = 1 << 0,

		/// <summary>
		/// The model-space transform is dirty and needs recalculation.
		/// </summary>
		ModelDirty = 1 << 1,
	}

	/// <summary>
	/// Pointer to the skeleton definition.
	/// </summary>
	[FieldOffset(0x00)]
	public HkaSkeleton* Skeleton;

	/// <summary>
	/// Array of bone transforms in local (parent-relative) space.
	/// </summary>
	[FieldOffset(0x08)]
	public HkArray<HkaTransform4> LocalPose;

	/// <summary>
	/// Array of bone transforms in model (character-relative) space.
	/// </summary>
	[FieldOffset(0x18)]
	public HkArray<HkaTransform4> ModelPose;

	/// <summary>
	/// Array of flags indicating the dirty state of each bone.
	/// </summary>
	[FieldOffset(0x28)]
	public HkArray<int> BoneFlags;

	/// <summary>
	/// A value indicating whether the model-space pose is synchronized.
	/// </summary>
	[FieldOffset(0x38)]
	public byte ModelInSync;

	/// <summary>
	/// A value indicating whether the local-space pose is synchronized.
	/// </summary>
	[FieldOffset(0x39)]
	public byte LocalInSync;

	/// <summary>
	/// Clears the model-dirty flag for the specified bone
	/// and all its ancestors  up to the parent chain.
	/// </summary>
	/// <param name="boneIndex">The index of the bone.</param>
	public void ClearModelDirtyChain(int boneIndex)
	{
		if (!this.BoneFlags.IsValid || this.Skeleton == null)
			return;

		if (!this.Skeleton->ParentIndices.IsValid)
			return;

		short* parentIndices = this.Skeleton->ParentIndices.GetPointer(0);
		if (parentIndices == null)
			return;

		int current = boneIndex;
		while (current >= 0 && current < this.BoneFlags.Length)
		{
			if ((this.BoneFlags[current] & (int)BoneFlag.ModelDirty) == 0)
				break;

			this.BoneFlags[current] &= ~(int)BoneFlag.ModelDirty;
			current = parentIndices[current];
		}
	}

	/// <summary>
	/// Gets a pointer to the local-space transform for the specified bone.
	/// </summary>
	/// <param name="boneIndex">The index of the bone.</param>
	/// <returns>
	/// A pointer to the transform, or null if invalid.
	/// </returns>
	public readonly HkaTransform4* GetLocalPoseTransform(int boneIndex)
	{
		if (!this.LocalPose.IsValid || boneIndex < 0 || boneIndex >= this.LocalPose.Length)
			return null;

		return this.LocalPose.GetPointer(boneIndex);
	}

	/// <summary>
	/// Gets a pointer to the model-space transform for the specified bone.
	/// </summary>
	/// <param name="boneIndex">The index of the bone.</param>
	/// <returns>
	/// A pointer to the transform, or null if invalid.
	/// </returns>
	public readonly HkaTransform4* GetModelPoseTransform(int boneIndex)
	{
		if (!this.ModelPose.IsValid || boneIndex < 0 || boneIndex >= this.ModelPose.Length)
			return null;

		return this.ModelPose.GetPointer(boneIndex);
	}

	/// <summary>
	/// Sets a local-space transform for the specified bone, marking model as dirty.
	/// </summary>
	/// <param name="boneIndex">The index of the bone.</param>
	/// <param name="transform">The new local-space transform.</param>
	public void SetBoneLocalSpace(int boneIndex, HkaTransform4 transform)
	{
		if (!this.LocalPose.IsValid || boneIndex < 0 || boneIndex >= this.LocalPose.Length)
			return;

		this.LocalPose[boneIndex] = transform;

		if (this.BoneFlags.IsValid && boneIndex < this.BoneFlags.Length)
		{
			this.BoneFlags[boneIndex] &= ~(int)BoneFlag.LocalDirty;
			this.BoneFlags[boneIndex] |= (int)BoneFlag.ModelDirty;
		}

		this.LocalInSync = 0;
	}

	/// <summary>
	/// Sets a model-space transform for the specified bone, marking local as dirty.
	/// </summary>
	/// <param name="boneIndex">The index of the bone.</param>
	/// <param name="transform">The new model-space transform.</param>
	public void SetBoneModelSpace(int boneIndex, HkaTransform4 transform)
	{
		if (!this.ModelPose.IsValid || boneIndex < 0 || boneIndex >= this.ModelPose.Length)
			return;

		this.ModelPose[boneIndex] = transform;

		if (this.BoneFlags.IsValid && boneIndex < this.BoneFlags.Length)
		{
			this.BoneFlags[boneIndex] &= ~(int)BoneFlag.ModelDirty;
			this.BoneFlags[boneIndex] |= (int)BoneFlag.LocalDirty;
		}

		this.ModelInSync = 0;
	}
}
