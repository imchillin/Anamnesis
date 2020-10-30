// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Controls
{
	using System.Windows.Controls;
	using Anamnesis.Styles.DependencyProperties;
	using FontAwesome.Sharp;

	/// <summary>
	/// Interaction logic for Panel.xaml.
	/// </summary>
	public partial class Header : UserControl
	{
		public static readonly IBind<IconChar> IconDp = Binder.Register<IconChar, Header>(nameof(Icon));
		public static readonly IBind<string> TextDp = Binder.Register<string, Header>(nameof(Text));

		public Header()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;
		}

		public IconChar Icon
		{
			get => IconDp.Get(this);
			set => IconDp.Set(this, value);
		}

		public string Text
		{
			get => TextDp.Get(this);
			set => TextDp.Set(this, value);
		}
	}
}
