using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using Hjson;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json.Linq;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.Utilities;
using Terraria.UI;

namespace Terraria.ModLoader;

public static class LocalizationLoader
{
	private static readonly Dictionary<string, ModTranslation> translations = new();

	/// <summary>
	/// Creates a <see cref="ModTranslation"/> object using the provided mod for a prefix to the key. You can use the resulting object in <see cref="AddTranslation"/>.
	/// </summary>
	/// <param name="mod"> The mod that this translation is for. </param>
	/// <param name="key"> The key for the ModTranslation. The full key will be 'Mods.{<paramref name="mod"/>.Name}.{<paramref name="key"/>}'. </param>
	public static ModTranslation CreateTranslation(Mod mod, string key)
		=> CreateTranslation($"Mods.{mod.Name}.{key}");

	/// <summary>
	/// Creates a <see cref="ModTranslation"/> object using the provided full key. You can use the resulting object in <see cref="AddTranslation"/>.
	/// </summary>
	/// <param name="key"> The full key for the ModTranslation. </param>
	public static ModTranslation CreateTranslation(string key)
		=> new ModTranslation(key);

	/// <summary>
	/// Adds a <see cref="ModTranslation"/> to the game so that you can use <see cref="Language.GetText"/> to get a <see cref="LocalizedText"/>.
	/// </summary>
	public static void AddTranslation(ModTranslation translation)
	{
		translations[translation.Key] = translation;
	}

	public static ModTranslation GetOrCreateTranslation(Mod mod, string key, bool defaultEmpty = false)
		=> GetOrCreateTranslation($"Mods.{mod.Name}.{key}", defaultEmpty);

	public static ModTranslation GetOrCreateTranslation(string key, bool defaultEmpty = false)
	{
		key = key.Replace(" ", "_");

		if (translations.TryGetValue(key, out var translation))
			return translation;
		
		var newTranslation = new ModTranslation(key, defaultEmpty);
		translations[key] = newTranslation;
		return newTranslation;
	}

	internal static void Autoload(Mod mod)
	{
		if (mod.File == null)
			return;

		var modTranslationDictionary = new Dictionary<string, ModTranslation>();

		AutoloadTranslations(mod, modTranslationDictionary);

		foreach (var value in modTranslationDictionary.Values) {
			AddTranslation(value);

			//This must be manually added here, since we need to know what mod is added in order to add GameTipData.
			if (value.Key.StartsWith($"Mods.{mod.Name}.GameTips.")) {
				Main.gameTips.allTips.Add(new GameTipData(new LocalizedText(value.Key, value.GetDefault()), mod));
			}
		}
	}

	internal static void Unload()
	{
		translations.Clear();
	}

	//TODO: Unhardcode ALL of this.
	public static void RefreshModLanguage(GameCulture culture)
	{
		Dictionary<string, LocalizedText> dict = LanguageManager.Instance._localizedTexts;

		foreach (ModItem item in ItemLoader.items) {
			var text = new LocalizedText(item.DisplayName.Key, item.DisplayName.GetTranslation(culture));

			Lang._itemNameCache[item.Item.type] = SetLocalizedText(dict, text);

			text = new LocalizedText(item.Tooltip.Key, item.Tooltip.GetTranslation(culture));

			if (text.Value != null) {
				text = SetLocalizedText(dict, text);
				Lang._itemTooltipCache[item.Item.type] = ItemTooltip.FromLanguageKey(text.Key);
				ContentSamples.ItemsByType[item.Item.type].RebuildTooltip();
			}
		}

		foreach (ModPrefix prefix in PrefixLoader.prefixes) {
			var text = new LocalizedText(prefix.DisplayName.Key, prefix.DisplayName.GetTranslation(culture));

			Lang.prefix[prefix.Type] = SetLocalizedText(dict, text);
		}

		foreach (var keyValuePair in MapLoader.tileEntries) {
			foreach (MapEntry entry in keyValuePair.Value) {
				if (entry.translation != null) {
					SetLocalizedText(dict, new LocalizedText(entry.translation.Key, entry.translation.GetTranslation(culture)));
				}
			}
		}

		foreach (var keyValuePair in MapLoader.wallEntries) {
			foreach (MapEntry entry in keyValuePair.Value) {
				if (entry.translation != null) {
					var text = new LocalizedText(entry.translation.Key, entry.translation.GetTranslation(culture));

					SetLocalizedText(dict, text);
				}
			}
		}

		foreach (ModProjectile proj in ProjectileLoader.projectiles) {
			var text = new LocalizedText(proj.DisplayName.Key, proj.DisplayName.GetTranslation(culture));
			Lang._projectileNameCache[proj.Projectile.type] = SetLocalizedText(dict, text);
		}

		foreach (ModNPC npc in NPCLoader.npcs) {
			var text = new LocalizedText(npc.DisplayName.Key, npc.DisplayName.GetTranslation(culture));

			Lang._npcNameCache[npc.NPC.type] = SetLocalizedText(dict, text);
		}

		foreach (ModBuff buff in BuffLoader.buffs) {
			var text = new LocalizedText(buff.DisplayName.Key, buff.DisplayName.GetTranslation(culture));

			Lang._buffNameCache[buff.Type] = SetLocalizedText(dict, text);

			text = new LocalizedText(buff.Description.Key, buff.Description.GetTranslation(culture));

			Lang._buffDescriptionCache[buff.Type] = SetLocalizedText(dict, text);
		}

		foreach (ModTranslation translation in translations.Values) {
			LocalizedText text = new LocalizedText(translation.Key, translation.GetTranslation(culture));

			SetLocalizedText(dict, text);
		}

		LanguageManager.Instance.ProcessCopyCommandsInTexts();
	}

	internal static void UpgradeLangFile(string langFile, string modName)
	{
		string[] contents = File.ReadAllLines(langFile, Encoding.UTF8);

		// Legacy .lang files had 'Mods.ModName.' prefixed to every key.
		// Modern .hjson localization does not have that.
		var modObject = new JObject();
		var modsObject = new JObject{
			{ modName, modObject }
		};
		var rootObject = new JObject {
			{ "Mods", modsObject }
		};

		foreach (string line in contents) {
			if (line.Trim().StartsWith("#"))
				continue;

			int split = line.IndexOf('=');

			if (split < 0)
				continue; // lines without an '=' are ignored

			string key = line.Substring(0, split).Trim().Replace(" ", "_");
			string value = line.Substring(split + 1); // removed .Trim() since sometimes it is desired.

			if (value.Length == 0) {
				continue;
			}

			value = value.Replace("\\n", "\n");

			string[] splitKey = key.Split(".");
			var curObj = modObject;

			foreach (string k in splitKey.SkipLast(1)) {
				if (!curObj.ContainsKey(k)) {
					curObj.Add(k, new JObject());
				}

				var existingVal = curObj.GetValue(k);

				if (existingVal.Type == JTokenType.Object) {
					curObj = (JObject)existingVal;
				}
				else {
					// Someone assigned a value to this key - move this value to special
					//  "$parentVal" key in newly created object
					curObj[k] = new JObject();
					curObj = (JObject)curObj.GetValue(k);
					curObj["$parentVal"] = existingVal;
				}
			}

			string lastKey = splitKey.Last();

			if (curObj.ContainsKey(splitKey.Last()) && curObj[lastKey] is JObject) {
				// this value has children - needs to go into object as a $parentValue entry
				((JObject)curObj[lastKey]).Add("$parentValue", value);
			}

			curObj.Add(splitKey.Last(), value);
		}

		// Convert JSON to HJSON and dump to new file
		// Don't delete old .lang file - let the user do this when they are happy
		string newFile = Path.ChangeExtension(langFile, "hjson");
		string hjsonContents = JsonValue.Parse(rootObject.ToString()).ToFancyHjsonString();

		File.WriteAllText(newFile, hjsonContents);
		File.Move(langFile, $"{langFile}.legacy", true);
	}

	private static void AutoloadTranslations(Mod mod, Dictionary<string, ModTranslation> modTranslationDictionary)
	{
		foreach (var translationFile in mod.File.Where(entry => Path.GetExtension(entry.Name) == ".hjson")) {
			using var stream = mod.File.GetStream(translationFile);
			using var streamReader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

			string translationFileContents = streamReader.ReadToEnd();

			(var culture, string prefix) = GameCulture.FromPath(translationFile.Name);

			// Parse HJSON and convert to standard JSON
			string jsonString = HjsonValue.Parse(translationFileContents).ToString();

			// Parse JSON
			var jsonObject = JObject.Parse(jsonString);
			// Flatten JSON into dot seperated key and value
			var flattened = new Dictionary<string, string>();

			foreach (JToken t in jsonObject.SelectTokens("$..*")) {
				if (t.HasValues) {
					continue;
				}

				// Due to comments, some objects can by empty
				if (t is JObject obj && obj.Count == 0)
					continue;

				// Custom implementation of Path to allow "x.y" keys
				string path = "";
				JToken current = t;

				for (JToken parent = t.Parent; parent != null; parent = parent.Parent) {
					path = parent switch {
						JProperty property => property.Name + (path == string.Empty ? string.Empty : "." + path),
						JArray array => array.IndexOf(current) + (path == string.Empty ? string.Empty : "." + path),
						_ => path
					};
					current = parent;
				}

				flattened.Add(path, t.ToString());
			}

			foreach (var (key, value) in flattened) {
				string effectiveKey = key.Replace(".$parentVal", "");
				if(!string.IsNullOrWhiteSpace(prefix))
					effectiveKey = prefix + "." + effectiveKey;
				if (!modTranslationDictionary.TryGetValue(effectiveKey, out ModTranslation mt)) {
					// removing instances of .$parentVal is an easy way to make this special key assign its value
					//  to the parent key instead (needed for some cases of .lang -> .hjson auto-conversion)
					modTranslationDictionary[effectiveKey] = mt = CreateTranslation(effectiveKey);
				}

				mt.AddTranslation(culture, value);
			}
		}
	}

	private static LocalizedText SetLocalizedText(Dictionary<string, LocalizedText> dict, LocalizedText value)
	{
		if (dict.TryGetValue(value.Key, out var localizedText)) {
			localizedText.SetValue(value.Value);
		}
		else {
			dict[value.Key] = value;
		}

		return dict[value.Key];
	}

	// Classes facilitating UpdateLocalizationFiles()
	public record LocalizationFile(string path, string prefix, List<LocalizationEntry> LocalizationEntries);

	public record LocalizationEntry(string key, string value, string comment, JsonType type = JsonType.String);

	public class CommentedWscJsonObject : WscJsonObject
	{
		public List<string> CommentedOut { get; private set; }

		public CommentedWscJsonObject()
		{
			CommentedOut = new List<string>();
		}
	}

	internal static void UpdateLocalizationFiles()
	{
		// For each mod with mod sources
		foreach (var mod in ModLoader.Mods) {
			UpdateLocalizationFilesForMod(mod);
		}
	}

	private static void UpdateLocalizationFilesForMod(Mod mod, string outputPath = null, GameCulture specificCulture = null)
	{
		string sourceFolder = outputPath ?? Path.Combine(ModCompile.ModSourcePath, mod.Name);
		if (!Directory.Exists(sourceFolder))
			return;

		// TODO: Maybe optimize to only recently built?

		var baseLocalizationFiles = new List<LocalizationFile>();

		// TODO: This is getting the hjson from the .tmod, should they be coming from Mod Sources? Mod Sources is quicker for organization changes, but usually we rebuild for changes...
		Dictionary<string, string> allLocalizationFileContents = new(); // <full filename , file contents>
		HashSet<GameCulture> foundCultures = new();
		foreach (var translationFile in mod.File.Where(entry => Path.GetExtension(entry.Name) == ".hjson")) {
			using var stream = mod.File.GetStream(translationFile);
			using var streamReader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

			string translationFileContents = streamReader.ReadToEnd();

			(var culture, string prefix) = GameCulture.FromPath(translationFile.Name);

			foundCultures.Add(culture);
			allLocalizationFileContents[translationFile.Name] = translationFileContents;

			// TODO: Support arbitrary default language?
			// Default language hjson files loaded into memory to gather comments and modder intended ordering.
			if (culture == GameCulture.FromCultureName(GameCulture.CultureName.English)) {
				JsonValue jsonValueEng = HjsonValue.Parse(translationFileContents, new HjsonOptions() { KeepWsc = true });
				// Default language files are flattened to a different data structure here to avoid confusing WscJsonObject manipulation with Prefix.AnotherPrefix-type keys and comment preservation.
				List<LocalizationEntry> entries = ParseLocalizationEntries((WscJsonObject)jsonValueEng, prefix);
				baseLocalizationFiles.Add(new(translationFile.Name, prefix, entries));
			}
		}

		// Abort if no default localization files found
		if (!baseLocalizationFiles.Any())
			return;

		// Find and add new content localization keys which are missing from the base (English) localization files
		var baseLocalizationKeys = baseLocalizationFiles.SelectMany(f => f.LocalizationEntries.Select(e => e.key)).ToHashSet();
		foreach (var translation in translations) {
			if (!translation.Key.StartsWith($"Mods.{mod.Name}."))
				continue;

			// Key already exists, no need to find where to put it
			if (baseLocalizationKeys.Contains(translation.Key))
				continue;

			// And then merge key into flattened in-memory model
			LocalizationEntry newEntry = new(translation.Key, translation.Value.GetDefault(), null);
			LocalizationFile suitableHJSONFile = FindBaseHJSONFileForKey(baseLocalizationFiles, newEntry.key);
			AddEntryToHJSON(suitableHJSONFile, newEntry.key, newEntry.value, null);
		}

		IEnumerable<GameCulture> targetCultures = foundCultures;
		if (specificCulture != null)
			targetCultures = new[] {specificCulture};

		// Update target culture lang files based on English
		foreach (var culture in foundCultures) {
			foreach (var baseFile in baseLocalizationFiles) {
				string hjsonContents = LocalizationFileToHjsonText(baseFile, culture);
				string outputFileName = GetPathForCulture(baseFile, culture);

				// Only write if changed
				if (!allLocalizationFileContents.TryGetValue(outputFileName, out string existingFileContents) || existingFileContents != hjsonContents) {
					var outputFilePath = Path.Combine(sourceFolder, outputFileName) + ".new";
					Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath)); // Folder might not exist when using Extract mode
					File.WriteAllText(outputFilePath, hjsonContents);

					// TODO: Indicate on Mods/Mod Sources that localizations have updated maybe?
				}
			}
		}

		// Clean up orphaned non-default language files, if any.
		if (specificCulture != null) {
			var outputPathsForAllLangs = foundCultures.SelectMany(culture => baseLocalizationFiles.Select(baseFile => GetPathForCulture(baseFile, culture))).ToHashSet();
			var orphanedFiles = allLocalizationFileContents.Keys.Except(outputPathsForAllLangs);

			foreach (var name in orphanedFiles) {
				string originalPath = Path.Combine(sourceFolder, name);
				string newPath = originalPath + ".legacy";


				File.Move(originalPath, newPath);
			}
		}
	}

	private static string GetPathForCulture(LocalizationFile file, GameCulture culture) => file.path.Replace("en-US", culture.CultureInfo.Name);

	private static string LocalizationFileToHjsonText(LocalizationFile baseFile, GameCulture culture)
	{
		const int minimumNumberOfEntriesInObject = 1;
		// TODO: Detect string entries that share a key with an object here, convert to "$parentVal" entry. We don't know if a translation key collides until all keys are collected, so here is a suitable place.

		// Count prefixes to determine candidates for non-object output.
		Dictionary<string, int> prefixCounts = new();
		foreach (var entry in baseFile.LocalizationEntries) {
			if (entry.type == JsonType.Object)
				continue;

			string key = GetKeyFromFilePrefixAndEntry(baseFile, entry);
			string[] splitKey = key.Split(".");
			for (int j = 0; j < splitKey.Length; j++) {
				string partialKey = string.Join(".", splitKey.Take(j + 1));
				prefixCounts.TryGetValue(partialKey, out var count);
				prefixCounts[partialKey] = count + 1;
			}
		}

		for (int i = baseFile.LocalizationEntries.Count - 1; i >= 0; i--) {
			var entry = baseFile.LocalizationEntries[i];
			if(entry.type == JsonType.Object) {
				string key = GetKeyFromFilePrefixAndEntry(baseFile, entry);
				if (prefixCounts.TryGetValue(key, out var count) && count <= minimumNumberOfEntriesInObject) {
					// Remove objects with too few children. Should this be ignored if comments exist?
					baseFile.LocalizationEntries.RemoveAt(i);
				}
			}
		}

		var rootObject = new CommentedWscJsonObject();
		// Convert back to JsonObject and write to disk
		foreach (var entry in baseFile.LocalizationEntries) {
			CommentedWscJsonObject parent = rootObject;
			string key = GetKeyFromFilePrefixAndEntry(baseFile, entry);

			// Find/Populate the parents of this translation entry
			string[] splitKey = key.Split(".");
			string finalKey = splitKey[^1];
			for (int j = 0; j < splitKey.Length - 1; j++) {
				string partialKey = string.Join(".", splitKey.Take(j + 1));
				if (prefixCounts.TryGetValue(partialKey, out var count) && count <= minimumNumberOfEntriesInObject) {
					finalKey = string.Join(".", splitKey.Skip(j));
					break;
				}

				string k = splitKey[j];
				if (parent.ContainsKey(k))
					parent = (CommentedWscJsonObject)parent[k];
				else {
					var newParent = new CommentedWscJsonObject();
					parent.Add(k, newParent);
					parent = newParent;
				}
			}

			// TODO: "$parentVal" support?
			// Populate parent object with this translation, manipulating comments to appear above the entry.

			if (entry.value == null && entry.type == JsonType.Object) {
				PlaceCommentAboveNewEntry(entry, parent);
				parent.Add(splitKey[^1], new CommentedWscJsonObject());
			}
			else {
				// Add values
				string value = translations[entry.key].GetTranslation(culture);
				key = splitKey[^1];
				if (culture.Name != "en-US" && value == translations[entry.key].GetDefault()) {
					// This might be messing up Russian: OctopusBanner: "{$CommonItemTooltip.BannerBonus}{$Mods.ExampleMod.NPCName.Octopus}"
					//key = "# " + key; // doesn't work, escaped by escapeName
					parent.CommentedOut.Add(key);
				}

				PlaceCommentAboveNewEntry(entry, parent);
				parent.Add(finalKey, value);
			}
		}

		return rootObject.ToFancyHjsonString();

		static void PlaceCommentAboveNewEntry(LocalizationEntry entry, CommentedWscJsonObject parent)
		{
			if (parent.Count == 0) {
				parent.Comments[""] = entry.comment;
			}
			else {
				string actualCommentKey = parent.Keys.Last();
				parent.Comments[actualCommentKey] = entry.comment;
			}
		}

		static string GetKeyFromFilePrefixAndEntry(LocalizationFile file, LocalizationEntry entry)
		{
			string key = entry.key;
			if (!string.IsNullOrWhiteSpace(file.prefix)) {
				key = key.Substring(file.prefix.Length + 1);
			}

			return key;
		}
	}

	private static List<LocalizationEntry> ParseLocalizationEntries(WscJsonObject jsonObjectEng, string prefix)
	{
		// TODO: How should "$parentVal" be handled?
		// TODO: Which entry should this comment attach to in the result, if it ends up being expanded?
		//       # Some Comment on ExampleMod.Common
		//       ExampleMod.Common: {...}

		var existingKeys = new List<LocalizationEntry>();
		RecurseThrough(jsonObjectEng, prefix);
		return existingKeys;

		void RecurseThrough(WscJsonObject original, string prefix) {
			int index = 0;
			foreach (var item in original) {
				if (item.Value.JsonType == JsonType.Object) {
					var entry = item.Value as WscJsonObject;
					string newPrefix = string.IsNullOrWhiteSpace(prefix) ? item.Key : prefix + "." + item.Key;

					string comment = GetCommentFromIndex(index, original);
					existingKeys.Add(new(newPrefix, null, comment, JsonType.Object));

					RecurseThrough(entry.Qo() as WscJsonObject, newPrefix);
				}
				else if (item.Value.JsonType == JsonType.String) {
					var localizationValue = item.Value.Qs();
					string key = string.IsNullOrWhiteSpace(prefix) ? item.Key : prefix + "." + item.Key;

					string comment = GetCommentFromIndex(index, original);
					existingKeys.Add(new(key, localizationValue, comment));
				}

				index++;
			}
		}

		string GetCommentFromIndex(int index, WscJsonObject original)
		{
			int actualOrderIndex = index - 1;
			string actualCommentKey = actualOrderIndex == -1 ? "" : original.Order[actualOrderIndex];
			string comment = original.Comments[actualCommentKey];
			return comment;
		}
	}

	private static LocalizationFile FindBaseHJSONFileForKey(List<LocalizationFile> files, string key)
	{
		// This method searches through all existing files (for default language) and finds the most suitable file
		// The most suitable file has existing entries that match as much of the "prefix" as possible.
		// If there are multiple files with the prefix, the first is chosen
		// If there are no files, use the default file, create if missing.
		// For non-English, missing files will need to be added to the List.

		int levelFound = -1;
		LocalizationFile mostSuitable = null;

		foreach (var file in files) {
			if (!string.IsNullOrWhiteSpace(file.prefix) && !key.StartsWith(file.prefix))
				continue;

			int level = LongestMatchingPrefix(file.LocalizationEntries, key);
			if (level > levelFound) {
				levelFound = level;
				mostSuitable = file;
			}
		}

		if (mostSuitable == null) {
			// Add a "en-US.hjson" if missing. (or "en-US_Mods.Modname.hjson" instead?)
			// TODO: detect common folder path and use that?
			mostSuitable = new("en-US.hjson", "", new List<LocalizationEntry>());
			files.Add(mostSuitable);
			//throw new Exception("Somehow there are no files for this key");
		}

		return mostSuitable;
	}

	internal static int LongestMatchingPrefix(List<LocalizationEntry> localizationEntries, string key)
	{
		// Returns 0 if no prefix matches, and up to the Key parts length depending on how much is found.

		string[] splitKey = key.Split(".");
		for (int i = 0; i < splitKey.Length; i++) {
			string k = splitKey[i];
			string partialKey = string.Join(".", splitKey.Take(i + 1));

			if (localizationEntries.Any(x => x.key.StartsWith(partialKey)))
				continue;
			else
				return i;
		}
		return splitKey.Length;
	}

	internal static void AddEntryToHJSON(LocalizationFile file, string key, string value, string comment = null)
	{
		var localizationEntries = file.LocalizationEntries;

		// If prefix exists, add to List after most specific prefix
		int index = 0;

		string[] splitKey = key.Split(".");
		for (int i = 0; i < splitKey.Length - 1; i++) {
			string k = splitKey[i];
			string partialKey = string.Join(".", splitKey.Take(i + 1));

			int newIndex = localizationEntries.FindLastIndex(x => x.key.StartsWith(partialKey));
			if (newIndex != -1)
				index = newIndex;
		}

		int placementIndex = file.LocalizationEntries.Count > 0 ? index + 1 : 0;
		file.LocalizationEntries.Insert(placementIndex, new(key, value, comment));
	}

	// Generates hjson files for the current culture in 
	internal static bool ExtractLocalizationFiles(string modName)
	{
		var dir = Path.Combine(Main.SavePath, "ModLocalization", modName);
		if (Directory.Exists(dir))
			Directory.Delete(dir, true);
		Directory.CreateDirectory(dir);

		ModLoader.TryGetMod(modName, out Mod mod);
		if (mod == null) {
			Logging.tML.Error($"Somehow {modName} was not loaded");
			return false;
		}

		UpdateLocalizationFilesForMod(mod, dir, Language.ActiveCulture);
		Utils.OpenFolder(dir);
		return true;
	}
}
