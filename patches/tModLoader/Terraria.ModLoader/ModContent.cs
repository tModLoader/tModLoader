using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Localization;
using Terraria.ModLoader.Audio;
using Terraria.ModLoader.Exceptions;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class serves as a central place which manages content added by mods, the loading, unloading and integration of such.
	/// </summary>
	public static class ModContent
	{
		internal static readonly IDictionary<string, ModHotKey> modHotKeys = new Dictionary<string, ModHotKey>();

		// TODO is this ever even used?
		internal static Action PostLoad;

		// Responsible for loading all content by loaded mod instances
		internal static void LoadContent(IDictionary<string, Mod> modInstances)
		{
			if (Main.dedServ)
			{
				Console.WriteLine(Language.GetTextValue("tModLoader.AddingModContent"));
			}

			int num = 0;
			foreach (Mod mod in modInstances.Values)
			{
				Interface.loadMods.SetProgressInit(mod.Name, num, modInstances.Count());
				try
				{
					mod.loading = true;
					mod.File?.Read(TmodFile.LoadedState.Streaming, mod.LoadResourceFromStream);
					mod.Autoload();
					Interface.loadMods.SetSubProgressInit("");
					mod.Load();
					mod.loading = false;
				}
				catch (Exception e)
				{
					ModOrganiser.DisableMod(mod.Name);
					ErrorLogger.LogLoadingError(mod.Name, mod.tModLoaderVersion, e);
					Main.menuMode = Interface.errorMessageID;
					return;
				}

				num++;
			}

			Interface.loadMods.SetProgressSetup(0f);
			ResizeArrays();
			RecipeGroupHelper.FixRecipeGroupLookups();
			num = 0;
			foreach (Mod mod in modInstances.Values)
			{
				Interface.loadMods.SetProgressLoad(mod.Name, num, modInstances.Count());
				try
				{
					mod.SetupContent();
					mod.PostSetupContent();
					mod.File?.UnloadAssets();
				}
				catch (Exception e)
				{
					ModOrganiser.DisableMod(mod.Name);
					ErrorLogger.LogLoadingError(mod.Name, mod.tModLoaderVersion, e);
					Main.menuMode = Interface.errorMessageID;
					return;
				}

				num++;
			}

			RefreshModLanguage(Language.ActiveCulture);

			if (Main.dedServ)
			{
				ModNet.AssignNetIDs();
				//Main.player[0] = new Player();
			}

			Main.player[255] = new Player(false); // setup inventory is unnecessary 

			MapLoader.SetupModMap();
			ItemSorting.SetupWhiteLists();

			Interface.loadMods.SetProgressRecipes();
			for (int k = 0; k < Recipe.maxRecipes; k++)
			{
				Main.recipe[k] = new Recipe();
			}

			Recipe.numRecipes = 0;
			RecipeGroupHelper.ResetRecipeGroups();
			try
			{
				Recipe.SetupRecipes();
			}
			catch (AddRecipesException e)
			{
				ErrorLogger.LogLoadingError(e.modName, ModLoader.version, e.InnerException, true);
				Main.menuMode = Interface.errorMessageID;
				return;
			}

			if (PostLoad != null)
			{
				PostLoad();
				PostLoad = null;
			}
			else
			{
				Main.menuMode = 0;
			}

			GameInput.PlayerInput.ReInitialize();
		}

		private static void ResizeArrays(bool unloading = false)
		{
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
			foreach (LocalizedText text in LanguageManager.Instance._localizedTexts.Values)
			{
				text.Override = null;
			}
		}

		private static void DisposeMusic()
		{
			for (int i = 0; i < Main.music.Length; i++)
			{
				if (Main.music[i] is MusicStreaming music)
				{
					if (i < Main.maxMusic)
					{
						Main.music[i] = Main.soundBank.GetCue("Music_" + i);
					}
					else
					{
						Main.music[i] = null;
					}

					music.Stop(AudioStopOptions.Immediate);
					music.Dispose();
				}
			}
		}

		internal static void Unload()
		{
			ModOrganiser.UnloadMods();

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
			ModOrganiser.mods.Clear();
			WorldHooks.Unload();
			ResizeArrays(true);
			for (int k = 0; k < Recipe.maxRecipes; k++)
			{
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
			GameContent.UI.CustomCurrencyManager.Initialize();
			ModOrganiser.CleanupModReferences();

			if (!Main.dedServ && Main.netMode != 1) //disable vanilla client compatibility restrictions when reloading on a client
				ModNet.AllowVanillaClients = false;
		}

		public static ModHotKey RegisterHotKey(Mod mod, string name, string defaultKey)
		{
			string key = mod.Name + ": " + name;
			modHotKeys[key] = new ModHotKey(mod, name, defaultKey);
			return modHotKeys[key];
		}

		public static void RefreshModLanguage(GameCulture culture)
		{
			Dictionary<string, LocalizedText> dict = LanguageManager.Instance._localizedTexts;
			foreach (ModItem item in ItemLoader.items)
			{
				LocalizedText text = new LocalizedText(item.DisplayName.Key, item.DisplayName.GetTranslation(culture));
				Lang._itemNameCache[item.item.type] = SetLocalizedText(dict, text);
				text = new LocalizedText(item.Tooltip.Key, item.Tooltip.GetTranslation(culture));
				if (text.Value != null)
				{
					text = SetLocalizedText(dict, text);
					Lang._itemTooltipCache[item.item.type] = ItemTooltip.FromLanguageKey(text.Key);
				}
			}

			foreach (ModPrefix prefix in ModPrefix.prefixes)
			{
				LocalizedText text = new LocalizedText(prefix.DisplayName.Key, prefix.DisplayName.GetTranslation(culture));
				Lang.prefix[prefix.Type] = SetLocalizedText(dict, text);
			}

			foreach (var keyValuePair in MapLoader.tileEntries)
			{
				foreach (MapEntry entry in keyValuePair.Value)
				{
					if (entry.translation != null)
					{
						LocalizedText text = new LocalizedText(entry.translation.Key, entry.translation.GetTranslation(culture));
						SetLocalizedText(dict, text);
					}
				}
			}

			foreach (var keyValuePair in MapLoader.wallEntries)
			{
				foreach (MapEntry entry in keyValuePair.Value)
				{
					if (entry.translation != null)
					{
						LocalizedText text = new LocalizedText(entry.translation.Key, entry.translation.GetTranslation(culture));
						SetLocalizedText(dict, text);
					}
				}
			}

			foreach (ModProjectile proj in ProjectileLoader.projectiles)
			{
				LocalizedText text = new LocalizedText(proj.DisplayName.Key, proj.DisplayName.GetTranslation(culture));
				Lang._projectileNameCache[proj.projectile.type] = SetLocalizedText(dict, text);
			}

			foreach (ModNPC npc in NPCLoader.npcs)
			{
				LocalizedText text = new LocalizedText(npc.DisplayName.Key, npc.DisplayName.GetTranslation(culture));
				Lang._npcNameCache[npc.npc.type] = SetLocalizedText(dict, text);
			}

			foreach (ModBuff buff in BuffLoader.buffs)
			{
				LocalizedText text = new LocalizedText(buff.DisplayName.Key, buff.DisplayName.GetTranslation(culture));
				Lang._buffNameCache[buff.Type] = SetLocalizedText(dict, text);
				text = new LocalizedText(buff.Description.Key, buff.Description.GetTranslation(culture));
				Lang._buffDescriptionCache[buff.Type] = SetLocalizedText(dict, text);
			}

			foreach (Mod mod in ModOrganiser.LoadedMods)
			{
				foreach (ModTranslation translation in mod.translations.Values)
				{
					LocalizedText text = new LocalizedText(translation.Key, translation.GetTranslation(culture));
					SetLocalizedText(dict, text);
				}
			}

			LanguageManager.Instance.ProcessCopyCommandsInTexts();
		}

		private static LocalizedText SetLocalizedText(IDictionary<string, LocalizedText> dict, LocalizedText value)
		{
			if (dict.ContainsKey(value.Key))
			{
				dict[value.Key].SetValue(value.Value);
			}
			else
			{
				dict[value.Key] = value;
			}

			return dict[value.Key];
		}

		private static void SplitName(string name, out string domain, out string subName)
		{
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
		public static byte[] GetFileBytes(string name)
		{
			string modName, subName;
			SplitName(name, out modName, out subName);

			Mod mod = ModOrganiser.GetMod(modName);
			if (mod == null)
				throw new MissingResourceException("Missing mod: " + name);

			return mod.GetFileBytes(subName);
		}

		/// <summary>
		/// Returns whether or not a file with the specified name exists.
		/// </summary>
		public static bool FileExists(string name)
		{
			if (!name.Contains('/'))
				return false;

			string modName, subName;
			SplitName(name, out modName, out subName);

			Mod mod = ModOrganiser.GetMod(modName);
			return mod != null && mod.FileExists(subName);
		}

		/// <summary>
		/// Gets the texture with the specified name. The name is in the format of "ModFolder/OtherFolders/FileNameWithoutExtension". Throws an ArgumentException if the texture does not exist. If a vanilla texture is desired, the format "Terraria/FileNameWithoutExtension" will reference an image from the "terraria/Content/Images" folder. Note: Texture2D is in the Microsoft.Xna.Framework.Graphics namespace.
		/// </summary>
		/// <exception cref="MissingResourceException">Missing mod: " + name</exception>
		public static Texture2D GetTexture(string name)
		{
			if (Main.dedServ)
				return null;

			string modName, subName;
			SplitName(name, out modName, out subName);
			if (modName == "Terraria")
				return Main.instance.Content.Load<Texture2D>("Images" + Path.DirectorySeparatorChar + subName);

			Mod mod = ModOrganiser.GetMod(modName);
			if (mod == null)
				throw new MissingResourceException("Missing mod: " + name);

			return mod.GetTexture(subName);
		}

		/// <summary>
		/// Returns whether or not a texture with the specified name exists.
		/// </summary>
		public static bool TextureExists(string name)
		{
			if (!name.Contains('/'))
				return false;

			string modName, subName;
			SplitName(name, out modName, out subName);

			if (modName == "Terraria")
				return File.Exists(ModLoader.ImagePath + Path.DirectorySeparatorChar + name + ".xnb");

			Mod mod = ModOrganiser.GetMod(modName);
			return mod != null && mod.TextureExists(subName);
		}

		/// <summary>
		/// Gets the sound with the specified name. The name is in the same format as for texture names. Throws an ArgumentException if the sound does not exist. Note: SoundEffect is in the Microsoft.Xna.Framework.Audio namespace.
		/// </summary>
		/// <exception cref="MissingResourceException">Missing mod: " + name</exception>
		public static SoundEffect GetSound(string name)
		{
			if (Main.dedServ)
				return null;

			string modName, subName;
			SplitName(name, out modName, out subName);

			Mod mod = ModOrganiser.GetMod(modName);
			if (mod == null)
				throw new MissingResourceException("Missing mod: " + name);

			return mod.GetSound(subName);
		}

		/// <summary>
		/// Returns whether or not a sound with the specified name exists.
		/// </summary>
		public static bool SoundExists(string name)
		{
			if (!name.Contains('/'))
				return false;

			string modName, subName;
			SplitName(name, out modName, out subName);

			Mod mod = ModOrganiser.GetMod(modName);
			return mod != null && mod.SoundExists(subName);
		}

		/// <summary>
		/// Gets the music with the specified name. The name is in the same format as for texture names. Throws an ArgumentException if the music does not exist. Note: SoundMP3 is in the Terraria.ModLoader namespace.
		/// </summary>
		/// <exception cref="MissingResourceException">Missing mod: " + name</exception>
		public static Music GetMusic(string name)
		{
			if (Main.dedServ)
			{
				return null;
			}

			string modName, subName;
			SplitName(name, out modName, out subName);
			Mod mod = ModOrganiser.GetMod(modName);
			if (mod == null)
			{
				throw new MissingResourceException("Missing mod: " + name);
			}

			return mod.GetMusic(subName);
		}

		/// <summary>
		/// Returns whether or not a sound with the specified name exists.
		/// </summary>
		public static bool MusicExists(string name)
		{
			if (!name.Contains('/'))
			{
				return false;
			}

			string modName, subName;
			SplitName(name, out modName, out subName);
			Mod mod = ModOrganiser.GetMod(modName);
			return mod != null && mod.MusicExists(subName);
		}
	}
}