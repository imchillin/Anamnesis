// © Anamnesis.
// Licensed under the MIT license.

namespace RemoteController.SourceGenerators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Immutable;
using System.Text;

/// <summary>
/// Source generator that creates the HookDelegateRegistry class based
/// on function hook delegates marked with the [FunctionBind] attribute.
/// </summary>
[Generator]
public class HookDelegateRegistryGenerator : IIncrementalGenerator
{
	private const string FUNC_BIND_ATTR_NAMESPACE = "RemoteController.Interop.FunctionBindAttribute";

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		// Find all delegate declarations with [FunctionBind] attribute
		var delegateDeclarations = context.SyntaxProvider
			.CreateSyntaxProvider(
				predicate: static (node, _) => IsDelegateWithAttributes(node),
				transform: static (ctx, _) => GetDelegateInfo(ctx))
			.Where(static info => info is not null)
			.Collect();

		// Combine with compilation and generate
		context.RegisterSourceOutput(
			delegateDeclarations,
			static (spc, delegates) => Execute(spc, delegates!));
	}

	private static bool IsDelegateWithAttributes(SyntaxNode node)
	{
		return node is DelegateDeclarationSyntax delegateSyntax
			&& delegateSyntax.AttributeLists.Count > 0;
	}

	private static DelegateInfo? GetDelegateInfo(GeneratorSyntaxContext context)
	{
		var delegateSyntax = (DelegateDeclarationSyntax)context.Node;
		var symbol = context.SemanticModel.GetDeclaredSymbol(delegateSyntax);
		if (symbol is not INamedTypeSymbol delegateSymbol)
			return null;

		// Check for FunctionBind attribute
		var functionBindAttr = delegateSymbol.GetAttributes()
			.FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == FUNC_BIND_ATTR_NAMESPACE);

		if (functionBindAttr is null)
			return null;

		// Extract attribute arguments
		var signature = functionBindAttr.ConstructorArguments.Length > 0
			? functionBindAttr.ConstructorArguments[0].Value?.ToString() ?? string.Empty
			: string.Empty;

		var offset = functionBindAttr.ConstructorArguments.Length > 1
			? (int)(functionBindAttr.ConstructorArguments[1].Value ?? 0)
			: 0;

		// Get parent class name for fully qualified delegate name
		var parentClass = delegateSyntax.Parent as ClassDeclarationSyntax;
		var parentClassName = parentClass?.Identifier.Text ?? string.Empty;

		// Get namespace
		var ns = GetNamespace(delegateSyntax);

		// Get return type and parameters
		var invokeMethod = delegateSymbol.DelegateInvokeMethod;
		if (invokeMethod is null)
			return null;

		var returnType = invokeMethod.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
		var parameters = invokeMethod.Parameters
			.Select(p => new ParameterInfo(
				p.Name,
				p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
				p.Type.IsUnmanagedType,
				IsPointerType(p.Type)))
			.ToImmutableArray();

		var isUnsafe = parameters.Any(p => p.IsPointer) ||
			(invokeMethod.ReturnType is IPointerTypeSymbol);

		return new DelegateInfo(
			delegateSyntax.Identifier.Text,
			parentClassName,
			ns,
			signature,
			offset,
			returnType,
			invokeMethod.ReturnsVoid,
			parameters,
			isUnsafe);
	}

	private static bool IsPointerType(ITypeSymbol type)
	{
		return type is IPointerTypeSymbol;
	}

	private static string GetNamespace(SyntaxNode node)
	{
		var ns = string.Empty;
		var parent = node.Parent;

		while (parent is not null)
		{
			if (parent is FileScopedNamespaceDeclarationSyntax fileScopedNs)
			{
				ns = fileScopedNs.Name.ToString();
				break;
			}

			if (parent is NamespaceDeclarationSyntax namespaceSyntax)
			{
				ns = string.IsNullOrEmpty(ns)
					? namespaceSyntax.Name.ToString()
					: $"{namespaceSyntax.Name}.{ns}";
			}

			parent = parent.Parent;
		}

		return ns;
	}

	private static void Execute(SourceProductionContext context, ImmutableArray<DelegateInfo?> delegates)
	{
		var validDelegates = delegates
			.Where(d => d is not null)
			.Cast<DelegateInfo>()
			.ToList();

		if (validDelegates.Count == 0)
			return;

		var source = GenerateRegistrySource(validDelegates);
		context.AddSource("HookDelegateRegistry.g.cs", SourceText.From(source, Encoding.UTF8));
	}

	private static string GenerateRegistrySource(List<DelegateInfo> delegates)
	{
		var sb = new StringBuilder();

		sb.AppendLine("""
			// © Anamnesis.
			// Licensed under the MIT license.

			// WARNING: This file is auto-generated. Do NOT modify it directly.
			// Update the HookDelegateRegistryGenerator source generator instead.

			#nullable enable

			namespace RemoteController.Interop;

			using Reloaded.Hooks;
			using RemoteController.Interop.Delegates;
			using RemoteController.IPC;
			using Serilog;
			using System.Collections.Concurrent;
			using System.Diagnostics.CodeAnalysis;
			using System.Numerics;

			/// <summary>
			/// Native AOT-compatibl registry of hook delegates that offers pre-compiled typed invokers.
			/// For hook creation, please refer to <see cref="HookFactory"/> and <see cref="DetourBuilder"/>.
			/// </summary>
			[RequiresUnreferencedCode("This class is not trimming-safe")]
			[RequiresDynamicCode("This class requires dynamic code due to hook reflection")]
			public static class HookDelegateRegistry
			{
			""");

		// Generate per-delegate storage fields
		sb.AppendLine("\t// ==========================================");
		sb.AppendLine("\t// Per-delegate storage");
		sb.AppendLine("\t// ==========================================");
		sb.AppendLine();

		foreach (var del in delegates)
		{
			var fieldName = GetFieldName(del);
			var fullTypeName = GetFullDelegateTypeName(del);
			sb.AppendLine($"\tprivate static {fullTypeName}? {fieldName};");
		}

		sb.AppendLine();

		// Generate shared infrastructure
		sb.AppendLine("\t// ==========================================");
		sb.AppendLine("\t// Shared infrastructure");
		sb.AppendLine("\t// ==========================================");
		sb.AppendLine();
		sb.AppendLine("""
			/// <summary>
			/// Delegate for span-based invokers that avoid array allocations on input.
			/// </summary>
			public delegate byte[] HookInvoker(ReadOnlySpan<byte> argsPayload);
		""");

		sb.AppendLine("\tprivate static readonly ConcurrentDictionary<uint, HookInvoker> s_typedInvokers = new();");
		sb.AppendLine();

		// Generate s_delegates array
		sb.AppendLine("\tprivate static readonly (Type DelegateType, FunctionBindAttribute Attribute)[] s_delegates =");
		sb.AppendLine("\t[");
		foreach (var del in delegates)
		{
			var fullTypeName = GetFullDelegateTypeName(del);
			sb.AppendLine($"\t\t(typeof({fullTypeName}), new FunctionBindAttribute(\"{del.Signature}\", {del.Offset})),");
		}
		sb.AppendLine("\t];");
		sb.AppendLine();

		// Generate s_setupActions dictionary
		sb.AppendLine("\t// Delegate-specific setup actions (registers typed invoker and stores hook/wrapper)");
		sb.AppendLine("\tprivate static readonly Dictionary<Type, Action<uint, IFunctionHook>> s_setupActions = new()");
		sb.AppendLine("\t{");
		foreach (var del in delegates)
		{
			var fullTypeName = GetFullDelegateTypeName(del);
			var setupMethodName = GetSetupMethodName(del);
			sb.AppendLine($"\t\t[typeof({fullTypeName})] = {setupMethodName},");
		}
		sb.AppendLine("\t};");
		sb.AppendLine();

		// Generate GetAll method
		sb.AppendLine("""
				/// <summary>
				/// Retrieves all registered delegate types and their descriptors.
				/// </summary>
				/// <returns>
				/// A tuple containing the delegate type and its associated function bind attribute.
				/// </returns>
				public static IEnumerable<(Type DelegateType, FunctionBindAttribute Attribute)> GetAll() => s_delegates;
			""");
		sb.AppendLine();

		// Generate CreateHook method
		sb.AppendLine("""
				/// <summary>
				/// Creates a hook/wrapper for the given delegate key and registration data.
				/// </summary>
				/// <param name="delegateKey">The unique identifier of the delegate</param>
				/// <param name="hookId">The unique hook identifier.</param>
				/// <param name="regData">The hook registration data.</param>
				/// <param name="reloadedHooks">A reference to a ReloadedHooks instance.</param>
				/// <returns>The created function hook/wrapper, or null on failure.</returns>
				public static IFunctionHook? CreateHook(string delegateKey, uint hookId, HookRegistrationData regData, ReloadedHooks reloadedHooks)
				{
					try
					{
						if (regData.Address == 0)
							return null;

						var delegateType = Type.GetType(delegateKey);
						if (delegateType == null)
						{
							Log.Error($"No delegate type found for key: {delegateKey}");
							return null;
						}

						if (!s_setupActions.TryGetValue(delegateType, out var setup))
						{
							Log.Error($"No setup action registered for delegate type: {delegateType.FullName}");
							return null;
						}

						var hook = HookFactory.Create(delegateKey, hookId, regData, reloadedHooks);
						if (hook == null)
							return null;

						setup(hookId, hook);
						return hook;
					}
					catch (Exception ex)
					{
						Log.Error(ex, $"Failed to create hook for: {delegateKey}");
						return null;
					}
				}
			""");
		sb.AppendLine();

		// Generate InvokeOriginal method
		sb.AppendLine("""
				/// <summary>
				/// Invokes the original function for the specified hook ID with
				/// the provided arguments payload.
				/// </summary>
				/// <param name="hookId">The unique identifier of the hook.</param>
				/// <param name="argsPayload">The serialized arguments payload.</param>
				/// <returns>
				/// The serialized return value payload, or null on error.
				/// </returns>
				public static byte[]? InvokeOriginal(uint hookId, ReadOnlySpan<byte> argsPayload)
				{
					if (s_typedInvokers.TryGetValue(hookId, out var invoker))
					{
						try
						{
							return invoker(argsPayload);
						}
						catch (Exception ex)
						{
							Log.Error(ex, $"Error in typed invoker for hook {hookId}");
							return null;
						}
					}

					return null;
				}
			""");
		sb.AppendLine();

		// Generate RemoveContext method
		sb.AppendLine("""
				internal static void RemoveContext(uint hookId)
				{
					s_typedInvokers.TryRemove(hookId, out _);
				}
			""");
		sb.AppendLine();

		// Generate Setup methods
		sb.AppendLine("\t// ==========================================");
		sb.AppendLine("\t// Setup actions");
		sb.AppendLine("\t// ==========================================");
		sb.AppendLine();

		foreach (var del in delegates)
		{
			GenerateSetupMethod(sb, del);
		}

		// Generate Typed handlers
		sb.AppendLine("\t// ==========================================");
		sb.AppendLine("\t// Typed handlers");
		sb.AppendLine("\t// ==========================================");
		sb.AppendLine();

		foreach (var del in delegates)
		{
			GenerateTypedHandler(sb, del);
		}

		sb.AppendLine("}");

		return sb.ToString();
	}

	private static void GenerateSetupMethod(StringBuilder sb, DelegateInfo del)
	{
		var setupMethodName = GetSetupMethodName(del);
		var invokeMethodName = GetInvokeMethodName(del);
		var fieldName = GetFieldName(del);
		var fullTypeName = GetFullDelegateTypeName(del);

		sb.AppendLine($"\tprivate static void {setupMethodName}(uint hookId, IFunctionHook registered)");
		sb.AppendLine("\t{");
		sb.AppendLine($"\t\ts_typedInvokers[hookId] = {invokeMethodName};");
		sb.AppendLine();
		sb.AppendLine($"\t\t{fieldName} = registered switch");
		sb.AppendLine("\t\t{");
		sb.AppendLine($"\t\t\tFunctionHook<{fullTypeName}> h => h.OriginalFunction,");
		sb.AppendLine($"\t\t\tFunctionWrapper<{fullTypeName}> w => w.OriginalFunction,");
		sb.AppendLine("\t\t\t_ => null,");
		sb.AppendLine("\t\t};");
		sb.AppendLine();
		sb.AppendLine($"\t\tif ({fieldName} == null)");
		sb.AppendLine($"\t\t\tLog.Error($\"{setupMethodName}: Failed to extract delegate for hookId {{hookId}}\");");
		sb.AppendLine("\t}");
		sb.AppendLine();
	}

	private static void GenerateTypedHandler(StringBuilder sb, DelegateInfo del)
	{
		var invokeMethodName = GetInvokeMethodName(del);
		var fieldName = GetFieldName(del);
		var unsafeKeyword = del.IsUnsafe ? "unsafe " : string.Empty;

		sb.AppendLine($"\tprivate static {unsafeKeyword}byte[] {invokeMethodName}(ReadOnlySpan<byte> argsPayload)");
		sb.AppendLine("\t{");
		sb.AppendLine($"\t\tif ({fieldName} == null)");
		sb.AppendLine("\t\t\treturn [];");
		sb.AppendLine();

		// Deserialize parameters
		if (del.Parameters.Length > 0)
		{
			sb.AppendLine("\t\tint offset = 0;");

			foreach (var param in del.Parameters)
			{
				var (readExpr, sizeExpr) = GetReadCall(param);
				sb.AppendLine($"\t\t{param.TypeName} {param.Name} = {readExpr};");
				sb.AppendLine($"\t\toffset += {sizeExpr};");
			}
			sb.AppendLine();
		}

		// Build invocation
		var args = string.Join(", ", del.Parameters.Select(p => p.Name));
		var invocation = $"{fieldName}({args})";

		if (del.IsVoidReturn)
		{
			sb.AppendLine($"\t\t{invocation};");
			sb.AppendLine("\t\treturn [];");
		}
		else
		{
			sb.AppendLine($"\t\tvar result = {invocation};");
			sb.AppendLine($"\t\treturn MarshalUtils.Serialize(({GetSerializeType(del.ReturnType)})result);");
		}

		sb.AppendLine("\t}");
		sb.AppendLine();
	}

	private static (string Expression, string SizeExpr) GetReadCall(ParameterInfo param)
	{
		if (param.IsPointer)
		{
			string cleanType = param.TypeName.Replace("global::", string.Empty);
			return ($"({cleanType})MarshalUtils.Read<nint>(argsPayload.Slice(offset))", "IntPtr.Size");
		}

		if (param.TypeName.Contains("nint") || param.TypeName.Contains("IntPtr"))
		{
			return ("(nint)System.Buffers.Binary.BinaryPrimitives.ReadInt64LittleEndian(argsPayload.Slice(offset))", "IntPtr.Size");
		}

		return ($"MarshalUtils.Read<{param.TypeName}>(argsPayload.Slice(offset))", $"System.Runtime.CompilerServices.Unsafe.SizeOf<{param.TypeName}>()");
	}

	private static string GetSerializeType(string returnType)
	{
		if (returnType.Contains("*"))
			return "nint"; // Cast pointers to nint for serialization

		return returnType.Replace("global::", string.Empty);
	}

	private static string GetFieldName(DelegateInfo del)
	{
		var name = del.DelegateName;
		return $"s_{char.ToLowerInvariant(name[0])}{name.Substring(1)}";
	}

	private static string GetFullDelegateTypeName(DelegateInfo del)
	{
		return string.IsNullOrEmpty(del.ParentClassName)
			? del.DelegateName
			: $"{del.ParentClassName}.{del.DelegateName}";
	}

	private static string GetSetupMethodName(DelegateInfo del)
	{
		return $"Setup{del.DelegateName}";
	}

	private static string GetInvokeMethodName(DelegateInfo del)
	{
		return $"Invoke{del.DelegateName}Typed";
	}

	private sealed class DelegateInfo(
		string delegateName,
		string parentClassName,
		string ns,
		string signature,
		int offset,
		string returnType,
		bool isVoidReturn,
		ImmutableArray<ParameterInfo> parameters,
		bool isUnsafe)
	{
		public string DelegateName { get; } = delegateName;
		public string ParentClassName { get; } = parentClassName;
		public string Namespace { get; } = ns;
		public string Signature { get; } = signature;
		public int Offset { get; } = offset;
		public string ReturnType { get; } = returnType;
		public bool IsVoidReturn { get; } = isVoidReturn;
		public ImmutableArray<ParameterInfo> Parameters { get; } = parameters;
		public bool IsUnsafe { get; } = isUnsafe;
	}

	private sealed class ParameterInfo(string name, string typeName, bool isUnmanaged, bool isPointer)
	{
		public string Name { get; } = name;
		public string TypeName { get; } = typeName;
		public bool IsUnmanaged { get; } = isUnmanaged;
		public bool IsPointer { get; } = isPointer;
	}
}
