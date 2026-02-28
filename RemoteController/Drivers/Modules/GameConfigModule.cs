// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Drivers;

using RemoteController.Interop;
using RemoteController.Interop.Delegates;
using RemoteController.Interop.Types;
using Serilog;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using FrameworkStruct = Interop.Types.Framework;

[RequiresUnreferencedCode("This class is not trimming-safe")]
[RequiresDynamicCode("This class requires dynamic code due to hook reflection")]
public sealed class GameConfigModule
{
	private readonly FrameworkDriver driver;
	private readonly nint frameworkPtr;
	private readonly FunctionWrapper<GameConfigEntry.SetValueUInt> setValueUInt;

	public GameConfigModule(FrameworkDriver driver, nint frameworkPtr)
	{
		this.driver = driver;
		if (frameworkPtr == nint.Zero)
			throw new ArgumentException("Framework pointer cannot be null.", nameof(frameworkPtr));

		this.setValueUInt = HookRegistry.CreateWrapper<GameConfigEntry.SetValueUInt>(out _)
			?? throw new InvalidOperationException("Failed to create wrapper for GameConfigEntry.SetValueUInt. The signature may have changed.");

		this.frameworkPtr = frameworkPtr;
	}

	public async Task<uint?> GetUIntAsync(string optionName)
	{
		uint? result = null;

		await this.driver.RunOnTickAsync(() =>
		{
			unsafe
			{
				var entry = this.FindSystemConfigEntry(optionName);
				if (entry != null && entry->Type == 2) // Type 2 is UInt
				{
					result = entry->Value.UInt;
				}
			}
		});

		return result;
	}

	public async Task<bool> SetUIntAsync(string optionName, uint value)
	{
		bool success = false;

		await this.driver.RunOnTickAsync(() =>
		{
			unsafe
			{
				var entry = this.FindSystemConfigEntry(optionName);
				if (entry != null && entry->Type == 2) // 2 = UInt
				{
					this.setValueUInt.OriginalFunction((nint)entry, value, 1);
					success = true;
				}
			}
		});

		return success;
	}

	private unsafe ConfigEntry* FindSystemConfigEntry(string name)
	{
		var fptr = this.frameworkPtr;
		if (fptr == nint.Zero)
			return null;

		try
		{
			var framework = (FrameworkStruct*)fptr;
			if (framework == null)
				return null;

			var systemConfig = &framework->SystemConfig;
			if (systemConfig == null)
				return null;

			var configBase = &systemConfig->SystemConfigBase;
			if (configBase == null)
				return null;

			if (configBase->ConfigEntryArray == null || configBase->ConfigCount == 0)
				return null;

			for (uint i = 0; i < configBase->ConfigCount; i++)
			{
				var entry = &configBase->ConfigEntryArray[i];
				if (entry == null || entry->Name == null)
					continue;

				string entryName = Marshal.PtrToStringAnsi((IntPtr)entry->Name) ?? "Unknown";
				if (entryName == name)
					return entry;
			}
		}
		catch (AccessViolationException ex)
		{
			Log.Error(ex, $"Encountered access while trying to read game config entry '{name}'.");
			return null;
		}
		catch (Exception ex)
		{
			Log.Error(ex, $"Unexpected error while trying to read game config entry '{name}'.");
			return null;
		}

		return null;
	}
}
