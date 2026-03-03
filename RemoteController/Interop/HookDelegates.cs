// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Interop.Delegates;

using RemoteController.Interop.Types;
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
	[FunctionBind("40 53 48 83 EC 20 80 B9 ?? ?? ?? ?? ?? 48 8B D9 7D 6E")]
	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	public delegate long DisableDraw(nint objPtr);

	[FunctionBind("E8 ?? ?? ?? ?? 48 8B 8B ?? ?? ?? ?? 48 85 C9 74 33 45 33 C0")]
	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	public delegate byte EnableDraw(nint objPtr);

	[FunctionBind("E8 ?? ?? ?? ?? 84 C0 8B CF")]
	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	public delegate bool IsWanderer(nint charPtr);
}

public static class Framework
{
	[FunctionBind("48 8D 05 ?? ?? ?? ?? 66 C7 41 ?? ?? ?? 48 89 01 48 8B F1", 0x20, SigResolveStrategy.VTableLookup)]
	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	public delegate byte Tick(nint fPtr);

	[FunctionBind("40 53 55 57 41 55 48 83 EC ?? ?? 48 ?? ?? ?? ?? ?? ?? ?? 48")]
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate void RenderGraphics(long a1);
}

public static class GameObject
{
	[FunctionBind("E8 ?? ?? ?? ?? 83 4B 70 01")]
	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	public delegate nint SetPosition(nint goPtr, float x, float y, float z);

	[FunctionBind("E8 ?? ?? ?? ?? 84 DB 74 3A")]
	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	public delegate byte UpdateVisualPosition(nint goPtr);

	[FunctionBind("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 41 0F 2F CC")]
	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	public delegate byte UpdateVisualRotation(nint goPtr);

	[FunctionBind("E8 ?? ?? ?? ?? 33 FF 4C 8D 75 90")]
	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	public delegate void UpdateVisualScale(nint goPtr, [MarshalAs(UnmanagedType.U1)] bool a2);
}

public static class GameMain
{
	[FunctionBind("E8 ?? ?? ?? ?? 83 7F ?? ?? 4C 8D 3D")]
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate bool IsInGPose();
}

public static class HkaPartialSkeleton
{
	[FunctionBind("48 8B C4 48 89 58 18 55 56 57 41 54 41 55 41 56 41 57 48 81 EC ?? ?? ?? ?? 0F 29 70 B8 0F 29 78 A8 44 0F 29 40 ?? 44 0F 29 48 ?? 48 8B 05 ?? ?? ?? ??")]
	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	public unsafe delegate nint SetBoneModelTransform(nint partialPtr, ulong boneId, HkaTransform4* transform, byte bUpdateSecondaryPose, byte bPropagate);
}

public static class HkaLookAtIkSolver
{
	[FunctionBind("E8 ?? ?? ?? ?? 80 7C 24 ?? ?? 48 8D 4C 24 ??")]
	[UnmanagedFunctionPointer(CallingConvention.FastCall)]
	public unsafe delegate byte* Solve(byte* a1, HkaVector4* a2, HkaVector4* a3, float a4, HkaVector4* a5, HkaVector4* a6);
}

public static class HkaPose
{
	[FunctionBind("E8 ?? ?? ?? ?? 0F 28 2F")]
	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	public unsafe delegate HkaTransform4* CalculateBoneModelSpace(nint posePtr, int boneIdx);

	[FunctionBind("E8 ?? ?? ?? ?? 49 8D 46 40")]
	[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
	public delegate void SyncModelSpace(nint posePtr);
}

public static class BoneKineDriver
{
	[FunctionBind("48 8B C4 55 57 48 83 EC 58")]
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	public delegate void ApplyKineDriverTransforms(IntPtr kineDriverPtr, IntPtr hkaPosePtr);
}
