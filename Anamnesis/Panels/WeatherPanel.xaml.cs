// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using Anamnesis.GameData.Excel;
using Anamnesis.Navigation;
using System.Windows;

public partial class WeatherPanel : PanelBase
{
	public WeatherPanel(IPanelGroupHost host)
		: base(host)
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public TimeService TimeService => TimeService.Instance;
	public TerritoryService TerritoryService => TerritoryService.Instance;

	public override Point GetSubPanelDockOffset()
	{
		return new(0, this.Rect.Height - 6);
	}
}
