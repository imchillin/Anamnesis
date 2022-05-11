// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Tabs;

using Anamnesis.Actor.Utilities;
using Anamnesis.GameData.Excel;
using Anamnesis.Memory;
using Anamnesis.Services;
using Anamnesis.Utils;
using Anamnesis.Views;
using Serilog;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using XivToolsWpf.Selectors;

/// <summary>
/// Interaction logic for DeveloperTab.xaml.
/// </summary>
public partial class DeveloperTab : UserControl
{
	public DeveloperTab()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public TargetService TargetService => TargetService.Instance;

	private void OnNpcNameSearchClicked(object sender, RoutedEventArgs e)
	{
		GenericSelectorUtil.Show(GameDataService.BattleNpcNames, (v) =>
		{
			if (v.Description == null)
				return;

			ClipboardUtility.CopyToClipboard(v.Description);
		});
	}

	private void OnFindNpcClicked(object sender, RoutedEventArgs e)
	{
		TargetSelectorView.Show((a) =>
		{
			ActorMemory memory = new();

			if (a is ActorMemory actorMemory)
				memory = actorMemory;

			memory.SetAddress(a.Address);

			NpcAppearanceSearch.Search(memory);
		});
	}

	private void OnCopyActorAddressClicked(object sender, RoutedEventArgs e)
	{
		ActorBasicMemory memory = this.TargetService.PlayerTarget;

		if (!memory.IsValid)
		{
			Log.Warning("Actor is invalid");
			return;
		}

		string address = memory.Address.ToString("X");

		ClipboardUtility.CopyToClipboard(address);
	}

	private void OnCopyAssociatedAddressesClick(object sender, RoutedEventArgs e)
	{
		ActorBasicMemory abm = this.TargetService.PlayerTarget;

		if (!abm.IsValid)
		{
			Log.Warning("Actor is invalid");
			return;
		}

		try
		{
			ActorMemory memory = new();
			memory.SetAddress(abm.Address);

			StringBuilder sb = new();

			sb.AppendLine("Base: " + memory.Address.ToString("X"));
			sb.AppendLine("Model: " + (memory.ModelObject?.Address.ToString("X") ?? "0"));
			sb.AppendLine("Extended Appearance: " + (memory.ModelObject?.ExtendedAppearance?.Address.ToString("X") ?? "0"));
			sb.AppendLine("Skeleton: " + (memory.ModelObject?.Skeleton?.Address.ToString("X") ?? "0"));
			sb.AppendLine("Main Hand Model: " + (memory.MainHand?.Model?.Address.ToString("X") ?? "0"));
			sb.AppendLine("Off Hand Model: " + (memory.OffHand?.Model?.Address.ToString("X") ?? "0"));
			sb.AppendLine("Mount: " + (memory.Mount?.Address.ToString("X") ?? "0"));
			sb.AppendLine("Companion: " + (memory.Companion?.Address.ToString("X") ?? "0"));
			sb.AppendLine("Ornament: " + (memory.Ornament?.Address.ToString("X") ?? "0"));

			ClipboardUtility.CopyToClipboard(sb.ToString());
		}
		catch
		{
			Log.Warning("Could not read addresses");
		}
	}
}
