// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Keyboard;

using Anamnesis.Core;
using Anamnesis.GUI;
using Anamnesis.Memory;
using Anamnesis.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

public class HotkeyService : ServiceBase<HotkeyService>
{
	private static readonly Hook s_hotkeyHook = new();
	private static readonly Dictionary<string, List<Handler>> s_functionToHandlers = new();
	private static readonly Dictionary<(Key, ModifierKeys), string> s_keyToFunction = new();
	private static readonly HashSet<Key> s_keyDownSentToGame = new();

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
			if (!s_functionToHandlers.ContainsKey(function))
				return;

			for (int i = s_functionToHandlers[function].Count - 1; i >= 0; i--)
			{
				if (s_functionToHandlers[function][i].Owner != owner)
					continue;

				s_functionToHandlers[function].RemoveAt(i);
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

	/// <inheritdoc/>
	public override async Task Initialize()
	{
		foreach ((string function, KeyCombination key) in SettingsService.Current.KeyboardBindings.GetBinds())
			RegisterHotkey(key.Key, key.Modifiers, function);

		await base.Initialize();
	}

	/// <inheritdoc/>
	public override Task Shutdown()
	{
		s_hotkeyHook.OnKeyboardInput -= this.OnKeyboardInput;
		s_hotkeyHook.Stop();
		return base.Shutdown();
	}

	/// <inheritdoc/>
	protected override Task OnStart()
	{
		s_hotkeyHook.OnKeyboardInput += this.OnKeyboardInput;
		s_hotkeyHook.Start();
		return base.OnStart();
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
				if (MainWindow.IsActive && state == KeyboardKeyStates.Pressed)
				{
					s_keyDownSentToGame.Add(key);
					MemoryService.SendKey(key, state);
				}
			}

			if (state == KeyboardKeyStates.Released && s_keyDownSentToGame.Contains(key))
			{
				s_keyDownSentToGame.Remove(key);
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
		bool processInputs = MainWindow.IsActive && !(Keyboard.FocusedElement is TextBoxBase);

		// Or if FFXIV has focus, the hooks are enabled, and the user is in gpose.
		if (MemoryService.DoesProcessHaveFocus && SettingsService.Current.EnableGameHotkeyHooks && GposeService.Instance.IsGpose)
			processInputs = true;

		if (!processInputs)
			return false;

		var dicKey = (key, modifiers);

		if (!s_keyToFunction.ContainsKey(dicKey))
			return false;

		string func = s_keyToFunction[dicKey];

		if (!s_functionToHandlers.ContainsKey(func))
			return false;

		foreach (Handler handler in s_functionToHandlers[func])
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
