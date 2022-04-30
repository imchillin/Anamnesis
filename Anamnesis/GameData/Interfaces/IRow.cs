// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData;

using XivToolsWpf.Selectors;

public interface IRow : ISelectable
{
	uint RowId { get; }
}
