// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Views;

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Anamnesis.Actor;
using Anamnesis.Brio;
using Anamnesis.Files;
using Anamnesis.Memory;
using Anamnesis.Services;
using Anamnesis.Styles.Drawers;
using XivToolsWpf;

public abstract class TargetSelectorDrawer : SelectorDrawer<ActorBasicMemory>
{
}

/// <summary>
/// Interaction logic for TargetSelectorView.xaml.
/// </summary>
public partial class TargetSelectorView : TargetSelectorDrawer
{
	private static bool includePlayers = true;
	private static bool includeCompanions = true;
	private static bool includeNPCs = true;
	private static bool includeMounts = true;
	private static bool includeOrnaments = true;
	private static bool includeOther = false;
	private static bool includeHidden = false;

	public TargetSelectorView()
	{
		this.InitializeComponent();

		this.ContentArea.DataContext = this;

		this.PropertyChanged += this.OnSelfPropertyChanged;
	}

	public TargetService TargetService => TargetService.Instance;
	public GposeService GPoseService => GposeService.Instance;
	public SettingsService SettingsService => SettingsService.Instance;

	public bool IncludePlayers
	{
		get => includePlayers;
		set => includePlayers = value;
	}

	public bool IncludeCompanions
	{
		get => includeCompanions;
		set => includeCompanions = value;
	}

	public bool IncludeNPCs
	{
		get => includeNPCs;
		set => includeNPCs = value;
	}

	public bool IncludeMounts
	{
		get => includeMounts;
		set => includeMounts = value;
	}

	public bool IncludeOrnaments
	{
		get => includeOrnaments;
		set => includeOrnaments = value;
	}

	public bool IncludeOther
	{
		get => includeOther;
		set => includeOther = value;
	}

	public bool IncludeHidden
	{
		get => includeHidden;
		set => includeHidden = value;
	}

	public static void Show(Action<ActorBasicMemory> sectionChanged)
	{
		TargetSelectorView view = new TargetSelectorView();
		view.SelectionChanged += (b) =>
		{
			if (view.Value == null)
				return;

			sectionChanged?.Invoke(view.Value);
		};

		ViewService.ShowDrawer(view, DrawerDirection.Left);
	}

	protected override Task LoadItems()
	{
		this.AddItems(ActorService.Instance.GetAllActors());
		return Task.CompletedTask;
	}

	protected override int Compare(ActorBasicMemory actorA, ActorBasicMemory actorB)
	{
		if (actorA.IsGPoseActor && !actorB.IsGPoseActor)
			return -1;

		if (!actorA.IsGPoseActor && actorB.IsGPoseActor)
			return 1;

		return actorA.DistanceFromPlayer.CompareTo(actorB.DistanceFromPlayer);
	}

	protected override bool Filter(ActorBasicMemory actor, string[]? search)
	{
		////if (GposeService.Instance.IsGpose != actor.IsGPoseActor)
		////	return false;

		if (!SearchUtility.Matches(actor.DisplayName, search) && !SearchUtility.Matches(actor.Name, search))
			return false;

		if (TargetService.IsPinned(actor))
			return false;

		if (!includeHidden && actor.IsHidden)
			return false;

		if (!includePlayers && actor.ObjectKind == Memory.ActorTypes.Player)
			return false;

		if (!includeCompanions && actor.ObjectKind == Memory.ActorTypes.Companion)
			return false;

		if (!includeMounts && actor.ObjectKind == Memory.ActorTypes.Mount)
			return false;

		if (!includeOrnaments && actor.ObjectKind == Memory.ActorTypes.Ornament)
			return false;

		if (!includeNPCs && (actor.ObjectKind == Memory.ActorTypes.BattleNpc || actor.ObjectKind == Memory.ActorTypes.EventNpc))
			return false;

		if (!includeOther
			&& actor.ObjectKind != Memory.ActorTypes.Player
			&& actor.ObjectKind != Memory.ActorTypes.Companion
			&& actor.ObjectKind != Memory.ActorTypes.BattleNpc
			&& actor.ObjectKind != Memory.ActorTypes.EventNpc
			&& actor.ObjectKind != Memory.ActorTypes.Mount
			&& actor.ObjectKind != Memory.ActorTypes.Ornament)
		{
			return false;
		}

		return true;
	}

	protected override void OnSelectionChanged(bool close)
	{
		base.OnSelectionChanged(close);
		this.Close();
	}

	private void OnSelfPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		this.FilterItems();
	}

	private void OnAddPlayerTargetActorClicked(object sender, RoutedEventArgs e)
	{
		this.Value = TargetService.GetTargetedActor();
		this.OnSelectionChanged(true);
	}

	private async void OnCreateActorClicked(object sender, RoutedEventArgs e)
	{
		SpawnOptions options = SpawnOptions.None;
		if (PoseService.Instance.IsEnabled || PoseService.Instance.FreezeWorldPosition)
			options |= SpawnOptions.ApplyModelPosition;

		var nextActorId = await Brio.Spawn(options);
		if(nextActorId != -1)
		{
			var actors = ActorService.Instance.GetAllActors(true);
			var newActor = actors.SingleOrDefault(i => i.ObjectIndex == nextActorId);
			if(newActor != null)
			{
				if (PoseService.Instance.IsEnabled)
				{
					// We try and load the A-Pose if it's available
					var path = FileService.ParseToFilePath(FileService.StandardPoseDirectory.Path);
					path = path + Path.Combine("Unisex", "A-Pose.pose");

					if (File.Exists(path))
					{
						PoseFile? poseFile = new PoseFile().Deserialize(File.OpenRead(path)) as PoseFile;
						if (poseFile == null)
							return;

						SkeletonVisual3d skeletonVisual3D = new();
						ActorMemory fullActor = new ActorMemory();
						fullActor.SetAddress(newActor.Address);
						fullActor.Tick();
						await skeletonVisual3D.SetActor(fullActor);
						await poseFile.Apply(fullActor, skeletonVisual3D, null, PoseFile.Mode.Rotation);
				}
				}

				this.Value = newActor;
				this.OnSelectionChanged(true);
			}
		}
	}
}
