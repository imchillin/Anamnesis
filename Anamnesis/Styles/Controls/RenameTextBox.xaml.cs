// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Controls
{
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;
	using Anamnesis.Styles.DependencyProperties;

	/// <summary>
	/// Interaction logic for RenameTextBox.xaml.
	/// </summary>
	public partial class RenameTextBox : UserControl
	{
		public static readonly IBind<string> TextDp = Binder.Register<string, RenameTextBox>(nameof(Text));

		public RenameTextBox()
		{
			this.InitializeComponent();
			this.TextArea.DataContext = this;
		}

		public string Text
		{
			get => TextDp.Get(this);
			set => TextDp.Set(this, value);
		}

		private async void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (!this.IsVisible)
				return;

			await Task.Delay(10);

			this.Dispatcher.Invoke(() =>
			{
				if (!this.IsVisible)
					return;

				FocusManager.SetFocusedElement(FocusManager.GetFocusScope(this.TextArea), this.TextArea);
				Keyboard.Focus(this.TextArea);
				this.TextArea.Focus();
				this.TextArea.SelectAll();
			});
		}

		private void TextArea_LostFocus(object sender, RoutedEventArgs e)
		{
			string newName = this.TextArea.Text;
		}

		private void TextArea_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				FocusManager.SetFocusedElement(FocusManager.GetFocusScope(this.TextArea), null);
				Keyboard.ClearFocus();
			}
		}
	}
}
