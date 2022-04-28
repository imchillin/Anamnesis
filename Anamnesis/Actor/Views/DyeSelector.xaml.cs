// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;
using System.Windows.Controls;
using Anamnesis.Actor.Utilities;
using Anamnesis.GameData;
using Anamnesis.Services;
using Anamnesis.Styles.Drawers;
using XivToolsWpf;

/// <summary>
/// Interaction logic for EquipmentSelector.xaml.
/// </summary>
public partial class DyeSelector : UserControl, SelectorDrawer.ISelectorView
{
	public DyeSelector()
	{
		this.InitializeComponent();
		this.DataContext = this;

		this.Selector.AddItem(DyeUtility.NoneDye);

		if (GameDataService.Dyes != null)
			this.Selector.AddItems(GameDataService.Dyes);

		this.Selector.FilterItems();
	}

	public event DrawerEvent? Close;

	public IDye? Value
	{
		get
		{
			return (IDye?)this.Selector.Value;
		}

		set
		{
			this.Selector.Value = value;
		}
	}

	SelectorDrawer SelectorDrawer.ISelectorView.Selector
	{
		get
		{
			return this.Selector;
		}
	}

	public void OnClosed()
	{
	}

	private void OnClose()
	{
		this.Close?.Invoke();
	}

	private bool OnFilter(object obj, string[]? search = null)
	{
		if (obj is IDye dye)
		{
			// skip items without names
			if (string.IsNullOrEmpty(dye.Name))
				return false;

			if (!SearchUtility.Matches(dye.Name, search))
				return false;

			return true;
		}

		return false;
	}

	private int OnSort(object a, object b)
	{
		if (a == b)
			return 0;

		if (a == DyeUtility.NoneDye && b != DyeUtility.NoneDye)
			return -1;

		if (a != DyeUtility.NoneDye && b == DyeUtility.NoneDye)
			return 1;

		if (a is IDye dyeA && b is IDye dyeB)
		{
			if (dyeA.IsFavorite && !dyeB.IsFavorite)
				return -1;

			if (!dyeA.IsFavorite && dyeB.IsFavorite)
				return 1;

			return -dyeB.RowId.CompareTo(dyeA.RowId);
		}

		return 0;
	}
}
