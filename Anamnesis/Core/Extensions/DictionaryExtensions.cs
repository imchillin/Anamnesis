// © Anamnesis.
// Licensed under the MIT license.

namespace System.Collections.Generic
{
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
	}
}