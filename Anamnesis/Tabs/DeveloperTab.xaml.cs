// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Tabs;

using Anamnesis.Actor.Utilities;
using Anamnesis.Files;
using Anamnesis.Memory;
using Anamnesis.Services;
using Anamnesis.Utils;
using Anamnesis.Views;
using Serilog;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

/// <summary>
/// Interaction logic for DeveloperTab.xaml.
/// </summary>
public partial class DeveloperTab : UserControl
{
	public DeveloperTab()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public TargetService TargetService => TargetService.Instance;
	public GposeService GposeService => GposeService.Instance;
	public SceneOptionsValues SceneOptions { get; init; } = new();

	private void OnNpcNameSearchClicked(object sender, RoutedEventArgs e)
	{
		GenericSelectorUtil.Show(GameDataService.BattleNpcNames, (v) =>
		{
			if (v.Description == null)
				return;

			ClipboardUtility.CopyToClipboard(v.Description);
		});
	}

	private void OnFindNpcClicked(object sender, RoutedEventArgs e)
	{
		TargetSelectorView.Show((a) =>
		{
			ActorMemory memory = new();

			if (a is ActorMemory actorMemory)
				memory = actorMemory;

			memory.SetAddress(a.Address);

			NpcAppearanceSearch.Search(memory);
		});
	}

	private void OnCopyActorAddressClicked(object sender, RoutedEventArgs e)
	{
		ActorBasicMemory memory = this.TargetService.PlayerTarget;

		if (!memory.IsValid)
		{
			Log.Warning("Actor is invalid");
			return;
		}

		string address = memory.Address.ToString("X");

		ClipboardUtility.CopyToClipboard(address);
	}

	private void OnCopyAssociatedAddressesClick(object sender, RoutedEventArgs e)
	{
		ActorBasicMemory abm = this.TargetService.PlayerTarget;

		if (!abm.IsValid)
		{
			Log.Warning("Actor is invalid");
			return;
		}

		try
		{
			ActorMemory memory = new();
			memory.SetAddress(abm.Address);

			StringBuilder sb = new();

			sb.AppendLine("Base: " + memory.Address.ToString("X"));
			sb.AppendLine("Model: " + (memory.ModelObject?.Address.ToString("X") ?? "0"));
			sb.AppendLine("Extended Appearance: " + (memory.ModelObject?.ExtendedAppearance?.Address.ToString("X") ?? "0"));
			sb.AppendLine("Skeleton: " + (memory.ModelObject?.Skeleton?.Address.ToString("X") ?? "0"));
			sb.AppendLine("Main Hand Model: " + (memory.MainHand?.Model?.Address.ToString("X") ?? "0"));
			sb.AppendLine("Off Hand Model: " + (memory.OffHand?.Model?.Address.ToString("X") ?? "0"));
			sb.AppendLine("Mount: " + (memory.Mount?.Address.ToString("X") ?? "0"));
			sb.AppendLine("Companion: " + (memory.Companion?.Address.ToString("X") ?? "0"));
			sb.AppendLine("Ornament: " + (memory.Ornament?.Address.ToString("X") ?? "0"));

			ClipboardUtility.CopyToClipboard(sb.ToString());
		}
		catch
		{
			Log.Warning("Could not read addresses");
		}
	}

	private async void OnSaveSceneClicked(object sender, RoutedEventArgs e)
	{
		try
		{
			SaveResult result = await FileService.Save<SceneFile>(null, FileService.DefaultSceneDirectory);

			if (result.Path == null)
				return;

			SceneFile file = new();
			await file.WriteToFile();

			using FileStream stream = new FileStream(result.Path.FullName, FileMode.Create);
			file.Serialize(stream);
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to save scene");
		}
	}

	private async void OnLoadSceneClicked(object sender, RoutedEventArgs e)
	{
		try
		{
			Shortcut[]? shortcuts = new[]
			{
				FileService.DefaultSceneDirectory,
			};

			Type[] types = new[]
			{
				typeof(SceneFile),
			};

			OpenResult result = await FileService.Open(null, shortcuts, types);

			if (result.File == null)
				return;

			if (result.File is SceneFile sceneFile)
			{
				await sceneFile.Apply(this.SceneOptions.GetMode());
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to load scene");
		}
	}

	public class SceneOptionsValues
	{
		public bool RelativePositions { get; set; } = true;
		public bool WorldPositions { get; set; } = false;
		public bool Poses { get; set; } = true;
		public bool Camera { get; set; } = false;
		public bool Weather { get; set; } = false;
		public bool Time { get; set; } = false;

		public SceneFile.Mode GetMode()
		{
			SceneFile.Mode mode = 0;

			if (this.RelativePositions)
				mode |= SceneFile.Mode.RelativePosition;

			if (this.WorldPositions)
				mode |= SceneFile.Mode.WorldPosition;

			if (this.Poses)
				mode |= SceneFile.Mode.Pose;

			if (this.Camera)
				mode |= SceneFile.Mode.Camera;

			if (this.Weather)
				mode |= SceneFile.Mode.Weather;

			if (this.Time)
				mode |= SceneFile.Mode.Time;

			return mode;
		}
	}
}
