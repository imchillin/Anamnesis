// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using System.Threading.Tasks;
using Anamnesis.Actor.Utilities;
using Anamnesis.GameData;
using Anamnesis.GameData.Interfaces;
using Anamnesis.Services;
using Anamnesis.Styles.Drawers;
using XivToolsWpf;

public abstract class GlassesSelectorDrawer : SelectorDrawer<IGlasses>
{
}

/// <summary>
/// Interaction logic for GlassesSelector.xaml.
/// </summary>
public partial class GlassesSelector : GlassesSelectorDrawer
{
	public GlassesSelector()
	{
		this.InitializeComponent();
		this.DataContext = this;
	}

	protected override Task LoadItems()
	{
		if (GameDataService.Glasses != null)
			this.AddItems(GameDataService.Glasses);

		return Task.CompletedTask;
	}

	protected override bool Filter(IGlasses glasses, string[]? search)
	{
		// skip items without names
		if (string.IsNullOrEmpty(glasses.Name))
			return false;

		if (!SearchUtility.Matches(glasses.Name, search))
			return false;

		return true;
	}

	protected override int Compare(IGlasses glassesA, IGlasses glassesB)
	{
		if (glassesA.IsFavorite && !glassesB.IsFavorite)
			return -1;

		if (!glassesA.IsFavorite && glassesB.IsFavorite)
			return 1;

		return -glassesB.RowId.CompareTo(glassesA.RowId);
	}
}
