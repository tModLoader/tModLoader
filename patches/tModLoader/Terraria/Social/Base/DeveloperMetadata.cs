using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI.ModBrowser;
using Terraria.Social.Steam;

namespace Terraria.Social.Base;

public struct ModVersionHash
{
	private string hash;

	public override string ToString() => hash;

	public ModVersionHash(TmodFile modFile)
	{
		hash = System.Convert.ToBase64String(modFile.Hash);
	}

	public byte[] GetHash()
	{
		return System.Convert.FromBase64String(hash);
	}

	public class VersionHashConverter : JsonConverter<ModVersionHash>
	{
		public override void WriteJson(JsonWriter writer, ModVersionHash value, JsonSerializer serializer)
		{
			writer.WriteValue(value.ToString());
		}

		public override ModVersionHash ReadJson(JsonReader reader, Type objectType, ModVersionHash existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			return new ModVersionHash() { hash = (string)reader.Value };
		}
	}
}

public class DeveloperMetadata
{
	public List<ModVersionHash> modVersionHashes { get; set; } = new List<ModVersionHash>();

	internal string Serialize()
	{
		return JsonConvert.SerializeObject(this, Formatting.None, new ModVersionHash.VersionHashConverter());
	}

	internal static DeveloperMetadata Deserialize(string serializedDevMetadata)
	{
		// If the item is new, or hasn't had Metadata set yet, it will be new dev metadata
		if (string.IsNullOrWhiteSpace(serializedDevMetadata))
			return new();

		try {
			return JsonConvert.DeserializeObject<DeveloperMetadata>(serializedDevMetadata, new ModVersionHash.VersionHashConverter());
		}
		catch (Exception) {
			// We will lose any metadata associated with what is on Workshop currently if the Json Deserialize fails...
			// but its already corrupt if so,  probably safe to rewrite with healthy data in this rare occurence - Solxan
			return new();
		}
	}

	// This methods trims contents of developer metadata based on the preferred order of discarding information.
	// It is primarily written with the intent of 'in case' we need to store other information in this Workshop text field
	internal void TrimDevMetadataForPublish()
	{
		const int MaxMetadataLength = Steamworks.Constants.k_cchDeveloperMetadataMax;

		// In case we want to store anything else down the road and it takes up space, this is the minimum amount of hashes we need to keep
		// This minimum avoids issues with delays in the deployment time on Steam from when it is published to when it actually arrives for all users globally
		var minNumberOfHashes = 2 * SocialBrowserModule.keepRequirements.Select(a => a.keepCount).Sum();

		while (Serialize().Length > MaxMetadataLength && modVersionHashes.Count() > minNumberOfHashes + 2) {
			modVersionHashes = modVersionHashes.Take(modVersionHashes.Count() - 2).ToList();
		}
	}
}
