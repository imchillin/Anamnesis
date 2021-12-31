// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services
{
	using System.Threading.Tasks;
	using Anamnesis.Connect;

	public class ActorRefreshService : ServiceBase<ActorRefreshService>
	{
		public bool CanRefresh { get; set; }

		public static bool GetCanRefresh()
		{
			bool canRefresh = GposeService.Instance.IsOverworld;

			if (AnamnesisConnectService.IsPenumbraConnected)
				canRefresh = true;

			return canRefresh;
		}

		public override Task Initialize()
		{
			GposeService.GposeStateChanging += () => this.UpdateCanRefresh();
			AnamnesisConnectService.Instance.PropertyChanged += (s, e) => this.UpdateCanRefresh();
			return base.Initialize();
		}

		public override Task Start()
		{
			this.UpdateCanRefresh();
			return base.Start();
		}

		private void UpdateCanRefresh()
		{
			bool canRefresh = GetCanRefresh();
			if (canRefresh != this.CanRefresh)
				this.CanRefresh = canRefresh;
		}
	}
}
