// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.TexTools;

using System;
using System.Collections.Generic;

[Serializable]
public class ModList
{
	public List<ModPack> ModPacks { get; set; } = new List<ModPack>();
	public List<Mod> Mods { get; set; } = new List<Mod>();
}
