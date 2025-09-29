// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Views;

using Anamnesis.Brio;
using Anamnesis.Core;
using Anamnesis.Files;
using Anamnesis.Memory;
using Anamnesis.Services;
using Anamnesis.Styles.Drawers;
using Serilog;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using XivToolsWpf;

public abstract class TargetSelectorDrawer : SelectorDrawer<ActorBasicMemory>
{
}

/// <summary>
/// Interaction logic for TargetSelectorView.xaml.
/// </summary>
public partial class TargetSelectorView : TargetSelectorDrawer
{
	private static bool s_includePlayers = true;
	private static bool s_includeCompanions = true;
	private static bool s_includeNPCs = true;
	private static bool s_includeMounts = true;
	private static bool s_includeOrnaments = true;
	private static bool s_includeOther = false;
	private static bool s_includeHidden = false;

	public TargetSelectorView()
	{
		this.InitializeComponent();

		this.ContentArea.DataContext = this;

		this.PropertyChanged += this.OnSelfPropertyChanged;
	}

	public static TargetService TargetService => TargetService.Instance;
	public static GposeService GPoseService => GposeService.Instance;
	public static SettingsService SettingsService => SettingsService.Instance;

	public static bool IncludePlayers
	{
		get => s_includePlayers;
		set => s_includePlayers = value;
	}

	public static bool IncludeCompanions
	{
		get => s_includeCompanions;
		set => s_includeCompanions = value;
	}

	public static bool IncludeNPCs
	{
		get => s_includeNPCs;
		set => s_includeNPCs = value;
	}

	public static bool IncludeMounts
	{
		get => s_includeMounts;
		set => s_includeMounts = value;
	}

	public static bool IncludeOrnaments
	{
		get => s_includeOrnaments;
		set => s_includeOrnaments = value;
	}

	public static bool IncludeOther
	{
		get => s_includeOther;
		set => s_includeOther = value;
	}

	public static bool IncludeHidden
	{
		get => s_includeHidden;
		set => s_includeHidden = value;
	}

	public static void Show(Action<ActorBasicMemory> sectionChanged)
	{
		var view = new TargetSelectorView();
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

		if (!s_includeHidden && actor.IsHidden)
			return false;

		if (!s_includePlayers && actor.ObjectKind == Memory.ActorTypes.Player)
			return false;

		if (!s_includeCompanions && actor.ObjectKind == Memory.ActorTypes.Companion)
			return false;

		if (!s_includeMounts && actor.ObjectKind == Memory.ActorTypes.Mount)
			return false;

		if (!s_includeOrnaments && actor.ObjectKind == Memory.ActorTypes.Ornament)
			return false;

		if (!s_includeNPCs && (actor.ObjectKind == Memory.ActorTypes.BattleNpc || actor.ObjectKind == Memory.ActorTypes.EventNpc))
			return false;

		if (!s_includeOther
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
		try
		{
			var nextActorId = await Brio.Spawn();
			if (nextActorId != -1)
			{
				var actors = ActorService.Instance.GetAllActors(true);
				var newActor = actors.SingleOrDefault(i => i.ObjectIndex == nextActorId);
				if (newActor != null)
				{
					if (PoseService.Instance.IsEnabled)
					{
						// We try and load the A-Pose if it's available
						var path = FileService.ParseToFilePath(FileService.StandardPoseDirectory.Path);
						path += Path.Combine("Unisex", "A-Pose.pose");

						if (File.Exists(path))
						{
							if (new PoseFile().Deserialize(File.OpenRead(path)) is not PoseFile poseFile)
								return;

							ActorMemory fullActor = new();
							fullActor.SetAddress(newActor.Address);
							fullActor.Synchronize();
							var skeleton = new Skeleton(fullActor);
							poseFile.Apply(fullActor, skeleton, null, PoseFile.Mode.Rotation, true);
						}
					}

					this.Value = newActor;
					this.OnSelectionChanged(true);
				}
			}
			else
			{
				throw new Exception("Brio could not spawn actor");
			}
		}
		catch (Exception ex)
		{
			Log.Error("Failed to spawn actor", ex);
		}
	}
}
