// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Penumbra;

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Anamnesis.Serialization;
using Serilog;

internal static class PenumbraApi
{
	private const string Url = "http://localhost:42069/api";
	private const int TimeoutMs = 500;

	public static async Task Post(string route, object content)
	{
		await PostRequest(route, content);
	}

	public static async Task<T> Post<T>(string route, object content)
		where T : notnull
	{
		HttpResponseMessage response = await PostRequest(route, content);

		using StreamReader? sr = new StreamReader(await response.Content.ReadAsStreamAsync());
		string json = sr.ReadToEnd();

		return SerializerService.Deserialize<T>(json);
	}

	private static async Task<HttpResponseMessage> PostRequest(string route, object content)
	{
		if (!route.StartsWith('/'))
			route = '/' + route;

		try
		{
			string json = SerializerService.Serialize(content);

			using HttpClient client = new HttpClient();
			client.Timeout = TimeSpan.FromMilliseconds(TimeoutMs);
			var buffer = Encoding.UTF8.GetBytes(json);
			var byteContent = new ByteArrayContent(buffer);
			byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			using var response = await client.PostAsync(Url + route, byteContent);

			return response;
		}
		catch (Exception ex)
		{
			Log.Warning(ex, "Penumbra Http API error");
			throw new Exception("Penumbra Http API error. (Have you enabled the Penumbra HTTP Api?)", ex);
		}
	}
}
