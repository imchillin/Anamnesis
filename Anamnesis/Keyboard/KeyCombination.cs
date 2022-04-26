// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Keyboard;

using System;
using System.Windows.Input;

[Serializable]
public class KeyCombination
{
	public KeyCombination()
	{
	}

	public KeyCombination(Key key, ModifierKeys modifiers = ModifierKeys.None)
	{
		this.Key = key;
		this.Modifiers = modifiers;
	}

	public Key Key { get; set; }
	public ModifierKeys Modifiers { get; set; }

	public static KeyCombination FromString(string str)
	{
		string[] parts = str.Split(';', StringSplitOptions.TrimEntries);

		if (parts.Length == 1)
			return new KeyCombination(Enum.Parse<Key>(parts[0]));

		if (parts.Length == 2)
			return new KeyCombination(Enum.Parse<Key>(parts[0]), Enum.Parse<ModifierKeys>(parts[1]));

		throw new Exception($"Unable to parse KeyCombination from string: {str}");
	}

	public override string ToString()
	{
		if (this.Modifiers == ModifierKeys.None)
			return this.Key.ToString();

		return $"{this.Key}; {this.Modifiers}";
	}
}
