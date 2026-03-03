// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Drivers;

using Reloaded.Hooks.Definitions;
using RemoteController.Interop;
using RemoteController.Interop.Delegates;
using RemoteController.Interop.Types;
using System;
using System.Diagnostics.CodeAnalysis;

using HkaPoseDelegate = Interop.Delegates.HkaPose;
using HkaPoseStruct = Interop.Types.HkaPose;

/// <summary>
/// A driver class for controlling the havok animation engine within
/// the game to enable pose functionality within GPose.
/// </summary>
[RequiresUnreferencedCode("This class is not trimming-safe")]
[RequiresDynamicCode("This class requires dynamic code due to hook reflection")]
public sealed class PosingDriver : DriverBase<PosingDriver>
{
	// NOTES:
	// - "SetBoneModelTransform" is used to freeze physics simulations by preventing the game from updating bone transforms.
	// - The kinematic driver is hooked to allow for the posing of secondary physics bones that are driven by kine drivers (e.g. visor, clothing top bones, etc.).
	// - "LookAtIkSolver.Solve" is hooked to prevent the game from updating head and eye look-at targets, allowing for manual posing of the head and eyes.
	// - "CalculateBoneModelSpace" and "SyncModelSpace" are hooked to prevent the game from overwriting manual pose adjustments when it calculates bone transforms.
	// - "GameObject.SetPosition" is hooked to prevent the GPose camera from resetting its position.
	// - "UpdateVisualPosition" and "UpdateVisualRotation" are hooked to prevent the game from updating the visual position and rotation of objects.

	private readonly GposeDriver gposeDriver;
	private readonly FunctionHook<GameObject.SetPosition> hookSetPosition;
	private readonly FunctionHook<HkaPartialSkeleton.SetBoneModelTransform> hookPhysics;
	private readonly FunctionHook<HkaLookAtIkSolver.Solve> hookLookAt;
	private readonly FunctionHook<HkaPoseDelegate.CalculateBoneModelSpace> hookCalculateBone;
	private readonly FunctionHook<HkaPoseDelegate.SyncModelSpace> hookSyncModel;
	private readonly FunctionHook<BoneKineDriver.ApplyKineDriverTransforms> hookKineDriver;
	private readonly FunctionHook<GameObject.UpdateVisualPosition> hookUpdateVisualPosition;
	private readonly FunctionHook<GameObject.UpdateVisualRotation> hookUpdateVisualRotation;
	private readonly FunctionHook<GameObject.UpdateVisualScale> hookUpdateVisualScale;

	private readonly GameObject.SetPosition setPositionDetour;
	private readonly HkaPartialSkeleton.SetBoneModelTransform physicsDetour;
	private readonly HkaLookAtIkSolver.Solve lookAtDetour;
	private readonly HkaPoseDelegate.CalculateBoneModelSpace calculateBoneDetour;
	private readonly HkaPoseDelegate.SyncModelSpace syncModelDetour;
	private readonly BoneKineDriver.ApplyKineDriverTransforms kineDriverDetour;
	private readonly GameObject.UpdateVisualPosition updateVisualPositionDetour;
	private readonly GameObject.UpdateVisualRotation updateVisualRotationDetour;
	private readonly GameObject.UpdateVisualScale updateVisualScaleDetour;

	private volatile bool isPosingEnabled = false;
	private volatile bool arePhysicsFrozen = false;
	private volatile bool isWorldVisualStateFrozen = false;

	/// <summary>
	/// Initializes a new instance of the <see cref="PosingDriver"/> class.
	/// </summary>
	/// <param name="frameworkDriver">
	/// A reference to the <see cref="FrameworkDriver"/> instance used to subscribe to game tick updates.
	/// </param>
	/// <exception cref="InvalidOperationException">
	/// Thrown if the signature resolver is not available, which is required for creating function hooks.
	/// </exception>
	public PosingDriver(GposeDriver gposeDriver)
	{
		if (Controller.SigResolver == null)
			throw new InvalidOperationException("Cannot initialize posing driver: signature resolver is not available.");

		unsafe
		{
			// NOTE: Keep references to the detour methods to prevent them from being garbage collected
			this.physicsDetour = this.DetourSetBoneModelTransform;
			this.lookAtDetour = this.DetourLookAtSolve;
			this.calculateBoneDetour = this.DetourCalculateBoneModelSpace;
			this.syncModelDetour = this.DetourSyncModelSpace;
			this.kineDriverDetour = this.DetourApplyKineDriverTransforms;
			this.setPositionDetour = this.DetourSetPosition;
			this.updateVisualPositionDetour = this.DetourUpdateVisualPosition;
			this.updateVisualRotationDetour = this.DetourUpdateVisualRotation;
			this.updateVisualScaleDetour = this.DetourUpdateVisualScale;

			this.hookPhysics = HookRegistry.CreateAndActivateHook(this.physicsDetour);
			this.hookLookAt = HookRegistry.CreateAndActivateHook(this.lookAtDetour);
			this.hookCalculateBone = HookRegistry.CreateAndActivateHook(this.calculateBoneDetour);
			this.hookSyncModel = HookRegistry.CreateAndActivateHook(this.syncModelDetour);
			this.hookKineDriver = HookRegistry.CreateAndActivateHook(this.kineDriverDetour);
			this.hookSetPosition = HookRegistry.CreateAndActivateHook(this.setPositionDetour);
			this.hookUpdateVisualPosition = HookRegistry.CreateAndActivateHook(this.updateVisualPositionDetour);
			this.hookUpdateVisualRotation = HookRegistry.CreateAndActivateHook(this.updateVisualRotationDetour);
			this.hookUpdateVisualScale = HookRegistry.CreateAndActivateHook(this.updateVisualScaleDetour);
		}

		this.gposeDriver = gposeDriver;
		this.gposeDriver.StateChanged += this.OnGposeStateChanged;
		this.RegisterInstance();
	}

	/// <summary>
	/// Gets or sets a value indicating whether physics simulations (clothing, hair, etc.) are frozen.
	/// </summary>
	public bool FreezePhysics
	{
		get => this.arePhysicsFrozen;
		set => this.arePhysicsFrozen = value;
	}

	/// <summary>
	/// Gets or sets a value indicating whether bone transforms (Position, Rotation, Scale) are frozen.
	/// Enabling this prevents the game from overwriting manual pose adjustments.
	/// </summary>
	public bool PosingEnabled
	{
		get => this.isPosingEnabled;
		set => this.isPosingEnabled = value;
	}

	/// <summary>
	/// Gets or sets a value indicating whether world visual state (position and rotation) is frozen.
	/// When enabled, game objects will retain their current visual position and rotation, preventing the game from updating them.
	/// </summary>
	public bool FreezeWorldVisualState
	{
		get => this.isWorldVisualStateFrozen;
		set => this.isWorldVisualStateFrozen = value;
	}

	/// <inheritdoc/>
	protected override void OnDispose()
	{
		this.gposeDriver.StateChanged -= this.OnGposeStateChanged;
		this.isPosingEnabled = false;
		this.arePhysicsFrozen = false;
		this.isWorldVisualStateFrozen = false;
		
		this.hookPhysics.Dispose();
		this.hookLookAt.Dispose();
		this.hookCalculateBone.Dispose();
		this.hookSyncModel.Dispose();
		this.hookKineDriver.Dispose();
		this.hookSetPosition.Dispose();
		this.hookUpdateVisualPosition.Dispose();
		this.hookUpdateVisualRotation.Dispose();
		this.hookUpdateVisualScale.Dispose();
	}

	private unsafe nint DetourSetBoneModelTransform(nint partialPtr, ulong boneId, HkaTransform4* transform, byte bUpdateSecondaryPose, byte bPropagate)
	{
		if (this.arePhysicsFrozen)
			return partialPtr;

		return this.hookPhysics.OriginalFunction(partialPtr, boneId, transform, bUpdateSecondaryPose, bPropagate);
	}

	private unsafe byte* DetourLookAtSolve(byte* a1, HkaVector4* a2, HkaVector4* a3, float a4, HkaVector4* a5, HkaVector4* a6)
	{
		if (this.isPosingEnabled)
		{
			*a1 = 0; // Tell the caller that the IK didn't resolve
			return a1;
		}

		return this.hookLookAt.OriginalFunction(a1, a2, a3, a4, a5, a6);
	}

	private unsafe HkaTransform4* DetourCalculateBoneModelSpace(nint posePtr, int boneIdx)
	{
		if (this.isPosingEnabled)
		{
			HkaPoseStruct* pose = (HkaPoseStruct*)posePtr;
			
			if (!pose->ModelPose.IsValid || !pose->BoneFlags.IsValid)
				return this.hookCalculateBone.OriginalFunction(posePtr, boneIdx);

			if (boneIdx < 0 || boneIdx >= pose->ModelPose.Length || boneIdx >= pose->BoneFlags.Length)
				return this.hookCalculateBone.OriginalFunction(posePtr, boneIdx);

			// Get the pointer to the bone's model-space transform
			HkaTransform4* transform = pose->GetModelPoseTransform(boneIdx);
			if (transform == null)
				return this.hookCalculateBone.OriginalFunction(posePtr, boneIdx);

			// Clear the dirty flag to indicate the bone is calculated
			pose->ClearModelDirtyChain(boneIdx);
			return transform;
		}

		return this.hookCalculateBone.OriginalFunction(posePtr, boneIdx);
	}

	private void DetourSyncModelSpace(nint posePtr)
	{
		if (this.isPosingEnabled)
			return;

		this.hookSyncModel.OriginalFunction(posePtr);
	}

	private void DetourApplyKineDriverTransforms(IntPtr kineDriverPtr, IntPtr hkaPosePtr)
	{
		if (this.isPosingEnabled)
			return;

		this.hookKineDriver.OriginalFunction(kineDriverPtr, hkaPosePtr);
	}

	private nint DetourSetPosition(nint goPtr, float x, float y, float z)
	{
		if (this.isWorldVisualStateFrozen)
			return goPtr;

		return this.hookSetPosition.OriginalFunction(goPtr, x, y, z);
	}

	private byte DetourUpdateVisualPosition(nint goPtr)
	{
		if (this.isWorldVisualStateFrozen)
			return 0;
		
		return this.hookUpdateVisualPosition.OriginalFunction(goPtr);
	}

	private byte DetourUpdateVisualRotation(nint goPtr)
	{
		if (this.isWorldVisualStateFrozen)
			return 0;
		
		return this.hookUpdateVisualRotation.OriginalFunction(goPtr);
	}

	private void DetourUpdateVisualScale(nint goPtr, bool a2)
	{
		if (this.isWorldVisualStateFrozen)
			return;
		
		this.hookUpdateVisualScale.OriginalFunction(goPtr, a2);
	}

	private void OnGposeStateChanged(bool isGpose)
	{
		if (!isGpose)
		{
			this.PosingEnabled = false;
			this.FreezePhysics = false;
			this.FreezeWorldVisualState = false;
		}
	}
}
