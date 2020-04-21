// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Views
{
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Controls;
	using ConceptMatrix.GUI.Services;
	using ConceptMatrix.WpfStyles;
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
					this.Icon.Icon = newSelection.Type.GetIcon();
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
	}
}
