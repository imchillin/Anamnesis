// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Drivers;

using RemoteController.Interop;
using RemoteController.IPC;
using Serilog;
using System;
using System.Diagnostics.CodeAnalysis;

using AtkModuleDelegates = RemoteController.Interop.Delegates.AtkModule;
using AtkModuleStruct = RemoteController.Interop.Types.AtkModule;

/// <summary>
/// A driver class for monitoring the client's text input (chat, search bars, etc.) state.
/// </summary>
[RequiresUnreferencedCode("This class is not trimming-safe")]
[RequiresDynamicCode("This class requires dynamic code due to hook reflection")]
public sealed class TextInputDriver : DriverBase<TextInputDriver>
{
	private readonly FrameworkDriver frameworkDriver;
	private FunctionHook<AtkModuleDelegates.HandleTextInputActive>? handleTextInputHook;
	private AtkModuleDelegates.HandleTextInputActive? textInputHookDetour;

	private bool isTextInputActive;

	/// <summary>
	/// Triggered when the client user activates or deactivates chat text input.
	/// </summary>
	public event Action<bool>? StateChanged;

	/// <summary>
	/// Initializes a new instance of the <see cref="TextInputDriver"/> class.
	/// </summary>
	/// <param name="frameworkDriver">
	/// A reference to the <see cref="FrameworkDriver"/> instance.
	/// </param>
	/// <exception cref="InvalidOperationException">
	/// Thrown if an issue is encountered during the creation of function hooks.
	/// </exception>
	public TextInputDriver(FrameworkDriver frameworkDriver)
	{
		if (Controller.SigResolver == null)
			throw new InvalidOperationException("Cannot initialize text input driver: signature resolver is not available.");

		this.frameworkDriver = frameworkDriver;

		this.RegisterInstance(() =>
		{
			this.textInputHookDetour = this.OnHandleTextInputActive;
			this.handleTextInputHook = HookRegistry.CreateAndActivateHook(this.textInputHookDetour, HookBehavior.After);

			this.StateChanged += this.OnTextInputStateChanged;

			// Get initial reading
			this.isTextInputActive = this.GetTextInputState();
			this.StateChanged?.Invoke(this.isTextInputActive);
		});
	}

	/// <summary>
	/// Gets a value indicating whether chat text input is currently active.
	/// </summary>
	public bool IsTextInputActive => this.isTextInputActive;

	/// <inheritdoc/>
	protected override void OnDispose()
	{
		this.StateChanged -= this.OnTextInputStateChanged;
		this.handleTextInputHook?.Dispose();
		this.StateChanged = null;
	}

	private unsafe bool GetTextInputState()
	{
		var module = this.frameworkDriver.GetAtkModule();
		return module != null && module->AtkInputManager.IsTextInputActive;
	}

	private unsafe nint OnHandleTextInputActive(nint atkModule, nint outResult, nint inArgs, int inArgsCount)
	{
		nint result = this.handleTextInputHook?.OriginalFunction(atkModule, outResult, inArgs, inArgsCount) ?? nint.Zero;

		try
		{
			if (atkModule == nint.Zero)
				return result;

			var module = (AtkModuleStruct*)atkModule;
			bool newState = module->AtkInputManager.IsTextInputActive;
			if (newState != this.isTextInputActive)
			{
				this.isTextInputActive = newState;
				this.StateChanged?.Invoke(newState);
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Error processing text input active state change.");
		}

		return result;
	}

	private void OnTextInputStateChanged(bool newState)
	{
		Controller.PublishEvent(EventId.TextInputStateChanged, new TextInputStateChangedPayload { IsActive = newState });
	}
}
