// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Controls
{
	using System.Windows.Controls;

	/// <summary>
	/// Interaction logic for InfoControl.xaml.
	/// </summary>
	public partial class InfoControl : UserControl
	{
		public InfoControl()
		{
			this.InitializeComponent();
		}

		public string? Key
		{
			get => this.TextBlock.Key;
			set => this.TextBlock.Key = value;
		}

		public string? Text
		{
			get => this.TextBlock.Text;
			set => this.TextBlock.Text = value;
		}
	}
}
