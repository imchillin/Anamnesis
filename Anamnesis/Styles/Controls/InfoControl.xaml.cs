// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Controls
{
	using System.Windows.Controls;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for InfoControl.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class InfoControl : UserControl
	{
		public InfoControl()
		{
			this.InitializeComponent();

			this.ContentArea.DataContext = this;

			this.IsError = false;
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

		public bool IsError
		{
			get;
			set;
		}
	}
}
