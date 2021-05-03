// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Controls
{
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Media;
	using Anamnesis.Styles.DependencyProperties;
	using FontAwesome.Sharp;
	using PropertyChanged;

	/// <summary>
	/// Interaction logic for IconButton.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class IconButton : UserControl
	{
		public static readonly IBind<IconChar> IconDp = Binder.Register<IconChar, IconButton>(nameof(Icon));

		public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(nameof(Click), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(IconButton));

		public IconButton()
		{
			this.InitializeComponent();

			this.ContentArea.DataContext = this;
		}

		public event RoutedEventHandler Click
		{
			add => this.AddHandler(ClickEvent, value);
			remove => this.RemoveHandler(ClickEvent, value);
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

		public IconChar Icon
		{
			get => IconDp.Get(this);
			set => IconDp.Set(this, value);
		}

		private void OnClick(object sender, RoutedEventArgs e)
		{
			this.RaiseEvent(new RoutedEventArgs(ClickEvent));
		}
	}
}
