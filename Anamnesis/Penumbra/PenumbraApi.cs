// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Penumbra
{
	using System;
	using System.IO;
	using System.Net;
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
			WebResponse response = await PostRequest(route, content);

			using StreamReader? sr = new StreamReader(response.GetResponseStream());
			string json = sr.ReadToEnd();

			return SerializerService.Deserialize<T>(json);
		}

		private static async Task<WebResponse> PostRequest(string route, object content)
		{
			if (!route.StartsWith('/'))
				route = '/' + route;

			string json = SerializerService.Serialize(content);

			WebRequest request = WebRequest.Create(Url + route);
			request.Timeout = TimeoutMs;
			request.ContentType = "application/json; charset=utf-8";
			request.Method = "POST";
			UTF8Encoding encoding = new UTF8Encoding();
			byte[] data = encoding.GetBytes(json);
			request.ContentLength = data.Length;
			Stream newStream = await request.GetRequestStreamAsync();
			newStream.Write(data, 0, data.Length);
			newStream.Close();

			try
			{
				return await request.GetResponseAsync();
			}
			catch (WebException ex)
			{
				using WebResponse? response = ex.Response;

				if (response is HttpWebResponse httpResponse)
				{
					using Stream responseData = response.GetResponseStream();
					using var reader = new StreamReader(responseData);
					string text = reader.ReadToEnd();

					Log.Warning("Penumbra Http API error\n\n" + text);
				}

				throw new Exception("Penumbra Http API error. (Have you enabled the Penumbra HTTP Api?)", ex);
			}
		}
	}
}
