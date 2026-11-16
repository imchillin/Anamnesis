// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Drivers;

using RemoteController.Interop;
using RemoteController.Interop.Delegates;
using RemoteController.Interop.Types;
using System;
using System.Diagnostics.CodeAnalysis;

using GameObjectDelegates = Interop.Delegates.GameObject;
using HkaPartialSkeletonDelegate = Interop.Delegates.HkaPartialSkeleton;
using HkaPartialSkeletonStruct = Interop.Types.HkaPartialSkeleton;
using HkaPoseDelegate = Interop.Delegates.HkaPose;
using HkaPoseStruct = Interop.Types.HkaPose;
using HumanPostBoneDeformerDelegate = Interop.Delegates.HumanPostBoneDeformer;
using HumanPostBoneDeformerStruct = Interop.Types.HumanPostBoneDeformer;

/// <summary>
/// A driver class for controlling the havok animation engine within
/// the game to enable pose functionality within GPose.
/// </summary>
/// <remarks>
/// - Credits to the Ktisis team and Karou for discovering the solution for the bone drifting issue.<br/>
/// - Credits to Karou for discovering the solution for eye pupil and iris bone manipulation.
/// - Credits to Karou for figuring out how to cleanly toggle physics freezing on/off when posing is enabled.
/// </remarks>
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
	// - "HkaPartialSkeleton.GetBoneLocalTransform" is hooked to allow for scale override of eye pupil and iris bones. This is necessary as Anamnesis does not currently modify the local-space pose, just model-space.
	// - "HumanPostBoneDeformer.Execute" is hooked to prevent customization deformations (bust size, tail length, ear length) from executing on human skeletons during posing.
	//   - When posing is enabled, transform updates are partially blocked by posing hooks, which causes the deformer to create an exponential feedback loop that breaks the mesh.
	//   - Helmet visor rotation does not suffer from said feedback loop and must continue to run so that visors remain posable.

	private readonly GposeDriver gposeDriver;
	private FunctionHook<GameObjectDelegates.SetPosition>? hookSetPosition;
	private FunctionHook<HkaPartialSkeletonDelegate.SetBoneModelTransform>? hookPhysics;
	private FunctionHook<HkaLookAtIkSolver.Solve>? hookLookAt;
	private FunctionHook<HkaPoseDelegate.CalculateBoneModelSpace>? hookCalculateBone;
	private FunctionHook<HkaPoseDelegate.SyncModelSpace>? hookSyncModel;
	private FunctionHook<BoneKineDriver.ApplyKineDriverTransforms>? hookKineDriver;
	private FunctionHook<GameObjectDelegates.UpdateVisualPosition>? hookUpdateVisualPosition;
	private FunctionHook<GameObjectDelegates.UpdateVisualRotation>? hookUpdateVisualRotation;
	private FunctionHook<GameObjectDelegates.UpdateVisualScale>? hookUpdateVisualScale;
	private FunctionHook<ProceduralBoneDriver.ApplyRootRelativeModelTransform>? hookProceduralBone;
	private FunctionHook<HkaPartialSkeletonDelegate.GetBoneLocalTransform>? hookGetBoneLocalTransform;
	private FunctionHook<HumanPostBoneDeformerDelegate.Execute>? hookPostBoneDeformer;

	private GameObjectDelegates.SetPosition? setPositionDetour;
	private HkaPartialSkeletonDelegate.SetBoneModelTransform? physicsDetour;
	private HkaLookAtIkSolver.Solve? lookAtDetour;
	private HkaPoseDelegate.CalculateBoneModelSpace? calculateBoneDetour;
	private HkaPoseDelegate.SyncModelSpace? syncModelDetour;
	private BoneKineDriver.ApplyKineDriverTransforms? kineDriverDetour;
	private GameObjectDelegates.UpdateVisualPosition? updateVisualPositionDetour;
	private GameObjectDelegates.UpdateVisualRotation? updateVisualRotationDetour;
	private GameObjectDelegates.UpdateVisualScale? updateVisualScaleDetour;
	private ProceduralBoneDriver.ApplyRootRelativeModelTransform? proceduralBoneDetour;
	private HkaPartialSkeletonDelegate.GetBoneLocalTransform? getBoneLocalTransformDetour;
	private HumanPostBoneDeformerDelegate.Execute? postBoneDeformerDetour;

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

		this.gposeDriver = gposeDriver;

		this.RegisterInstance(() =>
		{
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
				this.proceduralBoneDetour = this.DetourApplyRootRelativeModelTransform;
				this.getBoneLocalTransformDetour = this.DetourGetBoneLocalTransform;
				this.postBoneDeformerDetour = this.DetourPostBoneDeformerExecute;

				this.hookPhysics = HookRegistry.CreateAndActivateHook(this.physicsDetour);
				this.hookLookAt = HookRegistry.CreateAndActivateHook(this.lookAtDetour);
				this.hookCalculateBone = HookRegistry.CreateAndActivateHook(this.calculateBoneDetour);
				this.hookSyncModel = HookRegistry.CreateAndActivateHook(this.syncModelDetour);
				this.hookKineDriver = HookRegistry.CreateAndActivateHook(this.kineDriverDetour);
				this.hookSetPosition = HookRegistry.CreateAndActivateHook(this.setPositionDetour);
				this.hookUpdateVisualPosition = HookRegistry.CreateAndActivateHook(this.updateVisualPositionDetour);
				this.hookUpdateVisualRotation = HookRegistry.CreateAndActivateHook(this.updateVisualRotationDetour);
				this.hookUpdateVisualScale = HookRegistry.CreateAndActivateHook(this.updateVisualScaleDetour);
				this.hookProceduralBone = HookRegistry.CreateAndActivateHook(this.proceduralBoneDetour);
				this.hookGetBoneLocalTransform = HookRegistry.CreateAndActivateHook(this.getBoneLocalTransformDetour);
				this.hookPostBoneDeformer = HookRegistry.CreateAndActivateHook(this.postBoneDeformerDetour);
			}

			this.gposeDriver.StateChanged += this.OnGposeStateChanged;
		});
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
		if (this.gposeDriver != null)
			this.gposeDriver.StateChanged -= this.OnGposeStateChanged;

		this.isPosingEnabled = false;
		this.arePhysicsFrozen = false;
		this.isWorldVisualStateFrozen = false;

		this.hookPhysics?.Dispose();
		this.hookLookAt?.Dispose();
		this.hookCalculateBone?.Dispose();
		this.hookSyncModel?.Dispose();
		this.hookKineDriver?.Dispose();
		this.hookSetPosition?.Dispose();
		this.hookUpdateVisualPosition?.Dispose();
		this.hookUpdateVisualRotation?.Dispose();
		this.hookUpdateVisualScale?.Dispose();
		this.hookProceduralBone?.Dispose();
		this.hookGetBoneLocalTransform?.Dispose();
		this.hookPostBoneDeformer?.Dispose();
	}

	private unsafe nint DetourSetBoneModelTransform(HkaPartialSkeletonStruct* partialSkeleton, ulong boneId, HkaTransform4* transform, byte bUpdateSecondaryPose, byte bPropagate)
	{
		if (this.arePhysicsFrozen && partialSkeleton != null)
			return (nint)partialSkeleton;

		return this.hookPhysics!.OriginalFunction(partialSkeleton, boneId, transform, bUpdateSecondaryPose, bPropagate);
	}

	private unsafe byte* DetourLookAtSolve(byte* a1, HkaVector4* a2, HkaVector4* a3, float a4, HkaVector4* a5, HkaVector4* a6)
	{
		if (this.isPosingEnabled)
		{
			*a1 = 0; // Tell the caller that the IK didn't resolve
			return a1;
		}

		return this.hookLookAt!.OriginalFunction(a1, a2, a3, a4, a5, a6);
	}

	private unsafe HkaTransform4* DetourCalculateBoneModelSpace(nint posePtr, int boneIdx)
	{
		if (this.isPosingEnabled && posePtr != nint.Zero)
		{
			HkaPoseStruct* pose = (HkaPoseStruct*)posePtr;

			if (!pose->ModelPose.IsValid || !pose->BoneFlags.IsValid)
				return this.hookCalculateBone!.OriginalFunction(posePtr, boneIdx);

			if (boneIdx < 0 || boneIdx >= pose->ModelPose.Length || boneIdx >= pose->BoneFlags.Length)
				return this.hookCalculateBone!.OriginalFunction(posePtr, boneIdx);

			// Get the pointer to the bone's model-space transform
			HkaTransform4* transform = pose->GetModelPoseTransform(boneIdx);
			if (transform == null)
				return this.hookCalculateBone!.OriginalFunction(posePtr, boneIdx);

			// Clear the dirty flag to indicate the bone is calculated
			pose->ClearModelDirtyChain(boneIdx);
			return transform;
		}

		return this.hookCalculateBone!.OriginalFunction(posePtr, boneIdx);
	}

	private void DetourSyncModelSpace(nint posePtr)
	{
		if (this.isPosingEnabled && posePtr != nint.Zero)
			return;

		this.hookSyncModel!.OriginalFunction(posePtr);
	}

	private void DetourApplyKineDriverTransforms(IntPtr kineDriverPtr, IntPtr hkaPosePtr)
	{
		if (this.isPosingEnabled && kineDriverPtr != nint.Zero && hkaPosePtr != nint.Zero)
			return;

		this.hookKineDriver!.OriginalFunction(kineDriverPtr, hkaPosePtr);
	}

	private nint DetourSetPosition(nint goPtr, float x, float y, float z)
	{
		if (this.isWorldVisualStateFrozen && goPtr != nint.Zero)
			return goPtr;

		return this.hookSetPosition!.OriginalFunction(goPtr, x, y, z);
	}

	private byte DetourUpdateVisualPosition(nint goPtr)
	{
		if (this.isWorldVisualStateFrozen && goPtr != nint.Zero)
			return 0;

		return this.hookUpdateVisualPosition!.OriginalFunction(goPtr);
	}

	private byte DetourUpdateVisualRotation(nint goPtr)
	{
		if (this.isWorldVisualStateFrozen && goPtr != nint.Zero)
			return 0;

		return this.hookUpdateVisualRotation!.OriginalFunction(goPtr);
	}

	private void DetourUpdateVisualScale(nint goPtr, bool a2)
	{
		if (this.isWorldVisualStateFrozen && goPtr != nint.Zero)
			return;

		this.hookUpdateVisualScale!.OriginalFunction(goPtr, a2);
	}

	private unsafe nint DetourApplyRootRelativeModelTransform(nint modifierPtr, HkaQuaternion* deltaRotation, nint posePtr)
	{
		if (this.isPosingEnabled && posePtr != nint.Zero)
		{
			HkaPoseStruct* pose = (HkaPoseStruct*)posePtr;
			return (nint)pose->BoneFlags.Data;
		}

		return this.hookProceduralBone!.OriginalFunction(modifierPtr, deltaRotation, posePtr);
	}

	private unsafe HkaTransform4* DetourGetBoneLocalTransform(HkaPartialSkeletonStruct* partialSkeleton, HkaTransform4* outTransform, short boneIdx)
	{
		if (this.isPosingEnabled && partialSkeleton != null)
		{
			// Local-space of bone is marked as dirty so that the game recalculates from model-space, which Anamnesis uses to push transform updates
			partialSkeleton->SetBoneLocalDirty(boneIdx);
		}

		return this.hookGetBoneLocalTransform!.OriginalFunction(partialSkeleton, outTransform, boneIdx);
	}

	private unsafe void DetourPostBoneDeformerExecute(HumanPostBoneDeformerStruct* deformer)
	{
		if (this.isPosingEnabled && deformer != null)
		{
			// Suppress all body customization deformations (bust, tail, ears) to prevent a positive feedback loop.
			// Only helmet visor rotation is allowed so that visors remain posable.
			if ((deformer->Flags & PostBoneDeformerFlags.Visor) == 0)
				return;

			PostBoneDeformerFlags originalFlags = deformer->Flags;
			deformer->Flags &= PostBoneDeformerFlags.Visor;
			try
			{
				this.hookPostBoneDeformer!.OriginalFunction(deformer);
			}
			finally
			{
				deformer->Flags = originalFlags;
			}

			return;
		}

		this.hookPostBoneDeformer!.OriginalFunction(deformer);
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
