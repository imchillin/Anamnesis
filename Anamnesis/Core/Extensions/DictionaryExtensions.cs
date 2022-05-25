// © Anamnesis.
// Licensed under the MIT license.

namespace System.Collections.Generic;

public static class DictionaryExtensions
{
	public static (TKey, TValue) Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> self)
	{
		return (self.Key, self.Value);
	}

	public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> self, out TKey key, out TValue value)
	{
		key = self.Key;
		value = self.Value;
	}

	public static void Set<TKey, TValue>(this Dictionary<TKey, TValue> self, TKey key, TValue value)
		where TKey : notnull
	{
		if (!self.ContainsKey(key))
			self.Add(key, value);

		self[key] = value;
	}

	public static TValue Get<TKey, TValue>(this Dictionary<TKey, TValue> self, TKey key, TValue defaultValue)
		where TKey : notnull
	{
		if (!self.ContainsKey(key))
			return defaultValue;

		return self[key];
	}
}
