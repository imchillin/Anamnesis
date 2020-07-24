// Concept Matrix 3.
// Licensed under the MIT license.

namespace Styles.Controls
{
	using System.Windows;
	using System.Windows.Controls;
	using ConceptMatrix.GameData;
	using ConceptMatrix.WpfStyles.DependencyProperties;

	/// <summary>
	/// Interaction logic for ClassFilter.xaml.
	/// </summary>
	public partial class ClassFilter : UserControl
	{
		public static DependencyProperty<Classes> ValueDp = Binder.Register<Classes, ClassFilter>(nameof(ClassFilter.Value), OnValueChanged);

		public ClassFilter()
		{
			this.InitializeComponent();

			this.ContentArea.DataContext = this;
		}

		public Classes Value
		{
			get => ValueDp.Get(this);
			set => ValueDp.Set(this, value);
		}

		private static void OnValueChanged(ClassFilter sender, Classes value)
		{
		}

		private void OnNoneClicked(object sender, RoutedEventArgs e)
		{
			this.Value = Classes.None;
		}

		private void OnAllClicked(object sender, RoutedEventArgs e)
		{
			this.Value = Classes.All;
		}
	}
}
