// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.WpfStyles.Controls
{
	using System.Windows;
	using ConceptMatrix.Localization;
	using ConceptMatrix.WpfStyles.DependencyProperties;

	/// <summary>
	/// Interaction logic for Label.xaml.
	/// </summary>
	public partial class TextBlock : System.Windows.Controls.TextBlock
	{
		public static readonly IBind<string> KeyDp = Binder.Register<string, TextBlock>(nameof(Key), OnKeyChanged);

		private static readonly ILocalizationService Loc = Services.Get<ILocalizationService>();

		public TextBlock()
		{
			this.Loaded += this.TextBlock_Loaded;
		}

		public string Key { get; set; }

		public static void OnKeyChanged(TextBlock sender, string val)
		{
			sender.Key = val;
			sender.LoadString();
		}

		private void TextBlock_Loaded(object sender, RoutedEventArgs e)
		{
			this.LoadString();
		}

		private void LoadString()
		{
			if (string.IsNullOrEmpty(this.Key))
				return;

			this.Text = Loc.GetString(this.Key);
		}
	}
}
