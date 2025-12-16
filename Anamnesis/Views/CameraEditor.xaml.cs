// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Views;

using Anamnesis.Files;
using Anamnesis.Services;
using PropertyChanged;
using Serilog;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using static Anamnesis.Styles.Controls.SliderInputBox;

/// <summary>
/// Interaction logic for SceneView.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class CameraEditor : UserControl
{
	private static DirectoryInfo? s_lastLoadDir;
	private static DirectoryInfo? s_lastSaveDir;

	public CameraEditor()
	{
		this.InitializeComponent();

		this.ContentArea.DataContext = this;

		SettingsService.SettingsChanged += this.OnSettingsChanged;
	}

	public static GameService GameService => GameService.Instance;
	public static TargetService TargetService => TargetService.Instance;
	public static GposeService GposeService => GposeService.Instance;
	public static TerritoryService TerritoryService => TerritoryService.Instance;
	public static TimeService TimeService => TimeService.Instance;
	public static CameraService CameraService => CameraService.Instance;
	public static SettingsService SettingsService => SettingsService.Instance;
	public static Settings Settings => SettingsService.Current;

#pragma warning disable CA1822
	public OverflowModes RotationOverflowBehavior => Settings.WrapRotationSliders ? OverflowModes.Loop : OverflowModes.Clamp;
#pragma warning restore CA1822

	private static ILogger Log => Serilog.Log.ForContext<CameraEditor>();

	private async void OnImportCamera(object sender, RoutedEventArgs e)
	{
		var playerTarget = TargetService.PlayerTargetHandle;
		if (playerTarget == null || !playerTarget.IsValid)
			return;

		try
		{
			Shortcut[]? shortcuts = [FileService.DefaultCameraDirectory];

			Type[] types = [typeof(CameraShotFile)];

			OpenResult result = await FileService.Open(s_lastLoadDir, shortcuts, types);

			if (result.File == null)
				return;

			s_lastLoadDir = result.Directory;

			if (result.File is CameraShotFile camFile)
			{
				camFile.Apply(CameraService.Instance, playerTarget);
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to load camera");
		}
	}

	private async void OnExportCamera(object sender, RoutedEventArgs e)
	{
		var playerTarget = TargetService.PlayerTargetHandle;
		if (playerTarget == null || !playerTarget.IsValid)
			return;

		SaveResult result = await FileService.Save<CameraShotFile>(s_lastSaveDir, FileService.DefaultCameraDirectory);

		if (result.Path == null)
			return;

		s_lastSaveDir = result.Directory;

		var file = new CameraShotFile();
		file.WriteToFile(CameraService.Instance, playerTarget);

		using var stream = new FileStream(result.Path.FullName, FileMode.Create);
		file.Serialize(stream);
	}

	private void OnTargetActor(object sender, RoutedEventArgs e)
	{
		var playerTarget = TargetService.PlayerTargetHandle;
		if (playerTarget == null || !playerTarget.IsValid)
			return;

		playerTarget.Do(a =>
		{
			if (a.ModelObject?.Transform == null)
				return;

			CameraService.GPoseCamera.Position = a.ModelObject.Transform.Position;
		});
	}

	private void OnSettingsChanged(object? sender, EventArgs e)
	{
		this.OnPropertyChanged(nameof(this.RotationOverflowBehavior));
	}
}
