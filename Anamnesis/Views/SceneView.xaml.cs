// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Views
{
	using System;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis.Files;
	using Anamnesis.Scenes;
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

		private Task<SceneFile?> Save(bool useAdvancedSave)
		{
			SceneFile file = new SceneFile();

			// TODO: config editor
			SceneFile.Configuration config = new SceneFile.Configuration();

			file.WriteToFile(config);
			return Task.FromResult((SceneFile?)file);
		}
	}
}
