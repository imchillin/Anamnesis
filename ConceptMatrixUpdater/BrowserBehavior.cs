using System.Windows;
using System.Windows.Controls;

namespace ConceptMatrixUpdater
{
	public static class BrowserBehavior
	{
		public static readonly DependencyProperty HtmlProperty = DependencyProperty.RegisterAttached(
			"Html",
			typeof(string),
			typeof(BrowserBehavior),
			new FrameworkPropertyMetadata(OnHtmlChanged)
		);

		[AttachedPropertyBrowsableForType(typeof(WebBrowser))]
		public static string GetHtml(WebBrowser browser)
		{
			return (string)browser.GetValue(HtmlProperty);
		}

		public static void SetHtml(WebBrowser browser, string value)
		{
			browser.SetValue(HtmlProperty, value);
		}

		static void OnHtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is WebBrowser browser)
				browser.NavigateToString(e.NewValue as string);
		}
	}
}
