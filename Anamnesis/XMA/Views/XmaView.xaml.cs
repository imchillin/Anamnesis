// © Anamnesis.
// Developed by W and A Walsh.
// Licensed under the MIT license.

namespace Anamnesis.XMA.Views
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.IO;
	using System.Net;
	using System.Text.Json.Serialization;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using Anamnesis.Serialization;
	using Anamnesis.Services;
	using Anamnesis.Styles.DependencyProperties;
	using PropertyChanged;
	using SimpleLog;

	/// <summary>
	/// Interaction logic for XmaView.xaml.
	/// </summary>
	[AddINotifyPropertyChangedInterface]
	public partial class XmaView : UserControl
	{
		// Views today, descending, sfw only, poses only.
		public const string PopularTodaySearchUrl = "https://www.xivmodarchive.com/search?sortby=views_today&sortorder=desc&nsfw=false&types=11";

		// Views today, descending, sfw only, poses only.
		public const string RecentSearchUrl = "https://www.xivmodarchive.com/search?sortby=time_posted&sortorder=desc&nsfw=false&types=11";

		public static readonly IBind<Settings.HomeWidgetType> TypeDp = Binder.Register<Settings.HomeWidgetType, XmaView>(nameof(Type), OnTypeChanged);

		private static readonly Logger Log = SimpleLog.Log.GetLogger<XmaView>();

		public XmaView()
		{
			this.InitializeComponent();
			this.ContentArea.DataContext = this;

			Task.Run(this.Refresh);
		}

		public bool IsLoading { get; set; } = false;
		public ObservableCollection<SearchResult> Items { get; set; } = new ObservableCollection<SearchResult>();

		public Settings.HomeWidgetType Type
		{
			get => TypeDp.Get(this);
			set => TypeDp.Set(this, value);
		}

		private static void OnTypeChanged(XmaView sender, Settings.HomeWidgetType newType)
		{
			Task.Run(sender.Refresh);
		}

		private void OnEntryClicked(object sender, RoutedEventArgs e)
		{
			if (sender is FrameworkElement senderElement && senderElement.DataContext is SearchResult sr)
			{
				sr.Open();
			}
		}

		private async Task Refresh()
		{
			await Dispatch.MainThread();

			if (this.Type != Settings.HomeWidgetType.XmaLatest && this.Type != Settings.HomeWidgetType.XmaTop)
				return;

			this.IsLoading = true;

			string url = this.Type == Settings.HomeWidgetType.XmaLatest ? RecentSearchUrl : PopularTodaySearchUrl;

			try
			{
				WebRequest req = WebRequest.Create(url + "&json=true");
				WebResponse response = await req.GetResponseAsync();
				using StreamReader reader = new StreamReader(response.GetResponseStream());
				string json = reader.ReadToEnd();
				SearchResultWrapper result = SerializerService.Deserialize<SearchResultWrapper>(json);

				if (result.Success && result.SearchResults != null)
				{
					await Dispatch.MainThread();
					this.Items.Clear();
					foreach (SearchResult? entry in result.SearchResults)
					{
						this.Items.Add(entry);
					}

					this.IsLoading = false;
				}
			}
			catch (Exception ex)
			{
				this.IsLoading = false;
				Log.Write(Severity.Warning, ex);
			}
		}

		[Serializable]
		public class SearchResultWrapper
		{
			[JsonPropertyName("success")]
			public bool Success { get; set; }

			[JsonPropertyName("searchResults")]
			public List<SearchResult>? SearchResults { get; set; }
		}
	}
}
