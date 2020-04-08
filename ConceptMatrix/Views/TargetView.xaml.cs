// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Views
{
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Controls;
	using ConceptMatrix.GUI.Services;
	using ConceptMatrix.Services;

	/// <summary>
	/// Interaction logic for TargetView.xaml.
	/// </summary>
	public partial class TargetView : UserControl
	{
		private SelectionService selection;

		public TargetView()
		{
			this.InitializeComponent();

			if (DesignerProperties.GetIsInDesignMode(this))
				return;

			this.selection = App.Services.Get<SelectionService>();
			this.selection.SelectionChanged += this.OnSelectionChanged;
			this.OnSelectionChanged(this.selection.CurrentSelection);
		}

		private void OnSelectionChanged(Selection newSelection)
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				string modeLabel = string.Empty;

				if (newSelection != null)
				{
					////this.Icon.Icon = this.GetIcon(newSelection.Type);
					this.NameLabel.Content = newSelection.Name;
					modeLabel = newSelection.Mode.ToString() + " - ";
				}
				else
				{
					////this.Icon.Icon = FontAwesomeIcon.None;
					this.NameLabel.Content = "None";
				}

				modeLabel += this.selection.UseGameTarget ? "Auto" : "Manual";
				this.ModeLabel.Content = modeLabel;
			});
		}

		private void OnClicked(object sender, RoutedEventArgs e)
		{
			App.Services.Get<IViewService>().ShowDrawer<TargetSelectorView>("Selection", DrawerDirection.Left);
		}

		/*private FontAwesomeIcon GetIcon(ActorTypes type)
		{
			switch (type)
			{
				case ActorTypes.None: return FontAwesomeIcon.None;
				case ActorTypes.Player: return FontAwesomeIcon.User;
				case ActorTypes.BattleNpc: return FontAwesomeIcon.user
				case ActorTypes.EventNpc:
				case ActorTypes.Treasure:
				case ActorTypes.Aetheryte:
				case ActorTypes.GatheringPoint:
				case ActorTypes.EventObj:
				case ActorTypes.MountType:
				case ActorTypes.Companion:
				case ActorTypes.Retainer:
				case ActorTypes.Area:
				case ActorTypes.Housing:
				case ActorTypes.Cutscene:
				case ActorTypes.CardStand:
			}
		}*/
	}
}
