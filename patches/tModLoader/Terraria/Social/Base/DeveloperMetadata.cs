using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI.ModBrowser;
using Terraria.Social.Steam;

namespace Terraria.Social.Base;

public struct ModVersionHash
{
	private string hash;

	public override string ToString() => hash;

	public ModVersionHash(string encodedHash)
	{
		hash = encodedHash;
	}

	public ModVersionHash(TmodFile modFile)
	{
		hash = Encoding.UTF32.GetString(modFile.Hash);
	}

	public byte[] GetHash()
	{
		return Encoding.UTF32.GetBytes(hash);
	}
}

public class DeveloperMetadata
{
	internal List<ModVersionHash> modVersionHashes;

	internal DeveloperMetadata(string workshopPath, bool useWebApi, ModDownloadItem modDownloadItemAsFound = null)
	{
		CalculateModHashes(workshopPath, useWebApi , modDownloadItemAsFound);
		TrimDevMetadata();
	}

	internal DeveloperMetadata(string serializedDevMetadata)
	{
		var devMetadata = JsonConvert.DeserializeObject<DeveloperMetadata>(serializedDevMetadata);

		if (devMetadata == null || devMetadata.modVersionHashes == null) {
			modVersionHashes = new List<ModVersionHash>();
			return;
		}

		modVersionHashes = devMetadata.modVersionHashes;
	}

	internal string GetSerialize()
	{
		return System.Text.Json.JsonSerializer.Serialize(this);
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
			tModFile.Open(); // Needed for Hash data to be populated
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

		var overflowLength = JsonConvert.SerializeObject(this).Length - MaxMetadataLength;
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
