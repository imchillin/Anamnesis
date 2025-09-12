// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Pages;

using Anamnesis.Actor.Posing;
using Anamnesis.Actor.Views;
using Anamnesis.Core;
using Anamnesis.Core.Extensions;
using Anamnesis.Files;
using Anamnesis.GUI.Dialogs;
using Anamnesis.Memory;
using Anamnesis.Services;
using PropertyChanged;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using XivToolsWpf;
using XivToolsWpf.Math3D.Extensions;
using CmQuaternion = System.Numerics.Quaternion;

/// <summary>
/// Interaction logic for CharacterPoseView.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class PosePage : UserControl, INotifyPropertyChanged
{
	public const double DRAG_THRESHOLD = 20;

	private static readonly Type[] s_poseFileTypes =
	[
		typeof(PoseFile),
		typeof(CmToolPoseFile),
	];

	private static readonly Shortcut[] s_poseDirShortcuts =
	[
		FileService.DefaultPoseDirectory,
		FileService.StandardPoseDirectory,
		FileService.CMToolPoseSaveDir,
	];

	private static DirectoryInfo? s_lastLoadDir;
	private static DirectoryInfo? s_lastSaveDir;
	private readonly System.Timers.Timer refreshDebounceTimer;

	private bool isLeftMouseButtonDownOnWindow;
	private bool isDragging;
	private Point origMouseDownPoint;

	private Task? skeletonUpdateThread;

	private bool importPoseRotation = true;
	private bool importPosePosition = true;
	private bool importPoseScale = true;

	private string? selectedBonesTooltipCache;
	private string? selectedBoneNameCache;
	private string? selectedBoneTextCache;

	public PosePage()
	{
		this.InitializeComponent();

		this.ContentArea.DataContext = this;

		// Initialize the debounce timer
		this.refreshDebounceTimer = new(200) { AutoReset = false };
		this.refreshDebounceTimer.Elapsed += async (s, e) => { await this.Refresh(); };
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	private enum PoseImportOptions
	{
		Character,      // (Default option) Imports full pose without positions to avoid deformations due to race bone position differences.
		FullTransform,  // Imports full pose with all transforms.
		BodyOnly,       // Imports only the body part of the pose.
		ExpressionOnly, // Imports only the facial expression part of the pose.
		SelectedBones,  // Imports only the selected bones.
		WeaponsOnly,    // Imports only the weapons (main hand and off-hand).
	}

	public static SettingsService SettingsService => SettingsService.Instance;
	public static GposeService GposeService => GposeService.Instance;
	public static PoseService PoseService => PoseService.Instance;
	public static TargetService TargetService => TargetService.Instance;

	public bool IsFlipping { get; private set; }
	public ActorMemory? Actor { get; private set; }
	public SkeletonEntity? Skeleton { get; private set; }

	public bool IsSingleBoneSelected => this.Skeleton?.SelectedBones.Count() == 1;
	public bool IsMultipleBonesSelected => this.Skeleton?.SelectedBones.Count() > 1;

	public string SelectedBonesText
	{
		get => this.selectedBoneTextCache ?? string.Empty;
		set
		{
			if (this.Skeleton == null)
				return;

			// Setter only handles renaming tooltips for single bone selections
			// Ignore everything else
			var selectedBones = this.Skeleton.SelectedBones.ToList();
			if (selectedBones.Count == 1)
			{
				selectedBones.First().Tooltip = value;
			}
		}
	}

	public string SelectedBoneName => this.selectedBoneNameCache ?? string.Empty;
	public string SelectedBonesTooltip => this.selectedBonesTooltipCache ?? string.Empty;

	public bool ImportPoseRotation
	{
		get => this.importPoseRotation;
		set
		{
			// Don't disable if it's the only one enabled
			// We want to have at least one option enabled at all times
			if (!value && !this.importPosePosition && !this.importPoseScale)
				return;

			this.importPoseRotation = value;
		}
	}

	public bool ImportPosePosition
	{
		get => this.importPosePosition;
		set
		{
			// Don't disable if it's the only one enabled
			// We want to have at least one option enabled at all times
			if (!value && !this.importPoseRotation && !this.importPoseScale)
				return;

			this.importPosePosition = value;
		}
	}

	public bool ImportPoseScale
	{
		get => this.importPoseScale;
		set
		{
			// Don't disable if it's the only one enabled
			// We want to have at least one option enabled at all times
			if (!value && !this.importPoseRotation && !this.importPosePosition)
				return;

			this.importPoseScale = value;
		}
	}

	private static ILogger Log => Serilog.Log.ForContext<PosePage>();

	/* Basic Idea:
	 * get mirrored quat of targetBone
	 * check if its a 'left' bone
	 *- if it is:
	 *          - get the mirrored quat of the corresponding right bone
	 *          - store the first quat (from left bone) on the right bone
	 *          - store the second quat (from right bone) on the left bone
	 *      - if not:
	 *          - store the quat on the target bone
	 *  - recursively flip on all child bones
	 */
	private static void FlipBoneInternal(Bone? targetBone, bool shouldFlip = true)
	{
		if (targetBone == null || targetBone.TransformMemory == null)
			throw new ArgumentException("The target bone and its transform memory cannot be null");

		CmQuaternion newRotation = targetBone.TransformMemory.Rotation.Mirror(); // character-relative transform
		if (shouldFlip && targetBone.Name.EndsWith("_l"))
		{
			string rightBoneString = string.Concat(targetBone.Name.AsSpan(0, targetBone.Name.Length - 2), "_r"); // removes the "_l" and replaces it with "_r"
			/*  Useful debug lines to make sure the correct bones are grabbed...
				*  Log.Information("flipping: " + targetBone.BoneName);
				*  Log.Information("right flip target: " + rightBoneString); */
			Bone? rightBone = targetBone.Skeleton.GetBone(rightBoneString);
			if (rightBone != null && rightBone.TransformMemory != null)
			{
				CmQuaternion rightRot = rightBone.TransformMemory.Rotation.Mirror();
				foreach (TransformMemory transformMemory in targetBone.TransformMemories)
				{
					transformMemory.Rotation = rightRot;
				}

				targetBone.ReadTransform();

				foreach (TransformMemory transformMemory in rightBone.TransformMemories)
				{
					transformMemory.Rotation = newRotation;
				}

				rightBone.ReadTransform();
			}
			else
			{
				Log.Warning("could not find right bone of: " + targetBone.Name);
			}
		}
		else if (shouldFlip && targetBone.Name.EndsWith("_r"))
		{
			// do nothing so it doesn't revert...
		}
		else
		{
			foreach (TransformMemory transformMemory in targetBone.TransformMemories)
			{
				transformMemory.Rotation = newRotation;
			}

			targetBone.ReadTransform();
		}

		if (PoseService.Instance.EnableParenting)
		{
			foreach (var child in targetBone.Children)
			{
				FlipBoneInternal(child, shouldFlip);
			}
		}
	}

	private void FlipBone(Bone? targetBone, bool shouldFlip = true)
	{
		if (this.Skeleton == null)
			throw new Exception("Skeleton is null");

		if (targetBone == null)
			throw new ArgumentException("The target bone cannot be null");

		// Save the positions of the target bone and its children
		// The transform memory is used to retrieve the parent-relative position of the bone
		Dictionary<Bone, Vector3> bonePositions = new()
		{
			{ targetBone, targetBone.Position },
		};

		if (PoseService.Instance.EnableParenting)
		{
			List<Bone> boneChildren = targetBone.GetDescendants();
			foreach (var childBone in boneChildren)
			{
				bonePositions.Add(childBone, childBone.Position);
			}
		}

		FlipBoneInternal(targetBone, shouldFlip);

		// Restore positions after flipping
		this.RestoreBonePositions(bonePositions);
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		this.OnDataContextChanged(null, default);

		HistoryService.OnHistoryApplied += this.OnHistoryApplied;
		PoseService.PropertyChanged += this.OnPoseServicePropertyChanged;
	}

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		PoseService.PropertyChanged -= this.OnPoseServicePropertyChanged;
		HistoryService.OnHistoryApplied -= this.OnHistoryApplied;

		if (this.Actor?.ModelObject != null)
		{
			this.Actor.Refreshed -= this.OnActorRefreshed;
			this.Actor.ModelObject.PropertyChanged -= this.OnModelObjectChanged;
		}

		if (this.Skeleton != null)
		{
			this.Skeleton.PropertyChanged -= this.OnSkeletonPropertyChanged;
		}

		this.Skeleton?.Clear();
		this.Skeleton = null;
	}

	private void OnPoseServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		// Don't refresh the skeleton if the selected bones text changes to prevent a loop
		if (e.PropertyName == nameof(PoseService.SelectedBonesText))
			return;

		this.Skeleton?.Reselect();
		this.Skeleton?.ReadTransforms();

		if (e.PropertyName == nameof(PoseService.IsEnabled))
		{
			if (!PoseService.IsEnabled)
				this.OnClearClicked(null, null);

			Application.Current?.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, BoneViewManager.Instance.Refresh);
		}
	}

	private async void OnDataContextChanged(object? sender, DependencyPropertyChangedEventArgs e)
	{
		ActorMemory? newActor = this.DataContext as ActorMemory;

		// Don't do all this work unless we need to.
		if (this.Actor == newActor)
			return;

		if (this.Actor?.ModelObject != null)
		{
			this.Actor.Refreshed -= this.OnActorRefreshed;
			this.Actor.ModelObject.PropertyChanged -= this.OnModelObjectChanged;
		}

		if (newActor?.ModelObject != null)
		{
			newActor.Refreshed += this.OnActorRefreshed;
			newActor.ModelObject.PropertyChanged += this.OnModelObjectChanged;
		}

		this.Actor = newActor;

		await this.Refresh();
	}

	private void OnActorRefreshed(object? sender, EventArgs? e)
	{
		// Restart the debounce timer if it's already running, otherwise start it.
		this.refreshDebounceTimer.Stop();
		this.refreshDebounceTimer.Start();
	}

	private void OnModelObjectChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(ActorModelMemory.Skeleton))
		{
			this.OnActorRefreshed(null, default);
		}
	}

	private async Task Refresh()
	{
		await Dispatch.MainThread();

		this.ThreeDView.DataContext = null;
		this.BodyGuiView.DataContext = null;
		this.FaceGuiView.DataContext = null;
		this.MatrixView.DataContext = null;

		if (this.Actor == null || this.Actor.ModelObject == null)
		{
			this.Skeleton?.Clear();
			this.Skeleton = null;
			return;
		}

		try
		{
			if (this.Skeleton != null)
			{
				this.Skeleton.PropertyChanged -= this.OnSkeletonPropertyChanged;
			}

			this.Skeleton = new SkeletonEntity(this.Actor);
			BoneViewManager.Instance.SetSkeleton(this.Skeleton);

			this.Skeleton.PropertyChanged += this.OnSkeletonPropertyChanged;

			this.ThreeDView.DataContext = this.Skeleton;
			this.BodyGuiView.DataContext = this.Skeleton;
			this.FaceGuiView.DataContext = this.Skeleton;
			this.MatrixView.DataContext = this.Skeleton;

			// Start the skeleton read/write tasks
			if (this.skeletonUpdateThread == null || this.skeletonUpdateThread.IsCompleted || this.skeletonUpdateThread.IsFaulted)
			{
				this.skeletonUpdateThread = Task.Run(this.SkeletonUpdateThread);
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to bind skeleton to view");
		}
	}

	private void OnSkeletonPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(SkeletonEntity.SelectedBones))
		{
			this.RaisePropertyChanged(nameof(this.IsSingleBoneSelected));
			this.RaisePropertyChanged(nameof(this.IsMultipleBonesSelected));

			this.UpdateSelectedBonesCache(); // Update selected bones text cache
			PoseService.SelectedBonesText = this.SelectedBonesTooltip;
		}
	}

	private async void OnImportClicked(object sender, RoutedEventArgs e)
	{
		bool isShiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
		PoseFile.Mode mode = isShiftPressed ? PoseFile.Mode.All : (PoseFile.Mode.All & ~PoseFile.Mode.Scale);
		await this.ImportPose(PoseImportOptions.Character, mode);
		this.Skeleton?.ClearSelection();
	}

	private async void OnImportFullCharClicked(object sender, RoutedEventArgs e)
	{
		await this.HandleSecondaryOptionImport(PoseImportOptions.FullTransform);
	}

	private async void OnImportBodyClicked(object sender, RoutedEventArgs e)
	{
		await this.HandleSecondaryOptionImport(PoseImportOptions.BodyOnly);
	}

	private async void OnImportExpressionClicked(object sender, RoutedEventArgs e)
	{
		await this.HandleSecondaryOptionImport(PoseImportOptions.ExpressionOnly);
	}

	private async void OnImportSelectedBonesClicked(object sender, RoutedEventArgs e)
	{
		await this.HandleSecondaryOptionImport(PoseImportOptions.SelectedBones);
	}

	private async void OnImportWeaponsClicked(object sender, RoutedEventArgs e)
	{
		await this.HandleSecondaryOptionImport(PoseImportOptions.WeaponsOnly);
	}

	private async Task ImportPose(PoseImportOptions importOption, PoseFile.Mode mode)
	{
		if (this.Actor == null || this.Skeleton == null)
			return;

		bool originalAutoCommitEnabled = this.Actor.History.AutoCommitEnabled;

		try
		{
			// Open and load pose file
			OpenResult result = await FileService.Open(s_lastLoadDir, s_poseDirShortcuts, s_poseFileTypes);
			if (result.File == null)
				return;

			s_lastLoadDir = result.Directory;

			bool isLegacyFile = false;
			if (result.File is CmToolPoseFile legacyFile)
			{
				isLegacyFile = true;
				result.File = legacyFile.Upgrade();
			}

			if (result.File is not PoseFile poseFile)
				return;

			Dictionary<Bone, Vector3> facePositions = [];
			bool mismatchedFaceBones = false;

			// Disable auto-commit at the beginning
			// Commit any changes if they are present to avoid falsely grouping actions
			this.Actor.History.AutoCommitEnabled = false;
			this.Actor.History.Commit();

			// Display a warning if there is a face bones mismatch.
			// We should not load expressions if the face bones are mismatched.
			// We skip this step for non-humanoid actors
			if (this.Actor.Customize!.Age != ActorCustomizeMemory.Ages.None
				&& importOption is PoseImportOptions.Character or PoseImportOptions.FullTransform or PoseImportOptions.ExpressionOnly)
			{
				mismatchedFaceBones = isLegacyFile || poseFile.IsPreDTPoseFile() != this.Skeleton.HasPreDTFace;
				if (mismatchedFaceBones)
				{
					string dialogMsgKey = isLegacyFile || poseFile.IsPreDTPoseFile()
						? "Pose_WarningExpresionOldOnNew"
						: "Pose_WarningExpresionNewOnOld";
					await GenericDialog.ShowLocalizedAsync(dialogMsgKey, "Common_Attention", MessageBoxButton.OK);
				}
			}

			// Exit early if bones are mismatched and we're only importing expressions
			if (mismatchedFaceBones && importOption == PoseImportOptions.ExpressionOnly)
				return;

			// Positions are not frozen yet, that will happen at the appropriate moment
			PoseService.Instance.SetEnabled(true);
			PoseService.Instance.FreezeScale |= mode.HasFlagUnsafe(PoseFile.Mode.Scale);
			PoseService.Instance.FreezeRotation |= mode.HasFlagUnsafe(PoseFile.Mode.Rotation);
			PoseService.Instance.FreezePositions |= mode.HasFlagUnsafe(PoseFile.Mode.Position);

			if (importOption == PoseImportOptions.SelectedBones)
			{
				// Don't unselected bones after import. Let the user decide what to do with the selection.
				var selectedBones = this.Skeleton.SelectedBones.Select(bone => bone.Name).ToHashSet();
				poseFile.Apply(this.Actor, this.Skeleton, selectedBones, mode, false);
				return;
			}

			if (importOption == PoseImportOptions.WeaponsOnly)
			{
				this.Skeleton.SelectWeapons();
				var selectedBoneNames = this.Skeleton.SelectedBones.Select(bone => bone.Name).ToHashSet();
				poseFile.Apply(this.Actor, this.Skeleton, selectedBoneNames, mode, false);
				this.Skeleton.ClearSelection();
				return;
			}

			// Backup face bone positions before importing the body pose.
			// "Freeze Position" toggle resets them, so restore after import. Relevant only when pose service is enabled.
			this.Skeleton.SelectHead();
			facePositions = this.Skeleton.SelectedBones.ToDictionary(bone => bone as Bone, bone => bone.Position);
			this.Skeleton.ClearSelection();

			// Step 1: Import body part of the pose
			if (importOption is PoseImportOptions.Character or PoseImportOptions.FullTransform or PoseImportOptions.BodyOnly)
			{
				this.Skeleton.SelectBody();
				var selectedBoneNames = this.Skeleton.SelectedBones.Select(bone => bone.Name).ToHashSet();
				this.Skeleton.ClearSelection();

				// Don't import body with positions during default pose import.
				// Otherwise, the body will be deformed if the pose file was created for another race.
				bool doLegacyImport = importOption == PoseImportOptions.Character && mode.HasFlagUnsafe(PoseFile.Mode.Position);
				if (doLegacyImport)
				{
					mode &= ~PoseFile.Mode.Position;
				}

				// Don't apply the facial expression hack for the body import step.
				// Otherwise, the head won't pose as intended and will return to its original position.
				poseFile.Apply(this.Actor, this.Skeleton, selectedBoneNames, mode, false);

				// Re-enable positions if they were disabled.
				if (doLegacyImport)
				{
					mode |= PoseFile.Mode.Position;
				}
			}

			// Step 2: Import the facial expression
			if (!mismatchedFaceBones && (importOption is PoseImportOptions.Character or PoseImportOptions.FullTransform or PoseImportOptions.ExpressionOnly))
			{
				this.Skeleton.SelectHead();
				var selectedBones = this.Skeleton.SelectedBones.Select(bone => bone.Name).ToHashSet();
				this.Skeleton.ClearSelection();

				// Pre-DT faces need to be imported without positions.
				bool doLegacyImport = this.Actor.Customize!.Age == ActorCustomizeMemory.Ages.None
									|| (poseFile.IsPreDTPoseFile() && this.Skeleton.HasPreDTFace && mode.HasFlagUnsafe(PoseFile.Mode.Position));
				if (doLegacyImport)
				{
					mode &= ~PoseFile.Mode.Position;
				}

				// Apply facial expression hack for the expression import
				poseFile.Apply(this.Actor, this.Skeleton, selectedBones, mode, true);

				// Re-enable positions if they were disabled.
				if (doLegacyImport)
				{
					mode |= PoseFile.Mode.Position;
				}
			}

			// Step 3: Restore face bone positions if face bones were mismatched
			// This is necessary as .Apply will not be called for the face bones
			if (mismatchedFaceBones)
			{
				this.RestoreBonePositions(facePositions);
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to load pose file");
		}
		finally
		{
			// Update the skeleton after applying the pose
			this.Skeleton.ReadTransforms();

			// Re-enable auto-commit and commit changes
			this.Actor.History.Commit();
			this.Actor.History.AutoCommitEnabled = originalAutoCommitEnabled;
		}
	}

	private async void OnExportClicked(object sender, RoutedEventArgs e)
	{
		if (this.Actor == null || this.Skeleton == null)
			return;

		s_lastSaveDir = await PoseFile.Save(s_lastSaveDir, this.Actor, this.Skeleton);
	}

	private async void OnExportMetaClicked(object sender, RoutedEventArgs e)
	{
		if (this.Actor == null || this.Skeleton == null)
			return;

		s_lastSaveDir = await PoseFile.Save(s_lastSaveDir, this.Actor, this.Skeleton, null, true);
	}

	private async void OnExportSelectedClicked(object sender, RoutedEventArgs e)
	{
		if (this.Skeleton == null)
			return;

		var bones = new HashSet<string>();
		foreach (Bone bone in this.Skeleton.SelectedBones)
		{
			bones.Add(bone.Name);
		}

		s_lastSaveDir = await PoseFile.Save(s_lastSaveDir, this.Actor, this.Skeleton, bones);
	}

	private void OnViewChanged(object sender, SelectionChangedEventArgs e)
	{
		int selected = this.ViewSelector.SelectedIndex;

		if (this.BodyGuiView == null)
			return;

		this.BodyGuiView.Visibility = selected == 0 ? Visibility.Visible : Visibility.Collapsed;
		this.FaceGuiView.Visibility = selected == 1 ? Visibility.Visible : Visibility.Collapsed;
		this.MatrixView.Visibility = selected == 2 ? Visibility.Visible : Visibility.Collapsed;
		this.ThreeDView.Visibility = selected == 3 ? Visibility.Visible : Visibility.Collapsed;
	}

	private void OnClearClicked(object? sender, RoutedEventArgs? e)
	{
		this.Skeleton?.ClearSelection();
	}

	private void OnSelectChildrenClicked(object sender, RoutedEventArgs e)
	{
		if (this.Skeleton == null)
			return;

		var bones = new List<BoneEntity>();
		foreach (Bone bone in this.Skeleton.SelectedBones)
		{
			bones.AddRange(bone.GetDescendants().Cast<BoneEntity>());
		}

		this.Skeleton.Select(bones, SkeletonEntity.SelectMode.Add);
	}

	private void OnFlipClicked(object sender, RoutedEventArgs e)
	{
		if (this.Actor == null)
			throw new ArgumentNullException(nameof(sender));

		if (this.Actor.ModelObject == null || this.Actor.ModelObject.Transform == null)
			throw new Exception("Actor has no model");

		if (this.Actor.ModelObject.Skeleton == null)
			throw new Exception("Actor model has no skeleton. Are you trying to load a pose outside of GPose?");

		if (this.Skeleton == null || this.IsFlipping)
			return;

		SkeletonMemory skeletonMem = this.Actor.ModelObject.Skeleton;
		skeletonMem.PauseSynchronization = true;

		bool originalAutoCommitEnabled = this.Actor.History.AutoCommitEnabled;

		try
		{
			// Disable auto-commit at the beginning
			// Commit any changes if they are present to avoid falsely grouping actions
			this.Actor.History.AutoCommitEnabled = false;
			this.Actor.History.Commit();

			// If no bone selected, flip both lumbar and waist bones
			this.IsFlipping = true;
			if (!this.Skeleton.HasSelection)
			{
				if (this.Skeleton.GetBone("n_hara") is Bone abdomenBone)
				{
					this.FlipBone(abdomenBone);
					abdomenBone.ReadTransform(true);
				}
				else
				{
					Log.Warning("Could not find abdomen bone");
				}
			}
			else
			{
				// If targeted bone is a limb don't switch the respective left and right sides
				if (this.Skeleton.SelectedBones.Any(b => b.Name.EndsWith("_l") || b.Name.EndsWith("_r")) == false)
				{
					foreach (Bone bone in this.Skeleton.SelectedBones)
					{
						this.FlipBone(bone);
						bone.ReadTransform(true);
					}
				}
				else
				{
					foreach (Bone bone in this.Skeleton.SelectedBones)
					{
						this.FlipBone(bone, false);
						bone.ReadTransform(true);
					}
				}
			}

			this.IsFlipping = false;

			skeletonMem.PauseSynchronization = false;
			skeletonMem.WriteDelayedBinds();
		}
		finally
		{
			// Re-enable auto-commit and commit changes
			this.Actor.History.Commit();
			this.Actor.History.AutoCommitEnabled = originalAutoCommitEnabled;
		}
	}

	private void OnParentClicked(object sender, RoutedEventArgs e)
	{
		// If any of the selected bones have no parent, don't do anything
		if (this.Skeleton == null || this.Skeleton.SelectedBones.Any(b => b.Parent == null) == true)
			return;

		// Select the parents of the selected bones.
		// If the selected bone has no parent, reselect the root bone.
		var selectedBonesParents = this.Skeleton.SelectedBones.Select(b => b.Parent ?? b).Distinct().ToList();
		this.Skeleton.Select(selectedBonesParents);
	}

	private void OnCanvasMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.Handled)
			return;

		if (e.ChangedButton == MouseButton.Left)
		{
			this.isLeftMouseButtonDownOnWindow = true;
			this.origMouseDownPoint = e.GetPosition(this.SelectionCanvas);
		}
	}

	private void OnCanvasMouseMove(object sender, MouseEventArgs e)
	{
		if (e.Handled || this.Skeleton == null)
			return;

		Point curMouseDownPoint = e.GetPosition(this.SelectionCanvas);

		if (this.isDragging)
		{
			e.Handled = true;

			this.DragSelectionBorder.Visibility = Visibility.Visible;

			double minx = Math.Min(curMouseDownPoint.X, this.origMouseDownPoint.X);
			double miny = Math.Min(curMouseDownPoint.Y, this.origMouseDownPoint.Y);
			double maxx = Math.Max(curMouseDownPoint.X, this.origMouseDownPoint.X);
			double maxy = Math.Max(curMouseDownPoint.Y, this.origMouseDownPoint.Y);

			minx = Math.Max(minx, 0);
			miny = Math.Max(miny, 0);
			maxx = Math.Min(maxx, this.SelectionCanvas.ActualWidth);
			maxy = Math.Min(maxy, this.SelectionCanvas.ActualHeight);

			Canvas.SetLeft(this.DragSelectionBorder, minx);
			Canvas.SetTop(this.DragSelectionBorder, miny);
			this.DragSelectionBorder.Width = maxx - minx;
			this.DragSelectionBorder.Height = maxy - miny;

			var bones = new List<BoneView>();

			foreach (BoneView bone in BoneViewManager.Instance.BoneViews)
			{
				if (bone.Bone == null)
					continue;

				if (!bone.IsDescendantOf(this.MouseCanvas))
					continue;

				if (!bone.IsVisible)
					continue;

				Point relativePoint = bone.TransformToAncestor(this.MouseCanvas).Transform(new Point(0, 0));
				if (relativePoint.X > minx && relativePoint.X < maxx && relativePoint.Y > miny && relativePoint.Y < maxy)
				{
					this.Skeleton.Hover(bone.Bone, true);
				}
				else
				{
					this.Skeleton.Hover(bone.Bone, false);
				}
			}
		}
		else if (this.isLeftMouseButtonDownOnWindow)
		{
			System.Windows.Vector dragDelta = curMouseDownPoint - this.origMouseDownPoint;
			double dragDistance = Math.Abs(dragDelta.Length);
			if (dragDistance > DRAG_THRESHOLD)
			{
				this.isDragging = true;
				this.MouseCanvas.CaptureMouse();
				e.Handled = true;
			}
		}
	}

	private void OnCanvasMouseUp(object sender, MouseButtonEventArgs e)
	{
		if (e.Handled || !this.isLeftMouseButtonDownOnWindow)
			return;

		this.isLeftMouseButtonDownOnWindow = false;
		if (this.isDragging)
		{
			this.isDragging = false;

			if (this.Skeleton != null)
			{
				double minx = Canvas.GetLeft(this.DragSelectionBorder);
				double miny = Canvas.GetTop(this.DragSelectionBorder);
				double maxx = minx + this.DragSelectionBorder.Width;
				double maxy = miny + this.DragSelectionBorder.Height;

				var toSelect = new List<BoneView>();

				foreach (BoneView bone in BoneViewManager.Instance.BoneViews)
				{
					if (bone.Bone == null)
						continue;

					if (!bone.IsVisible)
						continue;

					this.Skeleton.Hover(bone.Bone, false);

					Point relativePoint = bone.TransformToAncestor(this.MouseCanvas).Transform(new Point(0, 0));
					if (relativePoint.X > minx && relativePoint.X < maxx && relativePoint.Y > miny && relativePoint.Y < maxy)
					{
						toSelect.Add(bone);
					}
				}

				this.Skeleton.Select(toSelect.Where(b => b.Bone != null).Select(b => b.Bone!).ToList());
			}

			this.DragSelectionBorder.Visibility = Visibility.Collapsed;
			e.Handled = true;
		}
		else
		{
			if (this.Skeleton != null && !this.Skeleton.HasHover)
			{
				this.Skeleton.Select([]);
			}
		}

		this.MouseCanvas.ReleaseMouseCapture();
	}

	private void OnHistoryApplied()
	{
		if (this.Skeleton == null || this.Skeleton.Actor == null)
			return;

		this.Skeleton.ReadTransforms();
	}

	private async Task SkeletonUpdateThread()
	{
		while (this.Skeleton != null)
		{
			if (this.Skeleton == null)
				return;

			// Only update transforms while the pose service is disabled
			if (!PoseService.Instance.IsEnabled)
			{
				this.Skeleton.ReadTransforms();
			}
			else
			{
				this.Skeleton.WriteSkeleton();
			}

			await Task.Delay(16); // Up to 60 times a second
		}
	}

	private async Task HandleSecondaryOptionImport(PoseImportOptions importOption)
	{
		if (this.Skeleton == null)
			return;

		// Don't do anything if no import mode is selected.
		var importMode = this.GetSecondaryImportOptionMode();
		if (importMode == PoseFile.Mode.None)
			return;

		await this.ImportPose(importOption, importMode);
	}

	private PoseFile.Mode GetSecondaryImportOptionMode()
	{
		PoseFile.Mode mode = PoseFile.Mode.None;
		if (this.ImportPoseRotation)
			mode |= PoseFile.Mode.Rotation;
		if (this.ImportPosePosition)
			mode |= PoseFile.Mode.Position;
		if (this.ImportPoseScale)
			mode |= PoseFile.Mode.Scale;

		return mode;
	}

	private void RestoreBonePositions(Dictionary<Bone, Vector3> bonePositions)
	{
		if (this.Skeleton == null || bonePositions.Count == 0)
			return;

		// Sort the selected bones based on their hierarchy
		var sortedBones = Bone.SortBonesByHierarchy(bonePositions.Keys.ToList());

		foreach (var bone in sortedBones)
		{
			bone.Position = bonePositions[bone];
			bone.WriteTransform(false);
		}
	}

	private void RaisePropertyChanged(string propertyName)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	private void UpdateSelectedBonesCache()
	{
		if (this.Skeleton == null)
			return;

		var selectedBones = this.Skeleton.SelectedBones.ToList();
		int count = selectedBones.Count;

		this.selectedBonesTooltipCache = count switch
		{
			0 => (TargetService.SelectedActor != null) ? TargetService.SelectedActor.DisplayName : string.Empty,
			1 => selectedBones.First().Tooltip,
			<= 3 => string.Join(", ", selectedBones.Select(b => b.Tooltip)),
			_ => string.Join(", ", selectedBones.Take(3).Select(b => b.Tooltip)) + LocalizationService.GetStringFormatted("Pose_SelectedBones_TooltipTrimmed", (count - 3).ToString()),
		};

		this.selectedBoneNameCache = count == 1 ? selectedBones.First().Name : string.Empty;
		this.selectedBoneTextCache = count == 1 ? selectedBones.First().Tooltip : LocalizationService.GetStringFormatted("Pose_SelectedBones_MultiSelected", count.ToString());

		this.RaisePropertyChanged(nameof(this.SelectedBonesTooltip));
		this.RaisePropertyChanged(nameof(this.SelectedBoneName));
		this.RaisePropertyChanged(nameof(this.SelectedBonesText));
	}
}
