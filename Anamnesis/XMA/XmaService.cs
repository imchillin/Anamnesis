// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.XMA
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.IO;
	using System.Net;
	using System.Text.Json.Serialization;
	using System.Threading.Tasks;
	using System.Windows;
	using Anamnesis.Serialization;
	using Anamnesis.Services;

	public class XmaService : ServiceBase<XmaService>
	{
		// Views today, descending, sfw only, poses only, in json format.
		private const string SearchUrl = "https://www.xivmodarchive.com/search?sortby=views_today&sortorder=desc&nsfw=false&types=11";

		public ObservableCollection<SearchResult> PopularToday { get; set; } = new ObservableCollection<SearchResult>();
		public bool IsLoading { get; set; } = false;

		public override Task Start()
		{
			// Update the popular list only if we havn't hidden the xma view
			// dont wait for the list to update, let the UI load.
			if (!SettingsService.Current.HideXmaPoses)
				this.UpdatePopular();

			return base.Start();
		}

		public void UpdatePopular()
		{
			if (this.IsLoading)
				return;

			Task.Run(this.UpdatePopularAsync);
		}

		public async Task UpdatePopularAsync()
		{
			if (this.IsLoading)
				return;

			Application.Current.Dispatcher.Invoke(() => this.IsLoading = true);

			try
			{
				WebRequest req = WebRequest.Create(SearchUrl + "&json=true");
				WebResponse response = await req.GetResponseAsync();
				using StreamReader reader = new StreamReader(response.GetResponseStream());
				string json = reader.ReadToEnd();
				SearchResultWrapper result = SerializerService.Deserialize<SearchResultWrapper>(json);

				Application.Current.Dispatcher.Invoke(() =>
				{
					this.PopularToday.Clear();

					if (result.Success && result.SearchResults != null)
					{
						for (int i = 0; i < Math.Min(8, result.SearchResults.Count); i++)
						{
							SearchResult searchResult = result.SearchResults[i];
							this.PopularToday.Add(searchResult);
						}
					}

					this.IsLoading = false;
				});
			}
			catch (Exception ex)
			{
				this.IsLoading = false;
				Log.Write(SimpleLog.Severity.Warning, ex);
			}
		}

		public void OpenSearch()
		{
			UrlUtility.Open(SearchUrl);
		}

		public void Open(SearchResult result)
		{
			UrlUtility.Open("https://www.xivmodarchive.com/modid/" + result.Id);
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
