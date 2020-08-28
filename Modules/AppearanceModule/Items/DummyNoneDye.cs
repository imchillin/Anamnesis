// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Items
{
	using System;
	using ConceptMatrix;
	using ConceptMatrix.GameData;
	using ConceptMatrix.Memory;
	using PropertyChanged;

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

		public string Description { get; }
		public IImageSource Icon { get; }

		public int Key
		{
			get
			{
				return 0;
			}
		}
	}
}
