// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Services
{
	using System;
	using System.Diagnostics;
	using System.Threading.Tasks;
	using System.Windows;
	using ConceptMatrix;
	using ConceptMatrix.GUI.Dialogs;
	using ConceptMatrix.Memory;

	public class SelectionService : ISelectionService
	{
		private IMarshaler<bool> gposeMem;
		private IMarshaler<ushort> gposeMem2;

		public event SelectionModeEvent ModeChanged;
		public event SelectionEvent ActorSelected;

		public Actor SelectedActor { get; private set; }

		public bool IsAlive
		{
			get;
			private set;
		}

		public Task Initialize()
		{
			this.IsAlive = true;
			return Task.CompletedTask;
		}

		public Task Shutdown()
		{
			this.IsAlive = false;
			return Task.CompletedTask;
		}

		public Task Start()
		{
			IInjectionService injection = Services.Get<IInjectionService>();

			this.gposeMem = injection.GetMemory(Offsets.Main.GposeCheck);
			this.gposeMem2 = injection.GetMemory(Offsets.Main.GposeCheck2);

			Task.Run(this.Watch);

			return Task.CompletedTask;
		}

		public async Task SelectActor(Actor actor)
		{
			this.SelectedActor = actor;

			IInjectionService injection = Services.Get<IInjectionService>();
			using IMarshaler<int> territoryMem = injection.GetMemory(Offsets.Main.TerritoryAddress, Offsets.Main.Territory);

			int territoryId = territoryMem.Value;

			bool isBarracks = false;
			isBarracks |= territoryId == 534; // Twin adder barracks
			isBarracks |= territoryId == 535; // Immortal Flame barracks
			isBarracks |= territoryId == 536; // Maelstrom barracks

			// Mannequins and housing NPC's get actor type changed, but squadron members do not.
			if (!isBarracks && actor.Type == ActorTypes.EventNpc)
			{
				bool? result = await GenericDialog.Show($"The Actor: \"{actor.Name}\" appears to be a humanoid NPC. Do you want to change them to a player to allow for posing and appearance changes?", "Actor Selection", MessageBoxButton.YesNo);

				if (result == null)
					return;

				if (result == true)
				{
					actor.SetValue(Offsets.Main.ActorType, ActorTypes.Player);
					actor.Type = ActorTypes.Player;
					await actor.ActorRefreshAsync();

					if (actor.GetValue(Offsets.Main.ModelType) != 0)
					{
						actor.SetValue(Offsets.Main.ModelType, 0);
						await actor.ActorRefreshAsync();
					}
				}
			}

			// Carbuncles get model type set to player (but not actor type!)
			if (actor.Type == ActorTypes.BattleNpc)
			{
				int modelType = actor.GetValue(Offsets.Main.ModelType);
				if (modelType == 409 || modelType == 410 || modelType == 412)
				{
					bool? result = await GenericDialog.Show($"The Actor: \"{actor.Name}\" appears to be a Carbuncle. Do you want to change them to a player to allow for posing and appearance changes?", "Actor Selection", MessageBoxButton.YesNo);

					if (result == null)
						return;

					if (result == true)
					{
						actor.SetValue(Offsets.Main.ModelType, 0);
						await actor.ActorRefreshAsync();
					}
				}
			}

			this.ActorSelected?.Invoke(actor);
		}

		public Modes GetMode()
		{
			return this.gposeMem.Value && this.gposeMem2.Value == 4 ? Modes.GPose : Modes.Overworld;
		}

		private async Task Watch()
		{
			try
			{
				await Task.Delay(500);

				IActorRefreshService refreshService = Services.Get<IActorRefreshService>();

				Modes currentMode = this.GetMode();

				while (this.IsAlive)
				{
					await Task.Delay(50);

					while (refreshService.IsRefreshing)
						await Task.Delay(250);

					Modes newMode = this.GetMode();

					if (newMode != currentMode)
					{
						await Task.Delay(1000);

						currentMode = newMode;

						try
						{
							this.ModeChanged?.Invoke(newMode);
						}
						catch (Exception ex)
						{
							Log.Write(ex, "Selection", Log.Severity.Error);
						}
					}

					Actor selected;

					try
					{
						if (newMode == Modes.Overworld)
						{
							selected = new Actor(Offsets.Main.Target);
						}
						else
						{
							selected = new Actor(Offsets.Main.Gpose);
						}
					}
					catch (Exception ex)
					{
						Log.Write(new Exception("Failed to select current target", ex), "Selection", Log.Severity.Warning);
						selected = null;
					}

					if (selected == null || selected.Type == ActorTypes.None)
						continue;

					if (selected != this.SelectedActor)
					{
						await this.SelectActor(selected);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}
	}
}
