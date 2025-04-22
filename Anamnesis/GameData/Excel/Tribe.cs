// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.Memory;
using Lumina.Excel;

/// <summary>Represents the tribe data in the game data.</summary>
[Sheet("Tribe", 0xE74759FB)]
public readonly struct Tribe(ExcelPage page, uint offset, uint row)
	: IExcelRow<Tribe>
{
	/// <inheritdoc/>
	public readonly uint RowId => row;

	/// <summary>Gets the full tribe name.</summary>
	public readonly string Name => this.CustomizeTribe.ToString();

	/// <summary>Gets the customize tribe value.</summary>
	public readonly ActorCustomizeMemory.Tribes CustomizeTribe => (ActorCustomizeMemory.Tribes)this.RowId;

	/// <summary>Gets the masculine name of the tribe.</summary>
	public readonly string Masculine => page.ReadString(offset, offset).ToString();

	/// <summary> Gets the feminine name of the tribe.</summary>
	public readonly string Feminine => page.ReadString(offset + 4, offset).ToString();

	/// <summary>Gets the display name of the tribe.</summary>
	/// <remarks>This is used to simplify some tribe names for the UI.</remarks>
	public readonly string DisplayName
	{
		get
		{
			// Shorten miqo tribe names for the UI
			if (this.Feminine.StartsWith("Seeker"))
				return "Seeker";

			if (this.Feminine.StartsWith("Keeper"))
				return "Keeper";

			return this.Feminine;
		}
	}

	/// <summary>
	/// Creates a new instance of the <see cref="Tribe"/> struct.
	/// </summary>
	/// <param name="page">The Excel page.</param>
	/// <param name="offset">The offset within the page.</param>
	/// <param name="row">The row ID.</param>
	/// <returns>A new instance of the <see cref="Tribe"/> struct.</returns>
	static Tribe IExcelRow<Tribe>.Create(ExcelPage page, uint offset, uint row) =>
		new(page, offset, row);

	/// <summary>
	/// Determines whether the specified <see cref="Tribe"/> is equal to the current <see cref="Tribe"/>.
	/// </summary>
	/// <param name="other">The other tribe to compare.</param>
	/// <returns>True if the tribes are equal; otherwise, false.</returns>
	public readonly bool Equals(Tribe? other)
	{
		if (other is null)
			return false;

		return this.CustomizeTribe == other?.CustomizeTribe;
	}
}
