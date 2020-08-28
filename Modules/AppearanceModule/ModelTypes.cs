// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.AppearanceModule
{
	using System;

	[Serializable]
	public class ModelTypes
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

		public int Id { get; set; }
		public string Name { get; set; }
		public Types Type { get; set; } = Types.Unknown;

		public bool CanCustomize
		{
			get
			{
				return this.Type == Types.Character;
			}
		}
	}
}
