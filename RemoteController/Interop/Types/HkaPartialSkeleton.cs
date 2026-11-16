// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Interop.Types;

using System.Runtime.InteropServices;

/// <summary>
/// Represents a partial skeleton in the rendering engine (Client::Graphics::Render::PartialSkeleton).
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 0x230)]
public unsafe struct HkaPartialSkeleton
{
	/// <summary>
	/// Pointer to the primary Havok pose (HavokPoses[0]).
	/// </summary>
	[FieldOffset(0x148)]
	public HkaPose* Pose;

	/// <summary>
	/// Marks the local-space transform for the specified bone as dirty in the Havok pose.
	/// </summary>
	/// <param name="boneIndex">The index of the bone.</param>
	public readonly void SetBoneLocalDirty(short boneIndex)
	{
		if (this.Pose != null)
		{
			this.Pose->SetBoneLocalDirty(boneIndex);
		}
	}
}
