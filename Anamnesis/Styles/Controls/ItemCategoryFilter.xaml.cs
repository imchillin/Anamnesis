// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Controls
{
	using System.Windows;
	using System.Windows.Controls;
	using XivToolsWpf.DependencyProperties;

	/// <summary>
	/// Interaction logic for ItemCategoryFilter.xaml.
	/// </summary>
	public partial class ItemCategoryFilter : UserControl
	{
		public static DependencyProperty<ItemCategories> ValueDp
			= Binder.Register<ItemCategories, ItemCategoryFilter>(nameof(ItemCategoryFilter.Value));

		public ItemCategoryFilter()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;
		}

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
}
