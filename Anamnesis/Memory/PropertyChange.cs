// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System.Collections.Generic;

	public struct PropertyChange
	{
		public readonly List<MemoryBase.BindInfo> BindPath;
		public readonly object? OldValue;
		public readonly object? NewValue;

		private string path;

		public PropertyChange(MemoryBase.BindInfo bind, object? oldValue, object? newValue)
		{
			this.BindPath = new();
			this.BindPath.Add(bind);

			this.OldValue = oldValue;
			this.NewValue = newValue;

			this.path = bind.Name;
		}

		public readonly MemoryBase.BindInfo Origin => this.BindPath[0];

		public string TerminalPropertyName => this.BindPath[0].Name;

		public void AddPath(MemoryBase.BindInfo bind)
		{
			this.BindPath.Add(bind);
			this.path = bind.Name + "." + this.path;
		}

		public override string ToString()
		{
			return $"{this.path}: {this.OldValue} -> {this.NewValue}";
		}
	}
}
