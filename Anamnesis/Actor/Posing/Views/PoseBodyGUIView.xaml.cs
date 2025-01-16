// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using Anamnesis.Actor.Posing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

/// <summary>
/// Interaction logic for PoseBodyGuiView.xaml.
/// </summary>
public partial class PoseBodyGuiView : UserControl
{
	public PoseBodyGuiView()
	{
		this.InitializeComponent();
	}

	private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () => BoneViewManager.Instance.Refresh());
	}
}
