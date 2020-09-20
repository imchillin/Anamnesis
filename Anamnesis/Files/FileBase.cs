// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Files
{
	using System;
	using System.Threading.Tasks;
	using Anamnesis.Files.Types;

	[Serializable]
	public abstract class FileBase : IFileSource.IFile
	{
		public string? Path { get; set; }
		public bool UseAdvancedLoad { get; set; }
		public abstract FileType? Type { get; }

		public string? Name { get; protected set; }

		public virtual Task Delete()
		{
			return Task.CompletedTask;
		}
	}
}
