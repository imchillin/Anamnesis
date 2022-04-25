// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.TexTools;

using System;
using System.Text.Json.Serialization;

[Serializable]
public class Mod
{
	private string? trimmedName = null;

	// "name": "Dreadwyrm Choker of Aiming",
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	// "category": "Neck",
	[JsonPropertyName("category")]
	public string Category { get; set; } = string.Empty;

	// "fullPath": "chara/accessory/a0045/material/v0001/mt_c0101a0045_nek_a.mtrl",
	[JsonPropertyName("fullPath")]
	public string FillPath { get; set; } = string.Empty;

	[JsonPropertyName("enabled")]
	public bool Enabled { get; set; } = false;

	[JsonPropertyName("modPack")]
	public ModPack? ModPack { get; set; }

	public string TrimmedName
	{
		get
		{
			if (this.trimmedName == null)
			{
				this.trimmedName = this.Name;

				// TexTools adds left and right to the names of rings
				this.trimmedName = this.trimmedName.Replace(" - Right", string.Empty);
				this.trimmedName = this.trimmedName.Replace(" - Left", string.Empty);

				// TexTools adds handedness to names of weapons
				this.trimmedName = this.trimmedName.Replace(" - Main Hand", string.Empty);
				this.trimmedName = this.trimmedName.Replace(" - Off Hand", string.Empty);
			}

			return this.trimmedName;
		}
	}
}
