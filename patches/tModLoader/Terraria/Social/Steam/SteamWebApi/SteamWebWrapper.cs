using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.CodeAnalysis;

namespace Terraria.Social.Steam;

internal static class SteamWebWrapper
{
	internal static string PublisherKey { get; set; } = Environment.GetEnvironmentVariable("steam_publisherkey");

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
		[JsonPropertyName("publishedfileid")]
		public string PublishedFileId { get; set; }

		// Optional Properties from deep search
		[JsonPropertyName("banned")]
		public bool Banned { get; set; } = false;

		[JsonPropertyName("kvtags")]
		public List<KeyValueTags> KeyValuePairs { get; set; } = new List<KeyValueTags>();

		[JsonPropertyName("tags")]
		public List<UserTags> UserTags { get; set; } = new List<UserTags>();

		[JsonPropertyName("metadata")]
		public string Metadata { get; set; } = "";
	}

	public class KeyValueTags
	{
		[JsonPropertyName("key")]
		public string key { get; set; } = "";

		[JsonPropertyName("value")]
		public string value { get; set; } = "";
	}

	public class UserTags
	{
		[JsonPropertyName("tag")]
		public string tag { get; set; } = "";

		[JsonPropertyName("display_name")]
		public string displayName { get; set; } = "";
	}

	private static KeyValuePair<string, string> GetKeyValuePair(string key, string value) => new KeyValuePair<string, string>(key, value);

	internal static async Task<string> PostHttpsAsync(string apiEndpoint, List<KeyValuePair<string, string>> arguments) {
		using HttpResponseMessage response = await _httpClient.PostAsync(
			requestUri: apiEndpoint,
			new FormUrlEncodedContent(arguments)
		);

		response.EnsureSuccessStatusCode();

		return await response.Content.ReadAsStringAsync();
	}

	internal static async Task<string> GetHttpsAsync(string apiEndpoint, List<KeyValuePair<string, string>> arguments)
	{
		var argumentsEncoded = new FormUrlEncodedContent(arguments).ReadAsStringAsync().Result;

		using HttpResponseMessage response = await _httpClient.GetAsync(
			$"{apiEndpoint}?{argumentsEncoded}"
		);

		response.EnsureSuccessStatusCode();

		return await response.Content.ReadAsStringAsync();
	}

	internal static PublishedFileDetail GetItemMetadata(string publishedFileId)
	{
		if (string.IsNullOrEmpty(PublisherKey))
			throw new Exception("Publisher Key Must Be Initialized Before Use");

		const string ApiEndpoint = "IPublishedFileService/GetDetails/v1";

		List<KeyValuePair<string, string>> arguments = new List<KeyValuePair<string, string>>() {
			GetKeyValuePair("publishedfileids[0]", publishedFileId), // the ID of the item requested
			GetKeyValuePair("admin_query", "false"), // don't show 'hidden' items; this is setup to use anon login in SteamCMD
			GetKeyValuePair("appid", "1281930"), // tmodloader
			GetKeyValuePair("key", PublisherKey), // the web api authentication key
			GetKeyValuePair("includetags", "true"),
			GetKeyValuePair("includekvtags", "true"),
			GetKeyValuePair("includemetadata", "true"),
			GetKeyValuePair("short_description", "true"),
		};

		var encodedResponse = GetHttpsAsync(ApiEndpoint, arguments).Result;
		var root = JsonSerializer.Deserialize<PublishedIdQueryOuterResponse>(encodedResponse);

		return root.Response.PublishedFileDetails[0];
	}

	internal static string SetDeveloperMetadata(string publishedFileId, string metadata)
	{
		if (string.IsNullOrEmpty(PublisherKey))
			throw new Exception("Publisher Key Must Be Initialized Before Use");

		const string ApiEndpoint = "IPublishedFileService/SetDeveloperMetadata/v1";

		List<KeyValuePair<string, string>> arguments = new List<KeyValuePair<string, string>>() {
			GetKeyValuePair("publishedfileid", publishedFileId),
			GetKeyValuePair("metadata", metadata),
			GetKeyValuePair("key", PublisherKey),
			GetKeyValuePair("appid", "1281930")
		};

		return PostHttpsAsync(ApiEndpoint, arguments).Result;
	}

	// We don't know enough about what the formatting is for this method's tags_to_add to use it in Github Actions yet - Solxan
	// https://steamapi.xpaw.me/#IPublishedFileService/UpdateKeyValueTags
	/*
	internal static string SetKeyValueTags(string publishedFileId, List<KeyValueTags> keyValueTags)
	{
		if (string.IsNullOrEmpty(PublisherKey))
			throw new Exception("Publisher Key Must Be Initialized Before Use");

		const string ApiEndpoint = "IPublishedFileService/UpdateKeyValueTags/v1";

		List<KeyValuePair<string, string>> arguments = new List<KeyValuePair<string, string>>() {
			GetKeyValuePair("publishedfileid", publishedFileId),
			GetKeyValuePair("key", PublisherKey),
			GetKeyValuePair("appid", "1281930"),
			GetKeyValuePair("tags_to_add", JsonSerializer.Serialize(keyValueTags));
		};

		return PostHttpsAsync(ApiEndpoint, arguments).Result;
	}
	*/

	private const float NumberResultsPerPage = 50f;

	private static string QueryForPublisherIdsInnerCursor(string cursor)
	{
		if (string.IsNullOrEmpty(PublisherKey))
			throw new Exception("Publisher Key Must Be Initialized Before Use");

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
			GetKeyValuePair("return_metadata", "true"), // only return the published ID for speed
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
			Console.WriteLine($"Querying Page {pageTracker} of {totalBallparkEntries / NumberResultsPerPage}");
			var encodedResponse = QueryForPublisherIdsInnerCursor(cursor);

			var root = JsonSerializer.Deserialize<PublishedIdQueryOuterResponse>(encodedResponse);
			if (pageTracker == 0 && root.Response.NextCursor != "*")
				totalBallparkEntries = root.Response.Total;

			cursor = root.Response.NextCursor;
			publisherIdPages.Add(root.Response.PublishedFileDetails.Select(pid => pid.PublishedFileId).ToArray());
		}
		while (cursor != "*" && ++pageTracker < Math.Floor(totalBallparkEntries / NumberResultsPerPage) + 1);

		Console.WriteLine($"Querying for PublishedFileIds complete");

		return publisherIdPages;
	}
}
