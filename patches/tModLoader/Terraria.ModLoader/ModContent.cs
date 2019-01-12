using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.GameContent.UI;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader.Audio;
using Terraria.ModLoader.Exceptions;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace Terraria.ModLoader
{
	/// <summary>
	/// Manages content added by mods.
	/// Liasons between mod content and Terraria's arrays and oversees the Loader classes.
	/// </summary>
	public class ModContent
	{
		private static readonly string ImagePath = "Content" + Path.DirectorySeparatorChar + "Images";

		internal static readonly IDictionary<string, ModHotKey> modHotKeys = new Dictionary<string, ModHotKey>();

		private static void SplitName(string name, out string domain, out string subName) {
			int slash = name.IndexOf('/');
			if (slash < 0)
				throw new MissingResourceException("Missing mod qualifier: " + name);

			domain = name.Substring(0, slash);
			subName = name.Substring(slash + 1);
		}

		/// <summary>
		/// Gets the byte representation of the file with the specified name. The name is in the format of "ModFolder/OtherFolders/FileNameWithExtension". Throws an ArgumentException if the file does not exist.
		/// </summary>
		/// <exception cref="MissingResourceException">Missing mod: " + name</exception>
		public static byte[] GetFileBytes(string name) {
			string modName, subName;
			SplitName(name, out modName, out subName);

			Mod mod = ModLoader.GetMod(modName);
			if (mod == null)
				throw new MissingResourceException("Missing mod: " + name);

			return mod.GetFileBytes(subName);
		}

		/// <summary>
		/// Returns whether or not a file with the specified name exists.
		/// </summary>
		public static bool FileExists(string name) {
			if (!name.Contains('/'))
				return false;

			string modName, subName;
			SplitName(name, out modName, out subName);

			Mod mod = ModLoader.GetMod(modName);
			return mod != null && mod.FileExists(subName);
		}

		/// <summary>
		/// Gets the texture with the specified name. The name is in the format of "ModFolder/OtherFolders/FileNameWithoutExtension". Throws an ArgumentException if the texture does not exist. If a vanilla texture is desired, the format "Terraria/FileNameWithoutExtension" will reference an image from the "terraria/Content/Images" folder. Note: Texture2D is in the Microsoft.Xna.Framework.Graphics namespace.
		/// </summary>
		/// <exception cref="MissingResourceException">Missing mod: " + name</exception>
		public static Texture2D GetTexture(string name) {
			if (Main.dedServ)
				return null;

			string modName, subName;
			SplitName(name, out modName, out subName);
			if (modName == "Terraria")
				return Main.instance.Content.Load<Texture2D>("Images" + Path.DirectorySeparatorChar + subName);

			Mod mod = ModLoader.GetMod(modName);
			if (mod == null)
				throw new MissingResourceException("Missing mod: " + name);

			return mod.GetTexture(subName);
		}

		/// <summary>
		/// Returns whether or not a texture with the specified name exists.
		/// </summary>
		public static bool TextureExists(string name) {
			if (!name.Contains('/'))
				return false;

			string modName, subName;
			SplitName(name, out modName, out subName);

			if (modName == "Terraria")
				return File.Exists(ImagePath + Path.DirectorySeparatorChar + subName + ".xnb");

			Mod mod = ModLoader.GetMod(modName);
			return mod != null && mod.TextureExists(subName);
		}

		/// <summary>
		/// Returns whether or not a texture with the specified name exists. texture will be populated with null if not found, and the texture if found.
		/// </summary>
		/// <param name="name">The texture name that is requested</param>
		/// <param name="texture">The texture itself will be output to this</param>
		/// <returns>True if the texture is found, false otherwise.</returns>
		internal static bool TryGetTexture(string name, out Texture2D texture) {
			if (Main.dedServ) {
				texture = null;
				return false;
			}

			string modName, subName;
			SplitName(name, out modName, out subName);
			if (modName == "Terraria") {
				if (File.Exists(ImagePath + Path.DirectorySeparatorChar + subName + ".xnb")) {
					texture = Main.instance.Content.Load<Texture2D>("Images" + Path.DirectorySeparatorChar + subName);
					return true;
				}
				texture = null;
				return false;
			}

			Mod mod = ModLoader.GetMod(modName);
			if (mod == null) {
				texture = null;
				return false;
			}

			return mod.textures.TryGetValue(subName, out texture);
		}

		/// <summary>
		/// Gets the sound with the specified name. The name is in the same format as for texture names. Throws an ArgumentException if the sound does not exist. Note: SoundEffect is in the Microsoft.Xna.Framework.Audio namespace.
		/// </summary>
		/// <exception cref="MissingResourceException">Missing mod: " + name</exception>
		public static SoundEffect GetSound(string name) {
			if (Main.dedServ)
				return null;

			string modName, subName;
			SplitName(name, out modName, out subName);

			Mod mod = ModLoader.GetMod(modName);
			if (mod == null)
				throw new MissingResourceException("Missing mod: " + name);

			return mod.GetSound(subName);
		}

		/// <summary>
		/// Returns whether or not a sound with the specified name exists.
		/// </summary>
		public static bool SoundExists(string name) {
			if (!name.Contains('/'))
				return false;

			string modName, subName;
			SplitName(name, out modName, out subName);

			Mod mod = ModLoader.GetMod(modName);
			return mod != null && mod.SoundExists(subName);
		}

		/// <summary>
		/// Gets the music with the specified name. The name is in the same format as for texture names. Throws an ArgumentException if the music does not exist. Note: SoundMP3 is in the Terraria.ModLoader namespace.
		/// </summary>
		/// <exception cref="MissingResourceException">Missing mod: " + name</exception>
		public static Music GetMusic(string name) {
			if (Main.dedServ) { return null; }
			string modName, subName;
			SplitName(name, out modName, out subName);
			Mod mod = ModLoader.GetMod(modName);
			if (mod == null) { throw new MissingResourceException("Missing mod: " + name); }
			return mod.GetMusic(subName);
		}

		/// <summary>
		/// Returns whether or not a sound with the specified name exists.
		/// </summary>
		public static bool MusicExists(string name) {
			if (!name.Contains('/')) { return false; }
			string modName, subName;
			SplitName(name, out modName, out subName);
			Mod mod = ModLoader.GetMod(modName);
			return mod != null && mod.MusicExists(subName);
		}

		private static LocalizedText SetLocalizedText(Dictionary<string, LocalizedText> dict, LocalizedText value) {
			if (dict.ContainsKey(value.Key)) {
				dict[value.Key].SetValue(value.Value);
			}
			else {
				dict[value.Key] = value;
			}
			return dict[value.Key];
		}

		internal static ModHotKey RegisterHotKey(Mod mod, string name, string defaultKey) {
			string key = mod.Name + ": " + name;
			modHotKeys[key] = new ModHotKey(mod, name, defaultKey);
			return modHotKeys[key];
		}

		internal static void Load() {
			CacheVanillaState();

			Interface.loadMods.SetLoadStage("tModLoader.MSIntializing", ModLoader.Mods.Length);
			LoadModContent(mod => {
				mod.AutoloadConfig();
				mod.loading = true;
				mod.LoadResources();
				mod.Autoload();
				mod.Load();
				mod.loading = false;
			});

			Interface.loadMods.SetLoadStage("tModLoader.MSSettingUp");
			ResizeArrays();
			RecipeGroupHelper.FixRecipeGroupLookups();

			Interface.loadMods.SetLoadStage("tModLoader.MSLoading", ModLoader.Mods.Length);
			LoadModContent(mod => {
				mod.SetupContent();
				mod.PostSetupContent();
			});

			if (Main.dedServ)
				ModNet.AssignNetIDs();

			Main.player[255] = new Player(false); // setup inventory is unnecessary 

			RefreshModLanguage(Language.ActiveCulture);
			MapLoader.SetupModMap();
			ItemSorting.SetupWhiteLists();
			PlayerInput.ReInitialize();
			SetupRecipes();
		}

		private static void CacheVanillaState() {
			EffectsTracker.CacheVanillaState();
		}

		internal static Mod LoadingMod { get; private set; }
		private static void LoadModContent(Action<Mod> loadAction) {
			int num = 0;
			foreach (var mod in ModLoader.Mods) {
				Interface.loadMods.SetCurrentMod(num++, mod.Name);
				try {
					LoadingMod = mod;
					loadAction(mod);
				}
				catch (Exception e) {
					e.Data["mod"] = mod.Name;
					throw;
				}
				finally {
					LoadingMod = null;
				}
			}
		}

		private static void SetupRecipes() {
			Interface.loadMods.SetLoadStage("tModLoader.MSAddingRecipes");
			for (int k = 0; k < Recipe.maxRecipes; k++)
				Main.recipe[k] = new Recipe();

			Recipe.numRecipes = 0;
			RecipeGroupHelper.ResetRecipeGroups();
			Recipe.SetupRecipes();
		}

		internal static void UnloadModContent() {
			foreach (var mod in ModLoader.Mods.Reverse()) {
				try {
					mod.UnloadContent();
					mod.File?.Close();
				}
				catch (Exception e) {
					e.Data["mod"] = mod.Name;
					throw;
				}
				finally {
					MonoModHooks.RemoveAll(mod);
				}
			}
		}

		public static void Unload() {
			ItemLoader.Unload();
			EquipLoader.Unload();
			ModPrefix.Unload();
			ModDust.Unload();
			TileLoader.Unload();
			ModTileEntity.Unload();
			WallLoader.Unload();
			ProjectileLoader.Unload();
			NPCLoader.Unload();
			NPCHeadLoader.Unload();
			PlayerHooks.Unload();
			BuffLoader.Unload();
			MountLoader.Unload();
			ModGore.Unload();
			SoundLoader.Unload();
			DisposeMusic();
			BackgroundTextureLoader.Unload();
			UgBgStyleLoader.Unload();
			SurfaceBgStyleLoader.Unload();
			GlobalBgStyleLoader.Unload();
			WaterStyleLoader.Unload();
			WaterfallStyleLoader.Unload();
			WorldHooks.Unload();
			ResizeArrays(true);
			for (int k = 0; k < Recipe.maxRecipes; k++) {
				Main.recipe[k] = new Recipe();
			}
			Recipe.numRecipes = 0;
			RecipeGroupHelper.ResetRecipeGroups();
			Recipe.SetupRecipes();
			MapLoader.UnloadModMap();
			ItemSorting.SetupWhiteLists();
			modHotKeys.Clear();
			RecipeHooks.Unload();
			CommandManager.Unload();
			TagSerializer.Reload();
			ModNet.Unload();
			Config.ConfigManager.Unload();
			CustomCurrencyManager.Initialize();
			EffectsTracker.RemoveModEffects();

			CleanupModReferences();
		}

		private static void ResizeArrays(bool unloading = false) {
			ItemLoader.ResizeArrays(unloading);
			EquipLoader.ResizeAndFillArrays();
			ModPrefix.ResizeArrays();
			Main.InitializeItemAnimations();
			ModDust.ResizeArrays();
			TileLoader.ResizeArrays(unloading);
			WallLoader.ResizeArrays(unloading);
			ProjectileLoader.ResizeArrays();
			NPCLoader.ResizeArrays(unloading);
			NPCHeadLoader.ResizeAndFillArrays();
			ModGore.ResizeAndFillArrays();
			SoundLoader.ResizeAndFillArrays();
			MountLoader.ResizeArrays();
			BuffLoader.ResizeArrays();
			PlayerHooks.RebuildHooks();
			BackgroundTextureLoader.ResizeAndFillArrays();
			UgBgStyleLoader.ResizeAndFillArrays();
			SurfaceBgStyleLoader.ResizeAndFillArrays();
			GlobalBgStyleLoader.ResizeAndFillArrays(unloading);
			WaterStyleLoader.ResizeArrays();
			WaterfallStyleLoader.ResizeArrays();
			WorldHooks.ResizeArrays();
			foreach (LocalizedText text in LanguageManager.Instance._localizedTexts.Values) {
				text.Override = null;
			}
		}

		public static void RefreshModLanguage(GameCulture culture) {
			Dictionary<string, LocalizedText> dict = LanguageManager.Instance._localizedTexts;
			foreach (ModItem item in ItemLoader.items) {
				LocalizedText text = new LocalizedText(item.DisplayName.Key, item.DisplayName.GetTranslation(culture));
				Lang._itemNameCache[item.item.type] = SetLocalizedText(dict, text);
				text = new LocalizedText(item.Tooltip.Key, item.Tooltip.GetTranslation(culture));
				if (text.Value != null) {
					text = SetLocalizedText(dict, text);
					Lang._itemTooltipCache[item.item.type] = ItemTooltip.FromLanguageKey(text.Key);
				}
			}
			foreach (ModPrefix prefix in ModPrefix.prefixes) {
				LocalizedText text = new LocalizedText(prefix.DisplayName.Key, prefix.DisplayName.GetTranslation(culture));
				Lang.prefix[prefix.Type] = SetLocalizedText(dict, text);
			}
			foreach (var keyValuePair in MapLoader.tileEntries) {
				foreach (MapEntry entry in keyValuePair.Value) {
					if (entry.translation != null) {
						LocalizedText text = new LocalizedText(entry.translation.Key, entry.translation.GetTranslation(culture));
						SetLocalizedText(dict, text);
					}
				}
			}
			foreach (var keyValuePair in MapLoader.wallEntries) {
				foreach (MapEntry entry in keyValuePair.Value) {
					if (entry.translation != null) {
						LocalizedText text = new LocalizedText(entry.translation.Key, entry.translation.GetTranslation(culture));
						SetLocalizedText(dict, text);
					}
				}
			}
			foreach (ModProjectile proj in ProjectileLoader.projectiles) {
				LocalizedText text = new LocalizedText(proj.DisplayName.Key, proj.DisplayName.GetTranslation(culture));
				Lang._projectileNameCache[proj.projectile.type] = SetLocalizedText(dict, text);
			}
			foreach (ModNPC npc in NPCLoader.npcs) {
				LocalizedText text = new LocalizedText(npc.DisplayName.Key, npc.DisplayName.GetTranslation(culture));
				Lang._npcNameCache[npc.npc.type] = SetLocalizedText(dict, text);
			}
			foreach (ModBuff buff in BuffLoader.buffs) {
				LocalizedText text = new LocalizedText(buff.DisplayName.Key, buff.DisplayName.GetTranslation(culture));
				Lang._buffNameCache[buff.Type] = SetLocalizedText(dict, text);
				text = new LocalizedText(buff.Description.Key, buff.Description.GetTranslation(culture));
				Lang._buffDescriptionCache[buff.Type] = SetLocalizedText(dict, text);
			}
			foreach (Mod mod in ModLoader.Mods) {
				foreach (ModTranslation translation in mod.translations.Values) {
					LocalizedText text = new LocalizedText(translation.Key, translation.GetTranslation(culture));
					SetLocalizedText(dict, text);
				}
			}
			LanguageManager.Instance.ProcessCopyCommandsInTexts();
		}

		private static void DisposeMusic() {
			for (int i = 0; i < Main.music.Length; i++) {
				MusicStreaming music = Main.music[i] as MusicStreaming;
				if (music != null) {
					if (i < Main.maxMusic) {
						Main.music[i] = Main.soundBank.GetCue("Music_" + i);
					}
					else {
						Main.music[i] = null;
					}
					music.Stop(AudioStopOptions.Immediate);
					music.Dispose();
				}
			}
		}

		/// <summary>
		/// Several arrays and other fields hold references to various classes from mods, we need to clean them up to give properly coded mods a chance to be completely free of references
		/// so that they can be collected by the garbage collection. For most things eventually they will be replaced during gameplay, but we want the old instance completely gone quickly.
		/// </summary>
		internal static void CleanupModReferences() {
			// Clear references to ModPlayer instances
			for (int i = 0; i < 256; i++) {
				Main.player[i] = new Player();
			}

			Main.clientPlayer = new Player(false);
			Main.ActivePlayerFileData = new Terraria.IO.PlayerFileData();
			Main._characterSelectMenu._playerList?.Clear();
			Main.PlayerList.Clear();

			foreach (var npc in Main.npc) {
				npc.SetDefaults(0);
			}

			foreach (var item in Main.item) {
				item.SetDefaults(0);
			}
			ItemSlot.singleSlotArray[0]?.SetDefaults(0);

			for (int i = 0; i < Main.chest.Length; i++) {
				Main.chest[i] = new Chest();
			}
		}
	}
}
