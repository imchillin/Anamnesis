// © Anamnesis.
// Licensed under the MIT license.

// © NonInvasiveKeyboardHook library
// Licensed under the MIT license.
// https://github.com/kfirprods/NonInvasiveKeyboardHook

// boy I hope our users trust me, cause this catches /all/ keyboard inputs, system wide.
// This could be used to create a Keylogger. =(
// https://blogs.msdn.microsoft.com/toub/2006/05/03/low-level-keyboard-hook-in-c/
namespace Anamnesis.Keyboard
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;
	using System.Threading;
	using System.Windows.Input;
	using Serilog;

	public class Hook
	{
		private const int WhKeyboardLl = 13;
		private const int WmKeyDown = 0x0100;
		private const int WmKeyUp = 0x0101;
		private const int WmSysKeyDown = 0x0104;
		private const int WmSysKeyUp = 0x0105;

		private static IntPtr hookId = IntPtr.Zero;

		private readonly HashSet<int> downKeys = new HashSet<int>();
		private readonly object modifiersLock = new object();
		private ModifierKeys modifiers = ModifierKeys.None;
		private LowLevelKeyboardProc? hook;
		private bool isStarted;

		public delegate bool KeyboardInput(Key key, KeyboardKeyStates state, ModifierKeys modifiers);
		private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

		public event KeyboardInput? OnKeyboardInput;

		public void Start()
		{
			if (this.isStarted)
				return;

			this.hook = this.HookCallback;
			hookId = SetHook(this.hook);
			this.isStarted = true;
		}

		public void Stop()
		{
			if (this.isStarted)
			{
				UnhookWindowsHookEx(hookId);
				this.isStarted = false;
			}
		}

		private static IntPtr SetHook(LowLevelKeyboardProc proc)
		{
			var userLibrary = LoadLibrary("User32");
			return SetWindowsHookEx(WhKeyboardLl, proc, userLibrary, 0);
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool UnhookWindowsHookEx(IntPtr hhk);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

		/// <summary>
		/// Loads the library.
		/// </summary>
		/// <param name="lpFileName">Name of the library.</param>
		/// <returns>A handle to the library.</returns>
		[DllImport("kernel32.dll")]
		private static extern IntPtr LoadLibrary(string lpFileName);

		private bool HandleKeyPress(Key key, KeyboardKeyStates state)
		{
			if (this.OnKeyboardInput != null)
			{
				try
				{
					return this.OnKeyboardInput.Invoke(key, state, this.modifiers);
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Failed to handle keyboard input");
				}
			}

			return false;
		}

		private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode >= 0)
			{
				var vkCode = Marshal.ReadInt32(lParam);

				if (this.HandleSingleKeyboardInput(new KeyboardParams(wParam, vkCode)))
				{
					return new IntPtr(-1);
				}
			}

			return CallNextHookEx(hookId, nCode, wParam, lParam);
		}

		private bool HandleSingleKeyboardInput(KeyboardParams keyboardParams)
		{
			bool used = false;
			var wParam = keyboardParams.Param;
			var vkCode = keyboardParams.VKeyCode;

			var modifierKey = ModifierKeysUtilities.GetModifierKeyFromCode(vkCode);
			Key key = KeyInterop.KeyFromVirtualKey(vkCode);

			// If the keyboard event is a KeyDown event (i.e. key pressed)
			if (wParam == (IntPtr)WmKeyDown || wParam == (IntPtr)WmSysKeyDown)
			{
				// In this case, we only care about modifier keys
				if (modifierKey != ModifierKeys.None)
				{
					lock (this.modifiersLock)
					{
						this.modifiers |= modifierKey;
					}
				}

				// Trigger callbacks that are registered for this key, but only once per key press
				if (!this.downKeys.Contains(vkCode))
				{
					used = this.HandleKeyPress(key, KeyboardKeyStates.Pressed);
					this.downKeys.Add(vkCode);
				}
				else
				{
					used = this.HandleKeyPress(key, KeyboardKeyStates.Down);
				}
			}

			// If the keyboard event is a KeyUp event (i.e. key released)
			if (wParam == (IntPtr)WmKeyUp || wParam == (IntPtr)WmSysKeyUp)
			{
				used = this.HandleKeyPress(key, KeyboardKeyStates.Released);

				// If the released key is a modifier key, remove it from the HashSet of modifier keys
				if (modifierKey != ModifierKeys.None)
				{
					lock (this.modifiersLock)
					{
						this.modifiers &= ~modifierKey;
					}
				}

				this.downKeys.Remove(vkCode);
			}

			return used;
		}

		internal struct KeyboardParams
		{
			public IntPtr Param;
			public int VKeyCode;

			public KeyboardParams(IntPtr wParam, int vkCode)
			{
				this.Param = wParam;
				this.VKeyCode = vkCode;
			}
		}
	}
}
