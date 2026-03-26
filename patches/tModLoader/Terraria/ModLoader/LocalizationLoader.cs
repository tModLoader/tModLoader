using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Hjson;
using Newtonsoft.Json.Linq;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.Utilities;

namespace Terraria.ModLoader;

public static class LocalizationLoader
{
	internal static void Autoload(Mod mod)
	{
		var lang = LanguageManager.Instance;
		var gameTipPrefix = $"Mods.{mod.Name}.GameTips.";

		foreach (var (key, _) in LoadTranslations(mod.File, GameCulture.DefaultCulture)) {
			var text = lang.GetOrRegister(key); // adds the key but leaves it untranslated for now.

			if (key.StartsWith(gameTipPrefix))
				Main.gameTips.allTips.Add(new GameTipData(text, mod));
		}
	}

	public static void LoadModTranslations(GameCulture culture)
	{
		var lang = LanguageManager.Instance;
		foreach (var mod in ModLoader.Mods) {
			foreach (var (key, value) in LoadTranslations(mod.File, culture)) {
				lang.GetText(key).SetValue(value); // can only set the value of existing keys. Cannot register new keys.
			}
		}
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

	[Obsolete($"Use ${nameof(TryGetCultureAndPrefixFromPath)} instead.", error: true)]
	public static (GameCulture culture, string prefix) GetCultureAndPrefixFromPath(string path)
	{
		if (TryGetCultureAndPrefixFromPath(path, out var culture, out string prefix))
			return (culture, prefix);

		return (GameCulture.DefaultCulture, string.Empty);
	}

	/// <summary>
	/// Derives a culture and shared prefix from a localization file path. Prefix will be found after culture, either separated by an underscore or nested in the folder.
	/// <br/> Some examples:<code>
	/// Localization/en-US_Mods.ExampleMod.hjson
	/// Localization/en-US/Mods.ExampleMod.hjson
	/// en-US_Mods.ExampleMod.hjson
	/// en-US/Mods.ExampleMod.hjson
	/// </code>
	/// </summary>
	/// <param name="path"></param>
	/// <param name="culture"></param>
	/// <param name="prefix"></param>
	/// <returns></returns>
	#nullable enable
	public static bool TryGetCultureAndPrefixFromPath(string path, [NotNullWhen(true)] out GameCulture? culture, [NotNullWhen(true)] out string? prefix)
	#nullable disable
	{
		path = Path.ChangeExtension(path, null);
		path = path.Replace("\\", "/");

		culture = null;
		prefix = null;

		string[] splitByFolder = path.Split("/");
		foreach (var pathPart in splitByFolder) {
			string[] splitByUnderscore = pathPart.Split("_");
			for (int underscoreSplitIndex = 0; underscoreSplitIndex < splitByUnderscore.Length; underscoreSplitIndex++) {
				string underscorePart = splitByUnderscore[underscoreSplitIndex];
				GameCulture parsedCulture = GameCulture.KnownCultures.FirstOrDefault(culture => culture.Name == underscorePart);
				if (parsedCulture != null) {
					culture = parsedCulture;
					continue;
				}
				if (parsedCulture == null && culture != null) {
					prefix = string.Join("_", splitByUnderscore.Skip(underscoreSplitIndex)); // Some mod names have '_' in them
					return true;
				}
			}
		}

		if (culture != null) {
			prefix = string.Empty;
			return true;
		}

		/*
		string[] split = path.Split("/");
		for (int index = split.Length - 1; index >= 0; index--) {
			string pathPart = split[index];
			GameCulture culture = _legacyCultures.Values.FirstOrDefault(culture => culture.Name == pathPart);
			if (culture != null)
				return culture;
		}
		*/

		return false;
	}

	private static List<(string key, string value)> LoadTranslations(Mod mod, GameCulture culture) => LoadTranslations(mod.File, culture);

	private static List<(string key, string value)> LoadTranslations(TmodFile tModFile, GameCulture culture)
	{
		if (tModFile == null)
			return new();

		var properties = BuildProperties.ReadModFile(tModFile);
		string sourceFolder = Directory.Exists(properties.modSource) ? properties.modSource : "";

		try {
			// Flatten JSON into dot separated key and value
			var flattened = new List<(string, string)>();

			foreach (var translationFile in tModFile.Where(entry => Path.GetExtension(entry.Name) == ".hjson")) {
				if (!TryGetCultureAndPrefixFromPath(translationFile.Name, out var fileCulture, out string prefix))
					continue;

				if (fileCulture != culture)
					continue;

				using var stream = tModFile.GetStream(translationFile);
				using var streamReader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

				string translationFileContents = streamReader.ReadToEnd();

				string modpath = Path.Combine(tModFile.Name, translationFile.Name).Replace('/', '\\');
				if (!string.IsNullOrWhiteSpace(sourceFolder) && changedFiles.Select(x => Path.Join(x.Mod, x.fileName).Replace('/', '\\')).Contains(modpath)) {
					// TODO: we could skip this for GetLocalizationCounts to be more accurate to the entries in the mod itself, but that is not needed since we don't allow publishing on the client if any changedFiles and the command line publish won't detect changedFiles unless hosting.
					string path = Path.Combine(sourceFolder, translationFile.Name);
					if (File.Exists(path)) {
						try {
							translationFileContents = File.ReadAllText(path);
						}
						catch (Exception) {
						}
					}
				}

				// Parse HJSON and convert to standard JSON
				string jsonString;
				try {
					jsonString = HjsonValue.Parse(translationFileContents).ToString();
				}
				catch (Exception e) {
					string additionalContext = "";
					if (e is ArgumentException && Regex.Match(e.Message, "At line (\\d+),") is Match { Success: true } match && int.TryParse(match.Groups[1].Value, out int line)) {
						string[] lines = translationFileContents.Replace("\r", "").Replace("\t", "    ").Split('\n');
						int start = Math.Max(0, line - 4);
						int end = Math.Min(lines.Length, line + 3);
						var linesOutput = new StringBuilder();
						for (int i = start; i < end; i++) {
							if (line - 1 == i)
								linesOutput.Append($"\n{i + 1}[c/ff0000:>" + lines[i] + "]");
							else
								linesOutput.Append($"\n{i + 1}:" + lines[i]);
						}
						additionalContext = "\nContext:" + linesOutput.ToString();
					}
					throw new Exception($"The localization file \"{translationFile.Name}\" is malformed and failed to load:{additionalContext} ", e);
				}

				// Parse JSON
				var jsonObject = JObject.Parse(jsonString);

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

					// removing instances of .$parentVal is an easy way to make this special key assign its value
					//  to the parent key instead (needed for some cases of .lang -> .hjson auto-conversion)
					path = path.Replace(".$parentVal", "");
					if (!string.IsNullOrWhiteSpace(prefix))
						path = prefix + "." + path;

					flattened.Add((path, t.ToString()));
				}
			}

			return flattened;
		}
		catch (Exception e) {
			e.Data["mod"] = tModFile.Name;
			throw;
		}
	}

	// Classes facilitating UpdateLocalizationFiles()
	public record LocalizationFile(string path, string prefix, List<LocalizationEntry> Entries);

	public record LocalizationEntry(string key, string value, string comment, JsonType type = JsonType.String);

	public class CommentedWscJsonObject : WscJsonObject
	{
		public List<string> CommentedOut { get; private set; }

		public CommentedWscJsonObject()
		{
			CommentedOut = new List<string>();
		}
	}

	internal static void FinishSetup()
	{
		UpdateLocalizationFiles();
		SetupFileWatchers();
	}

	internal static void UpdateLocalizationFiles()
	{
		// For each mod with mod sources
		foreach (var mod in ModLoader.Mods) {
			try {
				UpdateLocalizationFilesForMod(mod);
			}
			catch (Exception e) {
				e.Data["mod"] = mod.Name;
				throw;
			}
		}
	}

	private static void UpdateLocalizationFilesForMod(Mod mod, string outputPath = null, GameCulture specificCulture = null)
	{
		// ModLoaderMod does not exist on disk and is not applicable.
		if (mod.File == null)
			return;

		var desiredCultures = new HashSet<GameCulture>();
		if (specificCulture != null)
			desiredCultures.Add(specificCulture);

		var mods = new List<Mod> {
			mod
		};

		// TODO: Maybe optimize to only recently built?
		string sourceFolder = outputPath ?? mod.SourceFolder;
		if (!Directory.Exists(sourceFolder))
			return;

		// Only update if mod is a locally built mod (not in workshop folder), to mitigate modders testing published versions of their mods removing text from their source folder
		string localBuiltTModFile = Path.Combine(ModLoader.ModPath, mod.Name + ".tmod"); // LocalMod not available in Mod class.
		if (outputPath == null && !File.Exists(localBuiltTModFile))
			return;

		DateTime modLastModified = File.GetLastWriteTime(mod.File.path);

		if (mod.TranslationForMods != null) {
			foreach (var translatedMod in mod.TranslationForMods) {
				ModLoader.TryGetMod(translatedMod, out Mod otherMod);
				if (otherMod == null) {
					// Skip Update since a mod is missing somehow.
					// TODO: Does this make sense? Do we need to skip just because a weak reference mod is missing? (Such as an Addon mod that this mod also translates?)
					return;
				}
				mods.Add(otherMod);

				// Use the newest of the mods to determine if localization files should update.
				DateTime otherModLastModified = File.GetLastWriteTime(otherMod.File.path);
				if (otherModLastModified > modLastModified) {
					modLastModified = otherModLastModified;
				}

				// In case of conflicts, how do we inherit comments?
				// Comments from the other mod should take priority usually.
				// Comments on Mods are used for credits, should we ignore that specifically?
			}
		}

		Dictionary<GameCulture, List<LocalizationFile>> localizationFilesByCulture = new();
		Dictionary<string, string> localizationFileContentsByPath = new(); // <full filename , file contents>. Actual files for this mod.

		// TODO: This is getting the hjson from the .tmod, should they be coming from Mod Sources? Mod Sources is quicker for organization changes, but usually we rebuild for changes...
		foreach (var inputMod in mods) {
			foreach (var translationFile in inputMod.File.Where(entry => Path.GetExtension(entry.Name) == ".hjson")) {
				if (!TryGetCultureAndPrefixFromPath(translationFile.Name, out var culture, out string prefix))
					continue;

				using var stream = inputMod.File.GetStream(translationFile);
				using var streamReader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

				string translationFileContents = streamReader.ReadToEnd();
				string fixedFileName = translationFile.Name;
				if (culture == GameCulture.DefaultCulture && !fixedFileName.Contains("en-US")) {
					fixedFileName = Path.Combine(Path.GetDirectoryName(fixedFileName), "en-US.hjson").Replace("\\", "/");
				}

				if (!localizationFilesByCulture.TryGetValue(culture, out var fileList))
					localizationFilesByCulture[culture] = fileList = new();

				if (inputMod == mod) {
					desiredCultures.Add(culture);

					// Check translationFile.Name instead of fixedFileName since this is used for modified and file cleanup.
					if (!localizationFileContentsByPath.ContainsKey(translationFile.Name))
						localizationFileContentsByPath[translationFile.Name] = translationFileContents;
					// If the file exists, it's from a supplementary mod, so the original file contents should be used for checks.
				}

				JsonValue jsonValueEng;
				try {
					jsonValueEng = HjsonValue.Parse(translationFileContents, new HjsonOptions() { KeepWsc = true });
				}
				catch (Exception e) {
					throw new Exception($"The localization file \"{translationFile.Name}\" is malformed and failed to load: ", e);
				}

				// Language files are flattened to a different data structure here to avoid confusing WscJsonObject manipulation with Prefix.AnotherPrefix-type keys and comment preservation.
				var entries = ParseLocalizationEntries((WscJsonObject)jsonValueEng, prefix);
				if (!fileList.Any(x => x.path == fixedFileName)) {
					fileList.Add(new(fixedFileName, prefix, entries));
				}
				else {
					// If file exists, then we are merging.
					// Resulting entries will have new entries added
					// Comments will be taken from 1st loaded English
					LocalizationFile localizationFile = fileList.First(x => x.path == fixedFileName);
					foreach (var entry in entries) {
						if (!localizationFile.Entries.Exists(x => x.key == entry.key)) {
							localizationFile.Entries.Add(entry);
						}
					}
				}
			}
		}

		// If no default localization files found, make one in the preferred path and prefix
		if (!localizationFilesByCulture.TryGetValue(GameCulture.DefaultCulture, out var baseLocalizationFiles)) {
			localizationFilesByCulture[GameCulture.DefaultCulture] = baseLocalizationFiles = new();
			desiredCultures.Add(GameCulture.DefaultCulture);

			string prefix = $"Mods.{mod.Name}";
			string translationFileName = $"Localization/en-US_{prefix}.hjson";
			baseLocalizationFiles.Add(new(translationFileName, prefix, new List<LocalizationEntry>()));
		}

		// Remove duplicates. Only remove string entries. Remove from longest filename.
		// TODO: could combine comments to remaining entry. Also consider removing empty objects somewhere.
		var duplicates = baseLocalizationFiles.SelectMany(f => f.Entries).Where(w => w.type == JsonType.String).GroupBy(x => x.key).Where(c => c.Count() > 1).ToDictionary(g => g.Key, g => g.ToList());
		foreach (var baseLocalizationFile in baseLocalizationFiles.OrderByDescending(x => x.path.Length)) {
			var toRemove = new List<LocalizationEntry>();
			foreach (var entry in baseLocalizationFile.Entries) {
				if (duplicates.ContainsKey(entry.key)) {
					duplicates.Remove(entry.key);
					toRemove.Add(entry);
				}
			}
			foreach (var entry in toRemove) {
				baseLocalizationFile.Entries.Remove(entry);
			}
		}

		// Find and add new content localization keys which are missing from the base (English) localization files
		var baseLocalizationKeys = baseLocalizationFiles.SelectMany(f => f.Entries.Select(e => e.key)).ToHashSet();
		foreach (var translation in LanguageManager.Instance._localizedTexts.Values) {
			if (!translation.Key.StartsWith($"Mods.{mod.Name}."))
				continue;

			// Key already exists, no need to find where to put it
			if (baseLocalizationKeys.Contains(translation.Key))
				continue;

			// And then merge key into flattened in-memory model
			LocalizationEntry newEntry = new(translation.Key, translation.Value, comment: null);
			LocalizationFile suitableHJSONFile = FindHJSONFileForKey(baseLocalizationFiles, newEntry.key);
			AddEntryToHJSON(suitableHJSONFile, newEntry.key, newEntry.value, null);
		}

		IEnumerable<GameCulture> targetCultures = desiredCultures.ToList();
		if (specificCulture != null) {
			targetCultures = new[] { specificCulture };
			if (!localizationFilesByCulture.TryGetValue(specificCulture, out var fileList))
				localizationFilesByCulture[specificCulture] = fileList = new();
		}

		// Update target culture lang files based on English
		foreach (var culture in targetCultures) {
			IEnumerable<LocalizationEntry> localizationEntriesForCulture = localizationFilesByCulture[culture].SelectMany(f => f.Entries);
			Dictionary<string, string> localizationsForCulture = new();
			foreach (var localizationEntry in localizationEntriesForCulture) {
				if (localizationEntry.value != null) {
					string key = localizationEntry.key;
					if (key.EndsWith(".$parentVal")) {
						key = key.Replace(".$parentVal", "");
					}
					localizationsForCulture[key] = localizationEntry.value;
				}
			}

			foreach (var baseFile in baseLocalizationFiles) {
				string hjsonContents = LocalizationFileToHjsonText(baseFile, localizationsForCulture).ReplaceLineEndings(); // need to compare with same line endings, as Git and OS will affect actual line endings.
				string outputFileName = GetPathForCulture(baseFile, culture);

				// Only write if file doesn't exist or if file has changed and .tmod file is newer than existing file.
				// File Modified date check allows edits to English files to be propagated with a build and reload without being accidentally reverted when tmod is launched.
				// Also write out if specificCulture isn't null. This is true when UpdateLocalizationFilesForMod is calling this method.
				var outputFilePath = Path.Combine(sourceFolder, outputFileName) /*+ ".new"*/;
				DateTime dateTime = File.GetLastWriteTime(outputFilePath);
				if (!localizationFileContentsByPath.TryGetValue(outputFileName, out string existingFileContents) || existingFileContents.ReplaceLineEndings() != hjsonContents && dateTime < modLastModified || specificCulture != null) {
					Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath)); // Folder might not exist when using Extract mode
					File.WriteAllText(outputFilePath, hjsonContents);
					changedMods.Add(mod.Name);
				}
			}
		}

		// Clean up orphaned language files, if any. This should remove any hjson not present in English, and any English files without "en-US"
		var outputPathsForAllLangs = localizationFilesByCulture.Keys.SelectMany(culture => baseLocalizationFiles.Select(baseFile => GetPathForCulture(baseFile, culture))).ToHashSet();
		var orphanedFiles = localizationFileContentsByPath.Keys.Except(outputPathsForAllLangs);

		foreach (var name in orphanedFiles) {
			string originalPath = Path.Combine(sourceFolder, name);
			string newPath = originalPath + ".legacy";

			if (File.Exists(originalPath)) { // File might have already been deleted
				Logging.tML.Warn($"The .hjson file \"{originalPath}\" was detected as a localization file but doesn't match the filename of any of the English template files. The file will be renamed to \"{newPath}\" and its contents will not be loaded. You should update the English template files or move these localization entries to a correctly named file to allow them to load.");
				File.Move(originalPath, newPath);
			}
		}

		// Update LocalizationCounts and optionally TranslationsNeeded.txt
		if (specificCulture == null) {
			var localizationCounts = new Dictionary<GameCulture, int>();
			foreach (var culture in targetCultures) {
				var localizationEntries = localizationFilesByCulture[culture].SelectMany(f => f.Entries).ToList();
				// Only count only non-"" entries. Also ignore entries that are just substitutions.
				int countNonTrivialEntries = localizationEntries.Where(x => HasTextThatNeedsLocalization(x.value)).Count();
				localizationCounts.Add(culture, countNonTrivialEntries);
			}
			localizationEntriesCounts[mod.Name] = localizationCounts;

			string translationsNeededPath = Path.Combine(sourceFolder, "Localization", "TranslationsNeeded.txt");
			if (File.Exists(translationsNeededPath)) {
				int countMaxEntries = localizationCounts.DefaultIfEmpty().Max(x => x.Value);
				string neededText = string.Join(Environment.NewLine, localizationCounts.OrderBy(x => x.Key.LegacyId).Select(x => $"{x.Key.Name}, {x.Value}/{countMaxEntries}, {(float)x.Value / countMaxEntries:0%}, missing {countMaxEntries - x.Value}")) + Environment.NewLine;
				if (File.ReadAllText(translationsNeededPath).ReplaceLineEndings() != neededText.ReplaceLineEndings()) {
					File.WriteAllText(translationsNeededPath, neededText);
				}
			}
		}
	}

	private static string GetPathForCulture(LocalizationFile file, GameCulture culture) => file.path.Replace("en-US", culture.CultureInfo.Name);

	private static string LocalizationFileToHjsonText(LocalizationFile baseFile, Dictionary<string, string> localizationsForCulture)
	{
		const int minimumNumberOfEntriesInObject = 1;

		// Count prefixes to determine candidates for non-object output.
		Dictionary<string, int> prefixCounts = new();
		foreach (var entry in baseFile.Entries) {
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

		for (int i = baseFile.Entries.Count - 1; i >= 0; i--) {
			var entry = baseFile.Entries[i];
			if (entry.type == JsonType.Object) {
				string key = GetKeyFromFilePrefixAndEntry(baseFile, entry);
				if (prefixCounts.TryGetValue(key, out var count) && count <= minimumNumberOfEntriesInObject) {
					// Remove objects with too few children. Should this be ignored if comments exist?
					baseFile.Entries.RemoveAt(i);
				}
			}
			if (entry.type == JsonType.String) {
				// We don't know if a translation key collides until all keys are collected, convert to "$parentVal" entry if any other entry shares the prefix
				string key = GetKeyFromFilePrefixAndEntry(baseFile, entry);
				if (prefixCounts.TryGetValue(key, out var count) && count > 1) {
					baseFile.Entries[i] = entry with { key = entry.key + ".$parentVal" };
					// Note: Editing baseFile changes English as well. Undone when localizationsForCulture calculated populated
				}
			}
		}

		var rootObject = new CommentedWscJsonObject();
		// Convert back to JsonObject and write to disk
		foreach (var entry in baseFile.Entries) {
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

			// Populate parent object with this translation, manipulating comments to appear above the entry.

			if (entry.value == null && entry.type == JsonType.Object) {
				if (!parent.ContainsKey(splitKey[^1])) {
					PlaceCommentAboveNewEntry(entry, parent);
					parent.Add(splitKey[^1], new CommentedWscJsonObject());
				}
			}
			else {
				// Add values
				string realKey = entry.key.Replace(".$parentVal", "");
				if (!localizationsForCulture.TryGetValue(realKey, out var value)) {
					parent.CommentedOut.Add(finalKey);
					value = entry.value;
				}

				PlaceCommentAboveNewEntry(entry, parent);
				parent.Add(finalKey, value);
			}
		}

		return rootObject.ToFancyHjsonString() + Environment.NewLine;

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

		static string GetKeyFromFilePrefixAndEntry(LocalizationFile baseLocalizationFileEntry, LocalizationEntry entry)
		{
			string key = entry.key;
			if (!string.IsNullOrWhiteSpace(baseLocalizationFileEntry.prefix)) {
				key = key.Substring(baseLocalizationFileEntry.prefix.Length + 1);
			}

			return key;
		}
	}

	private static List<LocalizationEntry> ParseLocalizationEntries(WscJsonObject jsonObjectEng, string prefix)
	{
		var existingKeys = new List<LocalizationEntry>();
		RecurseThrough(jsonObjectEng, prefix);
		return existingKeys;

		void RecurseThrough(WscJsonObject original, string prefix)
		{
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
					if (key.EndsWith(".$parentVal")) {
						key = key.Replace(".$parentVal", "");
					}
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

	private static LocalizationFile FindHJSONFileForKey(List<LocalizationFile> files, string key)
	{
		// This method searches through all existing files (for default language) and finds the most suitable file
		// The most suitable file has existing entries that match as much of the "prefix" as possible.
		// If there are multiple files with the prefix, the first is chosen
		// If there are no files, use the default file, create if missing.
		// For non-English, missing files will need to be added to the List.

		int levelFound = -1;
		LocalizationFile best = null;

		foreach (var file in files) {
			if (!string.IsNullOrWhiteSpace(file.prefix) && !key.StartsWith(file.prefix))
				continue;

			int level = LongestMatchingPrefix(file, key);
			if (level > levelFound) {
				levelFound = level;
				best = file;
			}
		}

		if (best == null) {
			// Add a "en-US.hjson" if missing. (or "en-US_Mods.Modname.hjson" instead?)
			// TODO: detect common folder path and use that?
			best = new("en-US.hjson", "", new List<LocalizationEntry>());
			files.Add(best);
			//throw new Exception("Somehow there are no files for this key");
		}

		return best;
	}

	internal static int LongestMatchingPrefix(LocalizationFile file, string key)
	{
		// Returns 0 if no prefix matches, and up to the Key parts length depending on how much is found.
		int start = string.IsNullOrWhiteSpace(file.prefix) ? 0 : file.prefix.Split(".").Length;
		List<LocalizationEntry> localizationEntries = file.Entries;
		string[] splitKey = key.Split(".");
		for (int i = start; i < splitKey.Length; i++) {
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
		// If prefix exists, add to List after most specific prefix
		int index = 0;

		string[] splitKey = key.Split(".");
		for (int i = 0; i < splitKey.Length - 1; i++) {
			string k = splitKey[i];
			string partialKey = string.Join(".", splitKey.Take(i + 1));

			int newIndex = file.Entries.FindLastIndex(x => x.key.StartsWith(partialKey));
			if (newIndex != -1)
				index = newIndex;
		}

		int placementIndex = file.Entries.Count > 0 ? index + 1 : 0;
		file.Entries.Insert(placementIndex, new(key, value, comment));
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

	private static readonly Dictionary<string, Dictionary<GameCulture, int>> localizationEntriesCounts = new();
	internal static Dictionary<GameCulture, int> GetLocalizationCounts(TmodFile tModFile)
	{
		if (localizationEntriesCounts.TryGetValue(tModFile.Name, out var results)) {
			return results;
		}

		results = new Dictionary<GameCulture, int>();
		foreach (var culture in GameCulture.KnownCultures) {
			var localizationEntries = LoadTranslations(tModFile, culture);
			// Only count only non-"" entries. Also ignore entries that are just substitutions.
			int countNonTrivialEntries = localizationEntries.Where(x => HasTextThatNeedsLocalization(x.value)).Count();
			results.Add(culture, countNonTrivialEntries);
		}
		localizationEntriesCounts[tModFile.Name] = results;
		return results;
	}

	private static Regex referenceRegex = new Regex(@"{\$([\w\.]+)(?:@(\d+))?}", RegexOptions.Compiled); // copied from ProcessCopyCommandsInTexts method
	private static bool HasTextThatNeedsLocalization(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
			return false;

		string final = referenceRegex.Replace(value, "");
		if (string.IsNullOrWhiteSpace(final))
			return false;

		return true;
	}

	private const int defaultWatcherCooldown = 60;
	private static readonly Dictionary<Mod, FileSystemWatcher> localizationFileWatchers = new();
	private static readonly HashSet<(string Mod, string fileName)> changedFiles = new();
	private static readonly HashSet<(string Mod, string fileName)> pendingFiles = new();
	internal static readonly HashSet<string> changedMods = new();
	private static int watcherCooldown;
	private static void SetupFileWatchers()
	{
		// Add a watcher for each loaded mod that has a corresponding mod sources folder
		// Don't worry about the mod being local or not, for now. The feature might be useful for even workshop tmod files
		foreach (var mod in ModLoader.Mods) {
			// ModLoaderMod does not exist on disk and is not applicable.
			if (mod.File == null)
				continue;

			string path = mod.SourceFolder;
			if (!Directory.Exists(path))
				continue;

			try {
				var localizationFileWatcher = new FileSystemWatcher();
				localizationFileWatcher.Path = path;
				localizationFileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
				localizationFileWatcher.Filter = "*.hjson";
				localizationFileWatcher.IncludeSubdirectories = true;

				localizationFileWatcher.Changed += (a, b) => {
					HandleFileChangedOrRenamed(mod.Name, b.Name);
				};
				localizationFileWatcher.Renamed += (a, b) => {
					HandleFileChangedOrRenamed(mod.Name, b.Name);
				};

				// Begin watching.
				localizationFileWatcher.EnableRaisingEvents = true;

				localizationFileWatchers[mod] = localizationFileWatcher;
			}
			catch (Exception) {
				throw;
			}
		}
	}

	internal static void Unload()
	{
		LanguageManager.Instance.UnloadModdedEntries();
		UnloadFileWatchers();
	}

	private static void UnloadFileWatchers()
	{
		foreach (var fileWatcher in localizationFileWatchers.Values) {
			fileWatcher.EnableRaisingEvents = false;
			fileWatcher.Dispose();
		}
		localizationFileWatchers.Clear();
	}

	private static void HandleFileChangedOrRenamed(string modName, string fileName)
	{
		// Ignore non-localization files
		if (!TryGetCultureAndPrefixFromPath(fileName, out _, out _))
			return;

		watcherCooldown = defaultWatcherCooldown;
		lock (pendingFiles) {
			pendingFiles.Add((modName, fileName));
		}
	}

	internal static void HandleModBuilt(string modName)
	{
		changedMods.Remove(modName);
		changedFiles.RemoveWhere(x => x.Mod == modName);
	}

	internal static void Update()
	{
		// Saving a file in some programs trigger the file multiple times. A cooldown allows tmod to wait until file is finished being changed.
		if (watcherCooldown <= 0)
			return;

		watcherCooldown--;
		if (watcherCooldown != 0)
			return;

		lock (pendingFiles) {
			string newText = Language.GetTextValue("tModLoader.WatchLocalizationFileMessage", string.Join(", ", pendingFiles.Select(x => Path.Join(x.Mod, x.fileName))));
			Utils.LogAndChatAndConsoleInfoMessage(newText);
		}

		lock (pendingFiles) {
			changedMods.UnionWith(pendingFiles.Select(x => x.Mod));
			changedFiles.UnionWith(pendingFiles);
			pendingFiles.Clear();
		}

		LanguageManager.Instance.ReloadLanguage();
	}
}
