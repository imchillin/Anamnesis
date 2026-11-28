// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Drivers;

using RemoteController.Extensions;
using RemoteController.Interop;
using RemoteController.Interop.Delegates;
using RemoteController.Interop.Types;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

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
/// - Credits to Karou for discovering the solution for eye pupil and iris bone manipulation.<br/>
/// - Credits to Karou for helping figure out how to cleanly toggle physics freezing on/off when posing is enabled.
/// </remarks>
[RequiresUnreferencedCode("This class is not trimming-safe")]
[RequiresDynamicCode("This class requires dynamic code due to hook reflection")]
public sealed class PosingDriver : DriverBase<PosingDriver>
{
	// NOTES:
	// - "SetBoneModelTransform" is used to freeze physics simulations by preventing the game from updating bone transforms.
	//   - When physics are unfrozen during posing, BonePhysicsChain writes rigid transforms with Scale = (1, 1, 1); preserving existing ModelPose.Scale keeps customization and manual scaling intact.
	// - "HkaPartialSkeleton.GetBoneModelMatrix" feeds bone matrices to BonePhysicsChain, which assumes unit-length orthogonal axes (Scale = 1); stripping Scale prevents non-uniform scale rotation loops (e.g., on breast bones) and other types of distortions.
	// - The kinematic driver is hooked to allow for the posing of secondary physics bones that are driven by kine drivers (e.g. visor, clothing top bones, etc.).
	// - "LookAtIkSolver.Solve" is hooked to prevent the game from updating head and eye look-at targets, allowing for manual posing of the head and eyes.
	// - "CalculateBoneModelSpace" and "SyncModelSpace" are hooked to prevent the game from overwriting manual pose adjustments when it calculates bone transforms.
	// - "GameObject.SetPosition" is hooked to prevent the GPose camera from resetting its position.
	// - "UpdateVisualPosition" and "UpdateVisualRotation" are hooked to prevent the game from updating the visual position and rotation of objects.
	// - "HkaPartialSkeleton.GetBoneLocalTransform" is hooked to allow for scale override of eye pupil and iris bones. This is necessary as Anamnesis does not currently modify the local-space pose, just model-space.
	// - "HumanPostBoneDeformer.Execute" is hooked to prevent customization deformations from compounding during posing, while still forwarding adjusted bust scale ratios and Viera ear length shifts when physics are active.

	private const PostBoneDeformerFlags TRANSLATION_PASS_FLAGS = PostBoneDeformerFlags.VieraEars;

	[ThreadStatic]
	private static DeformerPass s_deformerPass;

	private readonly FrameworkDriver frameworkDriver;
	private readonly GposeDriver gposeDriver;
	private readonly ConcurrentDictionary<(nint Deformer, nint Skeleton), Vector3> lastBustScales = new();
	private readonly ConcurrentDictionary<(nint PartialSkeleton, short BoneIndex), (Vector3 Shifted, Vector3 Unshifted, bool Pending)> deformerShifts = new();

	private FunctionHook<GameObjectDelegates.SetPosition>? hookSetPosition;
	private FunctionHook<HkaPartialSkeletonDelegate.SetBoneModelTransform>? hookPhysics;
	private FunctionHook<HkaPartialSkeletonDelegate.GetBoneModelMatrix>? hookGetBoneModelMatrix;
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
	private HkaPartialSkeletonDelegate.GetBoneModelMatrix? getBoneModelMatrixDetour;
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

	// Front-buffer (requested) state written by IPC commands and read by property getters
	private volatile bool pendingPosingEnabled = false;
	private volatile bool pendingPhysicsFrozen = false;
	private volatile bool pendingWorldVisualStateFrozen = false;

	// Back-buffer (active) state committed at the start of each frame and read by detours
	private volatile bool isPosingEnabled = false;
	private volatile bool arePhysicsFrozen = false;
	private volatile bool isWorldVisualStateFrozen = false;

	private enum DeformerPass : byte
	{
		None,
		Passthrough,
		TranslationOnly,
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="PosingDriver"/> class.
	/// </summary>
	/// <param name="frameworkDriver">
	/// A reference to the <see cref="FrameworkDriver"/> instance used to commit state changes at frame boundaries.
	/// </param>
	/// <param name="gposeDriver">
	/// A reference to the <see cref="GposeDriver"/> instance used to subscribe to GPose state updates.
	/// </param>
	/// <exception cref="InvalidOperationException">
	/// Thrown if the signature resolver is not available, which is required for creating function hooks.
	/// </exception>
	public PosingDriver(FrameworkDriver frameworkDriver, GposeDriver gposeDriver)
	{
		if (Controller.SigResolver == null)
			throw new InvalidOperationException("Cannot initialize posing driver: signature resolver is not available.");

		this.frameworkDriver = frameworkDriver;
		this.gposeDriver = gposeDriver;

		this.RegisterInstance(() =>
		{
			unsafe
			{
				// NOTE: Keep references to the detour methods to prevent them from being garbage collected
				this.physicsDetour = this.DetourSetBoneModelTransform;
				this.getBoneModelMatrixDetour = this.DetourGetBoneModelMatrix;
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
				this.hookGetBoneModelMatrix = HookRegistry.CreateAndActivateHook(this.getBoneModelMatrixDetour);
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

			this.frameworkDriver.GameTick += this.OnGameTick;
			this.gposeDriver.StateChanged += this.OnGposeStateChanged;
		});
	}

	/// <summary>
	/// Gets or sets a value indicating whether physics simulations (clothing, hair, etc.) are frozen.
	/// </summary>
	public bool FreezePhysics
	{
		get => this.pendingPhysicsFrozen;
		set => this.pendingPhysicsFrozen = value;
	}

	/// <summary>
	/// Gets or sets a value indicating whether bone transforms (Position, Rotation, Scale) are frozen.
	/// Enabling this prevents the game from overwriting manual pose adjustments.
	/// </summary>
	public bool PosingEnabled
	{
		get => this.pendingPosingEnabled;
		set => this.pendingPosingEnabled = value;
	}

	/// <summary>
	/// Gets or sets a value indicating whether world visual state (position and rotation) is frozen.
	/// When enabled, game objects will retain their current visual position and rotation, preventing the game from updating them.
	/// </summary>
	public bool FreezeWorldVisualState
	{
		get => this.pendingWorldVisualStateFrozen;
		set => this.pendingWorldVisualStateFrozen = value;
	}

	/// <inheritdoc/>
	protected override void OnDispose()
	{
		if (this.frameworkDriver != null)
			this.frameworkDriver.GameTick -= this.OnGameTick;

		if (this.gposeDriver != null)
			this.gposeDriver.StateChanged -= this.OnGposeStateChanged;

		this.pendingPosingEnabled = false;
		this.pendingPhysicsFrozen = false;
		this.pendingWorldVisualStateFrozen = false;
		this.isPosingEnabled = false;
		this.arePhysicsFrozen = false;
		this.isWorldVisualStateFrozen = false;
		this.lastBustScales.Clear();
		this.deformerShifts.Clear();

		this.hookPhysics?.Dispose();
		this.hookGetBoneModelMatrix?.Dispose();
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

	private unsafe void DetourGetBoneModelMatrix(HkaPartialSkeletonStruct* partialSkeleton, Matrix4x4* outMatrix, short boneIdx)
	{
		HkaTransform4* transform = this.isPosingEnabled && !this.arePhysicsFrozen && partialSkeleton != null && outMatrix != null
			? partialSkeleton->GetBoneModelTransform(boneIdx)
			: null;

		if (transform == null)
		{
			this.hookGetBoneModelMatrix!.OriginalFunction(partialSkeleton, outMatrix, boneIdx);
			return;
		}

		// Undo any post-physics deformer translation shifts so the solver sees the unshifted bone position
		Vector3 pos = transform->Translation.ToVector3();
		if (!this.deformerShifts.IsEmpty
			&& this.deformerShifts.TryGetValue(((nint)partialSkeleton, boneIdx), out var shift)
			&& shift.Shifted != shift.Unshifted
			&& pos.IsApproximately(shift.Shifted))
		{
			pos = shift.Unshifted;
		}

		// Return a unit-scale matrix so non-uniform bone scaling does not corrupt the physics calculations
		*outMatrix = transform->ToRigidMatrix(pos);
	}

	private unsafe nint DetourSetBoneModelTransform(HkaPartialSkeletonStruct* partialSkeleton, ulong boneId, HkaTransform4* transform, byte bUpdateSecondaryPose, byte bPropagate)
	{
		// Block physics writebacks while physics are frozen, but allow deformer passes through
		bool fromPhysics = s_deformerPass == DeformerPass.None;
		if (this.arePhysicsFrozen && partialSkeleton != null && fromPhysics)
			return (nint)partialSkeleton;

		HkaTransform4* existing = this.isPosingEnabled && !this.arePhysicsFrozen && partialSkeleton != null && transform != null && s_deformerPass != DeformerPass.Passthrough
			? partialSkeleton->GetBoneModelTransform((short)boneId)
			: null;

		if (existing == null)
			return this.hookPhysics!.OriginalFunction(partialSkeleton, boneId, transform, bUpdateSecondaryPose, bPropagate);

		var key = ((nint)partialSkeleton, (short)boneId);
		if (fromPhysics)
		{
			// Mark any tracked deformer-shifted bone so its translation shift is re-applied this frame
			if (!this.deformerShifts.IsEmpty && this.deformerShifts.TryGetValue(key, out var current) && !current.Pending)
				this.deformerShifts.TryUpdate(key, (default, default, true), current);
		}
		else
		{
			// Skip scale-only base writes; only allow translation-shifted bones
			Vector3 oldPos = existing->Translation.ToVector3();
			Vector3 newPos = transform->Translation.ToVector3();
			if (oldPos.IsApproximately(newPos))
				return (nint)partialSkeleton;

			// Register on first sight and only apply the shift once physics has written this bone
			var shift = this.deformerShifts.GetOrAdd(key, (default, default, false));
			if (!shift.Pending)
				return (nint)partialSkeleton;

			this.deformerShifts.TryUpdate(key, (newPos, oldPos, false), shift);
		}

		// Preserve existing model-space scale since physics writebacks reset scale to (1, 1, 1)
		HkaTransform4 preserved = *transform;
		preserved.Scale = existing->Scale;
		return this.hookPhysics!.OriginalFunction(partialSkeleton, boneId, &preserved, bUpdateSecondaryPose, bPropagate);
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
		if (!this.isPosingEnabled || deformer == null)
		{
			this.hookPostBoneDeformer!.OriginalFunction(deformer);
			return;
		}

		PostBoneDeformerFlags originalFlags = deformer->Flags;
		Vector3 originalBustScale = deformer->BustScale;
		PostBoneDeformerFlags passthroughFlags = PostBoneDeformerFlags.None;

		// Apply only the incremental bust scale ratio so manual bust scaling does not compound every frame
		if ((originalFlags & PostBoneDeformerFlags.Bust) != 0)
		{
			var bustKey = ((nint)deformer, deformer->Skeleton);
			Vector3 lastBustScale = this.lastBustScales.GetOrAdd(bustKey, originalBustScale);
			if (!originalBustScale.IsApproximately(lastBustScale))
			{
				deformer->BustScale = originalBustScale.SafeDivide(lastBustScale);
				passthroughFlags |= PostBoneDeformerFlags.Bust;
				this.lastBustScales[bustKey] = originalBustScale;
			}
		}

		// Re-apply translation-based deformer shifts after unfrozen physics reset bone positions to bind pose
		PostBoneDeformerFlags translationPassFlags = this.arePhysicsFrozen
			? PostBoneDeformerFlags.None
			: originalFlags & TRANSLATION_PASS_FLAGS;

		if ((passthroughFlags | translationPassFlags) == PostBoneDeformerFlags.None)
			return;

		try
		{
			// Run passthrough passes separately from translation-only passes so SetBoneModelTransform(..) can apply distinct policies
			ReadOnlySpan<(DeformerPass Pass, PostBoneDeformerFlags Flags)> passes =
			[
				(DeformerPass.Passthrough, passthroughFlags),
				(DeformerPass.TranslationOnly, translationPassFlags),
			];

			foreach (var (pass, flags) in passes)
			{
				if (flags == PostBoneDeformerFlags.None)
					continue;

				s_deformerPass = pass;
				deformer->Flags = flags;
				this.hookPostBoneDeformer!.OriginalFunction(deformer);
			}
		}
		finally
		{
			s_deformerPass = DeformerPass.None;
			deformer->Flags = originalFlags;
			deformer->BustScale = originalBustScale;
		}
	}

	private void OnGameTick()
	{
		bool posing = this.pendingPosingEnabled;
		bool physicsFrozen = this.pendingPhysicsFrozen;
		bool worldFrozen = this.pendingWorldVisualStateFrozen;

		if (posing == this.isPosingEnabled && physicsFrozen == this.arePhysicsFrozen && worldFrozen == this.isWorldVisualStateFrozen)
			return;

		if (!posing)
			this.lastBustScales.Clear();

		if (!posing || physicsFrozen)
			this.deformerShifts.Clear();

		this.isPosingEnabled = posing;
		this.arePhysicsFrozen = physicsFrozen;
		this.isWorldVisualStateFrozen = worldFrozen;
	}

	private void OnGposeStateChanged(bool isGpose)
	{
		if (!isGpose)
		{
			this.PosingEnabled = false;
			this.FreezePhysics = false;
			this.FreezeWorldVisualState = false;
			this.OnGameTick();
		}
	}
}
