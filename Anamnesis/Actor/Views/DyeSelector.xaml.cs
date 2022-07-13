// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using System.Threading.Tasks;
using System.Windows.Controls;
using Anamnesis.Actor.Utilities;
using Anamnesis.GameData;
using Anamnesis.Services;
using XivToolsWpf;

/// <summary>
/// Interaction logic for EquipmentSelector.xaml.
/// </summary>
public partial class DyeSelector : UserControl
{
	public DyeSelector()
	{
		this.InitializeComponent();
		this.DataContext = this;
	}

	protected Task LoadItems()
	{
		this.Selector.AddItem(DyeUtility.NoneDye);

		if (GameDataService.Dyes != null)
			this.Selector.AddItems(GameDataService.Dyes);

		return Task.CompletedTask;
	}

	protected bool Filter(IDye dye, string[]? search)
	{
		// skip items without names
		if (string.IsNullOrEmpty(dye.Name))
			return false;

		if (!SearchUtility.Matches(dye.Name, search))
			return false;

		return true;
	}

	protected int Compare(IDye dyeA, IDye dyeB)
	{
		if (dyeA == DyeUtility.NoneDye && dyeB != DyeUtility.NoneDye)
			return -1;

		if (dyeA != DyeUtility.NoneDye && dyeB == DyeUtility.NoneDye)
			return 1;

		if (dyeA.IsFavorite && !dyeB.IsFavorite)
			return -1;

		if (!dyeA.IsFavorite && dyeB.IsFavorite)
			return 1;

		return -dyeB.RowId.CompareTo(dyeA.RowId);
	}
}
