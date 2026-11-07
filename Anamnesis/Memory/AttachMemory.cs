// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

/// <summary>
/// Represents the attachment structure for a model in FFXIV (Client::Graphics::Scene::Attach).
/// </summary>
public class AttachMemory : MemoryBase
{
	/// <summary>
	/// Gets or sets the attachment execution type (e.g., 3 = Element ID / EID, 4 = Bone Index).
	/// </summary>
	[Bind(0x050, BindFlags.DontCacheOffsets)] public int ExecuteType { get; set; }

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
	private Transform cachedTransform;
	private Transform lastChildTransform;
	private ushort lastMask = ushort.MaxValue;
	private int lastExecuteType = -1;

	/// <summary>
	/// Gets or sets the target identifier: either a combined bone index mask (bits 0-11: BoneIdx, bits 12-15: PartialSkeletonIdx)
	/// when <see cref="AttachMemory.ExecuteType"/> is 4, or an Element ID (EID) when <see cref="AttachMemory.ExecuteType"/> is 3.
	/// </summary>
	[Bind(0x002, BindFlags.DontCacheOffsets)] public ushort BoneIndexMask { get; set; }

	/// <summary>
	/// Gets or sets the relative transform of the attached child model from the parent attachment point.
	/// </summary>
	[Bind(0x010, BindFlags.DontCacheOffsets)] public Transform ChildTransform { get; set; }

	/// <summary>
	/// Resolves the parent bone name from the character skeleton.
	/// </summary>
	/// <param name="actor">Optional actor memory instance; if null, resolved via memory hierarchy.</param>
	/// <returns>The resolved bone name, or null if resolution fails.</returns>
	public string? GetBoneName(ActorMemory? actor = null)
	{
		try
		{
			var actorSkeleton = this.GetParentSkeleton(actor);
			if (actorSkeleton == null)
				return null;

			ushort mask = this.BoneIndexMask;
			if (this.GetParent<AttachMemory>()?.ExecuteType == 3 &&
				!actorSkeleton.TryGetAttachBone(mask, out mask, out _))
			{
				return null;
			}

			return actorSkeleton.GetBoneName(mask);
		}
		catch
		{
			return null;
		}
	}

	/// <summary>
	/// Resolves the effective transform of the attachment relative to the parent bone.
	/// </summary>
	/// <param name="actor">Optional actor memory instance; if null, resolved via memory hierarchy.</param>
	/// <returns>The effective transform relative to the parent bone.</returns>
	public Transform GetTransform(ActorMemory? actor = null)
	{
		int executeType = this.GetParent<AttachMemory>()?.ExecuteType ?? 0;
		if (executeType != 3)
			return this.ChildTransform;

		if (this.lastMask == this.BoneIndexMask &&
			this.lastExecuteType == executeType &&
			this.lastChildTransform == this.ChildTransform)
		{
			return this.cachedTransform;
		}

		try
		{
			var actorSkeleton = this.GetParentSkeleton(actor);
			if (actorSkeleton != null &&
				actorSkeleton.TryGetAttachBone(this.BoneIndexMask, out _, out Transform eidTransform))
			{
				this.cachedTransform = Core.Bone.LocalToModelSpace(this.ChildTransform, eidTransform);
				this.lastChildTransform = this.ChildTransform;
				this.lastMask = this.BoneIndexMask;
				this.lastExecuteType = executeType;
				return this.cachedTransform;
			}
		}
		catch
		{
			// Fall back to ChildTransform on error
		}

		return this.ChildTransform;
	}

	private SkeletonMemory? GetParentSkeleton(ActorMemory? actor = null)
	{
		if (actor == null)
		{
			actor = this.GetParent<ActorMemory>();
			while (actor?.GetParent<ActorMemory>() is ActorMemory parentActor)
			{
				actor = parentActor;
			}
		}

		return actor?.ModelObject?.Skeleton;
	}
}
