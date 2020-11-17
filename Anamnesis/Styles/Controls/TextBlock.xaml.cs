// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Controls
{
	using System.ComponentModel;
	using System.Windows;
	using Anamnesis.Services;
	using Anamnesis.Styles.DependencyProperties;

	/// <summary>
	/// Interaction logic for Label.xaml.
	/// </summary>
	public partial class TextBlock : System.Windows.Controls.TextBlock
	{
		public static readonly IBind<string> KeyDp = Binder.Register<string, TextBlock>(nameof(Key), OnKeyChanged, BindMode.OneWay);

		public TextBlock()
		{
			this.Loaded += this.TextBlock_Loaded;
			LocalizationService.LocaleChanged += this.OnLocaleChanged;
		}

		public string? Key { get; set; }

		public static void OnKeyChanged(TextBlock sender, string val)
		{
			sender.Key = val;
			sender.LoadString();
		}

		private void TextBlock_Loaded(object sender, RoutedEventArgs e)
		{
			this.LoadString();
		}

		private void OnLocaleChanged()
		{
			this.LoadString();
		}

		private void LoadString()
		{
			if (string.IsNullOrEmpty(this.Key))
				return;

			string? val = null;

			if (!DesignerProperties.GetIsInDesignMode(this))
				val = LocalizationService.GetString(this.Key);

			if (val == null)
			{
				if (!string.IsNullOrEmpty(this.Text))
					return;

				val = "[" + this.Key + "]";
			}

			this.Text = val;
		}
	}
}
