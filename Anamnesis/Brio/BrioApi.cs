// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Brio;

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Anamnesis.Serialization;
using Serilog;

internal static class BrioApi
{
	private const string Url = "http://localhost:42428/brio";
	private const int TimeoutMs = 500;

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

			using HttpClient client = new HttpClient();
			client.Timeout = TimeSpan.FromMilliseconds(TimeoutMs);
			var buffer = Encoding.UTF8.GetBytes(json);
			var byteContent = new ByteArrayContent(buffer);
			byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			using var response = await client.PostAsync(Url + route, byteContent);
			using var responseContent = await response.Content.ReadAsStreamAsync();
			using StreamReader? sr = new StreamReader(responseContent);
			string body = sr.ReadToEnd();

			return body;
		}
		catch (Exception ex)
		{
			Log.Warning(ex, "Brio Http API error");
			throw new Exception("Brio Http API error. (Have you enabled the Brio HTTP Api?)", ex);
		}
	}
}
