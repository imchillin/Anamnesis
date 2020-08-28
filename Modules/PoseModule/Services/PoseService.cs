// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule
{
	using System;
	using System.ComponentModel;
	using System.Threading.Tasks;
	using System.Windows;
	using ConceptMatrix.Memory;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	[SuppressPropertyChangedWarnings]
	public class PoseService : IService, INotifyPropertyChanged
	{
		private IMemory<Flag> skel1Mem;
		private IMemory<Flag> skel2Mem;
		private IMemory<Flag> skel3Mem;
		////private IMemory<Flag> skel4Mem;
		////private IMemory<Flag> skel5Mem;
		////private IMemory<Flag> skel6Mem;

		private IMemory<Flag> phys1Mem;
		private IMemory<Flag> phys2Mem;
		////private IMemory<Flag> phys3Mem;

		private ISelectionService selectionService;

		private bool isEnabled;

		public delegate void PoseEvent(bool value);

		public event PoseEvent EnabledChanged;
		public event PoseEvent FreezePhysicsChanged;
		public event PoseEvent FreezePositionsChanged;
		public event PoseEvent FreezeScaleChanged;
		public event PoseEvent FreezeRotationChanged;
		public event PoseEvent AvailableChanged;
		public event PropertyChangedEventHandler PropertyChanged;

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

				this.SetEnabled(value);
			}
		}

		public bool FreezePhysics
		{
			get
			{
				return this.phys1Mem.Value.IsEnabled;
			}
			set
			{
				this.FreezePositions = value;
				this.FreezeScale = value;

				this.phys1Mem.Value = Flag.Get(value);
				this.phys2Mem.Value = Flag.Get(value);

				this.FreezePhysicsChanged?.Invoke(value);
			}
		}

		public bool FreezePositions
		{
			get
			{
				return false;
				////return this.skel5Mem.Value.IsEnabled;
			}
			set
			{
				////this.skel5Mem.Value = Flag.Get(value);

				this.FreezePositionsChanged?.Invoke(value);
			}
		}

		public bool FreezeScale
		{
			get
			{
				return false;
				////return this.skel4Mem.Value.IsEnabled;
			}
			set
			{
				////this.skel4Mem.Value = Flag.Get(value);
				////this.phys3Mem.Value = Flag.Get(value);
				////this.skel6Mem.Value = Flag.Get(value);

				this.FreezeScaleChanged?.Invoke(value);
			}
		}

		public bool FreezeRotation
		{
			get
			{
				return this.skel1Mem.Value.IsEnabled;
			}
			set
			{
				this.skel1Mem.Value = Flag.Get(value);
				this.skel2Mem.Value = Flag.Get(value);
				this.skel3Mem.Value = Flag.Get(value);

				this.FreezeRotationChanged?.Invoke(value);
			}
		}

		public Task Initialize()
		{
			IInjectionService injection = Services.Get<IInjectionService>();

			this.skel1Mem = injection.GetMemory(Offsets.Main.Skeleton1Flag);
			this.skel2Mem = injection.GetMemory(Offsets.Main.Skeleton2Flag);
			this.skel3Mem = injection.GetMemory(Offsets.Main.Skeleton3Flag);
			////this.skel4Mem = injection.GetMemory(Offsets.Main.Skeleton4flag);
			////this.skel5Mem = injection.GetMemory(Offsets.Main.Skeleton5Flag);
			////this.skel6Mem = injection.GetMemory(Offsets.Main.Skeleton6Flag);
			this.phys1Mem = injection.GetMemory(Offsets.Main.Physics1Flag);
			this.phys2Mem = injection.GetMemory(Offsets.Main.Physics2Flag);
			////this.phys3Mem = injection.GetMemory(Offsets.Main.Physics3Flag);

			this.selectionService = Services.Get<ISelectionService>();
			this.selectionService.ModeChanged += this.Selection_ModeChanged;

			return Task.CompletedTask;
		}

		public async Task Shutdown()
		{
			this.SetEnabled(false);
			await Task.Delay(100);

			this.skel1Mem?.Dispose();
			this.skel2Mem?.Dispose();
			this.skel3Mem?.Dispose();
			////this.skel4Mem?.Dispose();
			////this.skel5Mem?.Dispose();
			////this.skel6Mem?.Dispose();
			this.phys1Mem?.Dispose();
			this.phys2Mem?.Dispose();
			////this.phys3Mem?.Dispose();
		}

		public Task Start()
		{
			return Task.CompletedTask;
		}

		public void SetEnabled(bool enabled)
		{
			// Don't try to enable posing unless we are in gpose
			if (enabled && this.selectionService.GetMode() != Modes.GPose)
				throw new Exception("Attempt to enable posing outside of gpose");

			this.isEnabled = enabled;

			this.FreezePositions = enabled;
			this.FreezePhysics = enabled;
			this.FreezeRotation = enabled;
			this.FreezeScale = enabled;

			this.EnabledChanged?.Invoke(enabled);
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsEnabled)));
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
