// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule
{
	using System;
	using System.Threading.Tasks;
	using System.Windows;
	using Anamnesis;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class PoseService : IService
	{
		private IMemory<Flag> skel1Mem;
		private IMemory<Flag> skel2Mem;
		private IMemory<Flag> skel3Mem;
		private IMemory<Flag> skel4Mem;
		private IMemory<Flag> skel5Mem;
		private IMemory<Flag> skel6Mem;

		private IMemory<Flag> phys1Mem;
		private IMemory<Flag> phys2Mem;
		private IMemory<Flag> phys3Mem;

		private ISelectionService selectionService;

		private bool isEnabled;

		public delegate void PoseEvent(bool value);

		public event PoseEvent EnabledChanged;
		public event PoseEvent FreezePhysicsChanged;
		public event PoseEvent FreezePositionsChanged;
		public event PoseEvent AvailableChanged;

		public bool IsAvailable
		{
			get
			{
				return this.selectionService.GetMode() == Modes.GPose;
			}
		}

		public bool IsEnabled
		{
			get
			{
				return this.isEnabled;
			}

			set
			{
				if (this.IsEnabled == value)
					return;

				this.isEnabled = value;
				this.SetEnabled(value);
			}
		}

		// We need to unfreeze positions to allow us to set bone rotations
		// without calculating new positions. This is required for CM2 poses to load
		// correctly, since they don't have positions.
		public bool FreezePositions
		{
			get
			{
				return this.skel5Mem.Value.IsEnabled;
			}
			set
			{
				this.skel5Mem.Value = Flag.Get(value);

				this.FreezePositionsChanged?.Invoke(value);
			}
		}

		public Task Initialize()
		{
			IInjectionService injection = Services.Get<IInjectionService>();

			this.skel1Mem = injection.GetMemory(Offsets.Main.Skeleton1Flag);
			this.skel2Mem = injection.GetMemory(Offsets.Main.Skeleton2Flag);
			this.skel3Mem = injection.GetMemory(Offsets.Main.Skeleton3Flag);
			this.skel4Mem = injection.GetMemory(Offsets.Main.Skeleton4flag);
			this.skel5Mem = injection.GetMemory(Offsets.Main.Skeleton5Flag);
			this.skel6Mem = injection.GetMemory(Offsets.Main.Skeleton6Flag);
			this.phys1Mem = injection.GetMemory(Offsets.Main.Physics1Flag);
			this.phys2Mem = injection.GetMemory(Offsets.Main.Physics2Flag);
			this.phys3Mem = injection.GetMemory(Offsets.Main.Physics3Flag);

			this.selectionService = Services.Get<ISelectionService>();
			this.selectionService.ModeChanged += this.Selection_ModeChanged;

			return Task.CompletedTask;
		}

		public Task Shutdown()
		{
			this.IsEnabled = false;

			this.skel1Mem?.Dispose();
			this.skel2Mem?.Dispose();
			this.skel3Mem?.Dispose();
			this.skel4Mem?.Dispose();
			this.skel5Mem?.Dispose();
			this.skel6Mem?.Dispose();
			this.phys1Mem?.Dispose();
			this.phys2Mem?.Dispose();
			this.phys3Mem?.Dispose();

			return Task.CompletedTask;
		}

		public Task Start()
		{
			return Task.CompletedTask;
		}

		private async void SetEnabled(bool enabled)
		{
			// Don't try to enable posing unless we are in gpose
			if (enabled && this.selectionService.GetMode() != Modes.GPose)
				throw new Exception("Attempt to enable posing outside of gpose");

			// Physics
			this.phys1Mem.Value = Flag.Get(enabled);
			this.phys2Mem.Value = Flag.Get(enabled);
			this.phys3Mem.Value = Flag.Get(enabled);

			if (enabled)
				await Task.Delay(100);

			// rotations
			this.skel1Mem.Value = Flag.Get(enabled);
			this.skel2Mem.Value = Flag.Get(enabled);
			this.skel3Mem.Value = Flag.Get(enabled);

			// scale
			this.skel4Mem.Value = Flag.Get(enabled);
			this.skel6Mem.Value = Flag.Get(enabled);

			// positions
			this.FreezePositions = enabled;

			Application.Current.Dispatcher.Invoke(() =>
			{
				this.EnabledChanged?.Invoke(enabled);
			});
		}

		private void Selection_ModeChanged(Modes mode)
		{
			bool available = this.IsAvailable;

			if (!available)
				this.IsEnabled = false;

			Application.Current.Dispatcher.Invoke(() =>
			{
				this.AvailableChanged?.Invoke(available);
			});
		}
	}
}
