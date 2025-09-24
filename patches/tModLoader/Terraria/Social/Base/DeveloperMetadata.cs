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
		if (string.IsNullOrWhiteSpace(serializedDevMetadata))
			return new();

		try {
			var devMetadata = JsonConvert.DeserializeObject<DeveloperMetadata>(serializedDevMetadata, new ModVersionHash.VersionHashConverter());

			if (devMetadata == null || devMetadata.modVersionHashes == null) {
				return default;
			}

			return devMetadata;
		}
		catch (Exception) {
			return default;
		}
	}

	// This methods trims contents of developer metadata based on the preferred order of discarding information.
	// It is primarily written with the intent of 'in case' we need to store other information in this Workshop text field
	internal void TrimDevMetadataForPublish()
	{
		const int MaxMetadataLength = Steamworks.Constants.k_cchDeveloperMetadataMax;

		var overflowLength = Serialize().Length - MaxMetadataLength;
		if (overflowLength <= 0)
			return;

		int charsSaved = 0;

		// Check if we can reduce the number of ModHashes
		var minNumberOfHashes = 2 * SocialBrowserModule.keepRequirements.Select(a => a.keepCount).Sum();
		int hashesToKeep = modVersionHashes.Count;

		while (hashesToKeep > minNumberOfHashes) {
			charsSaved += modVersionHashes[hashesToKeep-- - 1].ToString().Length;

			if (overflowLength <= charsSaved)
				break;
		}
		modVersionHashes = modVersionHashes.Take(hashesToKeep).ToList();

		// Throw if we can't reduce the total character count to within limits
		if (overflowLength > charsSaved)
			throw new Exception("Developer Metadata Exceeds maximum allowed space while meeting minimum requirements. Mod could not be uploaded");
	}
}
