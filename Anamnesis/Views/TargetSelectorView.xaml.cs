// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Views;

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Anamnesis.Memory;
using XivToolsWpf;

/// <summary>
/// Interaction logic for TargetSelectorView.xaml.
/// </summary>
public partial class TargetSelectorView : UserControl, INotifyPropertyChanged
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

	public event PropertyChangedEventHandler? PropertyChanged;

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

	protected Task LoadItems()
	{
		this.Selector.AddItems(ActorService.Instance.GetAllActors());
		return Task.CompletedTask;
	}

	protected int Compare(object itemA, object itemB)
	{
		if (itemA is not ActorBasicMemory actorA)
			return 0;

		if (itemB is not ActorBasicMemory actorB)
			return 0;

		if (actorA.IsGPoseActor && !actorB.IsGPoseActor)
			return -1;

		if (!actorA.IsGPoseActor && actorB.IsGPoseActor)
			return 1;

		return actorA.DistanceFromPlayer.CompareTo(actorB.DistanceFromPlayer);
	}

	private bool FilterItems(object item, string search)
	{
		if (item is not ActorBasicMemory actor)
			return false;

		////if (GposeService.Instance.IsGpose != actor.IsGPoseActor)
		////	return false;

		if (!SearchUtility.Matches(actor.Names.FullName, search) && !SearchUtility.Matches(actor.Names.Nickname, search))
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

	private void OnSelfPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		this.Selector.FilterItems();
	}

	private void OnAddPlayerTargetActorClicked(object sender, RoutedEventArgs e)
	{
		this.Selector.Value = TargetService.GetTargetedActor();
		this.Selector.RaiseSelectionChanged();
	}
}
