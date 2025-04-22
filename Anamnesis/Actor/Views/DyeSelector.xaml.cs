// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using Anamnesis.GameData;
using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Anamnesis.Styles.Drawers;
using System.Threading.Tasks;
using XivToolsWpf;

public abstract class DyeSelectorDrawer : SelectorDrawer<IDye>
{
}

/// <summary>
/// Interaction logic for EquipmentSelector.xaml.
/// </summary>
public partial class DyeSelector : DyeSelectorDrawer
{
	public DyeSelector()
	{
		this.InitializeComponent();
		this.DataContext = this;
	}

	protected override Task LoadItems()
	{
		if (GameDataService.Dyes != null)
			this.AddItems(GameDataService.Dyes.ToEnumerable());

		return Task.CompletedTask;
	}

	protected override bool Filter(IDye dye, string[]? search)
	{
		// skip items without names
		if (string.IsNullOrEmpty(dye.Name))
			return false;

		if (!SearchUtility.Matches(dye.Name, search))
			return false;

		return true;
	}

	protected override int Compare(IDye dyeA, IDye dyeB)
	{
		if (dyeA == null && dyeB != null)
			return -1;

		if (dyeA != null && dyeB == null)
			return 1;

		if (dyeA == default && dyeB != default)
			return -1;

		if (dyeA != default && dyeB == default)
			return 1;

		if (dyeA!.IsFavorite && !dyeB!.IsFavorite)
			return -1;

		if (!dyeA.IsFavorite && dyeB!.IsFavorite)
			return 1;

		return -dyeB!.RowId.CompareTo(dyeA.RowId);
	}
}
