using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Terraria.Localization;

public partial class LanguageManager
{
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
		/* Alternate approach that won't reload all localization files, but might miss out on unloading some vanilla text changes.
		var toRemove = LanguageManager.Instance._localizedTexts.Where(entry => entry.Key.StartsWith("Mods."))
						 .Select(pair => pair.Key)
						 .ToList();
		foreach (var key in toRemove) {
			LanguageManager.Instance._localizedTexts.Remove(key);
		}
		*/

		LanguageManager.Instance._localizedTexts.Clear();
		if (LanguageManager.Instance.ActiveCulture != _fallbackCulture) {
			SetAllTextValuesToKeys();
			LanguageManager.Instance.LoadLanguage(_fallbackCulture);
		}
		LanguageManager.Instance.LoadLanguage(LanguageManager.Instance.ActiveCulture);
		Lang.InitializeLegacyLocalization();
		//Main.AssetSourceController.Refresh();
		// Issue: Current implementation doesn't reload resource packs. LoadLanguage doesn't load resource packs, and neither does vanilla when switching languages. UseSources also calls LoadLanguage itself. Main thread error if Refresh is called here since this might be called in Unload. Revisit once vanilla issue is resolved to get everything loading in all situations.

		ResetBoundTexts();
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
}
