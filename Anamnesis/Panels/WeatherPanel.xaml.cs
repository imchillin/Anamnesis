// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

public partial class WeatherPanel : PanelBase
{
	public WeatherPanel(IPanelGroupHost host)
		: base(host)
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
		this.Title = "Weather";
	}
}
