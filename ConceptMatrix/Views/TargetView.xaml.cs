// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Views
{
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Controls;
	using ConceptMatrix.GUI.Services;
	using ConceptMatrix.Services;
	using FontAwesome.Sharp;

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
					this.Icon.Icon = this.GetIcon(newSelection.Type);
					this.Icon.ToolTip = newSelection.Type.ToString();
					this.NameLabel.Text = newSelection.Name;
					modeLabel = newSelection.Mode.ToString() + " - ";
				}
				else
				{
					this.Icon.Icon = IconChar.None;
					this.Icon.ToolTip = null;
					this.NameLabel.Text = "None";
				}

				modeLabel += this.selection.UseGameTarget ? "Auto" : "Manual";
				this.ModeLabel.Text = modeLabel;
			});
		}

		private void OnClicked(object sender, RoutedEventArgs e)
		{
			App.Services.Get<IViewService>().ShowDrawer<TargetSelectorView>("Selection", DrawerDirection.Left);
		}

		private IconChar GetIcon(ActorTypes type)
		{
			switch (type)
			{
				case ActorTypes.None: return IconChar.None;
				case ActorTypes.Player: return IconChar.User;
				case ActorTypes.BattleNpc: return IconChar.UserShield;
				case ActorTypes.EventNpc: return IconChar.UserNinja;
				case ActorTypes.Treasure: return IconChar.Coins;
				case ActorTypes.Aetheryte: return IconChar.Gem;
				case ActorTypes.Companion: return IconChar.Cat;
				case ActorTypes.Retainer: return IconChar.ConciergeBell;
				case ActorTypes.Housing: return IconChar.Chair;
			}

			return IconChar.Question;
		}
	}
}
