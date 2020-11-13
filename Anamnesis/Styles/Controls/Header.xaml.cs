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
		public static readonly IBind<string> KeyDp = Binder.Register<string, Header>(nameof(Key));

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

		public string Key
		{
			get => KeyDp.Get(this);
			set => KeyDp.Set(this, value);
		}
	}
}
