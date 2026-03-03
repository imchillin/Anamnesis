// © Anamnesis.
// Licensed under the MIT license.

using RemoteController.Memory;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace RemoteController.IPC;

public enum DriverCommand : int
{
	Invalid = 0,

	// Posing driver commands
	GetPosingEnabled,
	SetPosingEnabled,
	GetFreezePhysics,
	SetFreezePhysics,
	GetFreezeWorldVisualState,
	SetFreezeWorldVisualState,

	// Gpose driver commands
	GetIsInGpose,
}

/// <summary>
/// Factory methods for creating type-safe driver command handlers.
/// </summary>
[RequiresUnreferencedCode("This class is not trimming-safe")]
[RequiresDynamicCode("This class requires dynamic code due to hook reflection")]
public static class DriverCommandHandler
{
	private static readonly byte[] s_successResponse = [1];
	private static readonly byte[] s_failureResponse = [0];

	/// <summary>
	/// Creates a getter handler for a boolean value.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Func<ReadOnlySpan<byte>, byte[]> Getter(Func<bool> getter)
	{
		return _ => [getter() ? (byte)1 : (byte)0];
	}

	/// <summary>
	/// Creates a setter handler for a boolean value.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Func<ReadOnlySpan<byte>, byte[]> Setter(Action<bool> setter)
	{
		return args =>
		{
			if (args.Length < 1)
				return s_failureResponse;

			setter(args[0] != 0);
			return s_successResponse;
		};
	}

	/// <summary>
	/// Creates a getter handler for an unmanaged value type.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Func<ReadOnlySpan<byte>, byte[]> Getter<T>(Func<T> getter)
		where T : unmanaged
	{
		return _ => MarshalUtils.Serialize(getter());
	}

	/// <summary>
	/// Creates a setter handler for an unmanaged value type.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Func<ReadOnlySpan<byte>, byte[]> Setter<T>(Action<T> setter)
		where T : unmanaged
	{
		return args =>
		{
			if (args.Length < Unsafe.SizeOf<T>())
				return s_failureResponse;

			setter(MarshalUtils.Deserialize<T>(args));
			return s_successResponse;
		};
	}

	/// <summary>
	/// Creates a handler that invokes an action with no arguments and returns success.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Func<ReadOnlySpan<byte>, byte[]> Action(Action action)
	{
		return _ =>
		{
			action();
			return s_successResponse;
		};
	}

	/// <summary>
	/// Creates a handler that invokes a function with typed arguments and returns a typed result.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Func<ReadOnlySpan<byte>, byte[]> Invoke<TArg, TResult>(Func<TArg, TResult> func)
		where TArg : unmanaged
		where TResult : unmanaged
	{
		return args =>
		{
			if (args.Length < Unsafe.SizeOf<TArg>())
				return [];

			TArg arg = MarshalUtils.Deserialize<TArg>(args);
			TResult result = func(arg);
			return MarshalUtils.Serialize(result);
		};
	}

	/// <summary>
	/// Creates a conditional getter that returns a default value if the condition is false.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Func<ReadOnlySpan<byte>, byte[]> ConditionalGetter<T>(Func<bool> condition, Func<T> getter, T defaultValue = default)
		where T : unmanaged
	{
		return _ => MarshalUtils.Serialize(condition() ? getter() : defaultValue);
	}

	/// <summary>
	/// Creates a conditional setter that only executes if the condition is true.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Func<ReadOnlySpan<byte>, byte[]> ConditionalSetter<T>(Func<bool> condition, Action<T> setter)
		where T : unmanaged
	{
		return args =>
		{
			if (!condition() || args.Length < Unsafe.SizeOf<T>())
				return s_failureResponse;

			setter(MarshalUtils.Deserialize<T>(args));
			return s_successResponse;
		};
	}
}
