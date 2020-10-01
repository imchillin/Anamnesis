// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Views
{
	using System;
	using System.Windows;
	using System.Windows.Controls;
	using PropertyChanged;
	using SimpleLog;

	/// <summary>
	/// Interaction logic for SceneView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	[SuppressPropertyChangedWarnings]
	public partial class SceneView : UserControl
	{
		private static readonly Logger Log = SimpleLog.Log.GetLogger<SceneView>();

		public SceneView()
		{
			this.InitializeComponent();

			this.ContentArea.DataContext = this;
		}

		public TerritoryService TerritoryService { get => TerritoryService.Instance; }
		public TimeService TimeService { get => TimeService.Instance; }
		public CameraService CameraService { get => CameraService.Instance; }

		private void OnLoadClicked(object sender, RoutedEventArgs e)
		{
			Log.Write(Severity.Error, new NotImplementedException());
		}

		private void OnSaveClicked(object sender, RoutedEventArgs e)
		{
			Log.Write(Severity.Error, new NotImplementedException());
		}
	}
}
