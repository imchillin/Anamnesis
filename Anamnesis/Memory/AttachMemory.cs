// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

/// <summary>
/// Represents the attachment structure for a model in FFXIV (Client::Graphics::Scene::Attach).
/// </summary>
public class AttachMemory : MemoryBase
{
	/// <summary>
	/// Gets or sets the collection of bone attachments.
	/// </summary>
	[Bind(0x068, BindFlags.DontCacheOffsets)] public AttachmentArrayMemory? Attachments { get; set; }

	/// <summary>
	/// Gets the attachment at the specified index, if available.
	/// </summary>
	/// <param name="index">The attachment index.</param>
	/// <returns>The attachment memory, or null if unavailable.</returns>
	public AttachmentMemory? GetAttachment(int index = 0)
	{
		try
		{
			if (this.Attachments == null || index < 0 || index >= this.Attachments.Count)
				return null;

			return this.Attachments[index];
		}
		catch
		{
			return null;
		}
	}
}

/// <summary>
/// Represents an array of bone attachments.
/// </summary>
public class AttachmentArrayMemory : ArrayMemory<AttachmentMemory, int>
{
	/// <inheritdoc/>
	public override int AddressOffset => 0x008;

	/// <inheritdoc/>
	public override int LengthOffset => 0x000;

	/// <inheritdoc/>
	public override int ElementSize => 0x058;
}

/// <summary>
/// Represents an individual bone attachment node in FFXIV (Client::Graphics::Scene::Attach::Attachment).
/// </summary>
public class AttachmentMemory : MemoryBase
{
	/// <summary>
	/// Gets or sets the combined bone index mask (bits 0-11: BoneIdx, bits 12-15: PartialSkeletonIdx).
	/// </summary>
	[Bind(0x002, BindFlags.DontCacheOffsets)] public ushort BoneIndexMask { get; set; }

	/// <summary>
	/// Gets or sets the relative transform of the attached child model from the parent attachment bone (hkQsTransformf).
	/// </summary>
	[Bind(0x010, BindFlags.DontCacheOffsets)] public Transform ChildTransform { get; set; }

	/// <summary>
	/// Gets the bone index within the partial skeleton.
	/// </summary>
	public int BoneIndex => this.BoneIndexMask & 0x0FFF;

	/// <summary>
	/// Gets the partial skeleton index within the character skeleton.
	/// </summary>
	public int PartialSkeletonIndex => (this.BoneIndexMask >> 12) & 0x000F;

	/// <summary>
	/// Resolves the parent bone name from the character skeleton.
	/// </summary>
	/// <param name="actor">Optional actor memory instance; if null, resolved via memory hierarchy.</param>
	/// <returns>The resolved bone name, or null if resolution fails.</returns>
	public string? GetBoneName(ActorMemory? actor = null)
	{
		try
		{
			actor ??= this.GetParent<ActorMemory>();
			var actorSkeleton = actor?.ModelObject?.Skeleton;
			if (actorSkeleton != null && this.PartialSkeletonIndex < actorSkeleton.Count)
			{
				var partialSkeleton = actorSkeleton[this.PartialSkeletonIndex];
				var bones = partialSkeleton?.Pose1?.Skeleton?.Bones;
				if (bones != null && this.BoneIndex < bones.Count)
				{
					string? boneName = bones[this.BoneIndex]?.Name.ToString();
					if (!string.IsNullOrEmpty(boneName))
						return boneName;
				}
			}

			return null;
		}
		catch
		{
			return null;
		}
	}
}
