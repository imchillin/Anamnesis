// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Services
{
	using System;
	using System.Threading.Tasks;
	using ConceptMatrix;
	using ConceptMatrix.Offsets;
	using ConceptMatrix.Services;

	public class SelectionService : ISelectionService
	{
		private Selection currentSelection;

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
			Task.Run(this.Watch);
			return Task.CompletedTask;
		}

		private async Task Watch()
		{
			await Task.Delay(100);
			IInjectionService injection = App.Services.Get<IInjectionService>();

			while (this.IsAlive)
			{
				await Task.Delay(100);

				////IMemory<string> actorIdMem1 = Offsets.ActorID.GetMemory(Offsets.TargetOffset);
				////IMemory<string> actorIdMem2 = Offsets.TargetOffset.GetMemory(Offsets.ActorID);

				IBaseMemoryOffset baseOffset = Offsets.TargetOffset;

				string actorId;
				using (IMemory<string> actorIdMem = Offsets.TargetOffset.GetMemory(Offsets.ActorID))
				{
					actorId = actorIdMem.Value;
				}

				if (string.IsNullOrEmpty(actorId))
				{
					this.CurrentGameTarget = null;
					continue;
				}

				if (this.CurrentGameTarget == null || this.CurrentGameTarget.ActorId != actorId)
				{
					string name;
					using (IMemory<string> nameMem = injection.GetMemory<string>(baseOffset, Offsets.Name))
					{
						name = nameMem.Value;
					}

					this.CurrentGameTarget = new Selection(Selection.Types.Character, baseOffset, actorId, name);
				}

				if (this.UseGameTarget && this.CurrentSelection != this.CurrentGameTarget)
				{
					this.CurrentSelection = this.CurrentGameTarget;
				}
			}
		}
	}
}
