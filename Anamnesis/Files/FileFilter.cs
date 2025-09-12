// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files;

using System;
using System.IO;

public class FileFilter(Type fileType, string extension, string? regex)
{
	public readonly string Extension = extension;
	public readonly string? Regex = regex;
	public readonly Type FileType = fileType;
	public Func<FileSystemInfo, string>? GetNameCallback;
	public Func<FileSystemInfo, string>? GetFullNameCallback;

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
