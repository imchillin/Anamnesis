// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Views
{
	using System;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis.Files;
	using Anamnesis.Memory;
	using Anamnesis.Scenes;
	using Anamnesis.Services;
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

		public TargetService TargetService => TargetService.Instance;
		public GposeService GposeService => GposeService.Instance;
		public TerritoryService TerritoryService => TerritoryService.Instance;
		public TimeService TimeService => TimeService.Instance;
		public CameraService CameraService => CameraService.Instance;
		public TipService TipService => TipService.Instance;

		private async void OnLoadClicked(object sender, RoutedEventArgs e)
		{
			try
			{
				OpenResult result = await FileService.Open<SceneFile>();

				if (result.File == null)
					return;

				SceneFile.Configuration config = new SceneFile.Configuration();

				////if (result.UseAdvancedLoad)
				////	config = await ViewService.ShowDialog<BoneGroupsSelectorDialog, PoseFile.Configuration>("Load Scene...");

				if (config == null)
					return;

				if (result.File is SceneFile sceneFile)
				{
					await sceneFile.Apply(config);
				}
			}
			catch (Exception ex)
			{
				Log.Write(Severity.Error, ex);
			}
		}

		private async void OnSaveClicked(object sender, RoutedEventArgs e)
		{
			await FileService.Save(this.Save);
		}

		private async Task<SceneFile?> Save(bool useAdvancedSave)
		{
			SceneFile file = new SceneFile();

			// TODO: config editor
			SceneFile.Configuration config = new SceneFile.Configuration();

			await file.WriteToFile(config);
			return file;
		}

		private void OnTipClicked(object sender, RoutedEventArgs e)
		{
			TipService.Instance.KnowMore();
		}
	}
}
