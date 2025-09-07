// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Penumbra;

using Anamnesis.Serialization;
using Serilog;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

internal static class PenumbraApi
{
	private const string URL = "http://localhost:42069/api";
	private const int TIMEOUT_MS = 500;

	private static readonly HttpClient s_client = new()
	{
		Timeout = TimeSpan.FromMilliseconds(TIMEOUT_MS),
	};

	public static async Task<string> Post(string route, object content)
	{
		var response = await PostRequest(route, content);
		return response;
	}

	private static async Task<string> PostRequest(string route, object content)
	{
		if (!route.StartsWith('/'))
			route = '/' + route;

		try
		{
			string json = SerializerService.Serialize(content);

			var buffer = Encoding.UTF8.GetBytes(json);
			var byteContent = new ByteArrayContent(buffer);
			byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
			HttpContent httpContent = byteContent;

			using var response = await s_client.PostAsync(URL + route, httpContent);
			response.EnsureSuccessStatusCode();

			using var responseContent = await response.Content.ReadAsStreamAsync();
			using var sr = new StreamReader(responseContent);
			string body = await sr.ReadToEndAsync();

			return body;
		}
		catch (Exception ex)
		{
			Log.Warning(ex, "Penumbra Http API error");
			throw new Exception("Penumbra Http API error. (Have you enabled the Penumbra HTTP Api?)", ex);
		}
	}
}
