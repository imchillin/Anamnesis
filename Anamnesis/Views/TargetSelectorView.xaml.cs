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

public abstract class TargetSelectorDrawer : SelectorDrawer<ObjectHandle<GameObjectMemory>>
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

#pragma warning disable CA1822
	public bool IncludePlayers
	{
		get => s_includePlayers;
		set => s_includePlayers = value;
	}

	public bool IncludeCompanions
	{
		get => s_includeCompanions;
		set => s_includeCompanions = value;
	}

	public bool IncludeNPCs
	{
		get => s_includeNPCs;
		set => s_includeNPCs = value;
	}

	public bool IncludeMounts
	{
		get => s_includeMounts;
		set => s_includeMounts = value;
	}

	public bool IncludeOrnaments
	{
		get => s_includeOrnaments;
		set => s_includeOrnaments = value;
	}

	public bool IncludeOther
	{
		get => s_includeOther;
		set => s_includeOther = value;
	}

	public bool IncludeHidden
	{
		get => s_includeHidden;
		set => s_includeHidden = value;
	}
#pragma warning restore CA1822

	public static void Show(Action<ObjectHandle<GameObjectMemory>> sectionChanged)
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
		var actorHandles = ActorService.Instance.ObjectTable.GetAll();

		// As we do not actively update non-pinned actors, synchronize first
		foreach (var handle in actorHandles)
		{
			if (!handle.IsValid)
				continue;

			handle.Do(actor => actor.Synchronize());
		}

		this.AddItems(actorHandles);
		return Task.CompletedTask;
	}

	protected override int Compare(ObjectHandle<GameObjectMemory> actorA, ObjectHandle<GameObjectMemory> actorB)
	{
		if (actorA.Do(a => a.IsGPoseActor) && !actorB.Do(a => a.IsGPoseActor))
			return -1;

		if (!actorA.Do(a => a.IsGPoseActor) && actorB.Do(a => a.IsGPoseActor))
			return 1;

		return actorA.Do(a => a.DistanceFromPlayer).CompareTo(actorB.Do(a => a.DistanceFromPlayer));
	}

	protected override bool Filter(ObjectHandle<GameObjectMemory> handle, string[]? search)
	{
		return handle.Do(actor =>
		{
			if (!SearchUtility.Matches(actor.DisplayName, search) && !SearchUtility.Matches(actor.Name, search))
				return false;

			if (TargetService.IsPinned(handle))
				return false;

			if (!actor.IsValid)
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
		});
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
				var actorHandles = ActorService.Instance.ObjectTable.GetAll();
				var newActorHandle = actorHandles.SingleOrDefault(h => h.Do(a => a.ObjectIndex) == nextActorId);
				if (newActorHandle != null && newActorHandle.IsValid)
				{
					if (PoseService.Instance.IsEnabled)
					{
						// Try to load the A-Pose if available
						var path = FileService.ParseToFilePath(FileService.StandardPoseDirectory.Path);
						path = Path.Combine(path, "Unisex", "A-Pose.pose");

						if (File.Exists(path))
						{
							if (new PoseFile().Deserialize(File.OpenRead(path)) is not PoseFile poseFile)
								return;

							await newActorHandle.DoAsync(async actor =>
							{
								var upgradedActorHandle = ActorService.Instance.ObjectTable.Get<ActorMemory>(newActorHandle.Address);
								if (upgradedActorHandle != null)
								{
									var skeleton = new Skeleton(upgradedActorHandle);
									poseFile.Apply(upgradedActorHandle, skeleton, null, PoseFile.Mode.Rotation, true);
								}

								await Task.CompletedTask;
							});
						}
					}

					this.Value = newActorHandle;
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
