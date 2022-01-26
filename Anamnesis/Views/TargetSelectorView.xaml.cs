// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Views
{
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis.Memory;
	using Anamnesis.Services;
	using PropertyChanged;
	using XivToolsWpf;

	/// <summary>
	/// Interaction logic for TargetSelectorView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class TargetSelectorView : UserControl, IDrawer, INotifyPropertyChanged
	{
		private static bool includePlayers = true;
		private static bool includeCompanions = true;
		private static bool includeNPCs = true;
		private static bool includeMounts = true;
		private static bool includeOrnaments = true;
		private static bool includeOther = false;

		public TargetSelectorView()
		{
			this.InitializeComponent();

			this.ContentArea.DataContext = this;

			this.PropertyChanged += this.OnSelfPropertyChanged;
		}

		public event DrawerEvent? Close;
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

		public void OnClosed()
		{
		}

		private async void OnAddPlayerTargetActorClicked(object sender, RoutedEventArgs e)
		{
			await TargetService.PinPlayerTargetedActor();
			this.OnClose();
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.Selector.AddItems(ActorService.Instance.GetAllActors());
			this.Selector.FilterItems();
		}

		private void OnClose()
		{
			this.Close?.Invoke();

			ActorBasicMemory? actor = this.Selector.Value as ActorBasicMemory;

			if (actor == null)
				return;

			Task.Run(() => TargetService.PinActor(actor));
		}

		private int OnSort(object a, object b)
		{
			if (a is ActorBasicMemory actorA && b is ActorBasicMemory actorB)
			{
				if (actorA.IsGPoseActor && !actorB.IsGPoseActor)
					return -1;

				if (!actorA.IsGPoseActor && actorB.IsGPoseActor)
					return 1;

				return actorA.DistanceFromPlayer.CompareTo(actorB.DistanceFromPlayer);
			}

			return 0;
		}

		private bool OnFilter(object obj, string[]? search = null)
		{
			if (obj is ActorBasicMemory actor)
			{
				////if (GposeService.Instance.IsGpose != actor.IsGPoseActor)
				////	return false;

				if (!SearchUtility.Matches(actor.DisplayName, search) && !SearchUtility.Matches(actor.Name, search))
					return false;

				if (TargetService.IsPinned(actor))
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

			return false;
		}

		private void OnSelfPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			this.Selector.FilterItems();
		}

		private void OnSelectionChanged()
		{
			this.OnClose();
		}
	}
}
