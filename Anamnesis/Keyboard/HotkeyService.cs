// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Keyboard;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Anamnesis.GUI;
using Anamnesis.Memory;
using Anamnesis.Services;
using XivToolsWpf;

public class HotkeyService : ServiceBase<HotkeyService>
{
	private static readonly Hook Hook = new();

	private static readonly Dictionary<string, List<Handler>> FunctionToHandlers = new();
	private static readonly Dictionary<(Key, ModifierKeys), string> KeyToFunction = new();

	private static readonly HashSet<Key> KeyDownSentToGame = new();

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
		lock (FunctionToHandlers)
		{
			if (!FunctionToHandlers.ContainsKey(function))
				FunctionToHandlers.Add(function, new());

			FunctionToHandlers[function].Insert(0, handler);
			Log.Verbose($"Adding hotkey binding: {function} for {handler.Owner}");
		}
	}

	public static void ClearHotkeyHandler(string function, object owner)
	{
		lock (FunctionToHandlers)
		{
			if (!FunctionToHandlers.ContainsKey(function))
				return;

			for (int i = FunctionToHandlers[function].Count - 1; i >= 0; i--)
			{
				if (FunctionToHandlers[function][i].Owner != owner)
					continue;

				FunctionToHandlers[function].RemoveAt(i);
			}

			Log.Verbose($"Clearing hotkey binding: {function} for {owner}");
		}
	}

	public static void RegisterHotkey(Key key, ModifierKeys modifiers, string function)
	{
		var dicKey = (key, modifiers);

		if (KeyToFunction.ContainsKey(dicKey))
			throw new Exception($"Duplicte key binding: {key} - {modifiers} - {function}");

		KeyToFunction.Add(dicKey, function);
	}

	public static KeyCombination? GetBind(string function)
	{
		foreach ((var keys, string func) in KeyToFunction)
		{
			if (func == function)
			{
				return new KeyCombination(keys.Item1, keys.Item2);
			}
		}

		return null;
	}

	public override Task Start()
	{
		Task.Run(async () =>
		{
			// Slight delay before starting the keyboard binding service.
			await Task.Delay(1000);
			await Dispatch.MainThread();

			Hook.OnKeyboardInput += this.OnKeyboardInput;

			foreach ((string function, KeyCombination key) in SettingsService.Current.KeyboardBindings.GetBinds())
			{
				RegisterHotkey(key.Key, key.Modifiers, function);
			}

			Hook.Start();
		});

		return base.Start();
	}

	public override Task Shutdown()
	{
		Hook.Stop();
		return base.Shutdown();
	}

	private bool AnamnesisWindowIsActive()
	{
		// TODO!
		return false;
	}

	private bool OnKeyboardInput(Key key, KeyboardKeyStates state, ModifierKeys modifiers)
	{
		// Do not intercept or forward these keys.
		if (key == Key.Tab || key == Key.Return || key == Key.Escape)
			return false;

		bool handled = this.HandleKey(key, state, modifiers);

		if (SettingsService.Current.ForwardKeys)
		{
			// Forward any unused keys to ffxiv if Anamnesis has focus
			if (!handled && !(Keyboard.FocusedElement is TextBoxBase))
			{
				if (this.AnamnesisWindowIsActive() && state == KeyboardKeyStates.Pressed)
				{
					KeyDownSentToGame.Add(key);
					MemoryService.SendKey(key, state);
				}
			}

			if (state == KeyboardKeyStates.Released && KeyDownSentToGame.Contains(key))
			{
				KeyDownSentToGame.Remove(key);
				MemoryService.SendKey(key, state);
			}
		}

		return handled;
	}

	private bool HandleKey(Key key, KeyboardKeyStates state, ModifierKeys modifiers)
	{
		if (!SettingsService.Current.EnableHotkeys)
			return false;

		// Only process the hotkeys if we have focus but not to a text box.
		bool processInputs = this.AnamnesisWindowIsActive() && !(Keyboard.FocusedElement is TextBoxBase);

		// Or if FFXIV has focus, the hooks are enabled, and the user is in gpose.
		if (MemoryService.DoesProcessHaveFocus && SettingsService.Current.EnableGameHotkeyHooks && GposeService.Instance.IsGpose)
			processInputs = true;

		if (!processInputs)
			return false;

		var dicKey = (key, modifiers);

		if (!KeyToFunction.ContainsKey(dicKey))
			return false;

		string func = KeyToFunction[dicKey];

		if (!FunctionToHandlers.ContainsKey(func))
			return false;

		foreach (Handler handler in FunctionToHandlers[func])
		{
			if (handler.Invoke(state))
			{
				return true;
			}
		}

		return false;
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
