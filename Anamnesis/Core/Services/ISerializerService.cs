// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis
{
	using System;

	public interface ISerializerService : IService
	{
		string Serialize(object obj);

		T Deserialize<T>(string json)
			where T : new();

		object Deserialize(string json, Type type);
	}
}
