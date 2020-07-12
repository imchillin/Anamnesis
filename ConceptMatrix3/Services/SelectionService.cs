// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Services
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Documents;
	using Anamnesis;
	using Anamnesis.Offsets;
	using ConceptMatrix;
	using ConceptMatrix.GUI.Dialogs;

	public class SelectionService : ISelectionService
	{
		private IMemory<bool> gposeMem;
		private IMemory<ushort> gposeMem2;

		private List<Actor> actors = new List<Actor>();

		public event SelectionModeEvent ModeChanged;
		public event SelectionEvent ActorSelected;

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

		public void SelectDefault()
		{
			Dictionary<string, Actor> selectable = this.GetSelectableActors();

			if (selectable.Count > 0)
			{
				Actor actor = selectable.First().Value;

				this.actors.Add(actor);
				this.ActorSelected?.Invoke(actor, false);
			}
		}

		public async Task SelectActor(Actor actor)
		{
			// Mannequins get actor type set to player
			if (actor.Type == ActorTypes.EventNpc)
			{
				bool? result = await GenericDialog.Show($"The Actor: \"{actor.Name}\" appears to be a humanoid NPC. Do you want to change them to a player to allow for posing and appearance changes?", "Actor Selection", MessageBoxButton.YesNo);

				if (result == null)
					return;

				if (result == true)
				{
					actor.SetValue(Offsets.Main.ActorType, ActorTypes.Player);
					actor.Type = ActorTypes.Player;
					await actor.ActorRefreshAsync();
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

			this.actors.Add(actor);
			this.ActorSelected?.Invoke(actor, true);
		}

		public Modes GetMode()
		{
			return this.gposeMem.Value && this.gposeMem2.Value == 4 ? Modes.GPose : Modes.Overworld;
		}

		public void RetargetActors()
		{
			Dictionary<string, Actor> selectable = this.GetSelectableActors();

			foreach (Actor actor in this.actors)
			{
				try
				{
					if (!selectable.ContainsKey(actor.Id))
						throw new Exception("Unable to locate actor by Id");

					actor.Retarget(selectable[actor.Id]);
				}
				catch (Exception ex)
				{
					Log.Write(new Exception("Failed to retarget actor", ex), "Selection", Log.Severity.Error);
				}
			}
		}

		public Dictionary<string, Actor> GetSelectableActors()
		{
			Modes mode = this.GetMode();
			ActorTableOffset actorTableOffset;
			BaseOffset targetOffset;

			if (mode == Modes.GPose)
			{
				actorTableOffset = Offsets.Main.GposeActorTable;
				targetOffset = Offsets.Main.Gpose;
			}
			else if (mode == Modes.Overworld)
			{
				actorTableOffset = Offsets.Main.ActorTable;
				targetOffset = Offsets.Main.Target;
			}
			else
			{
				throw new Exception("Unknown selection mode: " + mode);
			}

			byte count = actorTableOffset.GetCount();
			HashSet<string> ids = new HashSet<string>();

			Dictionary<string, Actor> actors = new Dictionary<string, Actor>();
			for (byte i = 0; i < count; i++)
			{
				Actor actor = new Actor(actorTableOffset.GetBaseOffset(i));
				actor.Description = mode + "_" + i;

				if (actors.ContainsKey(actor.Id))
				{
					// don't log actor id as it can be used to identify a player.
					Log.Write("Duplicate actor Id in selectable actors", "Selection", Log.Severity.Warning);
					continue;
				}

				actors.Add(actor.Id, actor);
			}

			return actors;
		}

		private async Task Watch()
		{
			await Task.Delay(500);

			IActorRefreshService refreshService = Services.Get<IActorRefreshService>();

			Modes currentMode = this.GetMode();

			while (this.IsAlive)
			{
				await Task.Delay(250);

				while (refreshService.IsRefreshing)
					await Task.Delay(250);

				Modes newMode = this.GetMode();

				if (newMode != currentMode)
				{
					await Task.Delay(1000);

					currentMode = newMode;
					this.RetargetActors();

					try
					{
						this.ModeChanged?.Invoke(newMode);
					}
					catch (Exception ex)
					{
						Log.Write(ex, "Selection", Log.Severity.Error);
					}
				}
			}
		}
	}
}
