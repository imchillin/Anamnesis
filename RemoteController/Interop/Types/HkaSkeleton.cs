// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Interop.Types;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Size = 0x88)]
public struct HkaSkeleton
{
	/// <summary>
	/// An array of parent indices for each bone in the skeleton.
	/// The bone index corresponds to the index in the skeleton's bone array, and the value is the index of the parent bone.
	/// </summary>
	[FieldOffset(0x18)]
	public HkArray<short> ParentIndices;
}