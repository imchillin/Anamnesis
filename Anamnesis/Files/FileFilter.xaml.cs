// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files
{
	using System;
	using System.IO;

	public class FileFilter
	{
		public readonly string Extension;
		public readonly string? Regex;
		public Func<FileSystemInfo, string>? GetNameCallback;
		public Func<FileSystemInfo, string>? GetFullNameCallback;

		public FileFilter(string extension, string? regex)
		{
			this.Extension = extension;
			this.Regex = regex;
		}

		public bool Passes(FileInfo file)
		{
			if (file.Extension != this.Extension)
				return false;

			if (this.Regex != null)
			{
				if (!System.Text.RegularExpressions.Regex.IsMatch(file.FullName, this.Regex))
				{
					return false;
				}
			}

			return true;
		}
	}
}
