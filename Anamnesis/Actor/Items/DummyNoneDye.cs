// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Items;

using System.Windows.Media;
using Anamnesis.GameData;
using Anamnesis.GameData.Sheets;
using Anamnesis.Services;

public class DummyNoneDye : IDye
{
	public uint RowId => 0;
	public byte Id => 0;
	public string Name => "None";
	public string? Description => null;
	public ImageReference? Icon => null;
	public Brush? Color => null;

	public bool IsFavorite
	{
		get => FavoritesService.IsFavorite(this);
		set => FavoritesService.SetFavorite(this, value);
	}
}
