// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using Anamnesis.GameData.Excel;
using Anamnesis.GameData.Sheets;
using Anamnesis.Memory;
using Anamnesis.Services;
using PropertyChanged;
using System.Windows.Controls;

/// <summary>
/// Interaction logic for CustomizeFeatureSelectorDrawer.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class CustomizeFeatureSelectorDrawer : UserControl, IDrawer
{
	private readonly ActorCustomizeMemory.Genders gender;
	private readonly ActorCustomizeMemory.Tribes tribe;
	private readonly CustomizeSheet.Features feature;

	private byte selected;
	private CharaMakeCustomize? selectedItem;

	public CustomizeFeatureSelectorDrawer(CustomizeSheet.Features feature, ActorCustomizeMemory.Genders gender, ActorCustomizeMemory.Tribes tribe, byte value)
	{
		this.InitializeComponent();

		this.feature = feature;
		this.gender = gender;
		this.tribe = tribe;

		this.ContentArea.DataContext = this;
		this.List.ItemsSource = GameDataService.CharacterMakeCustomize?.GetFeatureOptions(feature, tribe, gender);

		this.Selected = value;
	}

	public delegate void SelectorEvent(byte value);

	public event DrawerEvent? OnClosing;
	public event SelectorEvent? SelectionChanged;

	public byte Selected
	{
		get => this.selected;
		set
		{
			this.selected = value;

			this.SelectedItem = GameDataService.CharacterMakeCustomize?.GetFeature(this.feature, this.tribe, this.gender, value);

			if (!this.IsLoaded)
				return;

			this.SelectionChanged?.Invoke(this.selected);
		}
	}

	public CharaMakeCustomize? SelectedItem
	{
		get => this.selectedItem;
		set
		{
			if (this.selectedItem.HasValue && value.HasValue && this.selectedItem.Value.Equals(value.Value))
				return;

			this.selectedItem = value;

			if (value == null)
				return;

			this.Selected = value.Value.FeatureId;
		}
	}

	public void Close()
	{
		this.OnClosing?.Invoke();
	}

	public void OnClosed()
	{
		// Do nothing
	}
}
