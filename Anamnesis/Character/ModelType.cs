// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Character
{
	using System;
	using Anamnesis.GameData.Sheets;

	[Serializable]
	public class ModelType : IJsonRow
	{
		public enum Types
		{
			Unknown,

			Character,
			Mount,
			Minion,
			Effect,
			Monster,
		}

		public int Key { get; set; }
		public string Name { get; set; } = string.Empty;
		public Types Type { get; set; } = Types.Unknown;

		public bool CanCustomize => this.Type == Types.Character;
		public string? Description => null;
	}
}
