// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.TexTools;

using System;
using System.Text.Json.Serialization;

[Serializable]
public class ModPack
{
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	[JsonPropertyName("version")]
	public string Version { get; set; } = string.Empty;
}
