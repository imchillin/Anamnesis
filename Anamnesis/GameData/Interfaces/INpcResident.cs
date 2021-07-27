// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData
{
	using Anamnesis.Services;
	using Anamnesis.TexTools;

	public interface INpcResident : IRow
	{
		public string Singular { get; }
		public string Plural { get; }
		public string Title { get; }

		public INpcBase? Appearance { get; }

		Mod? Mod { get; }

		public bool IsFavorite
		{
			get => FavoritesService.IsFavorite(this);
			set => FavoritesService.SetFavorite(this, value);
		}
	}
}
