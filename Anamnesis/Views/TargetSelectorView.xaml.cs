// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Views
{
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
	[SuppressPropertyChangedWarnings]
	public partial class TargetSelectorView : UserControl, IDrawer, INotifyPropertyChanged
	{
		private static bool includePlayers = true;
		private static bool includeCompanions = true;
		private static bool includeNPCs = true;

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

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			foreach (TargetService.ActorTableActor? actor in TargetService.GetActors())
			{
				this.Selector.Items.Add(actor);
			}

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
			if (obj is TargetService.ActorTableActor item)
			{
				if (!SearchUtility.Matches(item.Name, search))
					return false;

				if (item.IsPinned)
					return false;

				if (!includePlayers && item.Kind == Memory.ActorTypes.Player)
					return false;

				if (!includeCompanions && item.Kind == Memory.ActorTypes.Companion)
					return false;

				if (!includeNPCs && (item.Kind == Memory.ActorTypes.BattleNpc || item.Kind == Memory.ActorTypes.EventNpc))
					return false;

				return true;
			}

			return false;
		}

		private void OnSelfPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			this.Selector.FilterItems();
		}

		private void OnSelectionChanged()
		{
			this.OnClose();
		}
	}
}
