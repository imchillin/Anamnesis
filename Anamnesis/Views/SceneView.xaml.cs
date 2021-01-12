// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Views
{
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis.Services;
	using PropertyChanged;
	using Serilog;

	/// <summary>
	/// Interaction logic for SceneView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class SceneView : UserControl
	{
		public SceneView()
		{
			this.InitializeComponent();

			this.ContentArea.DataContext = this;
		}

		public GameService GameService => GameService.Instance;
		public TargetService TargetService => TargetService.Instance;
		public GposeService GposeService => GposeService.Instance;
		public TerritoryService TerritoryService => TerritoryService.Instance;
		public TimeService TimeService => TimeService.Instance;
		public CameraService CameraService => CameraService.Instance;
		public TipService TipService => TipService.Instance;
		public SettingsService SettingsService => SettingsService.Instance;

		private static ILogger Log => Serilog.Log.ForContext<SceneView>();

		private void OnTipClicked(object sender, RoutedEventArgs e)
		{
			TipService.Instance.KnowMore();
		}
	}
}
