// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files;

using System;
using System.IO;
using Anamnesis.Serialization;

public class CmToolAppearanceJsonFile : CmToolAppearanceFile
{
	public override string FileExtension => ".json";

	public override FileBase Deserialize(Stream stream)
	{
		using TextReader reader = new StreamReader(stream);
		string json = reader.ReadToEnd();

		// Check for thse properties so that we can fallback to the even more legacy format
		// if they're not found.
		if (!json.Contains("characterDetails") && !json.Contains("CharacterBytes"))
			throw new Exception("Invalid CM Json character file");

		return (FileBase)SerializerService.Deserialize(json, this.GetType());
	}
}
