using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace SteamWebAPIPublisherTool.SteamWebApi;
internal static class SteamWebWrapper
{
	internal static string PublisherKey = null;

	private static HttpClient _httpClient = new() {
		BaseAddress = new Uri("https://partner.steam-api.com/")
	};

	public class PublishedIdQueryOuterResponse
	{
		[JsonPropertyName("response")]
		public PublishedIdQueryInnerResponse Response { get; set; }
	}

	public class PublishedIdQueryInnerResponse
	{
		[JsonPropertyName("total")]
		public int Total { get; set; }

		[JsonPropertyName("publishedfiledetails")]
		public List<PublishedFileDetail> PublishedFileDetails { get; set; }

		[JsonPropertyName("next_cursor")]
		public string NextCursor { get; set; }
	}

	public class PublishedFileDetail
	{
		[JsonPropertyName("result")]
		public int Result { get; set; }

		[JsonPropertyName("publishedfileid")]
		public string PublishedFileId { get; set; }

		[JsonPropertyName("language")]
		public int Language { get; set; }
	}

	//TODO: Replace with internal Set; Get;
	internal static void SetPublisherKey(string publisherKey)
	{
		PublisherKey = publisherKey;
	} 

	private static KeyValuePair<string, string> GetKeyValuePair(string key, string value) => new KeyValuePair<string, string>(key, value);

	internal static async Task<string> PostHttpsAsync(string apiEndpoint, List<KeyValuePair<string, string>> arguments) {
		if (PublisherKey is null)
			throw new Exception("Publisher Key Must Be Initialized Before Use");

		using HttpResponseMessage response = await _httpClient.PostAsync(
			requestUri: apiEndpoint,
			new FormUrlEncodedContent(arguments)
		);

		response.EnsureSuccessStatusCode();

		return await response.Content.ReadAsStringAsync();
	}

	internal static async Task<string> GetHttpsAsync(string apiEndpoint, List<KeyValuePair<string, string>> arguments)
	{
		if (PublisherKey is null)
			throw new Exception("Publisher Key Must Be Initialized Before Use");

		var argumentsEncoded = new FormUrlEncodedContent(arguments).ReadAsStringAsync().Result;

		using HttpResponseMessage response = await _httpClient.GetAsync(
			$"{apiEndpoint}?{argumentsEncoded}"
		);

		response.EnsureSuccessStatusCode();

		return await response.Content.ReadAsStringAsync();
	}

	internal static string SetDeveloperMetadata(string publishedFileId, string metadata)
	{
		const string ApiEndpoint = "IPublishedFileService/SetDeveloperMetadata/v1";

		List<KeyValuePair<string, string>> arguments = new List<KeyValuePair<string, string>>() {
			GetKeyValuePair("publishedfileid", publishedFileId),
			GetKeyValuePair("metadata", metadata),
			GetKeyValuePair("key", PublisherKey),
			GetKeyValuePair("appid", "1281930")
		};

		return PostHttpsAsync(ApiEndpoint, arguments).Result;
	}

	private const float NumberResultsPerPage = 100f;

	private static string QueryForPublisherIdsInnerCursor(string cursor)
	{
		const string ApiEndpoint = "IPublishedFileService/QueryFiles/v1";

		List<KeyValuePair<string, string>> arguments = new List<KeyValuePair<string, string>>() {
			GetKeyValuePair("query_type", "1"), // ordered by publication date, newest first
			GetKeyValuePair("page", "0"), // required
			GetKeyValuePair("numperpage", $"{NumberResultsPerPage}"), // up to 100 items per returned response
			GetKeyValuePair("creator_appid", "1281930"), // tmodloader
			GetKeyValuePair("appid", "1281930"), // tmodloader
			GetKeyValuePair("filetype", "0"), // workshop items
			GetKeyValuePair("admin_query", "false"), // don't show 'hidden' items; this is setup to use anon login
			GetKeyValuePair("ids_only", "true"), // only return the published ID for speed
			GetKeyValuePair("key", PublisherKey), // the web api authentication key
			GetKeyValuePair("cursor", cursor) // the cursor used for deep pagination
		};

		return GetHttpsAsync(ApiEndpoint, arguments).Result;
	}

	internal static List<string[]> QueryForPublisherIds()
	{
		string cursor = "*";
		int totalBallparkEntries = 20000;
		int pageTracker = 0;
		List<string[]> publisherIdPages = new List<string[]>();

		do {
			var encodedResponse = QueryForPublisherIdsInnerCursor(cursor);

			var root = JsonSerializer.Deserialize<PublishedIdQueryOuterResponse>(encodedResponse);
			if (pageTracker == 0 && root.Response.NextCursor != "*")
				totalBallparkEntries = root.Response.Total;

			cursor = root.Response.NextCursor;
			publisherIdPages.Add(root.Response.PublishedFileDetails.Select(pid => pid.PublishedFileId).ToArray());
		}
		while (cursor != "*" && ++pageTracker < Math.Floor(totalBallparkEntries / NumberResultsPerPage) + 1);

		return publisherIdPages;
	}
}
