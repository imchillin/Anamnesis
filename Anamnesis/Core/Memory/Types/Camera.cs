// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory.Types
{
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Explicit)]
	public struct Camera
	{
		[FieldOffset(0x114)] public float Zoom;
		[FieldOffset(0x118)] public float MinZoom;
		[FieldOffset(0x11C)] public float MaxZoom;
		[FieldOffset(0x12C)] public float FieldOfView;
		[FieldOffset(0x130)] public Vector2D Angle;
		[FieldOffset(0x14C)] public float YMin;
		[FieldOffset(0x148)] public float YMax;
		[FieldOffset(0x150)] public Vector2D Pan;
		[FieldOffset(0x160)] public float Rotation;
	}
}
