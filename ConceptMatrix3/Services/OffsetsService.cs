// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Services
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using System.Text;
	using System.Threading.Tasks;
	using ConceptMatrix.GUI.Serialization;
	using ConceptMatrix.Injection.Offsets;

	public class OffsetsService : IService
	{
		public Task Initialize()
		{
			MainOffsetFile file = Offsets.Main;
			string json = Serializer.Serialize(file);

			File.WriteAllText("MainOffsets.json", json);

			json = File.ReadAllText("MainOffsets.json");
			file = Serializer.Deserialize<MainOffsetFile>(json);

			// Set the names of all the offsets for debugging.
			PropertyInfo[] properties = file.GetType().GetProperties();
			foreach (PropertyInfo property in properties)
			{
				Offset offset = (Offset)property.GetValue(file);
				offset.Name = property.Name;
				property.SetValue(file, offset);
			}

			Offsets.Main = file;

			return Task.CompletedTask;
		}

		public Task Shutdown()
		{
			return Task.CompletedTask;
		}

		public Task Start()
		{
			return Task.CompletedTask;
		}
	}
}
