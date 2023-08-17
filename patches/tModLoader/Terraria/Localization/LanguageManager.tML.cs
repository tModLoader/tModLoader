using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ReLogic.Content.Sources;

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
		Regex referenceRegex = new Regex(@"{\$([\w\.]+)(?:@(\d+))?}", RegexOptions.Compiled);
		// The arg remapping regex matches both {0} and the pluralization pattern "{^0:item;items}" via positive lookbehind and lookahead
		Regex argRemappingRegex = new Regex(@"(?<={\^?)(\d+)(?=(?::[^\r\n]+?)?})", RegexOptions.Compiled);

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
					repl = argRemappingRegex.Replace(repl, match => (int.Parse(match.Groups[1].Value) + offset).ToString());

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
	private List<LocalizedText> boundTexts = new();

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

		text = new LocalizedText(key, GetTextValue(key));
		text.BindArgs(args);

		boundTextCache[binding] = text;
		boundTexts.Add(text);
		return text;
	}

	internal void RecalculateBoundTextValues()
	{
		foreach (var text in boundTexts) {
			var args = text.BoundArgs;
			text.SetValue(GetTextValue(text.Key));
			text.BindArgs(args);
		}
	}
	#endregion
}
