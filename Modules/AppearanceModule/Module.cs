// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.IO;
	using System.Text.Json;
	using System.Threading.Tasks;
	using System.Windows.Documents;
	using ConceptMatrix.AppearanceModule.Files;
	using ConceptMatrix.AppearanceModule.Pages;
	using ConceptMatrix.Memory;
	using ConceptMatrix.Memory.Serialization;
	using ConceptMatrix.Modules;

	public class Module : IModule
	{
		public static ReadOnlyCollection<ModelTypes> ModelTypes { get; private set; }
		public static ReadOnlyCollection<Prop> Props { get; private set; }

		public Task Initialize()
		{
			IFileService fileService = Services.Get<IFileService>();
			fileService.AddFileSource(new DatAppearanceFileSource());

			IViewService viewService = Services.Get<IViewService>();
			viewService.AddPage<AppearancePage>("Appearance", "user", this.IsActorSupported);

			return Task.CompletedTask;
		}

		public Task Start()
		{
			ISerializerService serializer = Services.Get<ISerializerService>();

			try
			{
				string json = File.ReadAllText("Modules/Appearance/ModelTypes.json");
				List<ModelTypes> modelTypes = serializer.Deserialize<List<ModelTypes>>(json);
				ModelTypes = modelTypes.AsReadOnly();
			}
			catch (Exception ex)
			{
				Log.Write(new Exception("Failed to load model type list", ex), "Appearance", Log.Severity.Error);
			}

			try
			{
				string json = File.ReadAllText("Modules/Appearance/Props.json");
				List<Prop> propList = serializer.Deserialize<List<Prop>>(json);

				propList.Sort((a, b) =>
				{
					return a.Name.CompareTo(b.Name);
				});

				Props = propList.AsReadOnly();
			}
			catch (Exception ex)
			{
				Log.Write(new Exception("Failed to load props list", ex), "Appearance", Log.Severity.Error);
			}

			return Task.CompletedTask;
		}

		public Task Shutdown()
		{
			return Task.CompletedTask;
		}

		private bool IsActorSupported(Actor actor)
		{
			if (actor.Type != ActorTypes.Player && actor.Type != ActorTypes.EventNpc && actor.Type != ActorTypes.BattleNpc)
				return false;

			return true;
		}
	}
}