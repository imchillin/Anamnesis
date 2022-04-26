// © Anamnesis.
// Licensed under the MIT license.

// © NonInvasiveKeyboardHook library
// Licensed under the MIT license.
// https://github.com/kfirprods/NonInvasiveKeyboardHook
namespace Anamnesis.Keyboard;

using System.Windows.Input;

public static class ModifierKeysUtilities
{
	public static ModifierKeys GetModifierKeyFromCode(int keyCode)
	{
		switch (keyCode)
		{
			case 0xA0:
			case 0xA1:
			case 0x10:
				return ModifierKeys.Shift;

			case 0xA2:
			case 0xA3:
			case 0x11:
				return ModifierKeys.Control;

			case 0x12:
			case 0xA4:
			case 0xA5:
				return ModifierKeys.Alt;

			case 0x5B:
			case 0x5C:
				return ModifierKeys.Windows;

			default:
				return ModifierKeys.None;
		}
	}
}
