// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Keyboard;

using Anamnesis.Core;
using Anamnesis.GUI;
using Anamnesis.Memory;
using Anamnesis.Services;
using RemoteController.IPC;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

public class HotkeyService : ServiceBase<HotkeyService>
{
	private const int INITIAL_STATE_RETRY_MS = 1000;

	private static readonly Hook s_hotkeyHook = new();
	private static readonly Dictionary<string, List<Handler>> s_functionToHandlers = new();
	private static readonly Dictionary<(Key, ModifierKeys), string> s_keyToFunction = new();
	private static readonly HashSet<Key> s_keyDownSentToGame = new();

	private EventSubscription? textInputActiveEvent;
	private bool isChatInputActive = false;
	private bool hasInitialChatState = false;

	/// <inheritdoc/>
	protected override IEnumerable<IService> Dependencies =>
	[
		SettingsService.Instance,
		MemoryService.Instance,
		GposeService.Instance
	];

	public static void RegisterHotkeyHandler(string function, Action callback)
	{
		RegisterHotkeyHandler(function, new Handler(callback));
	}

	public static void RegisterHotkeyHandler(string function, Func<bool> callback)
	{
		RegisterHotkeyHandler(function, new Handler(callback));
	}

	public static void RegisterHotkeyHandler(string function, Func<KeyboardKeyStates, bool> callback)
	{
		RegisterHotkeyHandler(function, new Handler(callback));
	}

	public static void RegisterHotkeyHandler(string function, Handler handler)
	{
		lock (s_functionToHandlers)
		{
			if (!s_functionToHandlers.ContainsKey(function))
				s_functionToHandlers.Add(function, new());

			s_functionToHandlers[function].Insert(0, handler);
			Log.Verbose($"Adding hotkey binding: {function} for {handler.Owner}");
		}
	}

	public static void ClearHotkeyHandler(string function, object owner)
	{
		lock (s_functionToHandlers)
		{
			if (!s_functionToHandlers.TryGetValue(function, out List<Handler>? value))
				return;

			for (int i = value.Count - 1; i >= 0; i--)
			{
				if (value[i].Owner != owner)
					continue;

				value.RemoveAt(i);
			}

			Log.Verbose($"Clearing hotkey binding: {function} for {owner}");
		}
	}

	public static void RegisterHotkey(Key key, ModifierKeys modifiers, string function)
	{
		var dicKey = (key, modifiers);

		if (s_keyToFunction.ContainsKey(dicKey))
			throw new Exception($"Duplicate key binding: {key} - {modifiers} - {function}");

		s_keyToFunction.Add(dicKey, function);
	}

	public static KeyCombination? GetBind(string function)
	{
		foreach ((var keys, string func) in s_keyToFunction)
		{
			if (func == function)
			{
				return new KeyCombination(keys.Item1, keys.Item2);
			}
		}

		return null;
	}

	/// <summary>
	/// Checks if the game process' chat has focus for text input.
	/// </summary>
	/// <returns>True if the chat window is active, false otherwise.</returns>
	public static bool? IsTextInputActive()
	{
		if (ControllerService.InstanceOrNull?.IsConnected != true)
			return null;

		bool? result = null;
		try
		{
			result = ControllerService.Instance.SendDriverCommand<bool>(DriverCommand.GetIsTextInputActive);
			if (result == null)
			{
				Log.Warning("Text input driver command did not return a result.");
			}
		}
		catch
		{
			Log.Verbose("Failed to query game chat focus state via driver command.");
		}

		return result;
	}

	/// <inheritdoc/>
	public override async Task Initialize()
	{
		foreach ((string function, KeyCombination key) in Settings.Binds.GetBinds())
			RegisterHotkey(key.Key, key.Modifiers, function);

		await base.Initialize();
	}

	/// <inheritdoc/>
	public override Task Shutdown()
	{
		this.textInputActiveEvent?.Dispose();
		this.textInputActiveEvent = null;
		this.isChatInputActive = false;
		this.hasInitialChatState = false;
		s_keyDownSentToGame.Clear();
		s_hotkeyHook.OnKeyboardInput -= this.OnKeyboardInput;
		s_hotkeyHook.Stop();

		return base.Shutdown();
	}

	/// <inheritdoc/>
	protected override Task OnStart()
	{
		this.textInputActiveEvent = ControllerService.Instance.SubscribeEvent<TextInputStateChangedPayload>(
			EventId.TextInputStateChanged,
			this.OnTextInputStateChanged);

		s_hotkeyHook.OnKeyboardInput -= this.OnKeyboardInput;
		s_hotkeyHook.OnKeyboardInput += this.OnKeyboardInput;
		s_hotkeyHook.Start();

		this.CancellationTokenSource = new CancellationTokenSource();
		this.BackgroundTask = Task.Run(() => this.QueryInitialState(this.CancellationToken));

		return base.OnStart();
	}

	private async Task QueryInitialState(CancellationToken cancellationToken)
	{
		while (!this.hasInitialChatState && !cancellationToken.IsCancellationRequested)
		{
			bool? initialState = IsTextInputActive();
			if (initialState.HasValue)
			{
				Log.Debug($"Initial game chat focus state: {(initialState.Value ? "Active" : "Inactive")}");
				if (initialState.Value != this.isChatInputActive)
				{
					this.isChatInputActive = initialState.Value;
				}

				this.hasInitialChatState = true;
				return;
			}

			try
			{
				await Task.Delay(INITIAL_STATE_RETRY_MS, cancellationToken);
			}
			catch (TaskCanceledException)
			{
				break;
			}
		}
	}

	private bool HandleKey(Key key, KeyboardKeyStates state, ModifierKeys modifiers)
	{
		if (!SettingsService.Current.EnableHotkeys || this.isChatInputActive)
			return false;

		// Only process the hotkeys if we have focus but not to a text box.
		bool processInputs = MainWindow.IsActive && Keyboard.FocusedElement is not TextBoxBase;

		// Or if FFXIV has focus, the hooks are enabled, and the user is in gpose.
		if (!processInputs && MemoryService.DoesProcessHaveFocus && SettingsService.Current.EnableGameHotkeyHooks && GposeService.Instance.IsGpose)
			processInputs = true;

		if (!processInputs)
			return false;

		var dicKey = (key, modifiers);

		if (!s_keyToFunction.TryGetValue(dicKey, out string? func))
			return false;

		if (!s_functionToHandlers.TryGetValue(func, out List<Handler>? value))
			return false;

		foreach (Handler handler in value)
		{
			if (handler.Invoke(state))
			{
				return true;
			}
		}

		return false;
	}

	private bool OnKeyboardInput(Key key, KeyboardKeyStates state, ModifierKeys modifiers)
	{
		// Skip special keys when chat is not active to preserve hotkey functionality in Anamnesis
		if (!this.isChatInputActive && key is Key.Tab or Key.Return or Key.Escape)
			return false;

		bool handled = this.HandleKey(key, state, modifiers);

		if (SettingsService.Current.ForwardKeys)
		{
			// Forward any unused keys to FFXIV if Anamnesis has focus
			if (!handled && MainWindow.IsActive && Keyboard.FocusedElement is not TextBoxBase)
			{
				if (state == KeyboardKeyStates.Pressed)
				{
					s_keyDownSentToGame.Add(key);
					MemoryService.SendKey(key, state);

					if (!ModifierKeysUtilities.IsModifierKey(key))
					{
						handled = true;
					}
				}
				else if (state == KeyboardKeyStates.Down && s_keyDownSentToGame.Contains(key))
				{
					if (!ModifierKeysUtilities.IsModifierKey(key))
					{
						MemoryService.SendKey(key, state);
						handled = true;
					}
				}
			}

			// Always ensure matching release is sent for any key that was previously forwarded
			if (state == KeyboardKeyStates.Released && s_keyDownSentToGame.Remove(key))
			{
				MemoryService.SendKey(key, state);

				if (!ModifierKeysUtilities.IsModifierKey(key))
				{
					handled = true;
				}
			}
		}

		return handled;
	}

	private void OnTextInputStateChanged(TextInputStateChangedPayload payload)
	{
		this.isChatInputActive = payload.IsActive;
		Log.Verbose($"Game chat focus state changed: {(this.isChatInputActive ? "Active" : "Inactive")}");

		// Release any currently forwarded keys immediately when chat opens
		if (this.isChatInputActive && s_keyDownSentToGame.Count > 0)
		{
			foreach (Key key in s_keyDownSentToGame)
			{
				MemoryService.SendKey(key, KeyboardKeyStates.Released);
			}

			s_keyDownSentToGame.Clear();
		}
	}

	public class Handler
	{
		public Func<KeyboardKeyStates, bool> Callback;
		public object? Owner;

		public Handler(Func<KeyboardKeyStates, bool> callback)
		{
			this.Callback = callback;
			this.Owner = callback.Target;
		}

		public Handler(Func<bool> callback)
		{
			this.Callback = (s) =>
			{
				if (s == KeyboardKeyStates.Pressed)
				{
					return callback.Invoke();
				}

				return true;
			};

			this.Owner = callback.Target;
		}

		public Handler(Action callback)
		{
			this.Callback = (s) =>
			{
				if (s == KeyboardKeyStates.Pressed)
					callback.Invoke();

				return true;
			};

			this.Owner = callback.Target;
		}

		public bool Invoke(KeyboardKeyStates state)
		{
			if (this.Callback.Target is UIElement uiElement)
			{
				if (!uiElement.IsVisible)
				{
					return false;
				}
			}

			return this.Callback.Invoke(state);
		}
	}
}
