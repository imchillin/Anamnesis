// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Drivers;

using RemoteController.Interop;
using RemoteController.Interop.Delegates;
using RemoteController.Interop.Types;
using Serilog;
using System;
using System.Diagnostics.CodeAnalysis;

using DrawDataContainerDelegates = Interop.Delegates.DrawDataContainer;
using DrawDataContainerStruct = Interop.Types.DrawDataContainer;
using DrawObjectStruct = Interop.Types.DrawObject;
using GameObjectStruct = Interop.Types.GameObject;

[Flags]
public enum RedrawFlags : byte
{
	None = 0,
	Appearance = 1 << 0, // Triggers UpdateDrawData
	Weapons = 1 << 1,    // Triggers LoadWeapon
	Facewear = 1 << 2,   // Triggers SetFacewear

	All = Appearance | Weapons | Facewear,
}

public enum RedrawType : byte
{
	None = 0, // Used for invalid or uninitialized redraw requests
	Full = 1,
	Partial = 2,
}

public class RedrawRequest(RedrawType type, int index)
{
	public RedrawType Type { get; } = type;
	public int ObjectIndex { get; } = index;
	public RedrawFlags Flags { get; set; }
	public WeaponModelId MainHandId { get; set; }
	public WeaponModelId OffHandId { get; set; }
	public ushort FacewearId { get; set; }
	public byte[]? DrawData { get; set; }
}

[RequiresUnreferencedCode("This class is not trimming-safe")]
[RequiresDynamicCode("This class requires dynamic code due to hook reflection")]
public sealed class RedrawModule
{
	private readonly FunctionWrapper<Character.DisableDraw> charDisableDraw;
	private readonly FunctionWrapper<Character.EnableDraw> charEnableDraw;
	private readonly FunctionWrapper<Human.UpdateDrawData> updateDrawData;
	private readonly FunctionWrapper<DrawDataContainerDelegates.SetFacewear> setFacewear;
	private readonly FunctionWrapper<DrawDataContainerDelegates.LoadWeapon> loadWeapon;

	private readonly nint objTablePtr;

	public RedrawModule(nint objTablePtr)
	{
		this.charDisableDraw = HookRegistry.CreateWrapper<Character.DisableDraw>(out _)
			?? throw new InvalidOperationException("Failed to create wrapper for Character.DisableDraw. The signature may have changed.");
		this.charEnableDraw = HookRegistry.CreateWrapper<Character.EnableDraw>(out _)
			?? throw new InvalidOperationException("Failed to create wrapper for Character.EnableDraw. The signature may have changed.");
		this.updateDrawData = HookRegistry.CreateWrapper<Human.UpdateDrawData>(out _)
			?? throw new InvalidOperationException("Failed to create wrapper for Human.UpdateDrawData. The signature may have changed.");
		this.setFacewear = HookRegistry.CreateWrapper<DrawDataContainerDelegates.SetFacewear>(out _)
			?? throw new InvalidOperationException("Failed to create wrapper for DrawDataContainer.SetFacewear. The signature may have changed.");
		this.loadWeapon = HookRegistry.CreateWrapper<DrawDataContainerDelegates.LoadWeapon>(out _)
			?? throw new InvalidOperationException("Failed to create wrapper for DrawDataContainer.LoadWeapon. The signature may have changed.");

		this.objTablePtr = objTablePtr;
	}

	/// <summary>
	/// Process game object redraw request
	/// </summary>
	/// <param name="req">
	/// The payload of the redraw request.
	/// </param>
	/// <returns>
	/// True if the redraw was successfully carried out; false otherwise.
	/// </returns>
	public bool RequestRedraw(RedrawRequest req)
	{
		byte[] result = FrameworkDriver.RunOnTick(() => this.ProcessInternal(req));
		bool success = result.Length > 0 && result[0] == 1;

		// If a full redraw took place, we need to wait for the game object to become visible again
		if (success && req.Type == RedrawType.Full)
		{
			this.WaitUntilVisible(req.ObjectIndex);
		}

		return success;
	}

	private byte[] ProcessInternal(RedrawRequest req)
	{
		if (req.ObjectIndex < 0 || this.objTablePtr == nint.Zero)
			return [0];

		nint entryPtr = this.objTablePtr + (req.ObjectIndex * IntPtr.Size);
		var gameObjPtr = Controller.MemoryReader?.Read<nint>(entryPtr) ?? nint.Zero;

		if (gameObjPtr == nint.Zero) return [0];

		return req.Type == RedrawType.Partial
			? this.ExecutePartialRedraw(gameObjPtr, req)
			: this.ExecuteFullRedraw(gameObjPtr);
	}

	private unsafe byte[] ExecutePartialRedraw(nint gameObjPtr, RedrawRequest req)
	{
		nint drawObjPtr = Controller.MemoryReader?.Read<nint>(gameObjPtr + GameObjectStruct.DRAW_OBJECT_OFFSET) ?? nint.Zero;
		nint drawDataPtr = gameObjPtr + Actor.DRAW_DATA_OFFSET;

		if (drawObjPtr == nint.Zero || drawDataPtr == nint.Zero)
			return [0];

		bool success = true;

		if (req.Flags.HasFlag(RedrawFlags.Appearance) && req.DrawData != null)
		{
			fixed (byte* p = req.DrawData)
			{
				success = this.updateDrawData.OriginalFunction(drawObjPtr, p, false);
			}
		}

		if (success)
		{
			if (req.Flags.HasFlag(RedrawFlags.Weapons))
			{
				this.loadWeapon.OriginalFunction(drawDataPtr, WeaponSlot.MainHand, req.MainHandId, 1, 0, 1, 0, 0);
				this.loadWeapon.OriginalFunction(drawDataPtr, WeaponSlot.OffHand, req.OffHandId, 1, 0, 1, 0, 0);
			}

			if (req.Flags.HasFlag(RedrawFlags.Facewear))
			{
				DrawDataContainerStruct* drawData = (DrawDataContainerStruct*)drawDataPtr;

				if (drawData->FacewearId == req.FacewearId)
				{
					Log.Verbose("Facewear ID matches; Manually tripping facewear dirty flag.");
					drawData->FacewearDirtyFlag |= 1;
				}
				else
				{
					byte retVal = this.setFacewear.OriginalFunction(drawDataPtr, 0, req.FacewearId);
					if (retVal != 1)
					{
						Log.Warning($"{nameof(DrawDataContainerDelegates.SetFacewear)} native function call failed for FacewearId: {req.FacewearId} on object index: {req.ObjectIndex}. Return value: {retVal}");
					}
				}
			}
		}

		return success ? [1] : [0];
	}

	private byte[] ExecuteFullRedraw(nint gameObjPtr)
	{
		this.charDisableDraw.OriginalFunction(gameObjPtr);
		this.charEnableDraw.OriginalFunction(gameObjPtr);
		return [1];
	}

	private void WaitUntilVisible(int index)
	{
		FrameworkDriver.Instance?.RunOnTickUntil(
			condition: () =>
			{
				nint entryPtr = this.objTablePtr + (index * IntPtr.Size);
				nint gameObjPtr = Controller.MemoryReader?.Read<nint>(entryPtr) ?? nint.Zero;
				if (gameObjPtr == nint.Zero)
					return true;

				nint modelPtr = Controller.MemoryReader?.Read<nint>(gameObjPtr + GameObjectStruct.DRAW_OBJECT_OFFSET) ?? nint.Zero;
				if (modelPtr == nint.Zero)
					return false;

				var drawObject = Controller.MemoryReader?.Read<DrawObjectStruct>(modelPtr);
				return drawObject?.IsVisible ?? false;
			},
			deferTicks: 0,
			timeoutTicks: 100);
	}
}
