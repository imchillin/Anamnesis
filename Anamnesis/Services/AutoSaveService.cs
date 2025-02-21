// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis;

using Anamnesis.Actor;
using Anamnesis.Core;
using Anamnesis.Files;
using Anamnesis.Memory;
using Anamnesis.Services;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// A auto-save service that performs a set of backup actions every <see cref="Settings.AutoSaveIntervalMinutes"/> minutes.
/// </summary>
[AddINotifyPropertyChangedInterface]
public class AutoSaveService : ServiceBase<AutoSaveService>
{
	private const int MinuteToMilliseconds = 60 * 1000;
	private static readonly int MaxStartAttempts = 10; // Maximum number of attempts to start the service
	private CancellationTokenSource? cancellationTokenSource;

	/// <summary>Starts the auto-save service.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public override async Task Start()
	{
		// Wait for Settings service to be initialized
		var servicesToCheck = new List<IService> { SettingsService.Instance, FileService.Instance, TargetService.Instance, PoseService.Instance };
		await EnsureServicesAreAlive(servicesToCheck, MaxStartAttempts, 1000);

		this.RestartUpdateTask();
		await base.Start();
	}

	/// <summary>Shuts down the auto-save service.</summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public override Task Shutdown()
	{
		this.cancellationTokenSource?.Cancel();
		return base.Shutdown();
	}

	/// <summary>(Re)starts the update task with the (new) wake-up time.</summary>
	public void RestartUpdateTask()
	{
		this.cancellationTokenSource?.Cancel();

		if (!SettingsService.Current.EnableAutoSave)
		{
			Log.Verbose("Auto-save is disabled. Update task will not be started.");
			return;
		}

		this.cancellationTokenSource = new CancellationTokenSource();
		_ = Task.Run(() => Update(this.cancellationTokenSource.Token));
	}

	/// <summary>
	/// The update task that will be executed every <see cref="Settings.AutoSaveIntervalMinutes"/> minutes.
	/// </summary>
	/// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the task.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	private static async Task Update(CancellationToken cancellationToken)
	{
		while (!cancellationToken.IsCancellationRequested)
		{
			try
			{
				await Task.Delay(SettingsService.Current.AutoSaveIntervalMinutes * MinuteToMilliseconds, cancellationToken);
				PerformAutoSave();
			}
			catch (TaskCanceledException)
			{
				// Task was canceled, exit the loop
				break;
			}
		}
	}

	/// <summary>
	/// Performs the auto-save action and manages the save files.
	/// </summary>
	private static void PerformAutoSave()
	{
		// Generate a new directory name with the current date and time
		char[] invalidPathChars = Path.GetInvalidPathChars();
		string baseDir = FileService.ParseToFilePath(SettingsService.Current.DefaultAutoSaveDirectory);
		string dirName = $"Autosave_{DateTime.Now:yyyyMMdd_HHmmss}";
		string dirPath = Path.Combine(baseDir, dirName);

		try
		{
			if (SettingsService.Current.AutoSaveOnlyInGpose && !GposeService.Instance.IsGpose)
			{
				Log.Verbose("Skipping auto-save due to backup settings.");
				return;
			}

			// Perform the actual save operation (implement the actual save logic here)
			Log.Verbose($"Performing auto-save to directory: {dirPath}");

			// Save all pinned actors' appearance/equipment
			string charDirPath = Path.Combine(dirPath, "Characters");
			if (!Directory.Exists(charDirPath))
				Directory.CreateDirectory(charDirPath);

			foreach (var pinnedActor in TargetService.Instance.PinnedActors.ToList())
			{
				var actor = pinnedActor.Memory;
				if (actor == null)
					continue;

				CharacterFile file = new();
				string fullFilePath = Path.Combine(charDirPath, $"{actor.Name}{file.FileExtension}");
				if (fullFilePath.Any(c => invalidPathChars.Contains(c)))
				{
					Log.Error($"Invalid character file path: {fullFilePath}");
					break;
				}

				file.WriteToFile(actor, CharacterFile.SaveModes.All);
				using FileStream stream = new(fullFilePath, FileMode.Create);
				file.Serialize(stream);
			}

			if (GposeService.Instance.IsGpose)
			{
				ActorMemory? actorMemory = null;
				ActorBasicMemory? targetActor = TargetService.Instance.PlayerTarget;
				if (targetActor != null && targetActor.IsValid)
				{
					actorMemory = new ActorMemory();
					actorMemory.SetAddress(targetActor.Address);
				}

				if (actorMemory != null)
				{
					// Save the current camera configuration
					string camShotsDir = Path.Combine(dirPath, "CameraShots");
					if (!Directory.Exists(camShotsDir))
						Directory.CreateDirectory(camShotsDir);

					CameraShotFile file = new();
					string fullFilePath = Path.Combine(camShotsDir, $"Camera{file.FileExtension}");
					if (fullFilePath.Any(c => invalidPathChars.Contains(c)))
						Log.Error($"Invalid camera shot file path: {fullFilePath}");

					file.WriteToFile(CameraService.Instance, actorMemory);
					using FileStream stream = new(fullFilePath, FileMode.Create);
					file.Serialize(stream);
				}

				// Save all pinned actors' pose configuration
				// Do not save poses if the pose service is disabled
				if (!PoseService.Instance.IsEnabled)
					return;

				string posesDir = Path.Combine(dirPath, "Poses");
				if (!Directory.Exists(posesDir))
					Directory.CreateDirectory(posesDir);

				foreach (var pinnedActor in TargetService.Instance.PinnedActors.ToList())
				{
					var actor = pinnedActor.Memory;
					if (actor == null)
						continue;

					var skeleton = new Skeleton(actor);

					PoseFile file = new();
					string fullFilePath = Path.Combine(posesDir, $"{actor.Name}{file.FileExtension}");
					if (fullFilePath.Any(c => invalidPathChars.Contains(c)))
					{
						Log.Error($"Invalid pose file path: {fullFilePath}");
						break;
					}

					file.WriteToFile(actor, skeleton, null);
					using FileStream stream = new(fullFilePath, FileMode.Create);
					file.Serialize(stream);
				}
			}
		}
		finally
		{
			var autoSaveDirectories = Array.Empty<string>();
			if (Directory.Exists(baseDir))
			{
				autoSaveDirectories = Directory.GetDirectories(baseDir, "Autosave_*");
			}

			// If the number of auto-save directories exceeds the maximum, delete the oldest
			if (autoSaveDirectories.Length > SettingsService.Current.AutoSaveFileCount)
			{
				// Order directories by creation time
				Array.Sort(autoSaveDirectories, (x, y) => Directory.GetCreationTime(x).CompareTo(Directory.GetCreationTime(y)));

				// Delete the oldest directories until the count is within the limit
				int directoriesToDelete = autoSaveDirectories.Length - SettingsService.Current.AutoSaveFileCount;
				for (int i = 0; i < directoriesToDelete; i++)
				{
					// Ensure all subdirectories and files are not read-only
					var dirInfo = new DirectoryInfo(autoSaveDirectories[i]);
					FileService.SetAttributesNormal(dirInfo);

					Directory.Delete(autoSaveDirectories[i], true);
				}
			}
		}
	}
}
