// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System.Collections.Generic;

	public struct PropertyChange
	{
		public readonly List<BindInfo> BindPath;
		public readonly object? OldValue;
		public readonly object? NewValue;

		private string path;

		public PropertyChange(BindInfo bind, object? oldValue, object? newValue)
		{
			this.BindPath = new();
			this.BindPath.Add(bind);

			this.OldValue = oldValue;
			this.NewValue = newValue;

			this.path = bind.Path;
		}

		public readonly BindInfo Origin => this.BindPath[0];

		public string TerminalPropertyName => this.BindPath[0].Name;

		public void AddPath(BindInfo bind)
		{
			this.BindPath.Add(bind);
			this.path = bind.Path + this.path;
		}

		public override string ToString()
		{
			return $"{this.path}: {this.OldValue} -> {this.NewValue}";
		}
	}
}
