// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using Anamnesis.Core.Extensions;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

/// <summary>
/// Represents binding information for a property.
/// </summary>
public class PropertyBindInfo : BindInfo
{
	/// <summary>The property information.</summary>
	public readonly PropertyInfo Property;

	/// <summary>The bind attribute associated with the property.</summary>
	public readonly BindAttribute Attribute;

	/// <summary>
	/// Gets a value indicating whether the bound property is a
	/// <see cref="MemoryBase"/> object type.
	/// </summary>
	public readonly bool IsMemoryBase;

	/// <summary>Gets the cached offsets for the property.</summary>
	public readonly int[] Offsets;

	private static readonly ConcurrentDictionary<PropertyInfo, Func<MemoryBase, object?>> s_getterCache = new();
	private static readonly ConcurrentDictionary<PropertyInfo, Action<MemoryBase, object?>> s_setterCache = new();

	private Func<MemoryBase, object?>? fastGetter;
	private Action<MemoryBase, object?>? fastSetter;

	/// <summary>
	/// Initializes a new instance of the <see cref="PropertyBindInfo"/> class.
	/// </summary>
	/// <param name="memory">The memory base instance.</param>
	/// <param name="property">The property information.</param>
	/// <param name="attribute">The bind attribute associated with the property.</param>
	public PropertyBindInfo(MemoryBase memory, PropertyInfo property, BindAttribute attribute)
		: base(memory)
	{
		this.Property = property;
		this.Attribute = attribute;

		Debug.Assert(this.Property != null, "Property is null");
		Debug.Assert(this.Attribute != null, "Attribute is null");

		this.Name = string.Intern(property.Name);
		this.Path = $".{this.Name}";
		this.Type = property.PropertyType;
		this.Flags = this.Attribute.Flags;
		this.SyncGroup = attribute.SyncGroup;
		this.Offsets = this.GetOffsetsInternal();

		if (this.Offsets == null || this.Offsets.Length == 0)
			throw new NullReferenceException("Cached offsets are not initialized.");

		this.IsMemoryBase = typeof(MemoryBase).IsAssignableFrom(property.PropertyType);
	}

	/// <summary>Gets the name of the bound property.</summary>
	public override string Name { get; }

	/// <summary>Gets the path of the bound property.</summary>
	public override string Path { get; }

	/// <summary>Gets the type of the bound property.</summary>
	public override Type Type { get; }

	/// <summary>Gets the synchronization group of the bound property.</summary>
	public override string? SyncGroup { get; }

	/// <summary>Gets the bind flags.</summary>
	public override BindFlags Flags { get; }

	/// <summary>
	/// Gets the address of the bind.
	/// </summary>
	/// <returns>The address of the bind.</returns>
	/// <exception cref="InvalidOperationException">
	/// Thrown when multiple offsets are provided to the bind attribute but
	/// the bind not a pointer.
	/// </exception>
	/// <exception cref="NullReferenceException">
	/// Thrown when the method fails to (cached) retrieve offsets.
	/// </exception>
	public override IntPtr GetAddress()
	{
		IntPtr bindAddress = this.Memory.Address;
		var offsets = this.Offsets;
		var flags = this.Flags;
		int count = offsets.Length;

		for (int i = 0; i < count - 1; ++i)
		{
			bindAddress += offsets[i];
			bindAddress = MemoryService.Read<IntPtr>(bindAddress);

			// If the intermediate pointer is null or invalid, abort early.
			if (bindAddress == IntPtr.Zero || bindAddress.ToInt64() < 0)
				return IntPtr.Zero;
		}

		bindAddress += offsets[count - 1];

		if (flags.HasFlagUnsafe(BindFlags.Pointer))
			bindAddress = MemoryService.Read<IntPtr>(bindAddress);

		return bindAddress;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public object? GetValue(MemoryBase target)
	{
		var getter = this.fastGetter ??= s_getterCache.GetOrAdd(this.Property, static prop =>
		{
			var targetParam = Expression.Parameter(typeof(MemoryBase), "target");
			var castTarget = Expression.Convert(targetParam, prop.DeclaringType!);
			var getCall = Expression.Call(castTarget, prop.GetGetMethod(true)!);
			var boxResult = Expression.Convert(getCall, typeof(object));
			return Expression.Lambda<Func<MemoryBase, object?>>(boxResult, targetParam).Compile();
		});

		return getter(target);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetValue(MemoryBase target, object? value)
	{
		var setter = this.fastSetter ??= s_setterCache.GetOrAdd(this.Property, static prop =>
		{
			var targetParam = Expression.Parameter(typeof(MemoryBase), "target");
			var valueParam = Expression.Parameter(typeof(object), "value");
			var castTarget = Expression.Convert(targetParam, prop.DeclaringType!);
			var castValue = Expression.Convert(valueParam, prop.PropertyType);
			var setCall = Expression.Call(castTarget, prop.GetSetMethod(true)!, castValue);
			return Expression.Lambda<Action<MemoryBase, object?>>(setCall, targetParam, valueParam).Compile();
		});

		setter(target, value);
	}

	/// <summary>
	/// Gets the offsets for the property.
	/// </summary>
	/// <returns>An array of offsets.</returns>
	/// <exception cref="InvalidOperationException">
	/// Thrown when the offset type is unknown or when no offset(s)
	/// and offset property are provided.
	/// </exception>
	private int[] GetOffsetsInternal()
	{
		if (this.Attribute.Offsets != null)
			return this.Attribute.Offsets;

		if (this.Attribute.OffsetPropertyName != null)
		{
			Type memoryType = this.Memory.GetType();
			PropertyInfo? offsetProperty = memoryType.GetProperty(this.Attribute.OffsetPropertyName);
			if (offsetProperty != null)
			{
				object? offsetValue = offsetProperty.GetValue(this.Memory);

				if (offsetValue is int[] offsetInts)
					return offsetInts;

				if (offsetValue is int offset)
					return [offset];

				throw new InvalidOperationException($"Unknown offset type: {offsetValue} bind: {this}");
			}
		}

		throw new InvalidOperationException($"No offsets for bind: {this}");
	}
}
