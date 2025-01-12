// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

/// <summary>
/// Represents a change in a property, including its old and new values, the
/// origin of the change, and the path of the property.
/// </summary>
public struct PropertyChange
{
	/// <summary>Gets the bind path associated with the property change.</summary>
	public readonly List<BindInfo> BindPath;

	/// <summary>Gets the origin of the property change.</summary>
	public readonly Origins Origin;

	/// <summary>Gets or sets the old value of the property.</summary>
	/// <remarks>
	/// This property can store reference types. If that is the case, they will point to
	/// the same object in memory as <see cref="NewValue"/>. For the intended purpose
	/// of this property, this is okay, as we are only interested in the values they
	/// point to, not the references themselves. Nevertheless, USE WITH CAUTION.
	/// </remarks>
	public object? OldValue;

	/// <summary>Gets or sets the new value of the property.</summary>
	/// <remarks>
	/// This property can store reference types. If that is the case, they will point to
	/// the same object in memory as <see cref="OldValue"/>. For the intended purpose
	/// of this property, this is okay, as we are only interested in the values they
	/// point to, not the references themselves. Nevertheless, USE WITH CAUTION.
	/// </remarks>
	public object? NewValue;

	/// <summary>Gets or sets the name of the property.</summary>
	public string? Name;

	/// <summary>The full path of the property change.</summary>
	private string path;

	/// <summary>
	/// Initializes a new instance of the <see cref="PropertyChange"/> struct.
	/// </summary>
	/// <param name="bind">The bind information associated with the property change.</param>
	/// <param name="oldValue">The old value of the property.</param>
	/// <param name="newValue">The new value of the property.</param>
	/// <param name="origin">The origin of the property change.</param>
	public PropertyChange(BindInfo bind, object? oldValue, object? newValue, Origins origin)
	{
		this.BindPath = new List<BindInfo>(1) { bind };
		this.path = bind.Path;
		this.OldValue = oldValue;
		this.NewValue = newValue;
		this.Origin = origin;
		this.Name = null;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="PropertyChange"/> struct by copying another instance.
	/// </summary>
	/// <param name="other">The other instance to copy.</param>
	public PropertyChange(PropertyChange other)
	{
		this.BindPath = new List<BindInfo>(other.BindPath.Count);
		this.BindPath.AddRange(other.BindPath);
		this.OldValue = other.OldValue;
		this.NewValue = other.NewValue;
		this.path = other.path;
		this.Origin = other.Origin;
		this.Name = other.Name;
	}

	/// <summary>The possible origins of a property change.</summary>
	public enum Origins
	{
		User,
		History,
		Game,
	}

	/// <summary>Gets the full path of the property change.</summary>
	public readonly string Path => this.path;

	/// <summary>Gets the bind information of the origin property bind.</summary>
	public readonly BindInfo OriginBind => this.BindPath[0];

	/// <summary>Gets the name of the top property in the bind path.</summary>
	public readonly string TopPropertyName => this.BindPath.Last().Name;

	/// <summary>
	/// Determines whether the property change should be recorded.
	/// </summary>
	/// <returns>True if the change should be recorded; otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool ShouldRecord()
	{
		// Don't record changes that originate anywhere other than the user interface.
		if (this.Origin != Origins.User)
			return false;

		foreach (BindInfo bind in this.BindPath)
		{
			if (bind.Flags.HasFlag(BindFlags.DontRecordHistory))
				return false;
		}

		return true;
	}

	/// <summary>
	/// Adds a bind to the property change, appended to the end of the bind path.
	/// </summary>
	/// <param name="bind">The bind information to add.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void AddPath(BindInfo bind)
	{
		this.BindPath.Add(bind);
		this.path += bind.Path;
	}

	/// <summary>
	/// Adds all ancestor binds of the parent bind to the property change.
	/// </summary>
	/// <remarks>
	/// Use this method only if the change's bind path is not already configured.
	/// </remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ConfigureBindPath()
	{
		var parentBind = this.BindPath[0].Memory.ParentBind;
		if (parentBind == null)
			return;

		this.AddPath(parentBind);
		var memParent = parentBind.Memory.Parent;
		while (memParent != null)
		{
			if (memParent.ParentBind != null)
				this.AddPath(memParent.ParentBind);

			memParent = memParent.Parent;
		}
	}

	/// <summary>
	/// Returns a string that represents the current object.
	/// </summary>
	/// <returns>A string that represents the current object.</returns>
	public override readonly string ToString()
	{
		return $"{this.path}: {this.OldValue} -> {this.NewValue}";
	}
}
