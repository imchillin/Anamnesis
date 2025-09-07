// © Anamnesis.
// Licensed under the MIT license.

using System.Drawing;
using System.Net;
using System.Text.Json;

namespace Scripts;

public class UpdateGallery : ScriptBase
{
	private const int MAX_THUMBNAIL_SIZE = 720;
	private const string FILE_PATH = "../../../../Anamnesis/Data/Images.json";

	public override string Name => "Update Gallery";

	public override void Run()
	{
		var op = new JsonSerializerOptions
		{
			AllowTrailingCommas = true,
			WriteIndented = true,
		};

		string json = File.ReadAllText(FILE_PATH);
		List<Entry>? entries = JsonSerializer.Deserialize<List<Entry>>(json, op)
			?? throw new Exception("Failed to deserialize npc names");

		HashSet<string> toRemove = new();

		Parallel.For(0, entries.Count, (i, t) =>
		{
			Entry entry = entries[i];

			if (entry.Url == null)
				return;

			HttpWebResponse? response = null;
			HttpWebRequest? request = null;

			// Check that Url is valid
			try
			{
				request = (HttpWebRequest)WebRequest.Create(entry.Url);
				request.Method = "HEAD";
				response = (HttpWebResponse)request.GetResponse();
			}
			catch (WebException)
			{
			}

			if (response == null || response.StatusCode != HttpStatusCode.OK)
			{
				Log($"{i:d3} [ X ]");
				toRemove.Add(entry.Url);
				return;
			}

			response?.Close();

			// Generte thumbnail url
			if (entry.Thumbnail == null)
			{
				var uri = new Uri(entry.Url);
				string imagePath = HashUtility.GetHashString(entry.Url) + "--" + uri.Segments[^1];

				try
				{
					request = (HttpWebRequest)WebRequest.Create(entry.Url);
					response = (HttpWebResponse)request.GetResponse();

					Stream responseStream = response.GetResponseStream();

					using var fileStream = new FileStream(imagePath, FileMode.Create, FileAccess.Write);
					responseStream.CopyTo(fileStream);
					fileStream.Close();

					using (Image img = Image.FromFile(imagePath))
					{
						entry.Thumbnail = GetOptimizedDiscordLink(entry.Url, img.Width, img.Height);
					}

					File.Delete(imagePath);

					Log($"{i:d3} [ T ]");
					return;
				}
				catch (WebException)
				{
				}
			}

			Log($"{i:d3} [ O ]");
		});

		foreach (string url in toRemove)
		{
			for (int i = entries.Count - 1; i > 0; i--)
			{
				if (entries[i].Url == url || entries[i].Url == null)
				{
					entries.RemoveAt(i);
				}
			}
		}

		json = JsonSerializer.Serialize(entries, op);
		File.WriteAllText(FILE_PATH, json);
	}

	private static string GetOptimizedDiscordLink(string url, int width, int height)
	{
		if (width == 0 || height == 0)
			return url;

		var uriBuilder = new UriBuilder(url)
		{
			Host = "media.discordapp.net",
		};

		var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);

		if (width > height)
		{
			double ratio = (double)height / (double)width;
			query["width"] = Convert.ToString(MAX_THUMBNAIL_SIZE);
			query["height"] = Convert.ToString((int)(ratio * MAX_THUMBNAIL_SIZE));
		}
		else
		{
			double ratio = (double)width / (double)height;
			query["width"] = Convert.ToString((int)(ratio * MAX_THUMBNAIL_SIZE));
			query["height"] = Convert.ToString(MAX_THUMBNAIL_SIZE);
		}

		uriBuilder.Query = query.ToString();

		try
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uriBuilder.ToString());
			request.Method = "HEAD";
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();

			if (response.StatusCode != HttpStatusCode.OK)
			{
				Log($"Failed to get samaller thumbnail from url: {uriBuilder}: {response.StatusCode}");
				response.Close();
				return url;
			}

			response.Close();
		}
		catch (Exception ex)
		{
			Log($"Failed to get smaller thumbnail from url: {uriBuilder}: {ex.Message}");
			return url;
		}

		return uriBuilder.ToString();
	}

	public class Entry
	{
		public string? Url { get; set; }
		public string? Author { get; set; }
		public string? Thumbnail { get; set; }
	}
}