// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Views;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using Anamnesis.Memory;
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

		ActorBasicMemory? actor = this.Selector.Value as ActorBasicMemory;

		if (actor == null)
			return;

		Task.Run(() => TargetService.PinActor(actor, true));
		this.Close();
	}

	private void OnSelfPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		this.FilterItems();
	}

	private async void OnAddPlayerTargetActorClicked(object sender, RoutedEventArgs e)
	{
		await TargetService.PinPlayerTargetedActor();
		this.Close();
	}
}
