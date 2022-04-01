// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Keyboard
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using System.Windows.Input;
	using Anamnesis.GUI;
	using Anamnesis.Memory;
	using Anamnesis.Services;

	public class HotkeyService : ServiceBase<HotkeyService>
	{
		private static readonly Hook Hook = new();

		private static readonly Dictionary<string, List<Func<KeyboardKeyStates, bool>>> FunctionToCallback = new();
		private static readonly Dictionary<(Key, ModifierKeys), string> KeyToFunction = new();

		public static void RegisterHotkeyHandler(string function, Action callback)
		{
			RegisterHotkeyHandler(function, () =>
			{
				callback.Invoke();
				return true;
			});
		}

		public static void RegisterHotkeyHandler(string function, Func<bool> callback)
		{
			RegisterHotkeyHandler(function, (s) =>
			{
				if (s == KeyboardKeyStates.Pressed)
				{
					return callback.Invoke();
				}

				return true;
			});
		}

		public static void RegisterHotkeyHandler(string function, Func<KeyboardKeyStates, bool> callback)
		{
			lock (FunctionToCallback)
			{
				if (!FunctionToCallback.ContainsKey(function))
					FunctionToCallback.Add(function, new());

				FunctionToCallback[function].Add(callback);
				Log.Verbose($"Adding hotkey binding: {function}");
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
			Hook.Start();
			Hook.OnKeyboardInput += this.OnKeyboardInput;

			foreach ((string function, KeyCombination keys) in SettingsService.Current.KeyboardBindings)
			{
				RegisterHotkey(keys.Key, keys.Modifiers, function);
			}

			return base.Start();
		}

		public override Task Shutdown()
		{
			Hook.Stop();
			return base.Shutdown();
		}

		private bool OnKeyboardInput(Key key, KeyboardKeyStates state, ModifierKeys modifiers)
		{
			bool handled = this.HandleKey(key, state, modifiers);

			// Forward any unused keys to ffxiv if Anamnesis has focus
			if (!handled && MainWindow.HasFocus && Keyboard.FocusedElement == null)
			{
				MemoryService.SendKey(key, state);
			}

			return handled;
		}

		private bool HandleKey(Key key, KeyboardKeyStates state, ModifierKeys modifiers)
		{
			// Only process the hotkeys if we have focus but not to a text box.
			bool processInputs = MainWindow.HasFocus && Keyboard.FocusedElement == null;

			// Or if FFXIV has focus, the hooks are enabled, and the user is in gpose.
			if (MemoryService.DoesProcessHaveFocus && SettingsService.Current.EnableGameHotkeyHooks && GposeService.Instance.IsGpose)
				processInputs = true;

			if (!processInputs)
				return false;

			var dicKey = (key, modifiers);

			if (!KeyToFunction.ContainsKey(dicKey))
				return false;

			string func = KeyToFunction[dicKey];

			if (!FunctionToCallback.ContainsKey(func))
				return false;

			foreach (var callback in FunctionToCallback[func])
			{
				if (callback.Invoke(state))
				{
					return true;
				}
			}

			return false;
		}
	}
}
