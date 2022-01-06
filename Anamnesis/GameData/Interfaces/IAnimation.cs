// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Interfaces
{
	using Anamnesis.GameData.Sheets;

	public interface IAnimation
    {
		public string? Name { get; }
		public uint ActionTimelineRowId { get; }
		public ImageReference? Icon { get; }
	}
}
