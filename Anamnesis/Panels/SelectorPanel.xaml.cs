// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using Anamnesis.GameData.Excel;
using Anamnesis.Memory;
using Anamnesis.Styles.Drawers;
using Anamnesis.Views;
using FontAwesome.Sharp;
using XivToolsWpf.Extensions;

// This selector panel nonsense is weird, but its here to get around the old selector drawer system
// We should consider refactoring the entire thing.
public partial class SelectorPanel : PanelBase
{
	public SelectorPanel(IPanelGroupHost host)
		: base(host)
	{
		this.InitializeComponent();
	}
}

public abstract class TemplateSelectorPanel<T> : SelectorPanel
	where T : SelectorDrawer, new()
{
	protected readonly SelectorDrawer selector;

	public TemplateSelectorPanel(IPanelGroupHost host)
		: base(host)
	{
		this.selector = new T();
		this.ContentArea.Content = this.selector;
		this.selector.SelectionChanged += (close) =>
		{
			if (close)
				this.Close();

			this.OnSelectionChanged(close);
		};
	}

	protected abstract void OnSelectionChanged(bool close);
}

public class WeatherSelectorPanel : TemplateSelectorPanel<WeatherSelector>
{
	public WeatherSelectorPanel(IPanelGroupHost host)
		: base(host)
	{
		this.TitleKey = "Scene_World_Weather";
		this.Icon = IconChar.CloudRain;
	}

	protected override void OnSelectionChanged(bool close)
	{
		TerritoryService.Instance.CurrentWeather = this.selector.Value as Weather;
	}
}

public class PinActorPanel : TemplateSelectorPanel<TargetSelectorView>
{
	public PinActorPanel(IPanelGroupHost host)
		: base(host)
	{
	}

	protected override void OnSelectionChanged(bool close)
	{
		if (!close)
			return;

		ActorBasicMemory? selection = this.selector.Value as ActorBasicMemory;

		if (selection == null)
			return;

		TargetService.PinActor(selection).Run();
	}
}