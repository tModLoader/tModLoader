using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terraria.Localization
{
	public partial class LanguageManager
	{
		public List<string> GetKeysInCategory(string categoryName) => _categoryGroupedKeys[categoryName];

		public List<LocalizedText> GetLocalizedEntriesInCategory(string categoryName) {
			List<string> list = GetKeysInCategory(categoryName);
			List<LocalizedText> localizedList = new List<LocalizedText>();
			foreach (string key in list) {
				localizedList = GetText(categoryName + "." + key);
			}
			return localizedList;
		}

		public List<string> GetLocalizedEntriesInCategoryRaw(string categoryName) {
			List<string> list = GetKeysInCategory(categoryName);
			List<string> localizedList = new List<string>();
			foreach (string key in list) {
				localizedList = GetText(categoryName + "." + key).Value;
			}
			return localizedList;
		}
	}
}
