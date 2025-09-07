// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Controls;

using Anamnesis.GameData;
using PropertyChanged;
using System.Windows;
using System.Windows.Controls;
using XivToolsWpf.DependencyProperties;

/// <summary>
/// Interaction logic for ItemCategoryFilter.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class ItemCategoryFilter : UserControl
{
	public static readonly DependencyProperty<ItemCategories> ValueDp = Binder.Register<ItemCategories, ItemCategoryFilter>(nameof(ItemCategoryFilter.Value));
	public static readonly DependencyProperty<bool> IsWeaponSlotDp = Binder.Register<bool, ItemCategoryFilter>(nameof(ItemCategoryFilter.IsWeaponSlot));

	public ItemCategoryFilter()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public bool IsWeaponSlot { get; set; }

	public ItemCategories Value
	{
		get => ValueDp.Get(this);
		set => ValueDp.Set(this, value);
	}

	private void OnNoneClicked(object sender, RoutedEventArgs e)
	{
		this.Value = ItemCategories.None;
	}

	private void OnAllClicked(object sender, RoutedEventArgs e)
	{
		this.Value = ItemCategories.All;
	}
}
