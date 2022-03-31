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

		private static readonly List<Binding> Bindings = new List<Binding>()
		{
			new Binding("TransformEditor.RotateXPlus", Key.W),
			new Binding("TransformEditor.RotateXMinus", Key.S),
		};

		private static readonly Dictionary<string, List<Func<bool>>> FunctionToCallback = new();
		private static readonly Dictionary<(Key, ModifierKeys), string> KeyToFunction = new();

		public static void RegisterHotKeyHandler(string function, Func<bool> callback)
		{
			lock (FunctionToCallback)
			{
				if (!FunctionToCallback.ContainsKey(function))
					FunctionToCallback.Add(function, new());

				FunctionToCallback[function].Add(callback);
				Log.Verbose($"Adding hotkey binding: {function}");
			}
		}

		public static void RegisterBinding(Binding bind)
		{
			if (bind.Function == null)
				return;

			RegisterHotkey(bind.Key, bind.Modifers, bind.Function);
		}

		public static void RegisterHotkey(Key key, ModifierKeys modifiers, string function)
		{
			var dicKey = (key, modifiers);

			if (KeyToFunction.ContainsKey(dicKey))
				throw new Exception($"Duplicte key binding: {key} - {modifiers} - {function}");

			KeyToFunction.Add(dicKey, function);
		}

		public override Task Start()
		{
			Hook.Start();
			Hook.OnKeyboardInput += this.OnKeyboardInput;

			foreach (Binding bind in Bindings)
			{
				RegisterBinding(bind);
			}

			return base.Start();
		}

		public override Task Shutdown()
		{
			Hook.Stop();
			return base.Shutdown();
		}

		private bool OnKeyboardInput(Key key, KeyStates state, ModifierKeys modifiers)
		{
			// Only process the hotkeys if we have focus
			bool processInputs = MainWindow.HasFocus;

			// Or if FFXIV has focus and the hooks are enabled
			if (MemoryService.DoesProcessHaveFocus && SettingsService.Current.EnableGameHotKeyHooks)
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
				if (callback.Invoke())
				{
					return true;
				}
			}

			return false;
		}

		[Serializable]
		public class Binding
		{
			public Binding(string fucntion, Key key, ModifierKeys modifiers = ModifierKeys.None)
			{
				this.Key = key;
				this.Modifers = modifiers;
				this.Function = fucntion;
			}

			public Binding()
			{
			}

			public Key Key { get; set; }
			public ModifierKeys Modifers { get; set; }
			public string? Function { get; set; }
		}
	}
}
