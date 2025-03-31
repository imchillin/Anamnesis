// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Lumina.Excel;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using MediaColor = System.Windows.Media.Color;

/// <summary>
/// Represents a Stain in the game data, which includes information about dyes and their associated items.
/// </summary>
[Sheet("Stain", 0x97C471BD)]
public readonly struct Stain(ExcelPage page, uint offset, uint row)
	: IExcelRow<Stain>, IDye
{
	/// <summary>Maps dye IDs to item keys.</summary>
	private static readonly Dictionary<uint, uint> DyeToItemKeyMap = new()
	{
		{ 1, 5729 }, { 2, 5730 }, { 3, 5731 }, { 4, 5732 }, { 5, 5733 },
		{ 6, 5734 }, { 7, 5735 }, { 8, 5736 }, { 9, 5737 }, { 10, 5738 },
		{ 11, 5739 }, { 12, 5740 }, { 13, 5741 }, { 14, 5742 }, { 15, 5743 },
		{ 16, 5744 }, { 17, 5745 }, { 18, 5746 }, { 19, 5747 }, { 20, 5748 },
		{ 21, 5749 }, { 22, 5750 }, { 23, 5751 }, { 24, 5752 }, { 25, 5753 },
		{ 26, 5754 }, { 27, 5755 }, { 28, 5756 }, { 29, 5757 }, { 30, 5758 },
		{ 31, 5759 }, { 32, 5760 }, { 33, 5761 }, { 34, 5762 }, { 35, 5763 },
		{ 36, 5764 }, { 37, 5765 }, { 38, 5766 }, { 39, 5767 }, { 40, 5768 },
		{ 41, 5769 }, { 42, 5770 }, { 43, 5771 }, { 44, 5772 }, { 45, 5773 },
		{ 46, 5774 }, { 47, 5775 }, { 48, 5776 }, { 49, 5777 }, { 50, 5778 },
		{ 51, 5779 }, { 52, 5780 }, { 53, 5781 }, { 54, 5782 }, { 55, 5783 },
		{ 56, 5784 }, { 57, 5785 }, { 58, 5786 }, { 59, 5787 }, { 60, 5788 },
		{ 61, 5789 }, { 62, 5790 }, { 63, 5791 }, { 64, 5792 }, { 65, 5793 },
		{ 66, 5794 }, { 67, 5795 }, { 68, 5796 }, { 69, 5797 }, { 70, 5798 },
		{ 71, 5799 }, { 72, 5800 }, { 73, 5801 }, { 74, 5802 }, { 75, 5803 },
		{ 76, 5804 }, { 77, 5805 }, { 78, 5806 }, { 79, 5807 }, { 80, 5808 },
		{ 81, 5809 }, { 82, 5810 }, { 83, 5811 }, { 84, 5812 }, { 85, 5813 },
		{ 86, 30116 }, { 87, 30117 }, { 88, 30118 }, { 89, 30119 }, { 90, 30120 },
		{ 91, 30121 }, { 92, 30122 }, { 93, 30123 }, { 94, 30124 }, { 101, 8732 },
		{ 102, 8733 }, { 103, 8734 }, { 104, 8735 }, { 105, 8736 }, { 106, 8737 },
		{ 107, 8738 }, { 108, 8739 }, { 109, 8740 }, { 110, 8741 }, { 111, 8742 },
		{ 112, 8743 }, { 113, 8744 }, { 114, 8745 }, { 115, 8746 }, { 116, 8747 },
		{ 117, 8748 }, { 118, 8749 }, { 119, 8750 }, { 120, 8751 },
	};

	/// <summary>Gets the row ID.</summary>
	public uint RowId => row;

	/// <summary>Gets the row ID as a byte type.</summary>
	public byte Id => (byte)this.RowId;

	/// <summary>Gets the stain's name.</summary>
	public string Name => page.ReadString(offset, offset).ToString() ?? string.Empty;

	/// <summary>Gets the stain's description.</summary>
	/// <remarks>
	/// Stains do not have descriptions. An empty string is returned.
	/// </remarks>
	public string Description => string.Empty;

	/// <summary>Gets the stain's shade.</summary>
	public readonly byte Shade => page.ReadUInt8(offset + 12);

	/// <summary>
	/// Gets the color of the stain as a <see cref="Brush"/>.
	/// </summary>
	public Brush? Color
	{
		get
		{
			// 0xAARRGGBB (RGBA)
			byte[] colorBytes = BitConverter.GetBytes(page.ReadUInt32(offset + 8));
			SolidColorBrush color = BitConverter.IsLittleEndian
				? new(MediaColor.FromRgb(colorBytes[2], colorBytes[1], colorBytes[0]))
				: new(MediaColor.FromRgb(colorBytes[1], colorBytes[2], colorBytes[3]));

			color.Freeze();
			return color;
		}
	}

	/// <summary>Gets the icon associated with the stain.</summary>
	public ImgRef? Icon
	{
		get
		{
			if (this.RowId == 0)
				return null;

			if (!DyeToItemKeyMap.TryGetValue(this.RowId, out uint itemKey) || itemKey == 0)
				return null;

			return this.Item?.Icon;
		}
	}

	/// <summary>Gets the item associated with the stain.</summary>
	public Item? Item
	{
		get
		{
			if (this.RowId == 0)
				return null;

			if (!DyeToItemKeyMap.TryGetValue(this.RowId, out uint itemKey) || itemKey == 0)
				return null;

			return GameDataService.Items.GetRow(itemKey);
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether the stain is favorited.
	/// </summary>
	public bool IsFavorite
	{
		get => FavoritesService.IsFavorite<IDye>(this);
		set => FavoritesService.SetFavorite<IDye>(this, nameof(FavoritesService.Favorites.Dyes), value);
	}

	/// <summary>
	/// Creates a new instance of the <see cref="Stain"/> struct.
	/// </summary>
	/// <param name="page">The Excel page.</param>
	/// <param name="offset">The offset within the page.</param>
	/// <param name="row">The row number.</param>
	/// <returns>A new instance of the <see cref="Stain"/> struct.</returns>
	static Stain IExcelRow<Stain>.Create(ExcelPage page, uint offset, uint row) =>
		new(page, offset, row);
}
