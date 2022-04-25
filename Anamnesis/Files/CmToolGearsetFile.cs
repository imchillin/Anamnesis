// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files;

using System;
using System.IO;
using Anamnesis.Serialization;

public class CmToolGearsetFile : CmToolAppearanceFile
{
	public override string FileExtension => ".json";

	public override FileBase Deserialize(Stream stream)
	{
		using TextReader reader = new StreamReader(stream);
		string json = reader.ReadToEnd();

		if (!json.Contains("EquipmentBytes"))
			throw new Exception("Invalid CM Gearset character file");

		return (FileBase)SerializerService.Deserialize(json, this.GetType());
	}
}
