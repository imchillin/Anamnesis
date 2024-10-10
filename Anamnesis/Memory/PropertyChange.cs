// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System.Collections.Generic;
using System.Linq;

public struct PropertyChange
{
	public readonly List<BindInfo> BindPath;
	public readonly Origins Origin;

	// NOTE: OldValue and NewValue can store reference types. If that is the case,
	// they will point to the same object in memory. This is fine, as we are only
	// interested in the values they point to, not the references themselves.
	// Use with caution.
	public object? OldValue;
	public object? NewValue;
	public string? Name;

	private string path;

	public PropertyChange(BindInfo? bind, object? oldValue, object? newValue, Origins origin)
	{
		// If a bind is not provided, then this change is not associated with a bind.
		if (bind != null)
		{
			this.BindPath = new() { bind };
			this.path = bind.Path;
		}
		else
		{
			this.BindPath = new();
			this.path = string.Empty;
		}

		this.OldValue = oldValue;
		this.NewValue = newValue;
		this.Origin = origin;
		this.Name = null;
	}

	public PropertyChange(PropertyChange other)
	{
		this.BindPath = new();
		this.BindPath.AddRange(other.BindPath);
		this.OldValue = other.OldValue;
		this.NewValue = other.NewValue;
		this.path = other.path;
		this.Origin = other.Origin;
		this.Name = other.Name;
	}

	public enum Origins
	{
		User,
		History,
		Game,
	}

	public readonly string Path => this.path;
	public readonly BindInfo OriginBind => this.BindPath[0];
	public readonly string TerminalPropertyName => this.BindPath[0].Name;
	public readonly string TopPropertyName => this.BindPath.Last().Name;

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

	public void AddPath(BindInfo bind)
	{
		this.BindPath.Add(bind);
		this.path = bind.Path + this.path;
	}

	public override readonly string ToString()
	{
		return $"{this.path}: {this.OldValue} -> {this.NewValue}";
	}
}
