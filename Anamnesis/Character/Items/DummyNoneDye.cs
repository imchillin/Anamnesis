// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Character.Items
{
	using System.Windows.Media;
	using Anamnesis.GameData;

	public class DummyNoneDye : IDye
	{
		public int Key => 0;
		public byte Id => 0;
		public string Name => "None";
		public string? Description => null;
		public ImageSource? Icon => null;
		public Brush? Color => null;
	}
}
