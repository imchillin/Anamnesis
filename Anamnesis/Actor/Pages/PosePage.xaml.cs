// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Pages;

using Anamnesis.Actor.Views;
using Anamnesis.Files;
using Anamnesis.GUI.Dialogs;
using Anamnesis.Memory;
using Anamnesis.Services;
using PropertyChanged;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using XivToolsWpf;
using XivToolsWpf.Math3D.Extensions;
using CmQuaternion = System.Numerics.Quaternion;

/// <summary>
/// Interaction logic for CharacterPoseView.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class PosePage : UserControl
{
	public const double DragThreshold = 20;

	public HashSet<BoneView> BoneViews = new();

	private static readonly Type[] PoseFileTypes = new[]
	{
		typeof(PoseFile),
		typeof(CmToolPoseFile),
	};

	private static readonly Shortcut[] PoseDirShortcuts = new[]
	{
		FileService.DefaultPoseDirectory,
		FileService.StandardPoseDirectory,
		FileService.CMToolPoseSaveDir,
	};

	private static DirectoryInfo? lastLoadDir;
	private static DirectoryInfo? lastSaveDir;
	private readonly System.Timers.Timer refreshDebounceTimer;

	private bool isLeftMouseButtonDownOnWindow;
	private bool isDragging;
	private Point origMouseDownPoint;

	private Task? writeSkeletonTask;

	private bool importPoseRotation = true;
	private bool importPosePosition = true;
	private bool importPoseScale = true;

	public PosePage()
	{
		this.InitializeComponent();

		this.ContentArea.DataContext = this;

		HistoryService.OnHistoryApplied += this.OnHistoryApplied;

		// Initialize the debounce timer
		this.refreshDebounceTimer = new(200) { AutoReset = false };
		this.refreshDebounceTimer.Elapsed += async (s, e) => { await this.Refresh(); };
	}

	private enum PoseImportOptions
	{
		Character,      // (Default option) Imports full pose without positions to avoid deformations due to race bone position differences.
		FullTransform,  // Imports full pose with all transforms.
		BodyOnly,       // Imports only the body part of the pose.
		ExpressionOnly, // Imports only the facial expression part of the pose.
		SelectedBones,  // Imports only the selected bones.
	}

	public SettingsService SettingsService => SettingsService.Instance;
	public GposeService GposeService => GposeService.Instance;
	public PoseService PoseService => PoseService.Instance;
	public TargetService TargetService => TargetService.Instance;

	public bool IsFlipping { get; private set; }
	public ActorMemory? Actor { get; private set; }
	public SkeletonVisual3d? Skeleton { get; private set; }

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

	public List<BoneView> GetBoneViews(BoneVisual3d bone)
	{
		List<BoneView> results = new List<BoneView>();
		foreach (BoneView boneView in this.BoneViews)
		{
			if (boneView.Bone == bone)
			{
				results.Add(boneView);
			}
		}

		return results;
	}

	private void FlipBone(BoneVisual3d? targetBone, bool shouldFlip = true)
	{
		if (this.Skeleton == null)
			throw new Exception("Skeleton is null");

		if (targetBone == null)
			throw new ArgumentException("The target bone cannot be null");

		// Save the positions of the target bone and its children
		// The transform memory is used to retrieve the parent-relative position of the bone
		Dictionary<BoneVisual3d, Vector3> bonePositions = new()
		{
			{ targetBone, targetBone.Position },
		};

		if (PoseService.Instance.EnableParenting)
		{
			List<BoneVisual3d> boneChildren = new();
			targetBone.GetChildren(ref boneChildren);
			foreach (BoneVisual3d childBone in boneChildren)
			{
				bonePositions.Add(childBone, childBone.Position);
			}
		}

		this.FlipBoneInternal(targetBone, shouldFlip);

		// Restore positions after flipping
		foreach ((BoneVisual3d bone, Vector3 parentRelPos) in bonePositions)
		{
			foreach (TransformMemory transformMemory in bone.TransformMemories)
			{
				bone.Position = parentRelPos;
				bone.WriteTransform(this.Skeleton, false);
			}
		}
	}

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
	private void FlipBoneInternal(BoneVisual3d? targetBone, bool shouldFlip = true)
	{
		if (targetBone == null)
			throw new ArgumentException("The target bone cannot be null");

		CmQuaternion newRotation = targetBone!.TransformMemory.Rotation.Mirror(); // character-relative transform
		if (shouldFlip && targetBone.BoneName.EndsWith("_l"))
		{
			string rightBoneString = targetBone.BoneName.Substring(0, targetBone.BoneName.Length - 2) + "_r"; // removes the "_l" and replaces it with "_r"
			/*	Useful debug lines to make sure the correct bones are grabbed...
				*	Log.Information("flipping: " + targetBone.BoneName);
				*	Log.Information("right flip target: " + rightBoneString); */
			BoneVisual3d? rightBone = targetBone.Skeleton.GetBone(rightBoneString);
			if (rightBone != null)
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
				Log.Warning("could not find right bone of: " + targetBone.BoneName);
			}
		}
		else if (shouldFlip && targetBone.BoneName.EndsWith("_r"))
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
			foreach (Visual3D? child in targetBone.Children)
			{
				if (child is BoneVisual3d childBone)
				{
					this.FlipBoneInternal(childBone, shouldFlip);
				}
			}
		}
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		this.OnDataContextChanged(null, default);

		PoseService.EnabledChanged += this.OnPoseServiceEnabledChanged;
		this.PoseService.PropertyChanged += this.PoseService_PropertyChanged;
	}

	private void PoseService_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		Application.Current?.Dispatcher.Invoke(() =>
		{
			this.Skeleton?.Reselect();
			this.Skeleton?.ReadTransforms();
		});
	}

	private void OnPoseServiceEnabledChanged(bool value)
	{
		if (!value)
		{
			this.OnClearClicked(null, null);
		}
		else
		{
			Application.Current?.Dispatcher.Invoke(() =>
			{
				this.Skeleton?.ReadTransforms();
			});
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

	private void OnActorRefreshed(object? sender, EventArgs e)
	{
		this.refreshDebounceTimer.Stop();
		this.refreshDebounceTimer.Start();
	}

	private void OnModelObjectChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(ActorModelMemory.Skeleton))
		{
			// Restart the debounce timer if it's already running, otherwise start it.
			this.refreshDebounceTimer.Stop();
			this.refreshDebounceTimer.Start();
		}
	}

	private async Task Refresh()
	{
		await Dispatch.MainThread();

		this.ThreeDView.DataContext = null;
		this.BodyGuiView.DataContext = null;
		this.FaceGuiView.DataContext = null;
		this.MatrixView.DataContext = null;

		this.BoneViews.Clear();

		if (this.Actor == null || this.Actor.ModelObject == null)
		{
			this.Skeleton?.Clear();
			this.Skeleton = null;
			return;
		}

		try
		{
			if (this.Skeleton == null)
				this.Skeleton = new SkeletonVisual3d();

			await this.Skeleton.SetActor(this.Actor);

			this.ThreeDView.DataContext = this.Skeleton;
			this.BodyGuiView.DataContext = this.Skeleton;
			this.FaceGuiView.DataContext = this.Skeleton;
			this.MatrixView.DataContext = this.Skeleton;

			if (this.writeSkeletonTask == null || this.writeSkeletonTask.IsCompleted)
			{
				this.writeSkeletonTask = Task.Run(this.WriteSkeletonThread);
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to bind skeleton to view");
		}
	}

	private async void OnImportClicked(object sender, RoutedEventArgs e)
	{
		await this.ImportPose(PoseImportOptions.Character, PoseFile.Mode.All);
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

	private async Task ImportPose(PoseImportOptions importOption, PoseFile.Mode mode)
	{
		if (this.Actor == null || this.Skeleton == null)
			return;

		bool originalAutoCommitEnabled = this.Actor.History.AutoCommitEnabled;

		try
		{
			// Open and load pose file
			OpenResult result = await FileService.Open(lastLoadDir, PoseDirShortcuts, PoseFileTypes);
			if (result.File == null)
				return;

			lastLoadDir = result.Directory;

			bool isLegacyFile = false;
			if (result.File is CmToolPoseFile legacyFile)
			{
				isLegacyFile = true;
				result.File = legacyFile.Upgrade(this.Actor.Customize?.Race ?? ActorCustomizeMemory.Races.Hyur);
			}

			if (result.File is not PoseFile poseFile)
				return;

			Dictionary<BoneVisual3d, Vector3> bodyPositions = new();
			Dictionary<BoneVisual3d, Vector3> facePositions = new();
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
			PoseService.Instance.FreezeScale |= mode.HasFlag(PoseFile.Mode.Scale);
			PoseService.Instance.FreezeRotation |= mode.HasFlag(PoseFile.Mode.Rotation);
			PoseService.Instance.FreezePositions |= mode.HasFlag(PoseFile.Mode.Position);

			if (importOption == PoseImportOptions.SelectedBones)
			{
				// Don't unselected bones after import. Let the user decide what to do with the selection.
				var selectedBones = this.Skeleton.SelectedBones.Select(bone => bone.BoneName).ToHashSet();
				poseFile.Apply(this.Actor, this.Skeleton, selectedBones, mode, false);
				return;
			}

			// Backup face bone positions before importing the body pose.
			// "Freeze Position" toggle resets them, so restore after import. Relevant only when pose service is enabled.
			this.Skeleton.SelectHead();
			facePositions = this.Skeleton.SelectedBones.ToDictionary(bone => bone, bone => bone.Position);
			this.Skeleton.ClearSelection();

			// Step 1: Import body part of the pose
			if (importOption is PoseImportOptions.Character or PoseImportOptions.FullTransform or PoseImportOptions.BodyOnly)
			{
				this.Skeleton.SelectBody();
				var selectedBoneNames = this.Skeleton.SelectedBones.Select(bone => bone.BoneName).ToHashSet();
				bodyPositions = this.Skeleton.SelectedBones.ToDictionary(bone => bone, bone => bone.Position);
				this.Skeleton.ClearSelection();

				// Don't import body with positions unless it's "Full Transform".
				// Otherwise, the body will be deformed if the pose file was created for another race.
				bool doLegacyImport = importOption == PoseImportOptions.Character && mode.HasFlag(PoseFile.Mode.Position);
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
					foreach ((BoneVisual3d bone, Vector3 position) in bodyPositions)
					{
						bone.Position = position;
						bone.WriteTransform(this.Skeleton, false);
					}
				}
			}

			// Step 2: Import the facial expression
			if (!mismatchedFaceBones && (importOption is PoseImportOptions.Character or PoseImportOptions.FullTransform or PoseImportOptions.ExpressionOnly))
			{
				this.Skeleton.SelectHead();
				var selectedBones = this.Skeleton.SelectedBones.Select(bone => bone.BoneName).ToHashSet();
				this.Skeleton.ClearSelection();

				// Pre-DT faces need to be imported without positions.
				bool doLegacyImport = this.Actor.Customize!.Age == ActorCustomizeMemory.Ages.None
									|| (poseFile.IsPreDTPoseFile() && this.Skeleton.HasPreDTFace && mode.HasFlag(PoseFile.Mode.Position));
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
					foreach ((BoneVisual3d bone, Vector3 position) in facePositions)
					{
						bone.Position = position;
						bone.WriteTransform(this.Skeleton, false);
					}
				}
			}

			// Step 3: Restore face bone positions if face bones were mismatched
			if (mismatchedFaceBones)
			{
				foreach ((BoneVisual3d bone, Vector3 position) in facePositions)
				{
					bone.Position = position;
					bone.WriteTransform(this.Skeleton, false);
				}
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
		lastSaveDir = await PoseFile.Save(lastSaveDir, this.Actor, this.Skeleton);
	}

	private async void OnExportMetaClicked(object sender, RoutedEventArgs e)
	{
		lastSaveDir = await PoseFile.Save(lastSaveDir, this.Actor, this.Skeleton, null, true);
	}

	private async void OnExportSelectedClicked(object sender, RoutedEventArgs e)
	{
		if (this.Skeleton == null)
			return;

		HashSet<string> bones = new HashSet<string>();
		foreach (BoneVisual3d bone in this.Skeleton.SelectedBones)
		{
			bones.Add(bone.BoneName);
		}

		lastSaveDir = await PoseFile.Save(lastSaveDir, this.Actor, this.Skeleton, bones);
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

		List<BoneVisual3d> bones = new List<BoneVisual3d>();
		foreach (BoneVisual3d bone in this.Skeleton.SelectedBones)
		{
			bone.GetChildren(ref bones);
		}

		this.Skeleton.Select(bones, SkeletonVisual3d.SelectMode.Add);
	}

	private void OnFlipClicked(object sender, RoutedEventArgs e)
	{
		if (this.Actor == null)
			throw new ArgumentNullException(nameof(this.Actor));

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
			if (this.Skeleton.CurrentBone == null)
			{
				BoneVisual3d? waistBone = this.Skeleton.GetBone("Waist");
				BoneVisual3d? lumbarBone = this.Skeleton.GetBone("SpineA");
				this.FlipBone(waistBone);
				this.FlipBone(lumbarBone);
				waistBone?.ReadTransform(true);
				lumbarBone?.ReadTransform(true);
			}
			else
			{
				// If targeted bone is a limb don't switch the respective left and right sides
				BoneVisual3d targetBone = this.Skeleton.CurrentBone;
				if (targetBone.BoneName.EndsWith("_l") || targetBone.BoneName.EndsWith("_r"))
				{
					this.FlipBone(targetBone, false);
				}
				else
				{
					this.FlipBone(targetBone);
				}

				targetBone.ReadTransform(true);
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
		if (this.Skeleton?.CurrentBone?.Parent == null)
			return;

		this.Skeleton.Select(this.Skeleton.CurrentBone.Parent);
	}

	private void OnCanvasMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Left)
		{
			this.isLeftMouseButtonDownOnWindow = true;
			this.origMouseDownPoint = e.GetPosition(this.SelectionCanvas);
		}
	}

	private void OnCanvasMouseMove(object sender, MouseEventArgs e)
	{
		if (this.Skeleton == null)
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

			List<BoneView> bones = new List<BoneView>();

			foreach (BoneView bone in this.BoneViews)
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
					this.Skeleton.Hover(bone.Bone, true, false);
				}
				else
				{
					this.Skeleton.Hover(bone.Bone, false);
				}
			}

			this.Skeleton.NotifyHover();
		}
		else if (this.isLeftMouseButtonDownOnWindow)
		{
			System.Windows.Vector dragDelta = curMouseDownPoint - this.origMouseDownPoint;
			double dragDistance = Math.Abs(dragDelta.Length);
			if (dragDistance > DragThreshold)
			{
				this.isDragging = true;
				this.MouseCanvas.CaptureMouse();
				e.Handled = true;
			}
		}
	}

	private void OnCanvasMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
	{
		if (!this.isLeftMouseButtonDownOnWindow)
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

				List<BoneView> toSelect = new List<BoneView>();

				foreach (BoneView bone in this.BoneViews)
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

				this.Skeleton.Select(toSelect);
			}

			this.DragSelectionBorder.Visibility = Visibility.Collapsed;
			e.Handled = true;
		}
		else
		{
			if (this.Skeleton != null && !this.Skeleton.HasHover)
			{
				this.Skeleton.Select(Enumerable.Empty<IBone>());
			}
		}

		this.MouseCanvas.ReleaseMouseCapture();
	}

	private async void OnHistoryApplied()
	{
		if (this.Skeleton == null || this.Skeleton.Actor == null)
			return;

		await Dispatch.MainThread();
		this.Skeleton.ReadTransforms();
	}

	private async Task WriteSkeletonThread()
	{
		while (Application.Current != null && this.Skeleton != null)
		{
			await Dispatch.MainThread();

			if (this.Skeleton == null)
				return;

			this.Skeleton.WriteSkeleton();

			// up to 60 times a second
			await Task.Delay(16);
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
}
