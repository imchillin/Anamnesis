// © Anamnesis.
// Licensed under the MIT license.

namespace Bootstrap;

using System;
using System.Runtime.InteropServices;

internal static class User32
{
	public static bool MessageBox(string text, string caption, MessageBoxButtons type, MessageBoxIcon icon)
	{
		uint v = (uint)type | (uint)icon;
		int ret = MessageBox(IntPtr.Zero, text, caption, v);
		return ret == 1 || ret == 6 || ret == 11;
	}

	[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
	private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

	public enum MessageBoxButtons : uint
	{
		AbortRetryIgnore = 0x00000002,
		CancelRetryContinue = 0x00000006,
		OkHelp = 0x00004000,
		Ok = 0x00000000,
		OkCancel = 0x00000001,
		RetryCancel = 0x00000005,
		YesNo = 0x00000004,
		YesNoCancel = 0x00000003,
	}

	public enum MessageBoxIcon : uint
	{
		Exclamation = 0x00000030,
		Warning = 0x00000030,
		Information = 0x00000040,
		Asterisk = 0x00000040,
		Question = 0x00000020,
		Stop = 0x00000010,
		Error = 0x00000010,
		Hand = 0x00000010,
	}
}
