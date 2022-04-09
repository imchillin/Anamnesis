// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory
{
	using System.Collections.Generic;

	public struct PropertyChange
	{
		public readonly MemoryBase.BindInfo Origin;
		public readonly List<string> PropertyNames;
		public readonly object? OldValue;
		public readonly object? NewValue;

		public PropertyChange(MemoryBase.BindInfo bind, object? oldValue, object? newValue)
		{
			this.Origin = bind;

			this.PropertyNames = new();
			this.PropertyNames.Add(bind.Property.Name);

			this.OldValue = oldValue;
			this.NewValue = newValue;
		}

		public string TerminalPropertyName => this.PropertyNames[0];

		public void AddPath(string propertyName)
		{
			this.PropertyNames.Add(propertyName);
		}
	}
}
