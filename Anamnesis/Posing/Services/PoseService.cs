// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.PoseModule
{
	using System;
	using System.IO;
	using System.Threading.Tasks;
	using Anamnesis.Core.Memory;
	using Anamnesis.Files;
	using Anamnesis.Memory;
	using Anamnesis.Services;
	using PropertyChanged;

	[AddINotifyPropertyChangedInterface]
	public class PoseService : ServiceBase<PoseService>
	{
		private NopHookViewModel? freezeRot1;
		private NopHookViewModel? freezeRot2;
		private NopHookViewModel? freezeRot3;
		private NopHookViewModel? freezeScale1;
		private NopHookViewModel? freezePosition;
		private NopHookViewModel? freezePosition2;
		private NopHookViewModel? freeseScale2;
		private NopHookViewModel? freezePhysics1;
		private NopHookViewModel? freezePhysics2;
		private NopHookViewModel? freezePhysics3;
		private NopHookViewModel? freezeWorldPosition;
		private NopHookViewModel? freezeWorldRotation;
		private NopHookViewModel? freezeGposeTargetPosition1;
		private NopHookViewModel? freezeGposeTargetPosition2;

		private bool isEnabled;

		public delegate void PoseEvent(bool value);

		public static event PoseEvent? EnabledChanged;

		public bool IsEnabled
		{
			get
			{
				return this.isEnabled;
			}

			set
			{
				if (this.IsEnabled == value)
					return;

				this.SetEnabled(value);
			}
		}

		public bool FreezePhysics
		{
			get
			{
				return this.freezePhysics1?.Enabled ?? false;
			}
			set
			{
				this.freezePhysics1?.SetEnabled(value);
				this.freezePhysics2?.SetEnabled(value);
			}
		}

		public bool FreezePositions
		{
			get
			{
				return this.freezePosition?.Enabled ?? false;
			}
			set
			{
				this.freezePosition?.SetEnabled(value);
				this.freezePosition2?.SetEnabled(value);
			}
		}

		public bool FreezeScale
		{
			get
			{
				return this.freezeScale1?.Enabled ?? false;
			}
			set
			{
				this.freezeScale1?.SetEnabled(value);
				this.freezePhysics3?.SetEnabled(value);
				this.freeseScale2?.SetEnabled(value);
			}
		}

		public bool FreezeRotation
		{
			get
			{
				return this.freezeRot1?.Enabled ?? false;
			}
			set
			{
				this.freezeRot1?.SetEnabled(value);
				this.freezeRot2?.SetEnabled(value);
				this.freezeRot3?.SetEnabled(value);
			}
		}

		public bool WorldPositionNotFrozen => !this.FreezeWorldPosition;

		public bool FreezeWorldPosition
		{
			get
			{
				return this.freezeWorldPosition?.Enabled ?? false;
			}
			set
			{
				this.freezeWorldPosition?.SetEnabled(value);
				this.freezeWorldRotation?.SetEnabled(value);
				this.freezeGposeTargetPosition1?.SetEnabled(value);
				this.freezeGposeTargetPosition2?.SetEnabled(value);
				this.RaisePropertyChanged(nameof(PoseService.FreezeWorldPosition));
				this.RaisePropertyChanged(nameof(PoseService.WorldPositionNotFrozen));
			}
		}

		public bool EnableParenting { get; set; } = true;

		public bool CanEdit { get; set; }

		public override async Task Initialize()
		{
			await base.Initialize();

			this.freezePosition = new NopHookViewModel(AddressService.SkeletonFreezePosition, 5);
			this.freezePosition2 = new NopHookViewModel(AddressService.SkeletonFreezePosition2, 5);
			this.freezeRot1 = new NopHookViewModel(AddressService.SkeletonFreezeRotation, 6);
			this.freezeRot2 = new NopHookViewModel(AddressService.SkeletonFreezeRotation2, 6);
			this.freezeRot3 = new NopHookViewModel(AddressService.SkeletonFreezeRotation3, 4);
			this.freezeScale1 = new NopHookViewModel(AddressService.SkeletonFreezeScale, 6);
			this.freeseScale2 = new NopHookViewModel(AddressService.SkeletonFreezeScale2, 6);
			this.freezePhysics1 = new NopHookViewModel(AddressService.SkeletonFreezePhysics, 4);
			this.freezePhysics2 = new NopHookViewModel(AddressService.SkeletonFreezePhysics2, 3);
			this.freezePhysics3 = new NopHookViewModel(AddressService.SkeletonFreezePhysics3, 4);
			this.freezeWorldPosition = new NopHookViewModel(AddressService.WorldPositionFreeze, 16);
			this.freezeWorldRotation = new NopHookViewModel(AddressService.WorldRotationFreeze, 4);

			// We need to keep the MOV in the middle here otherwise we invalidate the ptr, but we patch the rest:
			//     MOVSS dword ptr[RCX + 0xa0],XMM1
			//     MOV RBX,RCX
			//     MOVSS dword ptr[RCX + 0xa4],XMM2
			//     MOVSS dword ptr[RCX + 0xa8],XMM3
			this.freezeGposeTargetPosition1 = new NopHookViewModel(AddressService.GPoseCameraTargetPositionFreeze, 8);
			this.freezeGposeTargetPosition2 = new NopHookViewModel(AddressService.GPoseCameraTargetPositionFreeze + 8 + 3, 16);

			GposeService.GposeStateChanging += this.OnGposeStateChanging;

			_ = Task.Run(ExtractStandardPoses);
		}

		public override async Task Shutdown()
		{
			await base.Shutdown();
			this.SetEnabled(false);
		}

		public void SetEnabled(bool enabled)
		{
			// Don't try to enable posing unless we are in gpose
			if (enabled && !GposeService.Instance.IsGpose)
				throw new Exception("Attempt to enable posing outside of gpose");

			this.FreezeWorldPosition = enabled; // This one can be toggled externally so we always set it

			if (this.isEnabled == enabled)
				return;

			this.isEnabled = enabled;
			this.FreezePhysics = enabled;
			this.FreezeRotation = enabled;
			this.FreezePositions = false;
			this.FreezeScale = false;
			this.EnableParenting = true;

			EnabledChanged?.Invoke(enabled);

			this.RaisePropertyChanged(nameof(this.IsEnabled));
		}

		private static async Task ExtractStandardPoses()
		{
			try
			{
				DirectoryInfo standardPoseDir = FileService.StandardPoseDirectory.Directory;
				string verFile = standardPoseDir.FullName + "\\ver.txt";

				if (standardPoseDir.Exists)
				{
					string verText = await File.ReadAllTextAsync(verFile);
					DateTime standardPoseVersion = DateTime.Parse(verText);

					if (standardPoseVersion == VersionInfo.Date)
					{
						Log.Information($"Standard pose library up to date");
						return;
					}

					standardPoseDir.Delete(true);
				}

				standardPoseDir.Create();
				await File.WriteAllTextAsync(verFile, VersionInfo.Date.ToString());

				string[] poses = EmbeddedFileUtility.GetAllFilesInDirectory("\\Data\\StandardPoses\\");
				foreach (string posePath in poses)
				{
					string destPath = posePath;
					destPath = destPath.Replace('.', '\\');
					destPath = destPath.Replace('_', ' ');
					destPath = destPath.Replace("Data\\StandardPoses\\", string.Empty);

					// restore file extensions
					destPath = destPath.Replace("\\pose", ".pose");
					destPath = destPath.Replace("\\txt", ".txt");

					destPath = standardPoseDir.FullName + destPath;

					string? destDir = Path.GetDirectoryName(destPath);

					if (destDir == null)
						throw new Exception($"Failed to get directory name from path: {destPath}");

					if (!Directory.Exists(destDir))
						Directory.CreateDirectory(destDir);

					using Stream contents = EmbeddedFileUtility.Load(posePath);
					using FileStream fileStream = new FileStream(destPath, FileMode.Create);
					await contents.CopyToAsync(fileStream);
				}

				Log.Information($"Extracted standard pose library");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to extract standard pose library");
			}
		}

		private void OnGposeStateChanging()
		{
			if (GposeService.Instance.IsOverworld)
				this.SetEnabled(false);
		}
	}
}
