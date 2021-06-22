// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Views
{
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis.Services;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for TargetSelectorView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class TargetSelectorView : UserControl, IDrawer, INotifyPropertyChanged
	{
		private static bool includePlayers = true;
		private static bool includeCompanions = true;
		private static bool includeNPCs = true;
		private static bool includeOther = false;

		public TargetSelectorView()
		{
			this.InitializeComponent();

			this.ContentArea.DataContext = this;

			this.PropertyChanged += this.OnSelfPropertyChanged;
		}

		public event DrawerEvent? Close;
		public event PropertyChangedEventHandler? PropertyChanged;

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

		public bool IncludeOther
		{
			get => includeOther;
			set => includeOther = value;
		}

		public void OnClosed()
		{
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			List<TargetService.ActorTableActor> actors = TargetService.GetActors();
			actors.Sort((a, b) => a.DistanceFromPlayer.CompareTo(b.DistanceFromPlayer));
			this.Selector.AddItems(actors);
			this.Selector.FilterItems();
		}

		private void OnClose()
		{
			this.Close?.Invoke();

			TargetService.ActorTableActor? actor = this.Selector.Value as TargetService.ActorTableActor;

			if (actor == null)
				return;

			Task.Run(() => TargetService.PinActor(actor));
		}

		private bool OnFilter(object obj, string[]? search = null)
		{
			if (obj is TargetService.ActorTableActor actor)
			{
				if (!SearchUtility.Matches(actor.DisplayName, search) && !SearchUtility.Matches(actor.Model.Name, search))
					return false;

				if (actor.IsPinned)
					return false;

				if (!includePlayers && actor.Kind == Memory.ActorTypes.Player)
					return false;

				if (!includeCompanions && actor.Kind == Memory.ActorTypes.Companion)
					return false;

				if (!includeNPCs && (actor.Kind == Memory.ActorTypes.BattleNpc || actor.Kind == Memory.ActorTypes.EventNpc))
					return false;

				if (!includeOther
					&& actor.Kind != Memory.ActorTypes.Player
					&& actor.Kind != Memory.ActorTypes.Companion
					&& actor.Kind != Memory.ActorTypes.BattleNpc
					&& actor.Kind != Memory.ActorTypes.EventNpc)
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
