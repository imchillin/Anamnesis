// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Interop.Delegates;

using System.Numerics;
using System.Runtime.InteropServices;

public static class Camera
{
	[FunctionBind("E8 ?? ?? ?? ?? 0F 28 C7 0F 28 CE")]
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public unsafe delegate Vector2* WorldToScreenPoint(Vector2* screenPoint, Vector3* worldPoint);
}

public static class Character
{
	[FunctionBind("E8 ?? ?? ?? ?? 84 C0 8B CF")]
	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	public delegate bool IsWanderer(nint charPtr);
}

public static class Framework
{
	[FunctionBind("40 53 55 57 41 55 48 83 EC ?? ?? 48 ?? ?? ?? ?? ?? ?? ?? 48")]
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate void RenderGraphics(long a1);
}

public static class GameMain
{
	[FunctionBind("E8 ?? ?? ?? ?? 83 7F ?? ?? 4C 8D 3D")]
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate bool IsInGPose();
}
