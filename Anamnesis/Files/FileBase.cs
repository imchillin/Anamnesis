// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Files
{
	using System;
	using System.Threading.Tasks;
	using Anamnesis.Files.Types;

	[Serializable]
	public abstract class FileBase
	{
		public string? Path { get; set; }
		public string? Name { get; set; }

		public virtual Task Delete()
		{
			return Task.CompletedTask;
		}
	}
}
