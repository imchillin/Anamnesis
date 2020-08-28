// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GUI.Services
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using System.Text;
	using System.Threading.Tasks;
	using Anamnesis.Memory.Offsets;

	using static Anamnesis.Offsets;

	public class OffsetsService : IService
	{
		public Task Initialize()
		{
			ISerializerService serializer = Anamnesis.Services.Get<ISerializerService>();

			string json = File.ReadAllText("MainOffsets.json");
			MainOffsetFile file = serializer.Deserialize<MainOffsetFile>(json);

			// Set the names of all the offsets for debugging.
			PropertyInfo[] properties = file.GetType().GetProperties();
			foreach (PropertyInfo property in properties)
			{
				Offset? offset = property.GetValue(file) as Offset;

				if (offset == null)
					continue;

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
