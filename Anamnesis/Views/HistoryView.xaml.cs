// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Views;

using Anamnesis.Services;
using System.Windows.Controls;

/// <summary>
/// Interaction logic for HistoryView.xaml.
/// </summary>
public partial class HistoryView : UserControl
{
	public HistoryView()
	{
		this.InitializeComponent();
	}

	private void OnUndoClicked(object sender, System.Windows.RoutedEventArgs e)
	{
		HistoryService.Instance.StepBack();
	}

	private void OnRedoClicked(object sender, System.Windows.RoutedEventArgs e)
	{
		HistoryService.Instance.StepForward();
	}

	private void OnClearClicked(object sender, System.Windows.RoutedEventArgs e)
	{
		HistoryService.Instance.Clear();
	}
}
