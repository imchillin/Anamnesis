// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;
using System.Numerics;
using System.Runtime.InteropServices;

/// <summary>
/// Represents the object's skeleton, composed of an array of <see cref="PartialSkeletonMemory"/> elements.
/// </summary>
public class SkeletonMemory : ArrayMemory<PartialSkeletonMemory, ushort>
{
	/// <inheritdoc/>
	public override int AddressOffset => 0x068;

	/// <inheritdoc/>
	public override int LengthOffset => 0x050;

	/// <inheritdoc/>
	public override int ElementSize => 0x230;

	/// <summary>Gets or sets the array of attachment points (EIDs) in the skeleton.</summary>
	[Bind(0x088, BindFlags.DontCacheOffsets, SyncGroup = "SkeletonMetadata")]
	public AttachBoneArrayMemory? AttachBones { get; set; }

	/// <summary>Gets or sets the array of bone index masks for the attachment points in the skeleton.</summary>
	[Bind(0x098, BindFlags.DontCacheOffsets, SyncGroup = "SkeletonMetadata")]
	public AttachBoneMaskArrayMemory? AttachBoneMasks { get; set; }

	/// <summary>
	/// Resolves a bone name from a bone index mask.
	/// </summary>
	/// <param name="boneIndexMask">The combined bone index mask (bits 0-11: BoneIdx, bits 12-15: PartialSkeletonIdx).</param>
	/// <returns>The resolved bone name, or null if resolution fails.</returns>
	public string? GetBoneName(ushort boneIndexMask)
	{
		int partialSkeletonIndex = (boneIndexMask >> 12) & 0x000F;
		int boneIndex = boneIndexMask & 0x0FFF;

		if (partialSkeletonIndex < this.Count)
		{
			var partialSkeleton = this[partialSkeletonIndex];
			var bones = partialSkeleton?.Pose1?.Skeleton?.Bones;
			if (bones != null && boneIndex < bones.Count)
			{
				string? boneName = bones[boneIndex]?.Name.ToString();
				if (!string.IsNullOrEmpty(boneName))
					return boneName;
			}
		}

		return null;
	}

	/// <summary>
	/// Attempts to resolve an attachment point (EID) by its identifier.
	/// </summary>
	/// <param name="id">The element ID (EID) to look up.</param>
	/// <param name="boneIndexMask">The resolved parent bone index mask.</param>
	/// <param name="transform">The resolved parent-bone-relative transform of the attachment point.</param>
	/// <returns>True if the attachment point was found and has a valid bone mask; otherwise, false.</returns>
	public bool TryGetAttachBone(int id, out ushort boneIndexMask, out Transform transform)
	{
		boneIndexMask = 0xFFFF;
		transform = default;

		if (this.AttachBones == null || this.AttachBoneMasks == null)
			return false;

		int count = Math.Min(this.AttachBones.Count, this.AttachBoneMasks.Count);
		for (int i = 0; i < count; i++)
		{
			AttachBone attachBone = this.AttachBones[i];
			if (attachBone.Id != id)
				continue;

			ushort mask = this.AttachBoneMasks[i];
			if (mask == 0xFFFF)
				return false;

			boneIndexMask = mask;
			transform = attachBone.ToTransform();
			return true;
		}

		return false;
	}

	/// <summary>Represents an attachment point (EID) entry in the skeleton.</summary>
	[StructLayout(LayoutKind.Explicit, Size = 0x40)]
	public struct AttachBone
	{
		/// <summary>The element identifier (EID).</summary>
		[FieldOffset(0x20)]
		public int Id;

		/// <summary>The position offset relative to the parent bone.</summary>
		[FieldOffset(0x24)]
		public Vector3 Position;

		/// <summary>The Euler rotation angles (in radians) relative to the parent bone.</summary>
		[FieldOffset(0x30)]
		public Vector3 Rotation;

		/// <summary>Converts the attachment point to a <see cref="Transform"/>.</summary>
		/// <returns>The local transform relative to the parent bone.</returns>
		public readonly Transform ToTransform()
		{
			float hx = this.Rotation.X * 0.5f;
			float hy = this.Rotation.Y * 0.5f;
			float hz = this.Rotation.Z * 0.5f;
			float sx = MathF.Sin(hx), cx = MathF.Cos(hx);
			float sy = MathF.Sin(hy), cy = MathF.Cos(hy);
			float sz = MathF.Sin(hz), cz = MathF.Cos(hz);

			return new Transform
			{
				Position = this.Position,
				Rotation = new Quaternion(
					(cz * sx * cy) - (sz * cx * sy),
					(cz * cx * sy) + (sz * sx * cy),
					(sz * cx * cy) - (cz * sx * sy),
					(cz * cx * cy) + (sz * sx * sy)),
				Scale = Vector3.One,
			};
		}
	}

	/// <summary>Represents the array of attachment points in the skeleton.</summary>
	public class AttachBoneArrayMemory : ArrayMemory<AttachBone, int>
	{
		/// <inheritdoc/>
		public override int AddressOffset => 0x000;

		/// <inheritdoc/>
		public override int LengthOffset => 0x018;

		/// <inheritdoc/>
		public override int ElementSize => 0x040;
	}

	/// <summary>Represents the array of bone index masks for the attachment points in the skeleton.</summary>
	public class AttachBoneMaskArrayMemory : ArrayMemory<ushort, int>
	{
		/// <inheritdoc/>
		public override int AddressOffset => 0x000;

		/// <inheritdoc/>
		public override int LengthOffset => 0x008;

		/// <inheritdoc/>
		public override int ElementSize => 0x002;
	}
}
