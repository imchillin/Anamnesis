// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.Interop;

using Reloaded.Hooks;
using Reloaded.Hooks.Definitions;
using RemoteController.IPC;
using Serilog;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

/// <summary>
/// Factory for creating hooks at runtime using DetourBuilder.
/// This handles the generic hook creation logic that doesn't need to be generated.
/// </summary>
[RequiresUnreferencedCode("This class is not trimming-safe")]
[RequiresDynamicCode("This class requires dynamic code due to hook reflection")]
public static class HookFactory
{
	/// <summary>
	/// Creates a function hook based on the provided delegate key
	/// and registration data.
	/// </summary>
	/// <param name="delegateKey">
	/// The unique key identifying the delegate type.
	/// </param>
	/// <param name="hookId">
	/// The unique hook identifier.
	/// </param>
	/// <param name="regData">
	/// The hook registration data.
	/// </param>
	/// <param name="reloadedHooks">
	/// A reference to a ReloadedHooks instance.
	/// </param>
	/// <returns>
	/// The created function hook, or null if creation failed.
	/// </returns>
	public static IFunctionHook? Create(string delegateKey, uint hookId, HookRegistrationData regData, ReloadedHooks reloadedHooks)
	{
		Type? delegateType = null;
		foreach (var (type, _) in HookDelegateRegistry.GetAll())
		{
			if (HookUtils.GetKey(type) == delegateKey)
			{
				delegateType = type;
				break;
			}
		}

		if (delegateType == null)
		{
			Log.Error($"Delegate type not found for key: {delegateKey}");
			return null;
		}

		var createMethod = typeof(HookFactory)
			.GetMethod(nameof(CreateTyped), BindingFlags.NonPublic | BindingFlags.Static)!
			.MakeGenericMethod(delegateType);

		return createMethod.Invoke(null, [delegateKey, hookId, regData, reloadedHooks]) as IFunctionHook;
	}

	private static IFunctionHook? CreateTyped<TDelegate>(string delegateKey, uint hookId, HookRegistrationData regData, ReloadedHooks reloadedHooks)
		where TDelegate : Delegate
	{
		try
		{
			if (regData.HookType == HookType.Wrapper)
			{
				var wrapper = reloadedHooks.CreateWrapper<TDelegate>(regData.Address, out _);
				return new FunctionWrapper<TDelegate>(hookId, delegateKey, regData.Address, wrapper);
			}

			IHook<TDelegate>? hook = null;
			var detour = DetourBuilder.Build<TDelegate>(hookId, regData.HookBehavior, () => hook!.OriginalFunction);
			hook = reloadedHooks.CreateHook(detour, regData.Address);
			hook.Activate();

			return new FunctionHook<TDelegate>(hookId, delegateKey, regData.HookBehavior, regData.Address, hook);
		}
		catch (Exception ex)
		{
			Log.Error(ex, $"Failed to create typed hook for: {delegateKey}");
			return null;
		}
	}
}

/// <summary>
/// Builds detour delegates for interceptor hooks at runtime.
/// The hook detour builder is used during hook registration.
/// </summary>
[RequiresUnreferencedCode("This class is not trimming-safe")]
[RequiresDynamicCode("This class requires dynamic code due to hook reflection")]
public static class DetourBuilder
{
	private static readonly System.Collections.Concurrent.ConcurrentDictionary<uint, long> s_messageCounters = new();

	/// <summary>
	/// Builds a compiled detour lambda based on the specified hook behavior.
	/// </summary>
	/// <typeparam name="TDelegate">The delegate type of the hook.</typeparam>
	/// <param name="hookId">
	/// The unique hook identifier.
	/// </param>
	/// <param name="behavior">
	/// The hook behavior (Before, After, Replace).
	/// </param>
	/// <param name="getOriginal">
	/// The function to retrieve the original function code.
	/// </param>
	/// <returns>
	/// The compiled detour delegate.
	/// </returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown if an unsupported hook behavior is specified.
	/// </exception>
	public static TDelegate Build<TDelegate>(uint hookId, HookBehavior behavior, Func<TDelegate> getOriginal)
		where TDelegate : Delegate
	{
		var invoke = typeof(TDelegate).GetMethod("Invoke")!;
		var returnType = invoke.ReturnType;
		var parameters = invoke.GetParameters();
		var holder = new OriginalHolder<TDelegate>(getOriginal);

		return behavior switch
		{
			HookBehavior.Before => BuildBefore(hookId, holder, returnType, parameters),
			HookBehavior.After => BuildAfter(hookId, holder, returnType, parameters),
			HookBehavior.Replace => BuildReplace<TDelegate>(hookId, returnType, parameters),
			_ => throw new ArgumentOutOfRangeException(nameof(behavior)),
		};
	}

	private static TDelegate BuildBefore<TDelegate>(uint hookId, OriginalHolder<TDelegate> holder, Type returnType, ParameterInfo[] parameters)
		where TDelegate : Delegate
	{
		return CreateDetour<TDelegate>(parameters, returnType, (args, invoker) =>
		{
			try
			{
				byte[] argsPayload = MarshalUtils.SerializeArgs(args);

				long count = s_messageCounters.AddOrUpdate(hookId, 1, static (_, c) => c + 1);

				if (count % 1000 == 0)
				{
					var start = Stopwatch.GetTimestamp();
					var response = Controller.SendInterceptRequest(hookId, argsPayload, HookBehavior.Before);
					var end = Stopwatch.GetTimestamp();
					var elapsedMicroseconds = (end - start) * 1_000_000 / Stopwatch.Frequency;
					Log.Information($"Detour round-trip time (sampled): {elapsedMicroseconds} us");
				}
				else
				{
					Controller.SendInterceptRequest(hookId, argsPayload, HookBehavior.Before);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Error in Before detour for hook {hookId}");
			}

			return invoker(holder.Original, args);
		});
	}

	private static TDelegate BuildAfter<TDelegate>(uint hookId, OriginalHolder<TDelegate> holder, Type returnType, ParameterInfo[] parameters)
		where TDelegate : Delegate
	{
		return CreateDetour<TDelegate>(parameters, returnType, (args, invoker) =>
		{
			object? result = invoker(holder.Original, args);

			try
			{
				byte[] argsPayload = MarshalUtils.SerializeArgs(args);
				byte[] resultPayload = MarshalUtils.SerializeBoxed(result);

				byte[] payload = new byte[4 + argsPayload.Length + resultPayload.Length];
				BitConverter.GetBytes(argsPayload.Length).CopyTo(payload, 0);
				argsPayload.CopyTo(payload, 4);
				resultPayload.CopyTo(payload, 4 + argsPayload.Length);

				Controller.SendInterceptRequest(hookId, payload, HookBehavior.After);
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Error in After detour for hook {hookId}");
			}

			return result;
		});
	}

	private static TDelegate BuildReplace<TDelegate>(uint hookId, Type returnType, ParameterInfo[] parameters)
		where TDelegate : Delegate
	{
		bool hasReturn = returnType != typeof(void);

		return CreateDetour<TDelegate>(parameters, returnType, (args, _) =>
		{
			try
			{
				byte[] argsPayload = MarshalUtils.SerializeArgs(args);
				var response = Controller.SendInterceptRequest(hookId, argsPayload, HookBehavior.Replace);
				return hasReturn ? MarshalUtils.DeserializeBoxed(response, returnType) : null;
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Error in Replace detour for hook {hookId}");
				return hasReturn ? GetDefault(returnType) : null;
			}
		});
	}

	private static TDelegate CreateDetour<TDelegate>(ParameterInfo[] parameters, Type returnType, Func<object?[], Func<TDelegate, object?[], object?>, object?> handler)
		where TDelegate : Delegate
	{
		var invoker = BuildInvoker<TDelegate>(parameters, returnType);

		var paramExprs = parameters
			.Select(p => Expression.Parameter(p.ParameterType, p.Name))
			.ToArray();

		var argsArrayExpr = Expression.NewArrayInit(
			typeof(object),
			paramExprs.Select(p => Expression.Convert(p, typeof(object))));

		var handlerExpr = Expression.Constant(handler);
		var invokerExpr = Expression.Constant(invoker);
		var callExpr = Expression.Invoke(handlerExpr, argsArrayExpr, invokerExpr);

		Expression body = returnType == typeof(void)
			? callExpr
			: Expression.Convert(callExpr, returnType);

		var lambda = Expression.Lambda<TDelegate>(body, paramExprs);
		return lambda.Compile();
	}

	private static Func<TDelegate, object?[], object?> BuildInvoker<TDelegate>(ParameterInfo[] parameters, Type returnType)
		where TDelegate : Delegate
	{
		var delegateParam = Expression.Parameter(typeof(TDelegate), "del");
		var argsParam = Expression.Parameter(typeof(object[]), "args");

		var argExprs = parameters.Select((p, i) =>
			Expression.Convert(
				Expression.ArrayIndex(argsParam, Expression.Constant(i)),
				p.ParameterType)).ToArray();

		var callExpr = Expression.Invoke(delegateParam, argExprs);

		Expression body = returnType == typeof(void)
			? Expression.Block(callExpr, Expression.Constant(null, typeof(object)))
			: Expression.Convert(callExpr, typeof(object));

		return Expression.Lambda<Func<TDelegate, object?[], object?>>(body, delegateParam, argsParam).Compile();
	}

	private static object? GetDefault(Type type)
	{
		return type.IsValueType ? Activator.CreateInstance(type) : null;
	}

	/// <summary>
	/// Caches the original delegate on first access.
	/// </summary>
	private sealed class OriginalHolder<TDelegate>(Func<TDelegate> getOriginal)
		where TDelegate : Delegate
	{
		private TDelegate? cached;
		public TDelegate Original => this.cached ??= getOriginal();
	}
}