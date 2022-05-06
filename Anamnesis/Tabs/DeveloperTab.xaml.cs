// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Tabs;

using Anamnesis.Actor.Utilities;
using Anamnesis.GameData.Excel;
using Anamnesis.Memory;
using Anamnesis.Services;
using Anamnesis.Utils;
using Anamnesis.Views;
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
		GenericSelectorUtil.Show(GameDataService.BattleNpcNames, (v) =>
		{
			if (v.Description == null)
				return;

			ClipboardUtility.CopyToClipboard(v.Description);
		});
	}

	private void OnFindNpcClicked(object sender, RoutedEventArgs e)
	{
		TargetSelectorView.Show((a) =>
		{
			ActorMemory memory = new();

			if (a is ActorMemory actorMemory)
				memory = actorMemory;

			memory.SetAddress(a.Address);

			NpcAppearanceSearch.Search(memory);
		});
	}
}
