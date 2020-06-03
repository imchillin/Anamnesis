// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Services
{
	using System;
	using System.Diagnostics;
	using System.Threading.Tasks;
	using ConceptMatrix;
	using ConceptMatrix.Exceptions;

	public class SelectionService : IService
	{
		////private Actor currentSelection;

		private IMemory<bool> gposeMem;
		private IMemory<ushort> gposeMem2;
		////private bool isResetting = false;

		public bool IsAlive
		{
			get;
			private set;
		}

		public Actor CurrentGameTarget
		{
			get;
			private set;
		}

		public static string GetActorId(Actor.Modes mode, ActorTypes type, string name)
		{
			return mode.ToString() + "_" + type + "_" + name;
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

			////Task.Run(this.Watch);

			return Task.CompletedTask;
		}

		public Actor.Modes GetMode()
		{
			return this.gposeMem.Value && this.gposeMem2.Value == 4 ? Actor.Modes.GPose : Actor.Modes.Overworld;
		}

		/*public void ResetSelection()
		{
			this.isResetting = true;
			this.CurrentGameTarget = null;
		}

		public async Task ResetSelectionAsync()
		{
			this.isResetting = true;
			this.CurrentGameTarget = null;

			while (this.isResetting)
			{
				await Task.Delay(100);
			}
		}

		private async Task Watch()
		{
			await Task.Delay(500);
			IInjectionService injection = App.Services.Get<IInjectionService>();
			IActorRefreshService refreshService = Services.Get<IActorRefreshService>();

			while (this.IsAlive)
			{
				await Task.Delay(250);

				while (refreshService.IsRefreshing && !this.isResetting)
					await Task.Delay(250);

				Actor.Modes mode = this.GetMode();
				IBaseMemoryOffset baseOffset = mode == Actor.Modes.GPose ? Offsets.Main.Gpose : Offsets.Main.Target;

				try
				{
					ActorTypes type = baseOffset.GetValue(Offsets.Main.ActorType);
					string name = baseOffset.GetValue(Offsets.Main.Name);

					string actorId = GetActorId(mode, type, name);

					if (string.IsNullOrEmpty(actorId))
					{
						this.CurrentGameTarget = null;
						this.isResetting = false;
						continue;
					}

					if (this.CurrentGameTarget == null
						|| this.CurrentGameTarget.Type != type
						|| this.CurrentGameTarget.ActorId != actorId
						|| this.CurrentGameTarget.Mode != mode)
					{
						this.CurrentGameTarget = new Actor(type, baseOffset, actorId, name, mode);
					}

					if (this.UseGameTarget && this.CurrentSelection != this.CurrentGameTarget)
					{
						this.CurrentSelection = this.CurrentGameTarget;
					}
					else if (!this.UseGameTarget && this.CurrentSelection != null)
					{
						// Manually selected something
						ActorTypes currentType = this.currentSelection.BaseAddress.GetValue(Offsets.Main.ActorType);
						string currentName = this.currentSelection.BaseAddress.GetValue(Offsets.Main.Name);
						string currentId = GetActorId(mode, currentType, currentName);

						// If the id does not match, it means that actor has dissapeared, either changed zones, or entered/left gpose.
						if (currentId != this.currentSelection.ActorId)
						{
							// TODO: search for this actor and select them again?
							this.CurrentSelection = null;
						}
					}

					this.isResetting = false;
				}
				catch (MemoryException)
				{
					// If the user has _never_ selected anything in game, then the memory wont be read correctly.
					// once the user has selected something, even if they then select nothing, the memory will work
					// fine, leaving the old selected behind.
					// so in this case, we just swallow the error, and let the thread loop.
					await Task.Delay(750);
				}
				catch (Exception ex)
				{
					Log.Write(ex);
				}
			}
		}*/
	}
}
