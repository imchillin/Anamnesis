// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.PoseModule
{
	using System;
	using System.ComponentModel;
	using System.Threading.Tasks;
	using System.Windows;
	using Anamnesis.Core.Memory;
	using Anamnesis.Memory;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	[SuppressPropertyChangedWarnings]
	public class PoseService : ServiceBase<PoseService>
	{
		private NopHookViewModel skel1Mem;
		private NopHookViewModel skel2Mem;
		private NopHookViewModel skel3Mem;
		private NopHookViewModel skel4Mem;
		private NopHookViewModel skel5Mem;
		private NopHookViewModel skel6Mem;
		private NopHookViewModel phys1Mem;
		private NopHookViewModel phys2Mem;
		private NopHookViewModel phys3Mem;

		private bool isEnabled;

		public delegate void PoseEvent(bool value);

		public static event PoseEvent EnabledChanged;
		public static event PoseEvent FreezePhysicsChanged;
		public static event PoseEvent FreezePositionsChanged;
		public static event PoseEvent FreezeScaleChanged;
		public static event PoseEvent FreezeRotationChanged;
		public static event PoseEvent AvailableChanged;

		public bool IsAvailable
		{
			get
			{
				return TargetService.CurrentMode == Modes.GPose;
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
				return this.phys1Mem.Enabled;
			}
			set
			{
				this.FreezePositions = value;
				this.FreezeScale = value;

				this.phys1Mem.Enabled = value;
				this.phys2Mem.Enabled = value;

				FreezePhysicsChanged?.Invoke(value);
			}
		}

		public bool FreezePositions
		{
			get
			{
				return this.skel5Mem.Enabled;
			}
			set
			{
				this.skel5Mem.Enabled = value;

				FreezePositionsChanged?.Invoke(value);
			}
		}

		public bool FreezeScale
		{
			get
			{
				return this.skel4Mem.Enabled;
			}
			set
			{
				this.skel4Mem.Enabled = value;
				this.phys3Mem.Enabled = value;
				this.skel6Mem.Enabled = value;

				FreezeScaleChanged?.Invoke(value);
			}
		}

		public bool FreezeRotation
		{
			get
			{
				return this.skel1Mem.Enabled;
			}
			set
			{
				this.skel1Mem.Enabled = value;
				this.skel2Mem.Enabled = value;
				this.skel3Mem.Enabled = value;

				FreezeRotationChanged?.Invoke(value);
			}
		}

		public override async Task Initialize()
		{
			await base.Initialize();

			this.skel1Mem = new NopHookViewModel(AddressService.SkeletonFreezeRotation, 6);
			this.skel2Mem = new NopHookViewModel(AddressService.SkeletonFreezeRotation2, 6);
			this.skel3Mem = new NopHookViewModel(AddressService.SkeletonFreezeRotation3, 4);
			this.skel4Mem = new NopHookViewModel(AddressService.SkeletonFreezeScale, 6);
			this.skel5Mem = new NopHookViewModel(AddressService.SkeletonFreezePosition, 5);
			this.skel6Mem = new NopHookViewModel(AddressService.SkeletonFreezeScale2, 6);
			////this.skel7Mem = new NopHookViewModel(AddressService.SkeletonFreezePosition2, 5);

			this.phys1Mem = new NopHookViewModel(AddressService.SkeletonFreezePhysics, 4);
			this.phys2Mem = new NopHookViewModel(AddressService.SkeletonFreezePhysics2, 3);
			this.phys3Mem = new NopHookViewModel(AddressService.SkeletonFreezePhysics3, 4);

			TargetService.ModeChanged += this.Selection_ModeChanged;
		}

		public override async Task Shutdown()
		{
			await base.Shutdown();
			this.SetEnabled(false);
		}

		public void SetEnabled(bool enabled)
		{
			// Don't try to enable posing unless we are in gpose
			////if (enabled && TargetService.CurrentMode != Modes.GPose)
			////	throw new Exception("Attempt to enable posing outside of gpose");

			this.isEnabled = enabled;

			////this.FreezePositions = enabled;
			////this.FreezePhysics = enabled;
			this.FreezeRotation = enabled;
			////this.FreezeScale = enabled;

			EnabledChanged?.Invoke(enabled);

			this.RaisePropertyChanged(nameof(this.IsEnabled));
		}

		private void Selection_ModeChanged(Modes mode)
		{
			bool available = this.IsAvailable;

			if (!available)
				this.IsEnabled = false;

			Application.Current.Dispatcher.Invoke(() =>
			{
				AvailableChanged?.Invoke(available);
			});
		}
	}
}
