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
			if (AnamnesisConnectService.IsPenumbraConnected)
				return true;

			return GposeService.Instance.GetIsGPose();
		}

		public override Task Initialize()
		{
			GposeService.GposeStateChanging += () => this.SetCanRefresh();
			AnamnesisConnectService.Instance.PropertyChanged += (s, e) => this.SetCanRefresh();

			this.SetCanRefresh();

			return base.Initialize();
		}

		private void SetCanRefresh()
		{
			bool canRefresh = GposeService.Instance.IsOverworld;

			if (AnamnesisConnectService.IsPenumbraConnected)
				canRefresh = true;

			this.CanRefresh = canRefresh;
		}
	}
}
