// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Posing.Views
{
	using System.Windows.Controls;
	using Anamnesis.PoseModule.Pages;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for LoadOptions.xaml.
	/// </summary>
	public partial class LoadOptions : UserControl
	{
		public LoadOptions()
		{
			this.InitializeComponent();
			this.DataContext = PosePage.FileConfig;
		}
	}
}
