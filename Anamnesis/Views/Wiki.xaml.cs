// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.Views
{
	using System;
	using System.Diagnostics;
	using System.Net;
	using System.Threading.Tasks;
	using System.Windows.Controls;
	using Anamnesis.Extensions;

	/// <summary>
	/// Interaction logic for Wiki.xaml.
	/// </summary>
	public partial class Wiki : UserControl
	{
		public Wiki()
		{
			this.InitializeComponent();
			Task.Run(this.Load);
		}

		private async Task Load()
		{
			string url = "https://raw.githubusercontent.com/wiki/imchillin/Anamnesis/WidgetHelpHome.md";

			WebClient client = new WebClient();
			string markdown = await client.DownloadStringTaskAsync(url);
			await Dispatch.MainThread();
			this.MainViewer.Document = Markdown.ToDocument(markdown);
		}
	}
}
