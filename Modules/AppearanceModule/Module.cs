// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.AppearanceModule
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.IO;
	using System.Threading.Tasks;
	using Anamnesis.AppearanceModule.Files;
	using Anamnesis.AppearanceModule.Pages;
	using Anamnesis.Memory;
	using Anamnesis.Modules;
	using Anamnesis.Serialization;
	using Anamnesis.Services;

	public class Module : IModule
	{
		public static ReadOnlyCollection<ModelTypes> ModelTypes { get; private set; }
		public static ReadOnlyCollection<Prop> Props { get; private set; }

		public Task Initialize()
		{
			FileService.AddFileSource(new DatAppearanceFileSource());
			ViewService.AddPage<AppearancePage>("Appearance", "useralt", this.IsActorSupported);

			return Task.CompletedTask;
		}

		public Task Start()
		{
			try
			{
				List<ModelTypes> modelTypes = SerializerService.DeserializeFile<List<ModelTypes>>("Modules/Appearance/ModelTypes.json");
				ModelTypes = modelTypes.AsReadOnly();
			}
			catch (Exception ex)
			{
				Log.Write(new Exception("Failed to load model type list", ex), "Appearance", Log.Severity.Error);
			}

			try
			{
				List<Prop> propList = SerializerService.DeserializeFile<List<Prop>>("Modules/Appearance/Props.json");

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

		private bool IsActorSupported(ActorViewModel actor)
		{
			if (actor.ObjectKind != ActorTypes.Player && actor.ObjectKind != ActorTypes.EventNpc && actor.ObjectKind != ActorTypes.BattleNpc)
				return false;

			return true;
		}
	}
}