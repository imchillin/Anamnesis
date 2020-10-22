// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.XMA
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics;
	using System.IO;
	using System.Net;
	using System.Text.Json.Serialization;
	using System.Threading.Tasks;
	using System.Windows;
	using Anamnesis.Serialization;

	public class XmaService : ServiceBase<XmaService>
	{
		// Views today, descending, sfw only, poses only, in json format.
		private const string SearchUrl = "https://www.xivmodarchive.com/search?sortby=views_today&sortorder=desc&nsfw=false&types=11";

		public ObservableCollection<SearchResult> PopularToday { get; set; } = new ObservableCollection<SearchResult>();

		public override Task Start()
		{
			_ = Task.Run(this.Search);
			return base.Start();
		}

		public async Task Search()
		{
			WebRequest req = WebRequest.Create(SearchUrl + "&json=true");
			WebResponse response = await req.GetResponseAsync();
			using StreamReader reader = new StreamReader(response.GetResponseStream());
			string json = reader.ReadToEnd();
			SearchResultWrapper result = SerializerService.Deserialize<SearchResultWrapper>(json);

			if (!result.Success)
				throw new Exception("Did not succede");

			Application.Current.Dispatcher.Invoke(() =>
			{
				this.PopularToday.Clear();

				if (result.SearchResults != null)
				{
					foreach (SearchResult searchResult in result.SearchResults)
					{
						this.PopularToday.Add(searchResult);
					}
				}
			});
		}

		public void OpenSearch()
		{
			ProcessStartInfo psi = new ProcessStartInfo();
			psi.FileName = SearchUrl;
			psi.UseShellExecute = true;
			Process.Start(psi);
		}

		public void Open(SearchResult result)
		{
			ProcessStartInfo psi = new ProcessStartInfo();
			psi.FileName = "https://www.xivmodarchive.com/modid/" + result.Id;
			psi.UseShellExecute = true;
			Process.Start(psi);
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
