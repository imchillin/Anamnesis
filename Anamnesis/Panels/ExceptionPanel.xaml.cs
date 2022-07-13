// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using Anamnesis.Navigation;
using Anamnesis.Windows;
using FontAwesome.Sharp;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

public partial class ExceptionPanel : PanelBase
{
	public ExceptionPanel(IPanelGroupHost host, ErrorInfo data)
		: base(host)
	{
		this.InitializeComponent();

		Window hostWindow = App.Current.MainWindow;

		if (host is FloatingWindow wnd)
			hostWindow = wnd;

		this.ContentArea.Content = new XivToolsWpf.Dialogs.ErrorDialog(hostWindow, data.Error, data.IsCritical);
		this.Title = "Anamnesis v" + VersionInfo.Date.ToString("yyyy-MM-dd HH:mm");
		this.TitleColor = data.IsCritical ? Colors.Red : Colors.Yellow;
		this.Icon = data.IsCritical ? IconChar.ExclamationCircle : IconChar.ExclamationTriangle;
	}

	public static async Task Show(ExceptionDispatchInfo error, bool isCritical)
	{
		ErrorInfo info = new(error, isCritical);
		PanelBase? panel = NavigationService.Navigate(new("Exception", info));

		while (panel.IsOpen)
		{
			await Task.Delay(500);
		}
	}

	public class ErrorInfo
	{
		public ExceptionDispatchInfo Error;
		public bool IsCritical = false;

		public ErrorInfo(ExceptionDispatchInfo error, bool isCritical)
		{
			this.Error = error;
			this.IsCritical = isCritical;
		}
	}
}
