// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Character.Items
{
	using Anamnesis;
	using Anamnesis.GameData;

	public class DummyNoneDye : IDye
	{
		public byte Id
		{
			get
			{
				return 0;
			}
		}

		public string Name
		{
			get
			{
				return "None";
			}
		}

		public string? Description { get => null; }
		public IImageSource? Icon { get => null; }

		public int Key
		{
			get
			{
				return 0;
			}
		}
	}
}
