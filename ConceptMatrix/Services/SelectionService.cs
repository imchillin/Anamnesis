// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Services
{
	using System;
	using System.Threading.Tasks;
	using ConceptMatrix;
	using ConceptMatrix.Exceptions;

	public class SelectionService : ISelectionService
	{
		private Selection currentSelection;

		private IMemory<bool> gposeMem;
		private IMemory<ushort> gposeMem2;

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
					this.SelectionChanged?.Invoke(this.currentSelection);
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

		public Task Initialize(IServices services)
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

		private async Task Watch()
		{
			await Task.Delay(500);
			IInjectionService injection = App.Services.Get<IInjectionService>();

			while (this.IsAlive)
			{
				try
				{
					await Task.Delay(1000);

					Selection.Modes mode = this.GetMode();
					IBaseMemoryOffset baseOffset = mode == Selection.Modes.GPose ? Offsets.Gpose : Offsets.Target;

					ActorTypes type = baseOffset.GetValue(Offsets.ActorType);
					string name = baseOffset.GetValue(Offsets.Name);

					// Hide name while debugging
					#if DEBUG
					name = "Tester";
					#endif

					string actorId = mode.ToString() + "_" + name;

					if (string.IsNullOrEmpty(actorId))
					{
						this.CurrentGameTarget = null;
						continue;
					}

					if (this.CurrentGameTarget == null
						|| this.CurrentGameTarget.Type != type
						|| this.CurrentGameTarget.ActorId != actorId
						|| this.CurrentGameTarget.Mode != mode)
					{
						this.CurrentGameTarget = new Selection(type, baseOffset, actorId, name, mode);
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
				}
				catch (Exception ex)
				{
					Log.Write(ex);
				}
			}
		}
	}
}
