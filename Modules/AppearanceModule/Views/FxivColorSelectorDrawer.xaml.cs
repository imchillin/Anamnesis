// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.AppearanceModule.Views
{
	using System.Windows.Controls;
	using ConceptMatrix.AppearanceModule.Utilities;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for FxivColorSelectorDrawer.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class FxivColorSelectorDrawer : UserControl, IDrawer
	{
		private bool locked = false;
		private int selected;

		public delegate void SelectorEvent(int value);

		public FxivColorSelectorDrawer(ColorData.Entry[] colors, int selectedIndex)
		{
			this.InitializeComponent();

			this.locked = true;
			this.List.ItemsSource = colors;
			this.Selected = selectedIndex;
			this.locked = false;

			this.ContentArea.DataContext = this;
		}

		public event DrawerEvent Close;
		public event SelectorEvent SelectionChanged;

		public int Selected
		{
			get
			{
				return selected;
			}

			set
			{
				selected = value;

				if (this.locked)
					return;

				this.SelectionChanged?.Invoke(this.Selected);
			}
		}
	}
}
