// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Anamnesis.Serialization;

public static class EmbeddedFileUtility
{
	public static T Load<T>(string path)
		where T : notnull
	{
		string json = EmbeddedFileUtility.LoadText(path);
		return SerializerService.Deserialize<T>(json);
	}

	public static string LoadText(string path)
	{
		using Stream stream = EmbeddedFileUtility.Load(path);
		using StreamReader streamReader = new StreamReader(stream);

		return streamReader.ReadToEnd();
	}

	public static Stream Load(string path)
	{
		Assembly? assembly = Assembly.GetExecutingAssembly();
		string? assemblyName = assembly.GetName().Name;

		if (assemblyName == null)
			throw new Exception("failed to get executing assembly name");

		path = path.Replace("\\", ".");
		path = path.Replace("/", ".");
		path = assemblyName + '.' + path;

		Stream? stream = assembly.GetManifestResourceStream(path);

		if (stream == null)
			throw new FileNotFoundException(path);

		return stream;
	}

	public static byte[] LoadBytes(string path)
	{
		Stream? stream = Load(path);
		MemoryStream? memoryStream = new MemoryStream();
		stream.CopyTo(memoryStream);
		return memoryStream.ToArray();
	}

	public static string GetFileName(string path)
	{
		string[] filePathParts = path.Split('.');
		return filePathParts[filePathParts.Length - 2];
	}

	public static string[] GetAllFilesInDirectory(string dir)
	{
		Assembly? assembly = Assembly.GetExecutingAssembly();
		string? assemblyName = assembly.GetName().Name;

		if (assemblyName == null)
			throw new Exception("failed to get executing assembly name");

		if (dir.Contains('.'))
			throw new Exception($"Invalid embedded file directory: {dir}");

		dir = dir.Replace("\\", ".");
		dir = dir.Replace("/", ".");

		if (!dir.StartsWith("."))
			dir = "." + dir;

		string[] all = assembly.GetManifestResourceNames();

		List<string> files = new List<string>();
		foreach (string path in all)
		{
			string trimmedPath = path.Replace(assemblyName, string.Empty);

			if (!trimmedPath.StartsWith(dir))
				continue;

			files.Add(trimmedPath.Trim('.'));
		}

		return files.ToArray();
	}
}
