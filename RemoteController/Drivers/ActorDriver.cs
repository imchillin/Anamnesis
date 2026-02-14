// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Drivers;

using RemoteController.Interop;
using RemoteController.Interop.Delegates;
using Serilog;
using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// A driver class for actor management and actor-related operations.
/// </summary>
[RequiresUnreferencedCode("This class is not trimming-safe")]
[RequiresDynamicCode("This class requires dynamic code due to hook reflection")]
public sealed class ActorDriver : DriverBase<ActorDriver>
{
	private readonly FunctionWrapper<Human.UpdateDrawData> updateDrawDataWrapper;
	
	/// <summary>
	/// Initializes a new instance of the <see cref="ActorDriver"/> class.
	/// </summary>
	/// <exception cref="InvalidOperationException">
	/// Thrown if the function wrapper could not be created.
	/// </exception>
	public ActorDriver()
	{
		if (Controller.SigResolver == null)
			throw new InvalidOperationException("Cannot initialize actor driver: signature resolver is not available.");

		var wrapper = HookRegistry.CreateWrapper<Human.UpdateDrawData>(out _)
			?? throw new InvalidOperationException("Failed to create UpdateDrawData function wrapper.");
		this.updateDrawDataWrapper = wrapper;
		this.RegisterInstance();
	}

	/// <summary>
	/// Calls the game's native draw data upadte function.
	/// </summary>
	/// <param name="drawObjectAddress">T
	/// The address of the human draw object.
	/// </param>
	/// <param name="data">
	/// The draw data buffer (customize + equipment).
	/// </param>
	/// <param name="skipEquipment">
	/// Whether to skip equipment updates.
	/// </param>
	/// <returns
	/// >True if the call was successful; otherwise, false.
	/// </returns>
	public unsafe bool UpdateDrawData(nint drawObjectAddress, ReadOnlySpan<byte> data, bool skipEquipment)
	{
		if (drawObjectAddress == 0 || data.Length == 0)
			return false;

		try
		{
			fixed (byte* ptr = data)
			{
				return this.updateDrawDataWrapper.OriginalFunction(drawObjectAddress, ptr, skipEquipment);
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex, $"Failed to invoke UpdateDrawData on draw object at 0x{drawObjectAddress:X}");
			return false;
		}
	}

	/// <inheritdoc/>
	protected override void OnDispose() { }
}
