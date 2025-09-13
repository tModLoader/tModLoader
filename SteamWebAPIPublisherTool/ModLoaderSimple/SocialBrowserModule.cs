using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Text;
using System.Drawing.Drawing2D;
using System.Text.Json.Serialization;
using System.Text.Json;
using SteamWebAPIPublisherTool.SteamWebApi;

namespace ModLoaderSimple;

public struct ModPubId_t
{
	public string m_ModPubId;
}

public struct ModVersionHash
{
	private string hash; // 28+2 chars, SHA1. +2 is for string type

	public override string ToString() => $"{hash}";

	public ModVersionHash(string hash)
	{
		this.hash = hash;
	}

	public ModVersionHash(TmodFile modFile)
	{
		hash = Encoding.UTF8.GetString(modFile.Hash);
	}

	internal byte[] GetHash()
	{
		return Encoding.UTF8.GetBytes(hash);
	}
}

public class DeveloperMetadata
{
	[JsonInclude]
	public List<string> modVersionHashes { get; set; }
}

public class SocialBrowserException : Exception
{
	public SocialBrowserException(string message) : base(message)
	{
	}
}

public interface SocialBrowserModule
{ 
	public static string GetBrowserVersionNumber(Version tmlVersion)
	{
		if (tmlVersion < new Version(0, 12)) // Versions 0 to 0.11.8.9
			return "1.3"; // Long Term Service Version 1.3

		if (tmlVersion < new Version(2022, 10)) // Versions 0.12 to 2022.9
			return "1.4.3"; // Long Term Service version 1.4.3

		// We treat tModLoader versions between 2022.10.0.0 and 2023.3.85.0 as 'dead' versions.
		// Any mods built against these are not expected to actually work with tModLoader, and should be excluded in any ModBrowser or Mods Menu usage
		// The core reasonsing is due to systemic changes that broke nearly all mods during the 1.4.4 port (Localization rework)
		// It is recommended, given the timing of it, to ignore all tMods in publish folder with this.
		// NOTE: This does cause this tag to be added on Steam in the 'unsorted tags' category, for better or worse - Solxan
		if (tmlVersion < new Version(2023, 3, 85)) // Introduction of 1.4.4 tag and end of major 1.4.4 breaking changes
			return "1.4.4-Transitive";

		return "1.4.4"; // Long Term Service Version 1.4.4 (Current)
	}

	public static (string browserVersion, int keepCount)[] keepRequirements =
			{ ("1.4.3", 1), ("1.4.4", 3), ("1.3", 1), ("1.4.4-Transitive", 0) };

	// Developer Metadata Field
	internal static string CalculateDevMetadata(string workshopItemFolder)
	{
		var devMetadata = new DeveloperMetadata();
		CalculateModHashes(workshopItemFolder, /*modDownloadItemAsFound,*/ ref devMetadata);

		return JsonSerializer.Serialize<DeveloperMetadata>(devMetadata);
	}

	// PR 4345 - We combine the hash data that is currently on workshop with the hash data from the updated publishing folder to ensure that when mods are updated it is backwards compatible
	// It is backwards compatible while Steam spends up to an hour rolling out workshop item updates
	internal static void CalculateModHashes(string workshopPath, /*ModDownloadItem modDownloadItemAsFound,*/ ref DeveloperMetadata devMetadata)
	{
		// Get the hashes from the existing modDownloadItem as found on the workshop
		var itemDetails = SteamWebWrapper.GetItemMetadata(Path.GetFileNameWithoutExtension(workshopPath));

		var prevHashes = string.IsNullOrEmpty(itemDetails.Metadata) ? new List<ModVersionHash>() : JsonSerializer.Deserialize<DeveloperMetadata>(itemDetails.Metadata).modVersionHashes.Select(h => new ModVersionHash(h));

		// Get the new hashes
		var currentHashes = new List<ModVersionHash>();
		foreach (var tModPath in Directory.EnumerateFiles(workshopPath, "*.tmod*", SearchOption.AllDirectories)) {
			var tModFile = new TmodFile(tModPath);
			tModFile.Open(); // Needed for Hash data to be populated
			currentHashes.Add(new ModVersionHash(tModFile));

			tModFile.Close();
		}

		List<ModVersionHash> totalHash = currentHashes.Concat(prevHashes.Except(currentHashes).ToList()).ToList();

		devMetadata.modVersionHashes = totalHash.Select(h => h.ToString()).ToList();
	}
}