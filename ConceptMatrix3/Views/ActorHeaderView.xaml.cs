// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.GUI.Views
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Documents;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using System.Windows.Navigation;
	using System.Windows.Shapes;
	using ConceptMatrix.WpfStyles;

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
			Actor actor = this.DataContext as Actor;
			this.Icon.Icon = actor.Type.GetIcon();
		}
	}
}
