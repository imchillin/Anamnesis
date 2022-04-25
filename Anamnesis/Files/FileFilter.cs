// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files;

using System;
using System.IO;

public class FileFilter
{
	public readonly string Extension;
	public readonly string? Regex;
	public readonly Type FileType;
	public Func<FileSystemInfo, string>? GetNameCallback;
	public Func<FileSystemInfo, string>? GetFullNameCallback;

	public FileFilter(Type fileType, string extension, string? regex)
	{
		this.Extension = extension;
		this.Regex = regex;
		this.FileType = fileType;
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
