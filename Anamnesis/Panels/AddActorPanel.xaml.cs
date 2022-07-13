// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using Anamnesis.Memory;
using XivToolsWpf;
using XivToolsWpf.Selectors;

public partial class AddActorPanel : PanelBase
{
	private static readonly ActorFilter FilterInstance = new();

	public AddActorPanel(IPanelGroupHost host)
		: base(host)
	{
		this.InitializeComponent();

		this.ContentArea.DataContext = this;

		this.PropertyChanged += this.OnSelfPropertyChanged;
	}

	public ActorFilter Filter => FilterInstance;

	protected Task LoadItems()
	{
		this.Selector.AddItems(ActorService.Instance.GetAllActors());
		return Task.CompletedTask;
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

	public class ActorFilter : Selector.FilterBase<ActorBasicMemory>
	{
		public bool IncludePlayers { get; set; } = true;
		public bool IncludeCompanions { get; set; } = true;
		public bool IncludeNPCs { get; set; } = true;
		public bool IncludeMounts { get; set; } = true;
		public bool IncludeOrnaments { get; set; } = true;
		public bool IncludeOther { get; set; } = false;
		public bool IncludeHidden { get; set; } = false;

		public override int CompareItems(ActorBasicMemory actorA, ActorBasicMemory actorB)
		{
			if (actorA.IsGPoseActor && !actorB.IsGPoseActor)
				return -1;

			if (!actorA.IsGPoseActor && actorB.IsGPoseActor)
				return 1;

			return actorA.DistanceFromPlayer.CompareTo(actorB.DistanceFromPlayer);
		}

		public override bool FilterItem(ActorBasicMemory actor, string[]? search)
		{
			////if (GposeService.Instance.IsGpose != actor.IsGPoseActor)
			////	return false;

			if (!SearchUtility.Matches(actor.Names.FullName, search) && !SearchUtility.Matches(actor.Names.Nickname, search))
				return false;

			if (TargetService.IsPinned(actor))
				return false;

			if (!this.IncludeHidden && actor.IsHidden)
				return false;

			if (!this.IncludePlayers && actor.ObjectKind == Memory.ActorTypes.Player)
				return false;

			if (!this.IncludeCompanions && actor.ObjectKind == Memory.ActorTypes.Companion)
				return false;

			if (!this.IncludeMounts && actor.ObjectKind == Memory.ActorTypes.Mount)
				return false;

			if (!this.IncludeOrnaments && actor.ObjectKind == Memory.ActorTypes.Ornament)
				return false;

			if (!this.IncludeNPCs && (actor.ObjectKind == Memory.ActorTypes.BattleNpc || actor.ObjectKind == Memory.ActorTypes.EventNpc))
				return false;

			if (!this.IncludeOther
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
	}
}
