// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using Anamnesis.Memory;
using Anamnesis.Services;
using Anamnesis.Updater;
using Anamnesis.Views;
using System.Windows;
using System.Windows.Input;
using XivToolsWpf.Extensions;

/// <summary>
/// Interaction logic for Navigation.xaml.
/// </summary>
public partial class Navigation : PanelBase
{
	public Navigation()
	{
		this.InitializeComponent();

		this.ContentArea.DataContext = this;
	}

	public GameService GameService => GameService.Instance;
	public SettingsService SettingsService => SettingsService.Instance;
	public GposeService GposeService => GposeService.Instance;
	public TargetService TargetService => TargetService.Instance;
	public MemoryService MemoryService => MemoryService.Instance;
	public LogService LogService => LogService.Instance;
	public UpdateService UpdateService => UpdateService.Instance;

	private void OnIconMouseDown(object sender, MouseButtonEventArgs e)
	{
		this.DragMove();
	}

	private void OnAddActorClicked(object sender, RoutedEventArgs e)
	{
		TargetSelectorView.Show((a) =>
		{
			TargetService.PinActor(a, true).Run();
		});
	}

	private void OnUnpinActorClicked(object sender, RoutedEventArgs e)
	{
		if (sender is FrameworkElement el && el.DataContext is PinnedActor actor)
		{
			TargetService.UnpinActor(actor);
		}
	}

	private void OnTargetActorClicked(object sender, RoutedEventArgs e)
	{
		if (sender is FrameworkElement el && el.DataContext is PinnedActor actor)
		{
			TargetService.SetPlayerTarget(actor);
		}
	}

	private void OnActorPinPreviewMouseUp(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Middle)
		{
			this.OnUnpinActorClicked(sender, new RoutedEventArgs());
		}
	}
}
