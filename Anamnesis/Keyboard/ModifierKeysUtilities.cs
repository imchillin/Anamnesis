// © Anamnesis.
// Licensed under the MIT license.

// © NonInvasiveKeyboardHook library
// Licensed under the MIT license.
// https://github.com/kfirprods/NonInvasiveKeyboardHook
namespace Anamnesis.Keyboard;

using System.Windows.Input;

public static class ModifierKeysUtilities
{
	public static bool IsModifierKey(Key key)
	{
		return key is Key.LeftShift or Key.RightShift
			or Key.LeftCtrl or Key.RightCtrl
			or Key.LeftAlt or Key.RightAlt
			or Key.LWin or Key.RWin
			or Key.CapsLock or Key.Capital
			or Key.NumLock or Key.Scroll
			or Key.System;
	}

	public static ModifierKeys GetModifierKeyFromCode(int keyCode)
	{
		return keyCode switch
		{
			0xA0 or 0xA1 or 0x10 => ModifierKeys.Shift,
			0xA2 or 0xA3 or 0x11 => ModifierKeys.Control,
			0x12 or 0xA4 or 0xA5 => ModifierKeys.Alt,
			0x5B or 0x5C => ModifierKeys.Windows,
			_ => ModifierKeys.None,
		};
	}
}
