using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
		LanguageManager.Instance.LoadLanguage(LanguageManager.Instance.ActiveCulture);
		//Main.AssetSourceController.Refresh();
		// Issue: Current implementation doesn't reload resource packs. LoadLanguage doesn't load resource packs, and neither does vanilla when switching languages. UseSources also calls LoadLanguage itself. Main thread error if Refresh is called here since this might be called in Unload. Revisit once vanilla issue is resolved to get everything loading in all situations.
	}
}
