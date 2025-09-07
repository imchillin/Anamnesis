// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GUI.Views;

using Anamnesis.Memory;
using Anamnesis.Styles;
using System.Windows;
using System.Windows.Controls;

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
		if (this.DataContext is not ActorMemory actor)
			return;

		this.Icon.Icon = actor.ObjectKind.GetIcon();
	}
}
