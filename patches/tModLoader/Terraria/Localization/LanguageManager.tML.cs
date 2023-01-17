using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
