// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Interop.Types;

using System;
using System.Runtime.InteropServices;

/// <summary>
/// Bitmask flags controlling which bone deformations are executed by <see cref="HumanPostBoneDeformer"/>.
/// </summary>
[Flags]
public enum PostBoneDeformerFlags : uint
{
	None = 0,

	/// <summary>
	/// Breast bone scaling (j_mune_l, j_mune_r) based on the bust size customize vector.
	/// </summary>
	Bust = 1u << 0,

	/// <summary>
	/// Standard ear bone scaling (j_mimi_l, j_mimi_r) based on the ear size customize slider.
	/// </summary>
	Ears = 1u << 8,

	/// <summary>
	/// Tail bone scaling and interpolation (n_sippo_a, n_sippo_b) based on the tail length customize slider.
	/// </summary>
	Tail = 0x600,

	/// <summary>
	/// Viera ear bones scaling and positioning (j_zer*) based on the selected ear style and length.
	/// </summary>
	VieraEars = 0xF000,

	/// <summary>
	/// Helmet visor bone rotation (j_ex_met_va, vb, vc) based on visor open/closed angle.
	/// </summary>
	Visor = 1u << 28,
}

/// <summary>
/// Represents the post-bone deformer task in the rendering engine (Client::Graphics::Render::HumanPostBoneDeformer).
/// Runs after Havok physics and animation to apply character customization sliders and helmet visor rotation.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 0x88)]
public unsafe struct HumanPostBoneDeformer
{
	public const int FLAGS_OFFSET = 0x64;

	[FieldOffset(FLAGS_OFFSET)]
	public PostBoneDeformerFlags Flags;
}
