// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Services
{
	using System;
	using System.Diagnostics;
	using System.Threading.Tasks;
	using ConceptMatrix;
	using ConceptMatrix.Exceptions;

	public class SelectionService : ISelectionService
	{
		private Selection currentSelection;

		private IMemory<bool> gposeMem;
		private IMemory<ushort> gposeMem2;
		private bool isResetting = false;

		public event SelectionEvent SelectionChanged;

		public bool IsAlive
		{
			get;
			private set;
		}

		public bool UseGameTarget
		{
			get;
			set;
		}

		public Selection CurrentSelection
		{
			get
			{
				return this.currentSelection;
			}

			set
			{
				this.currentSelection = value;

				try
				{
					Log.Write("Changing Selection: " + value.ActorId + "(" + value.BaseAddress + ")", "Selection");

					Stopwatch sw = new Stopwatch();
					sw.Start();
					this.SelectionChanged?.Invoke(this.currentSelection);
					sw.Stop();
					Log.Write("took " + sw.ElapsedMilliseconds + "ms to change selection");
				}
				catch (Exception ex)
				{
					Log.Write(new Exception("Failed to invoke selection changed event.", ex));
				}
			}
		}

		public Selection CurrentGameTarget
		{
			get;
			private set;
		}

		public Task Initialize()
		{
			this.IsAlive = true;
			this.UseGameTarget = true;
			return Task.CompletedTask;
		}

		public Task Shutdown()
		{
			this.IsAlive = false;
			return Task.CompletedTask;
		}

		public Task Start()
		{
			this.gposeMem = Offsets.GposeCheck.GetMemory();
			this.gposeMem2 = Offsets.GposeCheck2.GetMemory();

			Task.Run(this.Watch);

			return Task.CompletedTask;
		}

		public Selection.Modes GetMode()
		{
			return this.gposeMem.Value && this.gposeMem2.Value == 4 ? Selection.Modes.GPose : Selection.Modes.Overworld;
		}

		public void ResetSelection()
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

				Selection.Modes mode = this.GetMode();
				IBaseMemoryOffset baseOffset = mode == Selection.Modes.GPose ? Offsets.Gpose : Offsets.Target;

				try
				{
					ActorTypes type = baseOffset.GetValue(Offsets.ActorType);
					string name = baseOffset.GetValue(Offsets.Name);

					string actorId = mode.ToString() + "_" + name;

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
						this.CurrentGameTarget = new Selection(type, baseOffset, actorId, name, mode);
						this.isResetting = false;
					}

					if (this.UseGameTarget && this.CurrentSelection != this.CurrentGameTarget)
					{
						this.CurrentSelection = this.CurrentGameTarget;
					}
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
		}
	}
}
