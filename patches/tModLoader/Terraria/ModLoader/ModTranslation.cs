using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Terraria.Localization;

namespace Terraria.ModLoader
{
	[CloneByReference]
	[DebuggerDisplay("{Key}, {GetDefault()}")]
	public class ModTranslation
	{
		private const int fallback = 1;
		public readonly string Key;
		private Dictionary<int, string> translations;

		internal ModTranslation(string key, bool defaultEmpty = false) {
			if (key.Contains(" "))
				throw new Exception("ModTranslation keys can't contain spaces.");
			this.Key = key;
			this.translations = new Dictionary<int, string>();
			this.translations[fallback] = defaultEmpty ? null : key;
		}

		public IEnumerable<GameCulture> GetSupportedCultures()
		{
			return translations.Select(x => GameCulture.FromLegacyId(x.Key));
		}

		public bool SupportsCulture(GameCulture culture) {
			return translations.ContainsKey(culture.LegacyId);
		}

		public void SetDefault(string value) {
			AddTranslation(fallback, value);
		}

		public void AddTranslation(int culture, string value) {
			translations[culture] = value;
		}

		public void AddTranslation(string culture, string value) {
			AddTranslation(GameCulture.FromName(culture).LegacyId, value);
		}

		public void AddTranslation(GameCulture culture, string value) {
			AddTranslation(culture.LegacyId, value);
		}

		public bool IsDefault() {
			return translations[fallback] == Key;
		}

		public string GetDefault() {
			return GetTranslation(fallback);
		}

		public string GetTranslation(int culture) {
			if (translations.ContainsKey(culture)) {
				return translations[culture];
			}
			return translations[fallback];
		}

		public string GetTranslation(string culture) {
			return GetTranslation(GameCulture.FromName(culture).LegacyId);
		}

		public string GetTranslation(GameCulture culture) {
			return GetTranslation(culture.LegacyId);
		}
	}
}
