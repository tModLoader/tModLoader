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

	private ModVersionHash(string encodedHash)
	{
		hash = encodedHash;
	}

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
			return new ModVersionHash((string)reader.Value);
		}
	}
}

public class DeveloperMetadata
{
	public List<ModVersionHash> modVersionHashes { get; set; } = new List<ModVersionHash>();

	[JsonConstructor]
	private DeveloperMetadata() { }

	internal DeveloperMetadata(string workshopPath, bool useWebApi, ModDownloadItem modDownloadItemAsFound = null)
	{
		CalculateModHashes(workshopPath, useWebApi , modDownloadItemAsFound);
		TrimDevMetadata();
	}
	
	internal DeveloperMetadata(string serializedDevMetadata)
	{
		// Try-Catch is for error correction for any bizarre dev metadata put on items previously
		try {
			var devMetadata = DeSerialize(serializedDevMetadata);

			if (devMetadata == null || devMetadata.modVersionHashes == null) {
				modVersionHashes = new List<ModVersionHash>();
				return;
			}

			modVersionHashes = devMetadata.modVersionHashes;
		}
		catch (Exception) {
			modVersionHashes = new List<ModVersionHash>();
		}
	}

	internal string GetSerialize()
	{
		return JsonConvert.SerializeObject(this, Formatting.None, new ModVersionHash.VersionHashConverter());
	}

	internal static DeveloperMetadata DeSerialize(string serializedDevMetadata)
	{
		return JsonConvert.DeserializeObject<DeveloperMetadata>(serializedDevMetadata, new ModVersionHash.VersionHashConverter());
	}

	// PR 4345 - We combine the hash data that is currently on workshop with the hash data from the updated publishing folder to ensure that when mods are updated it is backwards compatible
	// It is backwards compatible while Steam spends up to an hour rolling out workshop item updates
	private void CalculateModHashes(string workshopPath, bool useWebApi, ModDownloadItem modDownloadItemAsFound = null)
	{
		List<ModVersionHash> prevHashes = new List<ModVersionHash>();

		// Get the old / existing hashes on Workshop
		if (useWebApi) {
			var itemDetails = SteamWebWrapper.GetItemMetadata(Path.GetFileNameWithoutExtension(workshopPath));

			prevHashes = string.IsNullOrEmpty(itemDetails.Metadata) ?
				new List<ModVersionHash>() :
				new DeveloperMetadata(itemDetails.Metadata).modVersionHashes;
		}
		else {
			if (modDownloadItemAsFound is null)
				throw new Exception("Calculate Dev Metadata without WebApi requires the ModDownloadItem to be passed in.");

			// Get the hashes from the existing modDownloadItem as found on the workshop
			prevHashes = modDownloadItemAsFound.DevMetadata.modVersionHashes;
		}

		// Get the new hashes
		var currentHashes = new List<ModVersionHash>();
		foreach (var tModPath in Directory.EnumerateFiles(workshopPath, "*.tmod*", SearchOption.AllDirectories)) {
			var tModFile = new TmodFile(tModPath);
			using var _ = tModFile.Open(); // Needed for Hash data to be populated
			currentHashes.Add(new ModVersionHash(tModFile));
		}

		// Combine the hashes
		modVersionHashes = currentHashes.Concat(prevHashes.Except(currentHashes).ToList()).ToList();
	}

	// This methods trims contents of developer metadata based on the preferred order of discarding information.
	// It is primarily written with the intent of 'in case' we need to store other information in this Workshop text field
	private void TrimDevMetadata()
	{
		const int MaxMetadataLength = Steamworks.Constants.k_cchDeveloperMetadataMax;

		var overflowLength = GetSerialize().Length - MaxMetadataLength;
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
