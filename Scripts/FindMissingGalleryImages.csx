#r "nuget: Newtonsoft.Json, 13.0.1"

using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System;
using System.Net;

public class Entry
{
	public string Url { get; set; }
	public string Author { get; set; }
}

const string filePath = "../Anamnesis/Data/Images.json";

string json = File.ReadAllText(filePath);
List<Entry> entries = JsonConvert.DeserializeObject<List<Entry>>(json);

for (int i = entries.Count - 1; i >= 0; i--)
{
	Entry entry = entries[i];
	Console.WriteLine($"{entries.Count - i} / {entries.Count}");
	Console.SetCursorPosition(0, Console.CursorTop - 1);
	HttpWebResponse response = null;

	try
	{
		HttpWebRequest request = (HttpWebRequest)WebRequest.Create(entry.Url);
		request.Method = "HEAD";
		response = (HttpWebResponse)request.GetResponse();
	}
	catch (WebException)
	{
	}

	if (response == null || response.StatusCode != HttpStatusCode.OK)
	{
		Console.WriteLine($"Removing image: {entry.Author} url: {entry.Url}");
		entries.RemoveAt(i);
	}

	if (response != null)
	{
		response.Close();
	}
}

json = JsonConvert.SerializeObject(entries, Formatting.Indented);
File.WriteAllText(filePath, json);