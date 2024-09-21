// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System.Collections.Generic;
using System.Linq;

public struct PropertyChange
{
	public readonly List<BindInfo> BindPath;
	public readonly Origins Origin;

	public string? Name;

	private string path;

	public PropertyChange(BindInfo bind, Origins origin)
	{
		this.BindPath = new() { bind };

		this.path = bind.Path;
		this.Origin = origin;
		this.Name = null;
	}

	public PropertyChange(PropertyChange other)
	{
		this.BindPath = new();
		this.BindPath.AddRange(other.BindPath);

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
}
