// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Panels;

using Anamnesis.Actor.Utilities;
using Anamnesis.Memory;
using Anamnesis.Panels;
using Anamnesis.Services;
using System.Windows;

/// <summary>
/// Interaction logic for EquipmentPanel.xaml.
/// </summary>
public partial class EquipmentPanel : ActorPanelBase
{
	public EquipmentPanel(IPanelGroupHost host)
		: base(host)
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	private void OnClearClicked(object? sender = null, RoutedEventArgs? e = null)
	{
		if (this.Actor == null)
			return;

		this.Actor.MainHand?.Clear(this.Actor.IsHuman);
		this.Actor.OffHand?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Arms?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Chest?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Ear?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Feet?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Head?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Legs?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.LFinger?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Neck?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.RFinger?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Wrist?.Clear(this.Actor.IsHuman);

		this.Actor?.ModelObject?.Weapons?.Hide();
		this.Actor?.ModelObject?.Weapons?.SubModel?.Hide();
	}

	private void OnNpcSmallclothesClicked(object sender, RoutedEventArgs e)
	{
		if (this.Actor == null)
			return;

		if (!this.Actor.IsHuman)
		{
			this.OnClearClicked(sender, e);
			return;
		}

		this.Actor.Equipment?.Ear?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Head?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.LFinger?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Neck?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.RFinger?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Wrist?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Arms?.Equip(ItemUtility.NpcBodyItem);
		this.Actor.Equipment?.Chest?.Equip(ItemUtility.NpcBodyItem);
		this.Actor.Equipment?.Legs?.Equip(ItemUtility.NpcBodyItem);
		this.Actor.Equipment?.Feet?.Equip(ItemUtility.NpcBodyItem);
	}

	private void OnRaceGearClicked(object sender, RoutedEventArgs e)
	{
		if (this.Actor == null)
			return;

		if (this.Actor.Customize?.Race == null)
			return;

		var race = GameDataService.Races.GetRow((uint)this.Actor.Customize.Race);

		if (race == null)
			return;

		if (this.Actor.Customize.Gender == ActorCustomizeMemory.Genders.Masculine)
		{
			var body = GameDataService.Items.Get((uint)race.RSEMBody);
			var hands = GameDataService.Items.Get((uint)race.RSEMHands);
			var legs = GameDataService.Items.Get((uint)race.RSEMLegs);
			var feet = GameDataService.Items.Get((uint)race.RSEMFeet);

			this.Actor.Equipment?.Chest?.Equip(body);
			this.Actor.Equipment?.Arms?.Equip(hands);
			this.Actor.Equipment?.Legs?.Equip(legs);
			this.Actor.Equipment?.Feet?.Equip(feet);
		}
		else
		{
			var body = GameDataService.Items.Get((uint)race.RSEFBody);
			var hands = GameDataService.Items.Get((uint)race.RSEFHands);
			var legs = GameDataService.Items.Get((uint)race.RSEFLegs);
			var feet = GameDataService.Items.Get((uint)race.RSEFFeet);

			this.Actor.Equipment?.Chest?.Equip(body);
			this.Actor.Equipment?.Arms?.Equip(hands);
			this.Actor.Equipment?.Legs?.Equip(legs);
			this.Actor.Equipment?.Feet?.Equip(feet);
		}

		this.Actor.Equipment?.Ear?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Head?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.LFinger?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Neck?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.RFinger?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Wrist?.Clear(this.Actor.IsHuman);
	}
}
