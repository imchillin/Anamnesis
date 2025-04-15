// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData;

using XivToolsWpf.Selectors;

public interface IRow : ISelectable
{
	/// <summary>Gets the row ID.</summary>
	uint RowId { get; }
}
