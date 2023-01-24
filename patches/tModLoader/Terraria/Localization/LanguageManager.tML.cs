using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ReLogic.Content.Sources;
using Terraria.ModLoader;

namespace Terraria.Localization;

public partial class LanguageManager
{
	private IContentSource[] _contentSources = Array.Empty<IContentSource>();
	private HashSet<string> _moddedKeys = new();

	public List<string> GetKeysInCategory(string categoryName) => _categoryGroupedKeys[categoryName];

	public List<string> GetLocalizedEntriesInCategory(string categoryName)
	{
		List<string> list = GetKeysInCategory(categoryName);
		List<string> localizedList = new List<string>();
		foreach (string key in list) {
			localizedList.Add(GetText(categoryName + "." + key).Value);
		}
		return localizedList;
	}

	internal void UnloadModdedEntries()
	{
		foreach (var key in _moddedKeys)
			_localizedTexts.Remove(key);

		_moddedKeys.Clear();

		ResetBoundTexts();
		ReloadLanguage();
	}

	private void ProcessCopyCommandsInTexts()
	{
		// Matches {$key.subkey.etc}
		// Optional @n for arg index remapping, eg {$key.subkey.etc@5} to add 5 to all format arg indices
		Regex referenceRegex = new Regex(@"{\$([\w\.]+)(@\d+)?}", RegexOptions.Compiled);
		Regex argRemappingRegex = new Regex(@"{(\d+)}", RegexOptions.Compiled);

		// Use depth first processing to handle recursive arg mapping substitutions more easily
		var processed = new HashSet<LocalizedText>();
		void Process(LocalizedText text)
		{
			if (!processed.Add(text))
				return; // Already processed, or a recursive reference.

			var newValue = referenceRegex.Replace(text.Value, match => {
				var refText = GetText(FindKeyInScope(match.Groups[1].Value, text.Key));
				Process(refText);

				var repl = refText.Value;
				if (match.Groups[2].Success && int.TryParse(match.Groups[2].Value, out int offset))
					repl = argRemappingRegex.Replace(repl, match => "{" + (int.Parse(match.Groups[1].Value) + offset) + "}");

				return repl;
			});

			text.SetValue(newValue);
		}

		foreach (var text in _localizedTexts.Values)
			Process(text);

		// Provides additional functionality allowing substitutions to assume the scope of the key they belong to.
		// For example, if a key is Mods.ExampleMod.Common.Test with a match, {$Common.PaperAirplane}, Mods.ExampleMod.Common.PaperAirplane if it exists would be matched. All other key variations from each parent key are checked. 
		string FindKeyInScope(string key, string scope)
		{
			if (Exists(key))
				return key;

			string[] splitKey = scope.Split(".");
			for (int j = splitKey.Length - 1; j >= 0; j--) {
				string partialKey = string.Join(".", splitKey.Take(j + 1));
				string combinedKey = partialKey + "." + key;
				if (Exists(combinedKey))
					return combinedKey;
			}

			return key;
		}
	}

	#region "Text Binding"
	private struct TextBinding : IEquatable<TextBinding>
	{
		public readonly string key;
		public readonly object[] args;

		public TextBinding(string key, object[] args)
		{
			this.key = key;
			this.args = args;
		}

		public bool Equals(TextBinding other) => key == other.key && args.SequenceEqual(other.args);

		public override bool Equals(object obj) => obj is TextBinding && Equals((TextBinding)obj);

		public override int GetHashCode()
		{
			var hash = new HashCode();
			hash.Add(key);
			foreach (var arg in args)
				hash.Add(arg);

			return hash.ToHashCode();
}
	}

	private Dictionary<TextBinding, LocalizedText> boundTextCache = new();
	private List<(TextBinding, LocalizedText)> boundTexts = new();

	internal void ResetBoundTexts()
	{
		boundTextCache.Clear();
		boundTexts.Clear();
	}

	internal LocalizedText BindFormatArgs(string key, object[] args)
	{
		var binding = new TextBinding(key, args);
		if (boundTextCache.TryGetValue(binding, out var text))
			return text;

		text = new LocalizedText(key, ComputeBoundTextValue(binding));
		boundTextCache[binding] = text;
		boundTexts.Add((binding, text));
		return text;
	}

	internal void RecalculateBoundTextValues()
	{
		foreach (var (binding, text) in boundTexts)
			text.SetValue(ComputeBoundTextValue(binding));
	}

	private string ComputeBoundTextValue(TextBinding binding)
	{
		var value = GetTextValue(binding.key);

		// TODO, consider if we should do partial binding, shifting the higher args down
		return string.Format(value, binding.args);
	}
	#endregion

	private const int defaultWatcherCooldown = 15;
	private Dictionary<Mod, FileSystemWatcher> localizationFileWatchers = new();
	internal HashSet<string> changedFiles = new();
	internal HashSet<string> pendingFiles = new();
	internal int watcherCooldown;
	internal void SetupFileWatchers()
	{
		// Add a watcher for each loaded mod that has a corresponding mod sources folder
		// Don't worry about the mod being local or not, for now. The feature might be useful for even workshop tmod files
		foreach (var mod in ModLoader.ModLoader.Mods) {
			string path = Path.Combine(ModLoader.Core.ModCompile.ModSourcePath, mod.Name);
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

	internal void UnloadFileWatchers()
	{
		foreach (var fileWatcher in localizationFileWatchers.Values) {
			fileWatcher.EnableRaisingEvents = false;
			fileWatcher.Dispose();
		}
		localizationFileWatchers.Clear();
	}

	private void HandleFileChangedOrRenamed(string modName, string fileName)
	{
		watcherCooldown = defaultWatcherCooldown;
		pendingFiles.Add(Path.Combine(modName, fileName));
	}

	internal void Update()
	{
		// Saving a file in some programs trigger the file multiple times. A cooldown allows tmod to wait until file is finished being changed.
		if (watcherCooldown > 0) {
			watcherCooldown--;
			if (watcherCooldown == 0) {
				string newText = Language.GetTextValue("tModLoader.WatchLocalizationFileMessage", string.Join(", ", pendingFiles));
				Utils.LogAndChatAndConsoleInfoMessage(newText);

				changedFiles.UnionWith(pendingFiles);
				pendingFiles.Clear();

				LanguageManager.Instance.ReloadLanguage();
			}
		}
	}
}
