// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Character.Views
{
	using System.Windows.Controls;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for MonsterVariantView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class MonsterVariantView : UserControl
	{
		public MonsterVariantView()
		{
			this.InitializeComponent();
		}
	}
}
