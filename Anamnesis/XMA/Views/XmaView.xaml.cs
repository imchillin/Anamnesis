// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.XMA.Views
{
	using System.Windows;
	using System.Windows.Controls;

	/// <summary>
	/// Interaction logic for XmaView.xaml.
	/// </summary>
	public partial class XmaView : UserControl
	{
		public XmaView()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;
		}

		public XmaService XmaService => XmaService.Instance;

		private void OnHeaderClicked(object sender, RoutedEventArgs e)
		{
			this.XmaService.OpenSearch();
		}

		private void OnEntryClicked(object sender, RoutedEventArgs e)
		{
			if (sender is FrameworkElement senderElement && senderElement.DataContext is SearchResult sr)
			{
				this.XmaService.Open(sr);
			}
		}

		private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (this.IsVisible && this.XmaService.PopularToday.Count <= 0)
			{
				this.XmaService.UpdatePopular();
			}
		}
	}
}
