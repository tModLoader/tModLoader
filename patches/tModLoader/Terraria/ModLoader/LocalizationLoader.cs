﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader
{
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
		public static void AddTranslation(ModTranslation translation) {
			translations[translation.Key] = translation;
		}

		internal static ModTranslation GetOrCreateTranslation(Mod mod, string key, bool defaultEmpty = false)
			=> GetOrCreateTranslation($"Mods.{mod.Name}.{key}", defaultEmpty);

		internal static ModTranslation GetOrCreateTranslation(string key, bool defaultEmpty = false) {
			key = key.Replace(" ", "_");

			if (translations.TryGetValue(key, out var translation))
				return translation;

			return new ModTranslation(key, defaultEmpty);
		}

		internal static void Autoload(Mod mod) {
			if (mod.File == null)
				return;

			var modTranslationDictionary = new Dictionary<string, ModTranslation>();

			foreach (var translationFile in mod.File.Where(entry => Path.GetExtension(entry.Name) == ".lang")) {
				// .lang files need to be UTF8 encoded.
				string translationFileContents = Encoding.UTF8.GetString(mod.File.GetBytes(translationFile));
				var culture = GameCulture.FromName(Path.GetFileNameWithoutExtension(translationFile.Name));

				using StringReader reader = new StringReader(translationFileContents);

				string line;
				
				while ((line = reader.ReadLine()) != null) {
					int split = line.IndexOf('=');
					if (split < 0)
						continue; // lines witout a = are ignored

					string key = line.Substring(0, split).Trim().Replace(" ", "_");
					string value = line.Substring(split + 1); // removed .Trim() since sometimes it is desired.
					
					if (value.Length == 0) {
						continue;
					}

					value = value.Replace("\\n", "\n");

					// TODO: Maybe prepend key with filename: en.US.ItemName.lang would automatically assume "ItemName." for all entries.
					//string key = key;
					if (!modTranslationDictionary.TryGetValue(key, out ModTranslation mt))
						modTranslationDictionary[key] = mt = CreateTranslation(mod, key);

					mt.AddTranslation(culture, value);
				}
			}

			foreach (var value in modTranslationDictionary.Values) {
				AddTranslation(value);
			}
		}

		internal static void Unload() {
			translations.Clear();
		}

		//TODO: Unhardcode ALL of this.
		public static void RefreshModLanguage(GameCulture culture) {
			Dictionary<string, LocalizedText> dict = LanguageManager.Instance._localizedTexts;

			foreach (ModItem item in ItemLoader.items) {
				var text = new LocalizedText(item.DisplayName.Key, item.DisplayName.GetTranslation(culture));

				Lang._itemNameCache[item.Item.type] = SetLocalizedText(dict, text);

				text = new LocalizedText(item.Tooltip.Key, item.Tooltip.GetTranslation(culture));

				if (text.Value != null) {
					text = SetLocalizedText(dict, text);
					Lang._itemTooltipCache[item.Item.type] = ItemTooltip.FromLanguageKey(text.Key);
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

		private static LocalizedText SetLocalizedText(Dictionary<string, LocalizedText> dict, LocalizedText value) {
			if (dict.TryGetValue(value.Key, out var localizedText)) {
				localizedText.SetValue(value.Value);
			}
			else {
				dict[value.Key] = value;
			}

			return dict[value.Key];
		}
	}
}
