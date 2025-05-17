// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Tabs;

using Anamnesis.Actor.Utilities;
using Anamnesis.Files;
using Anamnesis.Memory;
using Anamnesis.Services;
using Anamnesis.Utils;
using Anamnesis.Views;
using Microsoft.Win32;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security.Claims;
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
			file.WriteToFile();

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

	private void OnInstallThumbHandlerClicked(object sender, RoutedEventArgs e) {
		string version = "1.0.4";
		string appDataPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			$"Anamnesis/PoseFileCOMHandler/{version}");
		string poseDllName = "PoseFileCOMHandler.DLL";
		string poseDllPath = Path.Combine(appDataPath, poseDllName);
		string codeBase = "file:///" + poseDllPath.Replace("\\", "/");
		string threadingModel = "Both";
		string @class = "PoseFileCOMHandler.JsonBase64ThumbHandler";
		string assembly = "PoseFileCOMHandler, Version=1.0.0.0, Culture=neutral, PublicKeyToken=a8d30131506ef419";
		string runtimeVersion = "v4.0.30319";

		string clsid = "{3908774C-CD56-4E9B-9CE2-871A978652A6}";
		string standardRegPath = $@"CLSID\{clsid}";
		string impRegPath = $@"{standardRegPath}\Implemented Categories\{{62C8FE65-4EBB-45e7-B440-6E39B2CDBF29}}";
		string inpRegPath = $@"{standardRegPath}\InprocServer32";
		string inpVerRegPath = $@"{inpRegPath}\1.0.0.0";
		string progRegPath = $@"{standardRegPath}\ProgId";
		string fileRegPath = @".pose";
		string fullFileRegPath = $@"{fileRegPath}\ShellEx\{{E357FCCD-A995-4576-B01F-234630154E96}}";

		try {
			if (!Directory.Exists(appDataPath))
				Directory.CreateDirectory(appDataPath);

			string currentDir = Path.Combine(Directory.GetCurrentDirectory(), "Updater");

			void CopyDep(string fileName) {
				string src = Path.Combine(currentDir, fileName);
				string dest = Path.Combine(appDataPath, fileName);
				if (!File.Exists(src)) {
					Log.Error($"Dependency missing: {src}");
					throw new FileNotFoundException($"Dependency missing: {src}");
				}
				File.Copy(src, dest, true);
			}

			CopyDep(poseDllName);
			CopyDep("Newtonsoft.Json.dll");
			CopyDep("SharpShell.dll");
			CopyDep("System.Drawing.Common.dll");

			using (var keyImp = Registry.ClassesRoot.CreateSubKey(impRegPath))
			using (var keyInp = Registry.ClassesRoot.CreateSubKey(inpRegPath))
			using (var keyInpVer = Registry.ClassesRoot.CreateSubKey(inpVerRegPath))
			using (var keyFile = Registry.ClassesRoot.CreateSubKey(fileRegPath))
			using (var keyFileEx = Registry.ClassesRoot.CreateSubKey(fullFileRegPath))
			using (var keyClsid = Registry.ClassesRoot.CreateSubKey(standardRegPath))
			using (var keyProgId = Registry.ClassesRoot.CreateSubKey(progRegPath)) {
				keyClsid.SetValue(null, "Pose File Thumbnail Handler");
				keyProgId.SetValue(null, @class);

				keyInp.SetValue(null, "mscoree.dll");
				keyInp.SetValue("ThreadingModel", threadingModel);
				keyInp.SetValue("Class", @class);
				keyInp.SetValue("Assembly", assembly);
				keyInp.SetValue("RuntimeVersion", runtimeVersion);
				keyInp.SetValue("CodeBase", codeBase);

				keyInpVer.SetValue("Class", @class);
				keyInpVer.SetValue("Assembly", assembly);
				keyInpVer.SetValue("RuntimeVersion", runtimeVersion);
				keyInpVer.SetValue("CodeBase", codeBase);

				keyFile.SetValue(null, "pose.1");
				keyFileEx.SetValue(null, clsid.ToLower());
			}

			Log.Information("Thumbnail handler installed successfully.");
		} catch (Exception ex) {
			Log.Error($"Failed to install thumbnail handler: {ex}");
		}
	}


	private void OnUninstallThumbHandlerClicked(object sender, RoutedEventArgs e) {
		string version = "1.0.4";
		string appDataPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			$"Anamnesis/PoseFileCOMHandler/{version}");
		string clsid = "{3908774C-CD56-4E9B-9CE2-871A978652A6}";
		string standardRegPath = $@"CLSID\{clsid}";
		string fileRegPath = @".pose";

		try {
			// Remove CLSID key (and all subkeys, like InprocServer32, Implemented Categories, etc.) and .pose key.
			Registry.ClassesRoot.DeleteSubKeyTree(standardRegPath, false);
			Registry.ClassesRoot.DeleteSubKeyTree(fileRegPath, false);
			Directory.Delete(appDataPath);
			Log.Information("Thumbnail handler uninstalled successfully.");
		} catch (Exception ex) {
			Log.Error($"Uninstall failed: {ex}");
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
