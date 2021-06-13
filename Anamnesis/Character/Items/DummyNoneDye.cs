// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Character.Items
{
	using System.Windows.Media;
	using Anamnesis.GameData;
	using Anamnesis.Services;

	public class DummyNoneDye : IDye
	{
		public uint Key => 0;
		public byte Id => 0;
		public string Name => "None";
		public string? Description => null;
		public ImageSource? Icon => null;
		public Brush? Color => null;

		public bool IsFavorite
		{
			get => FavoritesService.IsFavorite(this);
			set => FavoritesService.SetFavorite(this, value);
		}
	}
}