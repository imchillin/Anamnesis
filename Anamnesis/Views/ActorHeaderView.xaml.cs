// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.GUI.Views
{
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis.WpfStyles;

	/// <summary>
	/// Interaction logic for ActorHeaderView.xaml.
	/// </summary>
	public partial class ActorHeaderView : UserControl
	{
		public ActorHeaderView()
		{
			this.InitializeComponent();
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			Actor? actor = this.DataContext as Actor;

			if (actor == null)
				return;

			this.Icon.Icon = actor.Type.GetIcon();
		}
	}
}
