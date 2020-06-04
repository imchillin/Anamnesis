// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule
{
	using System.Threading.Tasks;

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

		public delegate void PoseEvent(bool value);

		public event PoseEvent OnEnabledChanged;
		public event PoseEvent OnFreezePhysicsChanged;
		public event PoseEvent OnFreezePositionsChanged;

		public bool IsEnabled
		{
			get
			{
				return this.skel1Mem.Value.IsEnabled;
			}

			set
			{
				if (this.IsEnabled == value)
					return;

				// rotations
				this.skel1Mem.Value = Flag.Get(value);
				this.skel2Mem.Value = Flag.Get(value);
				this.skel3Mem.Value = Flag.Get(value);

				// scale
				this.skel4Mem.Value = Flag.Get(value);
				this.skel6Mem.Value = Flag.Get(value);

				this.FreezePositions = value;
				this.FreezePhysics = value;

				this.OnEnabledChanged?.Invoke(value);
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
				this.phys1Mem.Value = Flag.Get(value);
				this.phys2Mem.Value = Flag.Get(value);
				this.phys3Mem.Value = Flag.Get(value);

				this.OnFreezePhysicsChanged?.Invoke(value);
			}
		}

		public bool FreezePositions
		{
			get
			{
				return this.skel5Mem.Value.IsEnabled;
			}
			set
			{
				this.skel5Mem.Value = Flag.Get(value);

				this.OnFreezePositionsChanged?.Invoke(value);
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
	}
}
