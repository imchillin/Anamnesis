// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Views
{
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis.Services;
	using PropertyChanged;
	using SimpleLog;

	/// <summary>
	/// Interaction logic for SceneView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class SceneView : UserControl
	{
		private static readonly Logger Log = SimpleLog.Log.GetLogger<SceneView>();

		public SceneView()
		{
			this.InitializeComponent();

			this.ContentArea.DataContext = this;
		}

		public TargetService TargetService => TargetService.Instance;
		public GposeService GposeService => GposeService.Instance;
		public TerritoryService TerritoryService => TerritoryService.Instance;
		public TimeService TimeService => TimeService.Instance;
		public CameraService CameraService => CameraService.Instance;
		public TipService TipService => TipService.Instance;
		public SettingsService SettingsService => SettingsService.Instance;

		private void OnTipClicked(object sender, RoutedEventArgs e)
		{
			TipService.Instance.KnowMore();
		}
	}
}
