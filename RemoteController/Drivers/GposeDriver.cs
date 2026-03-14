// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Drivers;

using RemoteController.Interop;
using RemoteController.Interop.Delegates;
using RemoteController.IPC;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// A driver class for monitoring the client's GPose state.
/// </summary>
[RequiresUnreferencedCode("This class is not trimming-safe")]
[RequiresDynamicCode("This class requires dynamic code due to hook reflection")]
public sealed class GposeDriver : DriverBase<GposeDriver>
{
	public const string TARGET_SYSTEM_INSTANCE_SIGNATURE = "48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 3B C6 0F 95 C0";

	private readonly FrameworkDriver frameworkDriver;
	private readonly FunctionWrapper<GameMain.IsInGPose> isInGposeWrapper;
	private readonly nint targetSystemPtr;

	private bool isInGpose;

	/// <summary>
	/// Triggered when the client user enters or exits GPose.
	/// </summary>
	public event Action<bool>? StateChanged;

	/// <summary>
	/// Initializes a new instance of the <see cref="GposeDriver"/> class.
	/// </summary>
	/// <param name="frameworkDriver">
	/// A reference to the <see cref="FrameworkDriver"/> instance used to subscribe to game tick updates.
	/// </param>
	/// <exception cref="InvalidOperationException">
	/// Thrown if an issue is encountered during the creation of the function wrapper.
	/// </exception>
	public GposeDriver(FrameworkDriver frameworkDriver)
	{
		if (Controller.SigResolver == null)
			throw new InvalidOperationException("Cannot initialize gpose driver: signature resolver is not available.");

		var wrapper = HookRegistry.CreateWrapper<GameMain.IsInGPose>(out _)
			?? throw new InvalidOperationException("Failed to create gpose function wrapper.");
		this.isInGposeWrapper = wrapper;

		this.targetSystemPtr = Controller.Scanner?.GetStaticAddressFromSig(TARGET_SYSTEM_INSTANCE_SIGNATURE) ?? nint.Zero;
		if (this.targetSystemPtr == nint.Zero)
			throw new InvalidOperationException("Failed to resolve target system signature.");

		this.frameworkDriver = frameworkDriver;
		this.frameworkDriver.GameTick += this.Update;
		this.StateChanged += this.OnGposeStateChanged;
		this.RegisterInstance();

		// Get initial reading
		this.isInGpose = this.GetGposeState();
	}

	/// <summary>
	/// Gets the current GPose state.
	/// </summary>
	public bool IsInGpose => this.isInGpose;

	/// <inheritdoc/>
	protected override void OnDispose()
	{
		this.StateChanged -= this.OnGposeStateChanged;
		this.frameworkDriver.GameTick -= this.Update;
		this.StateChanged = null;
	}

	private unsafe bool GetGposeState()
	{
		bool state = this.isInGposeWrapper.OriginalFunction();
		if (state && this.targetSystemPtr != nint.Zero)
		{
			var targetSystem = (Interop.Types.TargetSystem*)this.targetSystemPtr;
			if (targetSystem == null)
				return false;

			state = targetSystem->GPoseTarget != nint.Zero;
		}

		return state;
	}

	private void Update()
	{
		if (this.DisposedValue)
			return;

		bool newState = this.GetGposeState();
		if (newState != this.isInGpose)
		{
			this.isInGpose = newState;
			this.StateChanged?.Invoke(newState);
		}
	}

	private void OnGposeStateChanged(bool newState)
	{
		Controller.PublishEvent(EventId.GposeStateChanged, new GposeStateChangedPayload { IsInGpose = newState });
	}
}
