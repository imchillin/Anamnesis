// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using Anamnesis.Styles.Drawers;
using Anamnesis.Views;
using FontAwesome.Sharp;

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

public class TemplateSelectorPanel<T> : SelectorPanel
	where T : SelectorDrawer, new()
{
	private readonly SelectorDrawer selector;

	public TemplateSelectorPanel(IPanelGroupHost host)
		: base(host)
	{
		this.selector = new T();
		this.ContentArea.Content = this.selector;
		this.selector.SelectionChanged += this.OnSelectionChanged;
	}

	private void OnSelectionChanged(bool close)
	{
		this.NavigationResultCallback?.Invoke(this.selector.Value);
	}
}

public class WeatherSelectorPanel : TemplateSelectorPanel<WeatherSelector>
{
	public WeatherSelectorPanel(IPanelGroupHost host)
		: base(host)
	{
		this.Title = "Scene_World_Weather";
		this.Icon = IconChar.CloudRain;
	}
}