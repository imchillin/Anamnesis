// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files
{
	using System;
	using System.Threading.Tasks;

	[Serializable]
	public abstract class FileBase
	{
		public string? Author { get; set; }
	}
}
