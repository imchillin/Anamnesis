// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Tabs;

using Anamnesis.GameData.Excel;
using Anamnesis.Services;
using System.Windows;
using System.Windows.Controls;
using XivToolsWpf.Selectors;

/// <summary>
/// Interaction logic for DeveloperTab.xaml.
/// </summary>
public partial class DeveloperTab : UserControl
{
    public DeveloperTab()
    {
        this.InitializeComponent();
        this.ContentArea.DataContext = this;
    }

    private void OnNpcNameSearchClicked(object sender, RoutedEventArgs e)
	{
		GenericSelector sel = new GenericSelector(GameDataService.BattleNpcNames);
		ViewService.ShowDrawer(sel);
	}
}
