// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GUI.Views;

using System.Windows;
using System.Windows.Controls;
using Anamnesis.Memory;
using Anamnesis.Styles;

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
		ActorMemory? actor = this.DataContext as ActorMemory;

		if (actor == null)
			return;

		this.Icon.Icon = actor.ObjectKind.GetIcon();
	}
}
