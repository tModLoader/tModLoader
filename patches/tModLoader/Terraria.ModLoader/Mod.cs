using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Liquid;
using Terraria.ID;
using Terraria.ModLoader.Exceptions;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using Terraria.Audio;

namespace Terraria.ModLoader
{
	/// <summary>
	/// Mod is an abstract class that you will override. It serves as a central place from which the mod's contents are stored. It provides methods for you to use or override.
	/// </summary>
	public abstract partial class Mod
	{
		/// <summary>
		/// The TmodFile object created when tModLoader reads this mod.
		/// </summary>
		public TmodFile File { get; internal set; }
		/// <summary>
		/// The assembly code this is loaded when tModLoader loads this mod.
		/// </summary>
		public Assembly Code { get; internal set; }

		/// <summary>
		/// Stores the name of the mod. This name serves as the mod's identification, and also helps with saving everything your mod adds. By default this returns the name of the folder that contains all your code and stuff.
		/// </summary>
		public virtual string Name => File.name;
		/// <summary>
		/// The version of tModLoader that was being used when this mod was built.
		/// </summary>
		public virtual Version tModLoaderVersion => File.tModLoaderVersion;
		/// <summary>
		/// This version number of this mod.
		/// </summary>
		public virtual Version Version => File.version;

		public ModProperties Properties { get; protected set; } = ModProperties.AutoLoadAll;
		/// <summary>
		/// The ModSide that controls how this mod is synced between client and server.
		/// </summary>
		public ModSide Side { get; internal set; }
		/// <summary>
		/// The display name of this mod in the Mods menu.
		/// </summary>
		public string DisplayName { get; internal set; }

		internal short netID = -1;

		internal readonly IDictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
		internal readonly IDictionary<string, SoundEffect> sounds = new Dictionary<string, SoundEffect>();
		internal readonly IDictionary<string, SpriteFont> fonts = new Dictionary<string, SpriteFont>();
		internal readonly IDictionary<string, Effect> effects = new Dictionary<string, Effect>();
		internal readonly IList<ModRecipe> recipes = new List<ModRecipe>();
		internal readonly IDictionary<string, ModItem> items = new Dictionary<string, ModItem>();
		internal readonly IDictionary<string, GlobalItem> globalItems = new Dictionary<string, GlobalItem>();
		internal readonly IDictionary<Tuple<string, EquipType>, EquipTexture> equipTextures = new Dictionary<Tuple<string, EquipType>, EquipTexture>();
		internal readonly IDictionary<string, ModDust> dusts = new Dictionary<string, ModDust>();
		internal readonly IDictionary<string, ModTile> tiles = new Dictionary<string, ModTile>();
		internal readonly IDictionary<string, GlobalTile> globalTiles = new Dictionary<string, GlobalTile>();
		internal readonly IDictionary<string, ModTileEntity> tileEntities = new Dictionary<string, ModTileEntity>();
		internal readonly IDictionary<string, ModWall> walls = new Dictionary<string, ModWall>();
		internal readonly IDictionary<string, GlobalWall> globalWalls = new Dictionary<string, GlobalWall>();
		internal readonly IDictionary<string, ModProjectile> projectiles = new Dictionary<string, ModProjectile>();
		internal readonly IDictionary<string, GlobalProjectile> globalProjectiles = new Dictionary<string, GlobalProjectile>();
		internal readonly IDictionary<string, ModNPC> npcs = new Dictionary<string, ModNPC>();
		internal readonly IDictionary<string, GlobalNPC> globalNPCs = new Dictionary<string, GlobalNPC>();
		internal readonly IDictionary<string, ModPlayer> players = new Dictionary<string, ModPlayer>();
		internal readonly IDictionary<string, ModMountData> mountDatas = new Dictionary<string, ModMountData>();
		internal readonly IDictionary<string, ModBuff> buffs = new Dictionary<string, ModBuff>();
		internal readonly IDictionary<string, GlobalBuff> globalBuffs = new Dictionary<string, GlobalBuff>();
		internal readonly IDictionary<string, ModWorld> worlds = new Dictionary<string, ModWorld>();
		internal readonly IDictionary<string, ModUgBgStyle> ugBgStyles = new Dictionary<string, ModUgBgStyle>();
		internal readonly IDictionary<string, ModSurfaceBgStyle> surfaceBgStyles = new Dictionary<string, ModSurfaceBgStyle>();
		internal readonly IDictionary<string, GlobalBgStyle> globalBgStyles = new Dictionary<string, GlobalBgStyle>();
		internal readonly IDictionary<string, ModWaterStyle> waterStyles = new Dictionary<string, ModWaterStyle>();
		internal readonly IDictionary<string, ModWaterfallStyle> waterfallStyles = new Dictionary<string, ModWaterfallStyle>();
		internal readonly IDictionary<string, GlobalRecipe> globalRecipes = new Dictionary<string, GlobalRecipe>();
		internal readonly IDictionary<string, ModTranslation> translations = new Dictionary<string, ModTranslation>();

		/// <summary>
		/// Override this method to add most of your content to your mod. Here you will call other methods such as AddItem. This is guaranteed to be called after all content has been autoloaded.
		/// </summary>
		public virtual void Load()
		{
		}

		/// <summary>
		/// Allows you to load things in your mod after its content has been setup (arrays have been resized to fit the content, etc).
		/// </summary>
		public virtual void PostSetupContent()
		{
		}

		/// <summary>
		/// This is called whenever this mod is unloaded from the game. Use it to undo changes that you've made in Load that aren't automatically handled (for example, modifying the texture of a vanilla item). Mods are guaranteed to be unloaded in the reverse order they were loaded in.
		/// </summary>
		public virtual void Unload()
		{
		}

		/// <summary>
		/// Override this method to add recipe groups to this mod. You must add recipe groups by calling the RecipeGroup.RegisterGroup method here. A recipe group is a set of items that can be used interchangeably in the same recipe.
		/// </summary>
		public virtual void AddRecipeGroups()
		{
		}

		/// <summary>
		/// Override this method to add recipes to the game. It is recommended that you do so through instances of ModRecipe, since it provides methods that simplify recipe creation.
		/// </summary>
		public virtual void AddRecipes()
		{
		}

		internal void Autoload()
		{
			if (!Main.dedServ && File != null)
			{
				foreach (var file in File)
				{
					var path = file.Key;
					var data = file.Value;
					string extension = Path.GetExtension(path);
					switch (extension)
					{
						case ".png":
							string texturePath = Path.ChangeExtension(path, null);
							using (MemoryStream buffer = new MemoryStream(data))
							{
								textures[texturePath] = Texture2D.FromStream(Main.instance.GraphicsDevice, buffer);
								textures[texturePath].Name = Name + "/" + texturePath;
							}
							break;
						case ".wav":
							string soundPath = Path.ChangeExtension(path, null);
							using (MemoryStream buffer = new MemoryStream(data))
							{
								try
								{
									sounds[soundPath] = SoundEffect.FromStream(buffer);
								}
								catch
								{
									sounds[soundPath] = null;
								}
							}
							break;
						case ".mp3":
							string mp3Path = Path.ChangeExtension(path, null);
							string wavCacheFilename = this.Name + "_" + mp3Path.Replace('/', '_') + "_" + Version + ".wav";
							WAVCacheIO.DeleteIfOlder(File.path, wavCacheFilename);
							try
							{
								sounds[mp3Path] = WAVCacheIO.WAVCacheAvailable(wavCacheFilename)
									? SoundEffect.FromStream(WAVCacheIO.GetWavStream(wavCacheFilename))
									: WAVCacheIO.CacheMP3(wavCacheFilename, data);
							}
							catch
							{
								sounds[mp3Path] = null;
							}
							break;
						case ".xnb":
							string xnbPath = Path.ChangeExtension(path, null);
							if (xnbPath.StartsWith("Fonts/"))
							{
								string fontFilenameNoExtension = Name + "_" + xnbPath.Replace('/', '_') + "_" + Version;
								string fontFilename = fontFilenameNoExtension + ".xnb";
								FontCacheIO.DeleteIfOlder(File.path, fontFilename);
								if (!FontCacheIO.FontCacheAvailable(fontFilename))
								{
									FileUtilities.WriteAllBytes(FontCacheIO.FontCachePath + Path.DirectorySeparatorChar + fontFilename, data, false);
								}
								try
								{
									fonts[xnbPath] = Main.instance.Content.Load<SpriteFont>("Fonts" + Path.DirectorySeparatorChar + "ModFonts" + Path.DirectorySeparatorChar + fontFilenameNoExtension);
								}
								catch
								{
									fonts[xnbPath] = null;
								}
							}
							else if (xnbPath.StartsWith("Effects/"))
							{
								string effectFilenameNoExtension = Name + "_" + xnbPath.Replace('/', '_') + "_" + Version;
								string effectFilename = effectFilenameNoExtension + ".xnb";
								try
								{
									using (MemoryStream ms = new MemoryStream(data))
									using (BinaryReader br = new BinaryReader(ms))
									{
										char x = (char)br.ReadByte();//x
										char n = (char)br.ReadByte();//n
										char b = (char)br.ReadByte();//b
										char w = (char)br.ReadByte();//w
										byte xnbFormatVersion = br.ReadByte();//5
										byte flags = br.ReadByte();//flags
										UInt32 compressedDataSize = br.ReadUInt32();
										if ((flags & 0x80) != 0)
										{
											UInt32 decompressedDataSize = br.ReadUInt32();
										}
										int typeReaderCount = br.ReadVarInt();
										string typeReaderName = br.ReadString();
										int typeReaderVersion = br.ReadInt32();
										int sharedResourceCount = br.ReadVarInt();
										int typeid = br.ReadVarInt();
										UInt32 size = br.ReadUInt32();
										byte[] effectBytecode = br.ReadBytes((int)size);
										effects[xnbPath] = new Effect(Main.instance.GraphicsDevice, effectBytecode);
									}
								}
								catch
								{
									effects[xnbPath] = null;
								}
							}
							break;
					}
				}
			}

			if (Code == null)
				return;

			IList<Type> modGores = new List<Type>();
			IList<Type> modSounds = new List<Type>();
			foreach (Type type in Code.GetTypes().OrderBy(type => type.FullName, StringComparer.InvariantCulture))
			{
				if (type.IsAbstract)
				{
					continue;
				}
				if (type.IsSubclassOf(typeof(ModItem)))
				{
					AutoloadItem(type);
				}
				else if (type.IsSubclassOf(typeof(GlobalItem)))
				{
					AutoloadGlobalItem(type);
				}
				else if (type.IsSubclassOf(typeof(ModDust)))
				{
					AutoloadDust(type);
				}
				else if (type.IsSubclassOf(typeof(ModTile)))
				{
					AutoloadTile(type);
				}
				else if (type.IsSubclassOf(typeof(GlobalTile)))
				{
					AutoloadGlobalTile(type);
				}
				else if (type.IsSubclassOf(typeof(ModTileEntity)))
				{
					AutoloadTileEntity(type);
				}
				else if (type.IsSubclassOf(typeof(ModWall)))
				{
					AutoloadWall(type);
				}
				else if (type.IsSubclassOf(typeof(GlobalWall)))
				{
					AutoloadGlobalWall(type);
				}
				else if (type.IsSubclassOf(typeof(ModProjectile)))
				{
					AutoloadProjectile(type);
				}
				else if (type.IsSubclassOf(typeof(GlobalProjectile)))
				{
					AutoloadGlobalProjectile(type);
				}
				else if (type.IsSubclassOf(typeof(ModNPC)))
				{
					AutoloadNPC(type);
				}
				else if (type.IsSubclassOf(typeof(GlobalNPC)))
				{
					AutoloadGlobalNPC(type);
				}
				else if (type.IsSubclassOf(typeof(ModPlayer)))
				{
					AutoloadPlayer(type);
				}
				else if (type.IsSubclassOf(typeof(ModBuff)))
				{
					AutoloadBuff(type);
				}
				else if (type.IsSubclassOf(typeof(GlobalBuff)))
				{
					AutoloadGlobalBuff(type);
				}
				else if (type.IsSubclassOf(typeof(ModMountData)))
				{
					AutoloadMountData(type);
				}
				else if (type.IsSubclassOf(typeof(ItemInfo)))
				{
					AutoloadItemInfo(type);
				}
				else if (type.IsSubclassOf(typeof(ProjectileInfo)))
				{
					AutoloadProjectileInfo(type);
				}
				else if (type.IsSubclassOf(typeof(NPCInfo)))
				{
					AutoloadNPCInfo(type);
				}
				else if (type.IsSubclassOf(typeof(ModGore)))
				{
					modGores.Add(type);
				}
				else if (type.IsSubclassOf(typeof(ModSound)))
				{
					modSounds.Add(type);
				}
				else if (type.IsSubclassOf(typeof(ModWorld)))
				{
					AutoloadModWorld(type);
				}
				else if (type.IsSubclassOf(typeof(ModUgBgStyle)))
				{
					AutoloadUgBgStyle(type);
				}
				else if (type.IsSubclassOf(typeof(ModSurfaceBgStyle)))
				{
					AutoloadSurfaceBgStyle(type);
				}
				else if (type.IsSubclassOf(typeof(GlobalBgStyle)))
				{
					AutoloadGlobalBgStyle(type);
				}
				else if (type.IsSubclassOf(typeof(ModWaterStyle)))
				{
					AutoloadWaterStyle(type);
				}
				else if (type.IsSubclassOf(typeof(ModWaterfallStyle)))
				{
					AutoloadWaterfallStyle(type);
				}
				else if (type.IsSubclassOf(typeof(GlobalRecipe)))
				{
					AutoloadGlobalRecipe(type);
				}
				else if (type.IsSubclassOf(typeof(ModCommand)))
				{
					AutoloadCommand(type);
				}
			}
			if (Properties.AutoloadGores)
			{
				AutoloadGores(modGores);
			}
			if (Properties.AutoloadSounds)
			{
				AutoloadSounds(modSounds);
			}
			if (Properties.AutoloadBackgrounds)
			{
				AutoloadBackgrounds();
			}
		}

		/// <summary>
		/// Manually add a Command during Load
		/// </summary>
		public void AddCommand(string name, ModCommand mc)
		{
			mc.Name = name;
			mc.mod = this;

			CommandManager.Add(mc);
		}

		/// <summary>
		/// Adds a type of item to your mod with the specified internal name. This method should be called in Load. You can obtain an instance of ModItem by overriding it then creating an instance of the subclass. The texture parameter follows the same format for texture names of ModLoader.GetTexture.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="item">The item.</param>
		/// <param name="texture">The texture.</param>
		/// <exception cref="System.Exception">You tried to add 2 ModItems with the same name: " + name + ". Maybe 2 classes share a classname but in different namespaces while autoloading or you manually called AddItem with 2 items of the same name.</exception>
		public void AddItem(string name, ModItem item, string texture)
		{
			int id = ItemLoader.ReserveItemID();
			item.Name = name;
			item.item.ResetStats(id);
			item.item.modItem = item;
			if (items.ContainsKey(name))
			{
				throw new Exception("You tried to add 2 ModItems with the same name: " + name + ". Maybe 2 classes share a classname but in different namespaces while autoloading or you manually called AddItem with 2 items of the same name.");
			}
			items[name] = item;
			ItemLoader.items.Add(item);
			item.texture = texture;
			item.mod = this;
			item.DisplayName = new ModTranslation(string.Format("ItemName.{0}.{1}", Name, name));
			item.Tooltip = new ModTranslation(string.Format("ItemTooltip.{0}.{1}", Name, name), true);
			if (item.IsQuestFish())
			{
				ItemLoader.questFish.Add(id);
			}
		}

		/// <summary>
		/// Gets the ModItem instance corresponding to the name. Because this method is in the Mod class, conflicts between mods are avoided. Returns null if no ModItem with the given name is found.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public ModItem GetItem(string name)
		{
			if (items.ContainsKey(name))
			{
				return items[name];
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Same as the other GetItem, but assumes that the class name and internal name are the same.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T GetItem<T>() where T : ModItem
		{
			return (T)GetItem(typeof(T).Name);
		}

		/// <summary>
		/// Gets the internal ID / type of the ModItem corresponding to the name. Returns 0 if no ModItem with the given name is found.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public int ItemType(string name)
		{
			ModItem item = GetItem(name);
			if (item == null)
			{
				return 0;
			}
			return item.item.type;
		}

		/// <summary>
		/// Same as the other ItemType, but assumes that the class name and internal name are the same.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public int ItemType<T>() where T : ModItem
		{
			return ItemType(typeof(T).Name);
		}

		/// <summary>
		/// Adds the given GlobalItem instance to this mod with the provided name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="globalItem">The global item.</param>
		public void AddGlobalItem(string name, GlobalItem globalItem)
		{
			globalItem.mod = this;
			globalItem.Name = name;
			this.globalItems[name] = globalItem;
			ItemLoader.globalItems.Add(globalItem);
		}

		/// <summary>
		/// Gets the GlobalItem instance with the given name from this mod.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public GlobalItem GetGlobalItem(string name)
		{
			if (this.globalItems.ContainsKey(name))
			{
				return this.globalItems[name];
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Same as the other GetGlobalItem, but assumes that the class name and internal name are the same.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T GetGlobalItem<T>() where T : GlobalItem
		{
			return (T)GetGlobalItem(typeof(T).Name);
		}

		/// <summary>
		/// Adds the given type of item information storage to the game, using the provided name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="info">The information.</param>
		public void AddItemInfo(string name, ItemInfo info)
		{
			info.mod = this;
			info.Name = name;
			ItemLoader.infoIndexes[Name + ':' + name] = ItemLoader.infoList.Count;
			ItemLoader.infoList.Add(info);
		}

		/// <summary>
		/// Adds an equipment texture of the specified type, internal name, and associated item to your mod. (The item parameter may be null if you don't want to associate an item with the texture.) You can then get the ID for your texture by calling EquipLoader.GetEquipTexture, and using the EquipTexture's Slot property. If the EquipType is EquipType.Body, make sure that you also provide an armTexture and a femaleTexture. Returns the ID / slot that is assigned to the equipment texture.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="type">The type.</param>
		/// <param name="name">The name.</param>
		/// <param name="texture">The texture.</param>
		/// <param name="armTexture">The arm texture.</param>
		/// <param name="femaleTexture">The female texture.</param>
		/// <returns></returns>
		public int AddEquipTexture(ModItem item, EquipType type, string name, string texture,
			string armTexture = "", string femaleTexture = "")
		{
			return AddEquipTexture(new EquipTexture(), item, type, name, texture, armTexture, femaleTexture);
		}

		/// <summary>
		/// Adds an equipment texture of the specified type, internal name, and associated item to your mod. This method is different from the other AddEquipTexture in that you can specify the class of the equipment texture, thus allowing you to override EquipmentTexture's hooks. All other parameters are the same as the other AddEquipTexture.
		/// </summary>
		/// <param name="equipTexture">The equip texture.</param>
		/// <param name="item">The item.</param>
		/// <param name="type">The type.</param>
		/// <param name="name">The name.</param>
		/// <param name="texture">The texture.</param>
		/// <param name="armTexture">The arm texture.</param>
		/// <param name="femaleTexture">The female texture.</param>
		/// <returns></returns>
		public int AddEquipTexture(EquipTexture equipTexture, ModItem item, EquipType type, string name, string texture,
			string armTexture = "", string femaleTexture = "")
		{
			int slot = EquipLoader.ReserveEquipID(type);
			equipTexture.Texture = texture;
			equipTexture.mod = this;
			equipTexture.Name = name;
			equipTexture.Type = type;
			equipTexture.Slot = slot;
			equipTexture.item = item;
			EquipLoader.equipTextures[type][slot] = equipTexture;
			equipTextures[new Tuple<string, EquipType>(name, type)] = equipTexture;
			ModLoader.GetTexture(texture);
			if (type == EquipType.Body)
			{
				EquipLoader.armTextures[slot] = armTexture;
				EquipLoader.femaleTextures[slot] = femaleTexture.Length > 0 ? femaleTexture : texture;
				ModLoader.GetTexture(armTexture);
				ModLoader.GetTexture(femaleTexture);
			}
			if (item != null && (type == EquipType.Head || type == EquipType.Body || type == EquipType.Legs))
			{
				EquipLoader.slotToId[type][slot] = item.item.type;
			}
			return slot;
		}

		/// <summary>
		/// Gets the EquipTexture instance corresponding to the name and EquipType. Returns null if no EquipTexture with the given name and EquipType is found.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		public EquipTexture GetEquipTexture(string name, EquipType type)
		{
			if (equipTextures.ContainsKey(new Tuple<string, EquipType>(name, type)))
			{
				return equipTextures[new Tuple<string, EquipType>(name, type)];
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Gets the slot/ID of the equipment texture corresponding to the given name. Returns -1 if no EquipTexture with the given name is found.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public int GetEquipSlot(string name, EquipType type)
		{
			EquipTexture texture = GetEquipTexture(name, type);
			if (texture == null)
			{
				return -1;
			}
			return texture.Slot;
		}

		/// <summary>
		/// Same as GetEquipSlot, except returns the number as an sbyte (signed byte) for your convenience.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public sbyte GetAccessorySlot(string name, EquipType type)
		{
			return (sbyte)GetEquipSlot(name, type);
		}

		/// <summary>
		/// Assigns a flame texture to the given item added by your mod. Flame textures are drawn when held by the player if the item's "flame" field is set to true. Flame textures are currently only used for torches.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="texture">The texture.</param>
		public void AddFlameTexture(ModItem item, string texture)
		{
			ModLoader.GetTexture(texture);
			item.flameTexture = texture;
		}

		private void AutoloadItem(Type type)
		{
			ModItem item = (ModItem)Activator.CreateInstance(type);
			item.mod = this;
			string name = type.Name;
			string texture = (type.Namespace + "." + type.Name).Replace('.', '/');
			IList<EquipType> equips = new List<EquipType>();
			if (item.Autoload(ref name, ref texture, equips))
			{
				AddItem(name, item, texture);
				if (equips.Count > 0)
				{
					EquipLoader.idToSlot[item.item.type] = new Dictionary<EquipType, int>();
					foreach (EquipType equip in equips)
					{
						string equipTexture = texture + "_" + equip.ToString();
						string armTexture = texture + "_Arms";
						string femaleTexture = texture + "_FemaleBody";
						item.AutoloadEquip(equip, ref equipTexture, ref armTexture, ref femaleTexture);
						int slot = AddEquipTexture(item, equip, name, equipTexture, armTexture, femaleTexture);
						EquipLoader.idToSlot[item.item.type][equip] = slot;
					}
				}
				string flameTexture = texture + "_" + "Flame";
				item.AutoloadFlame(ref flameTexture);
				if (ModLoader.TextureExists(flameTexture))
				{
					AddFlameTexture(item, flameTexture);
				}
			}
		}

		private void AutoloadGlobalItem(Type type)
		{
			GlobalItem globalItem = (GlobalItem)Activator.CreateInstance(type);
			globalItem.mod = this;
			string name = type.Name;
			if (globalItem.Autoload(ref name))
			{
				AddGlobalItem(name, globalItem);
			}
		}

		private void AutoloadItemInfo(Type type)
		{
			ItemInfo itemInfo = (ItemInfo)Activator.CreateInstance(type);
			itemInfo.mod = this;
			string name = type.Name;
			if (itemInfo.Autoload(ref name))
			{
				AddItemInfo(name, itemInfo);
			}
		}

		/// <summary>
		/// Adds a type of dust to your mod with the specified name. Create an instance of ModDust normally, preferably through the constructor of an overriding class. Leave the texture as an empty string to use the vanilla dust sprite sheet.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="dust">The dust.</param>
		/// <param name="texture">The texture.</param>
		public void AddDust(string name, ModDust dust, string texture = "")
		{
			int id = ModDust.ReserveDustID();
			ModDust.dusts.Add(dust);
			dust.Type = id;
			dust.Name = name;
			dust.Texture = texture.Length > 0 ? ModLoader.GetTexture(texture) : Main.dustTexture;
			dust.mod = this;
			dusts[name] = dust;
		}

		/// <summary>
		/// Gets the ModDust of this mod corresponding to the given name. Returns null if no ModDust with the given name is found.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public ModDust GetDust(string name)
		{
			if (dusts.ContainsKey(name))
			{
				return dusts[name];
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Same as the other GetDust, but assumes that the class name and internal name are the same.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T GetDust<T>() where T : ModDust
		{
			return (T)GetDust(typeof(T).Name);
		}

		/// <summary>
		/// Gets the type of the ModDust of this mod with the given name. Returns 0 if no ModDust with the given name is found.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public int DustType(string name)
		{
			ModDust dust = GetDust(name);
			if (dust == null)
			{
				return 0;
			}
			return dust.Type;
		}

		/// <summary>
		/// Same as the other DustType, but assumes that the class name and internal name are the same.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public int DustType<T>() where T : ModDust
		{
			return DustType(typeof(T).Name);
		}

		private void AutoloadDust(Type type)
		{
			ModDust dust = (ModDust)Activator.CreateInstance(type);
			dust.mod = this;
			string name = type.Name;
			string texture = (type.Namespace + "." + type.Name).Replace('.', '/');
			if (dust.Autoload(ref name, ref texture))
			{
				AddDust(name, dust, texture);
			}
		}

		/// <summary>
		/// Adds a type of tile to the game with the specified name and texture.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="tile">The tile.</param>
		/// <param name="texture">The texture.</param>
		public void AddTile(string name, ModTile tile, string texture)
		{
			int id = TileLoader.ReserveTileID();
			tile.Name = name;
			tile.Type = (ushort)id;
			if (tiles.ContainsKey(name))
			{
				throw new Exception("You tried to add 2 ModTile with the same name: " + name + ". Maybe 2 classes share a classname but in different namespaces while autoloading or you manually called AddTile with 2 tiles of the same name.");
			}
			tiles[name] = tile;
			TileLoader.tiles.Add(tile);
			tile.texture = texture;
			tile.mod = this;
		}

		/// <summary>
		/// Gets the ModTile of this mod corresponding to the given name. Returns null if no ModTile with the given name is found.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public ModTile GetTile(string name)
		{
			if (tiles.ContainsKey(name))
			{
				return tiles[name];
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Same as the other GetTile, but assumes that the class name and internal name are the same.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T GetTile<T>() where T : ModTile
		{
			return (T)GetTile(typeof(T).Name);
		}

		/// <summary>
		/// Gets the type of the ModTile of this mod with the given name. Returns 0 if no ModTile with the given name is found.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public int TileType(string name)
		{
			ModTile tile = GetTile(name);
			if (tile == null)
			{
				return 0;
			}
			return (int)tile.Type;
		}

		/// <summary>
		/// Same as the other TileType, but assumes that the class name and internal name are the same.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public int TileType<T>() where T : ModTile
		{
			return TileType(typeof(T).Name);
		}

		/// <summary>
		/// Adds the given GlobalTile instance to this mod with the provided name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="globalTile">The global tile.</param>
		public void AddGlobalTile(string name, GlobalTile globalTile)
		{
			globalTile.mod = this;
			globalTile.Name = name;
			this.globalTiles[name] = globalTile;
			TileLoader.globalTiles.Add(globalTile);
		}

		/// <summary>
		/// Gets the GlobalTile instance with the given name from this mod.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public GlobalTile GetGlobalTile(string name)
		{
			if (this.globalTiles.ContainsKey(name))
			{
				return globalTiles[name];
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Same as the other GetGlobalTile, but assumes that the class name and internal name are the same.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T GetGlobalTile<T>() where T : GlobalTile
		{
			return (T)GetGlobalTile(typeof(T).Name);
		}

		private void AutoloadTile(Type type)
		{
			ModTile tile = (ModTile)Activator.CreateInstance(type);
			tile.mod = this;
			string name = type.Name;
			string texture = (type.Namespace + "." + type.Name).Replace('.', '/');
			if (tile.Autoload(ref name, ref texture))
			{
				AddTile(name, tile, texture);
			}
		}

		private void AutoloadGlobalTile(Type type)
		{
			GlobalTile globalTile = (GlobalTile)Activator.CreateInstance(type);
			globalTile.mod = this;
			string name = type.Name;
			if (globalTile.Autoload(ref name))
			{
				AddGlobalTile(name, globalTile);
			}
		}

		/// <summary>
		/// Manually add a tile entity during Load.
		/// </summary>
		public void AddTileEntity(string name, ModTileEntity entity)
		{
			int id = ModTileEntity.ReserveTileEntityID();
			entity.mod = this;
			entity.Name = name;
			entity.Type = id;
			entity.type = (byte)id;
			tileEntities[name] = entity;
			ModTileEntity.tileEntities.Add(entity);
		}

		/// <summary>
		/// Gets the ModTileEntity of this mod corresponding to the given name. Returns null if no ModTileEntity with the given name is found.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public ModTileEntity GetTileEntity(string name)
		{
			if (tileEntities.ContainsKey(name))
			{
				return tileEntities[name];
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Same as the other GetTileEntity, but assumes that the class name and internal name are the same.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T GetTileEntity<T>() where T : ModTileEntity
		{
			return (T)GetTileEntity(typeof(T).Name);
		}

		/// <summary>
		/// Gets the type of the ModTileEntity of this mod with the given name. Returns -1 if no ModTileEntity with the given name is found.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public int TileEntityType(string name)
		{
			ModTileEntity tileEntity = GetTileEntity(name);
			if (tileEntity == null)
			{
				return -1;
			}
			return tileEntity.Type;
		}

		/// <summary>
		/// Same as the other TileEntityType, but assumes that the class name and internal name are the same.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public int TileEntityType<T>() where T : ModTileEntity
		{
			return TileEntityType(typeof(T).Name);
		}

		private void AutoloadTileEntity(Type type)
		{
			ModTileEntity tileEntity = (ModTileEntity)Activator.CreateInstance(type);
			tileEntity.mod = this;
			string name = type.Name;
			if (tileEntity.Autoload(ref name))
			{
				AddTileEntity(name, tileEntity);
			}
		}

		/// <summary>
		/// Adds a type of wall to the game with the specified name and texture.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="wall">The wall.</param>
		/// <param name="texture">The texture.</param>
		public void AddWall(string name, ModWall wall, string texture)
		{
			int id = WallLoader.ReserveWallID();
			wall.Name = name;
			wall.Type = (ushort)id;
			walls[name] = wall;
			WallLoader.walls.Add(wall);
			wall.texture = texture;
			wall.mod = this;
		}

		/// <summary>
		/// Gets the ModWall of this mod corresponding to the given name. Returns null if no ModWall with the given name is found.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public ModWall GetWall(string name)
		{
			if (walls.ContainsKey(name))
			{
				return walls[name];
			}
			else
			{
				return null;
			}
		}

		public T GetWall<T>() where T : ModWall
		{
			return (T)GetWall(typeof(T).Name);
		}

		/// <summary>
		/// Gets the type of the ModWall of this mod with the given name. Returns 0 if no ModWall with the given name is found.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public int WallType(string name)
		{
			ModWall wall = GetWall(name);
			if (wall == null)
			{
				return 0;
			}
			return (int)wall.Type;
		}

		/// <summary>
		/// Same as the other WallType, but assumes that the class name and internal name are the same.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public int WallType<T>() where T : ModWall
		{
			return WallType(typeof(T).Name);
		}

		/// <summary>
		/// Adds the given GlobalWall instance to this mod with the provided name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="globalWall">The global wall.</param>
		public void AddGlobalWall(string name, GlobalWall globalWall)
		{
			globalWall.mod = this;
			globalWall.Name = name;
			this.globalWalls[name] = globalWall;
			WallLoader.globalWalls.Add(globalWall);
		}

		/// <summary>
		/// Gets the GlobalWall instance with the given name from this mod.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public GlobalWall GetGlobalWall(string name)
		{
			if (this.globalWalls.ContainsKey(name))
			{
				return globalWalls[name];
			}
			else
			{
				return null;
			}
		}

		public T GetGlobalWall<T>() where T : GlobalWall
		{
			return (T)GetGlobalWall(typeof(T).Name);
		}

		private void AutoloadWall(Type type)
		{
			ModWall wall = (ModWall)Activator.CreateInstance(type);
			wall.mod = this;
			string name = type.Name;
			string texture = (type.Namespace + "." + type.Name).Replace('.', '/');
			if (wall.Autoload(ref name, ref texture))
			{
				AddWall(name, wall, texture);
			}
		}

		private void AutoloadGlobalWall(Type type)
		{
			GlobalWall globalWall = (GlobalWall)Activator.CreateInstance(type);
			globalWall.mod = this;
			string name = type.Name;
			if (globalWall.Autoload(ref name))
			{
				AddGlobalWall(name, globalWall);
			}
		}

		/// <summary>
		/// Adds a type of projectile to the game with the specified name and texture.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="projectile">The projectile.</param>
		/// <param name="texture">The texture.</param>
		public void AddProjectile(string name, ModProjectile projectile, string texture)
		{
			int id = ProjectileLoader.ReserveProjectileID();
			projectile.Name = name;
			projectile.projectile.type = id;
			if (projectiles.ContainsKey(name))
			{
				throw new Exception("You tried to add 2 ModProjectile with the same name: " + name + ". Maybe 2 classes share a classname but in different namespaces while autoloading or you manually called AddProjectile with 2 projectiles of the same name.");
			}
			projectiles[name] = projectile;
			ProjectileLoader.projectiles.Add(projectile);
			projectile.texture = texture;
			projectile.mod = this;
			projectile.DisplayName = new ModTranslation(string.Format("ProjectileName.{0}.{1}", Name, name));
		}

		/// <summary>
		/// Gets the ModProjectile of this mod corresponding to the given name. Returns null if no ModProjectile with the given name is found.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public ModProjectile GetProjectile(string name)
		{
			if (projectiles.ContainsKey(name))
			{
				return projectiles[name];
			}
			else
			{
				return null;
			}
		}

		public T GetProjectile<T>() where T : ModProjectile
		{
			return (T)GetProjectile(typeof(T).Name);
		}

		/// <summary>
		/// Gets the type of the ModProjectile of this mod with the given name. Returns 0 if no ModProjectile with the given name is found.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public int ProjectileType(string name)
		{
			ModProjectile projectile = GetProjectile(name);
			if (projectile == null)
			{
				return 0;
			}
			return projectile.projectile.type;
		}

		/// <summary>
		/// Same as the other ProjectileType, but assumes that the class name and internal name are the same.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public int ProjectileType<T>() where T : ModProjectile
		{
			return ProjectileType(typeof(T).Name);
		}

		/// <summary>
		/// Adds the given GlobalProjectile instance to this mod with the provided name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="globalProjectile">The global projectile.</param>
		public void AddGlobalProjectile(string name, GlobalProjectile globalProjectile)
		{
			Type intRefClass = typeof(int).MakeByRefType();
			globalProjectile.mod = this;
			globalProjectile.Name = name;
			this.globalProjectiles[name] = globalProjectile;
			ProjectileLoader.globalProjectiles.Add(globalProjectile);
		}

		/// <summary>
		/// Gets the GlobalProjectile instance with the given name from this mod.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public GlobalProjectile GetGlobalProjectile(string name)
		{
			if (this.globalProjectiles.ContainsKey(name))
			{
				return this.globalProjectiles[name];
			}
			else
			{
				return null;
			}
		}

		public T GetGlobalProjectile<T>() where T : GlobalProjectile
		{
			return (T)GetGlobalProjectile(typeof(T).Name);
		}

		/// <summary>
		/// Adds the given type of projectile information storage to the game, using the provided name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="info">The information.</param>
		public void AddProjectileInfo(string name, ProjectileInfo info)
		{
			info.mod = this;
			info.Name = name;
			ProjectileLoader.infoIndexes[Name + ':' + name] = ProjectileLoader.infoList.Count;
			ProjectileLoader.infoList.Add(info);
		}

		private void AutoloadProjectile(Type type)
		{
			ModProjectile projectile = (ModProjectile)Activator.CreateInstance(type);
			projectile.mod = this;
			string name = type.Name;
			string texture = (type.Namespace + "." + type.Name).Replace('.', '/');
			if (projectile.Autoload(ref name, ref texture))
			{
				AddProjectile(name, projectile, texture);
			}
		}

		private void AutoloadGlobalProjectile(Type type)
		{
			GlobalProjectile globalProjectile = (GlobalProjectile)Activator.CreateInstance(type);
			globalProjectile.mod = this;
			string name = type.Name;
			if (globalProjectile.Autoload(ref name))
			{
				AddGlobalProjectile(name, globalProjectile);
			}
		}

		private void AutoloadProjectileInfo(Type type)
		{
			ProjectileInfo projectileInfo = (ProjectileInfo)Activator.CreateInstance(type);
			projectileInfo.mod = this;
			string name = type.Name;
			if (projectileInfo.Autoload(ref name))
			{
				AddProjectileInfo(name, projectileInfo);
			}
		}

		/// <summary>
		/// Adds a type of NPC to the game with the specified name and texture. Also allows you to give the NPC alternate textures.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="npc">The NPC.</param>
		/// <param name="texture">The texture.</param>
		/// <param name="altTextures">The alt textures.</param>
		public void AddNPC(string name, ModNPC npc, string texture, string[] altTextures = null)
		{
			int id = NPCLoader.ReserveNPCID();
			npc.Name = name;
			npc.npc.type = id;
			if (npcs.ContainsKey(name))
			{
				throw new Exception("You tried to add 2 ModNPC with the same name: " + name + ". Maybe 2 classes share a classname but in different namespaces while autoloading or you manually called AddNPC with 2 npcs of the same name.");
			}
			npcs[name] = npc;
			NPCLoader.npcs.Add(npc);
			npc.texture = texture;
			npc.altTextures = altTextures;
			npc.mod = this;
			npc.DisplayName = new ModTranslation(string.Format("NPCName.{0}.{1}", Name, name));
		}

		/// <summary>
		/// Gets the ModNPC of this mod corresponding to the given name. Returns null if no ModNPC with the given name is found.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public ModNPC GetNPC(string name)
		{
			if (npcs.ContainsKey(name))
			{
				return npcs[name];
			}
			else
			{
				return null;
			}
		}

		public T GetNPC<T>() where T : ModNPC
		{
			return (T)GetNPC(typeof(T).Name);
		}

		/// <summary>
		/// Gets the type of the ModNPC of this mod with the given name. Returns 0 if no ModNPC with the given name is found.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public int NPCType(string name)
		{
			ModNPC npc = GetNPC(name);
			if (npc == null)
			{
				return 0;
			}
			return npc.npc.type;
		}

		/// <summary>
		/// Same as the other NPCType, but assumes that the class name and internal name are the same.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public int NPCType<T>() where T : ModNPC
		{
			return NPCType(typeof(T).Name);
		}

		/// <summary>
		/// Adds the given GlobalNPC instance to this mod with the provided name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="globalNPC">The global NPC.</param>
		public void AddGlobalNPC(string name, GlobalNPC globalNPC)
		{
			globalNPC.mod = this;
			globalNPC.Name = name;
			this.globalNPCs[name] = globalNPC;
			NPCLoader.globalNPCs.Add(globalNPC);
		}

		/// <summary>
		/// Gets the GlobalNPC instance with the given name from this mod.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public GlobalNPC GetGlobalNPC(string name)
		{
			if (this.globalNPCs.ContainsKey(name))
			{
				return this.globalNPCs[name];
			}
			else
			{
				return null;
			}
		}

		public T GetGlobalNPC<T>() where T : GlobalNPC
		{
			return (T)GetGlobalNPC(typeof(T).Name);
		}

		/// <summary>
		/// Adds the given type of NPC information storage to the game, using the provided name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="info">The information.</param>
		public void AddNPCInfo(string name, NPCInfo info)
		{
			info.mod = this;
			info.Name = name;
			NPCLoader.infoIndexes[Name + ':' + name] = NPCLoader.infoList.Count;
			NPCLoader.infoList.Add(info);
		}

		/// <summary>
		/// Assigns a head texture to the given town NPC type.
		/// </summary>
		/// <param name="npcType">Type of the NPC.</param>
		/// <param name="texture">The texture.</param>
		/// <exception cref="Terraria.ModLoader.Exceptions.MissingResourceException"></exception>
		public void AddNPCHeadTexture(int npcType, string texture)
		{
			int slot = NPCHeadLoader.ReserveHeadSlot();
			NPCHeadLoader.heads[texture] = slot;
			if (!Main.dedServ)
			{
				ModLoader.GetTexture(texture);
			}
			else if (Main.dedServ && !ModLoader.FileExists(texture + ".png"))
			{
				throw new MissingResourceException(texture);
			}
			NPCHeadLoader.npcToHead[npcType] = slot;
			NPCHeadLoader.headToNPC[slot] = npcType;
		}

		/// <summary>
		/// Assigns a head texture that can be used by NPCs on the map.
		/// </summary>
		/// <param name="texture">The texture.</param>
		public void AddBossHeadTexture(string texture)
		{
			int slot = NPCHeadLoader.ReserveBossHeadSlot(texture);
			NPCHeadLoader.bossHeads[texture] = slot;
			ModLoader.GetTexture(texture);
		}

		private void AutoloadNPC(Type type)
		{
			ModNPC npc = (ModNPC)Activator.CreateInstance(type);
			npc.mod = this;
			string name = type.Name;
			string texture = (type.Namespace + "." + type.Name).Replace('.', '/');
			string defaultTexture = texture;
			string[] altTextures = new string[0];
			if (npc.Autoload(ref name, ref texture, ref altTextures))
			{
				AddNPC(name, npc, texture, altTextures);
				string headTexture = defaultTexture + "_Head";
				string bossHeadTexture = headTexture + "_Boss";
				npc.AutoloadHead(ref headTexture, ref bossHeadTexture);
				if (ModLoader.TextureExists(headTexture) || (Main.dedServ && ModLoader.FileExists(headTexture + ".png")))
				{
					AddNPCHeadTexture(npc.npc.type, headTexture);
				}
				if (ModLoader.TextureExists(bossHeadTexture))
				{
					AddBossHeadTexture(bossHeadTexture);
					NPCHeadLoader.npcToBossHead[npc.npc.type] = NPCHeadLoader.bossHeads[bossHeadTexture];
				}
			}
		}

		private void AutoloadGlobalNPC(Type type)
		{
			GlobalNPC globalNPC = (GlobalNPC)Activator.CreateInstance(type);
			globalNPC.mod = this;
			string name = type.Name;
			if (globalNPC.Autoload(ref name))
			{
				AddGlobalNPC(name, globalNPC);
			}
		}

		private void AutoloadNPCInfo(Type type)
		{
			NPCInfo npcInfo = (NPCInfo)Activator.CreateInstance(type);
			npcInfo.mod = this;
			string name = type.Name;
			if (npcInfo.Autoload(ref name))
			{
				AddNPCInfo(name, npcInfo);
			}
		}

		/// <summary>
		/// Adds a type of ModPlayer to this mod. All ModPlayer types will be newly created and attached to each player that is loaded.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="player">The player.</param>
		public void AddPlayer(string name, ModPlayer player)
		{
			player.Name = name;
			players[name] = player;
			player.mod = this;
			PlayerHooks.Add(player);
		}

		private void AutoloadPlayer(Type type)
		{
			ModPlayer player = (ModPlayer)Activator.CreateInstance(type);
			player.mod = this;
			string name = type.Name;
			if (player.Autoload(ref name))
			{
				AddPlayer(name, player);
			}
		}

		/// <summary>
		/// Adds a type of buff to the game with the specified internal name and texture.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="buff">The buff.</param>
		/// <param name="texture">The texture.</param>
		public void AddBuff(string name, ModBuff buff, string texture)
		{
			int id = BuffLoader.ReserveBuffID();
			buff.Name = name;
			buff.Type = id;
			if (buffs.ContainsKey(name))
			{
				throw new Exception("You tried to add 2 ModBuff with the same name: " + name + ". Maybe 2 classes share a classname but in different namespaces while autoloading or you manually called AddBuff with 2 buffs of the same name.");
			}
			buffs[name] = buff;
			BuffLoader.buffs.Add(buff);
			buff.texture = texture;
			buff.mod = this;
			buff.DisplayName = new ModTranslation(string.Format("BuffName.{0}.{1}", Name, name));
			buff.Description = new ModTranslation(string.Format("BuffDescription.{0}.{1}", Name, name));
		}

		/// <summary>
		/// Gets the ModBuff of this mod corresponding to the given name. Returns null if no ModBuff with the given name is found.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public ModBuff GetBuff(string name)
		{
			if (buffs.ContainsKey(name))
			{
				return buffs[name];
			}
			else
			{
				return null;
			}
		}

		public T GetBuff<T>() where T : ModBuff
		{
			return (T)GetBuff(typeof(T).Name);
		}

		/// <summary>
		/// Gets the type of the ModBuff of this mod corresponding to the given name. Returns 0 if no ModBuff with the given name is found.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public int BuffType(string name)
		{
			ModBuff buff = GetBuff(name);
			if (buff == null)
			{
				return 0;
			}
			return buff.Type;
		}

		/// <summary>
		/// Same as the other BuffType, but assumes that the class name and internal name are the same.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public int BuffType<T>() where T : ModBuff
		{
			return BuffType(typeof(T).Name);
		}

		/// <summary>
		/// Adds the given GlobalBuff instance to this mod using the provided name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="globalBuff">The global buff.</param>
		public void AddGlobalBuff(string name, GlobalBuff globalBuff)
		{
			globalBuff.mod = this;
			globalBuff.Name = name;
			this.globalBuffs[name] = globalBuff;
			BuffLoader.globalBuffs.Add(globalBuff);
		}

		/// <summary>
		/// Gets the GlobalBuff with the given name from this mod.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public GlobalBuff GetGlobalBuff(string name)
		{
			if (this.globalBuffs.ContainsKey(name))
			{
				return globalBuffs[name];
			}
			else
			{
				return null;
			}
		}

		public T GetGlobalBuff<T>() where T : GlobalBuff
		{
			return (T)GetGlobalBuff(typeof(T).Name);
		}

		private void AutoloadBuff(Type type)
		{
			ModBuff buff = (ModBuff)Activator.CreateInstance(type);
			buff.mod = this;
			string name = type.Name;
			string texture = (type.Namespace + "." + type.Name).Replace('.', '/');
			if (buff.Autoload(ref name, ref texture))
			{
				AddBuff(name, buff, texture);
			}
		}

		private void AutoloadGlobalBuff(Type type)
		{
			GlobalBuff globalBuff = (GlobalBuff)Activator.CreateInstance(type);
			globalBuff.mod = this;
			string name = type.Name;
			if (globalBuff.Autoload(ref name))
			{
				AddGlobalBuff(name, globalBuff);
			}
		}

		private void AutoloadMountData(Type type)
		{
			ModMountData mount = (ModMountData)Activator.CreateInstance(type);
			mount.mod = this;
			string name = type.Name;
			string texture = (type.Namespace + "." + type.Name).Replace('.', '/');
			IDictionary<MountTextureType, string> extraTextures = new Dictionary<MountTextureType, string>();
			foreach (MountTextureType textureType in Enum.GetValues(typeof(MountTextureType)))
			{
				extraTextures[textureType] = texture + "_" + textureType.ToString();
			}
			if (mount.Autoload(ref name, ref texture, extraTextures))
			{
				AddMount(name, mount, texture, extraTextures);
			}
		}

		/// <summary>
		/// Adds the given mount to the game with the given name and texture. The extraTextures dictionary should optionally map types of mount textures to the texture paths you want to include.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="mount">The mount.</param>
		/// <param name="texture">The texture.</param>
		/// <param name="extraTextures">The extra textures.</param>
		public void AddMount(string name, ModMountData mount, string texture,
			IDictionary<MountTextureType, string> extraTextures = null)
		{
			int id;
			if (Mount.mounts == null || Mount.mounts.Length == MountID.Count)
			{
				Mount.Initialize();
			}
			id = MountLoader.ReserveMountID();
			mount.Name = name;
			mount.Type = id;
			mountDatas[name] = mount;
			MountLoader.mountDatas[id] = mount;
			mount.texture = texture;
			mount.mod = this;
			if (extraTextures != null)
			{
				foreach (MountTextureType textureType in Enum.GetValues(typeof(MountTextureType)))
				{
					if (extraTextures.ContainsKey(textureType) && ModLoader.TextureExists(extraTextures[textureType]))
					{
						Texture2D extraTexture = ModLoader.GetTexture(extraTextures[textureType]);
						switch (textureType)
						{
							case MountTextureType.Back:
								mount.mountData.backTexture = extraTexture;
								break;
							case MountTextureType.BackGlow:
								mount.mountData.backTextureGlow = extraTexture;
								break;
							case MountTextureType.BackExtra:
								mount.mountData.backTextureExtra = extraTexture;
								break;
							case MountTextureType.BackExtraGlow:
								mount.mountData.backTextureExtraGlow = extraTexture;
								break;
							case MountTextureType.Front:
								mount.mountData.frontTexture = extraTexture;
								break;
							case MountTextureType.FrontGlow:
								mount.mountData.frontTextureGlow = extraTexture;
								break;
							case MountTextureType.FrontExtra:
								mount.mountData.frontTextureExtra = extraTexture;
								break;
							case MountTextureType.FrontExtraGlow:
								mount.mountData.frontTextureExtraGlow = extraTexture;
								break;
						}
					}
				}
			}
		}

		/// <summary>
		/// Gets the ModMountData instance of this mod corresponding to the given name. Returns null if no ModMountData has the given name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public ModMountData GetMount(string name)
		{
			if (mountDatas.ContainsKey(name))
			{
				return mountDatas[name];
			}
			else
			{
				return null;
			}
		}

		public T GetMount<T>() where T : ModMountData
		{
			return (T)GetMount(typeof(T).Name);
		}

		/// <summary>
		/// Gets the ID of the ModMountData instance corresponding to the given name. Returns 0 if no ModMountData has the given name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public int MountType(string name)
		{
			ModMountData mountData = GetMount(name);
			if (mountData == null)
			{
				return 0;
			}
			return mountData.Type;
		}

		/// <summary>
		/// Same as the other MountType, but assumes that the class name and internal name are the same.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public int MountType<T>() where T : ModMountData
		{
			return MountType(typeof(T).Name);
		}

		/// <summary>
		/// Adds a ModWorld to this mod with the given name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="modWorld">The mod world.</param>
		public void AddModWorld(string name, ModWorld modWorld)
		{
			modWorld.Name = name;
			worlds[name] = modWorld;
			modWorld.mod = this;
			WorldHooks.Add(modWorld);
		}

		private void AutoloadModWorld(Type type)
		{
			ModWorld modWorld = (ModWorld)Activator.CreateInstance(type);
			modWorld.mod = this;
			string name = type.Name;
			if (modWorld.Autoload(ref name))
			{
				AddModWorld(name, modWorld);
			}
		}

		/// <summary>
		/// Gets the ModWorld instance with the given name from this mod.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public ModWorld GetModWorld(string name)
		{
			if (worlds.ContainsKey(name))
			{
				return worlds[name];
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Same as the other GetModWorld, but assumes that the class name and internal name are the same.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T GetModWorld<T>() where T : ModWorld
		{
			return (T)GetModWorld(typeof(T).Name);
		}

		/// <summary>
		/// Adds the given underground background style with the given name to this mod.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="ugBgStyle">The ug bg style.</param>
		public void AddUgBgStyle(string name, ModUgBgStyle ugBgStyle)
		{
			int slot = UgBgStyleLoader.ReserveBackgroundSlot();
			ugBgStyle.mod = this;
			ugBgStyle.Name = name;
			ugBgStyle.Slot = slot;
			ugBgStyles[name] = ugBgStyle;
			UgBgStyleLoader.ugBgStyles.Add(ugBgStyle);
		}

		/// <summary>
		/// Returns the underground background style corresponding to the given name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public ModUgBgStyle GetUgBgStyle(string name)
		{
			if (ugBgStyles.ContainsKey(name))
			{
				return ugBgStyles[name];
			}
			else
			{
				return null;
			}
		}

		public T GetUgBgStyle<T>() where T : ModUgBgStyle
		{
			return (T)GetUgBgStyle(typeof(T).Name);
		}

		private void AutoloadUgBgStyle(Type type)
		{
			ModUgBgStyle ugBgStyle = (ModUgBgStyle)Activator.CreateInstance(type);
			ugBgStyle.mod = this;
			string name = type.Name;
			if (ugBgStyle.Autoload(ref name))
			{
				AddUgBgStyle(name, ugBgStyle);
			}
		}

		/// <summary>
		/// Adds the given surface background style with the given name to this mod.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="surfaceBgStyle">The surface bg style.</param>
		public void AddSurfaceBgStyle(string name, ModSurfaceBgStyle surfaceBgStyle)
		{
			int slot = SurfaceBgStyleLoader.ReserveBackgroundSlot();
			surfaceBgStyle.mod = this;
			surfaceBgStyle.Name = name;
			surfaceBgStyle.Slot = slot;
			surfaceBgStyles[name] = surfaceBgStyle;
			SurfaceBgStyleLoader.surfaceBgStyles.Add(surfaceBgStyle);
		}

		/// <summary>
		/// Returns the surface background style corresponding to the given name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public ModSurfaceBgStyle GetSurfaceBgStyle(string name)
		{
			if (surfaceBgStyles.ContainsKey(name))
			{
				return surfaceBgStyles[name];
			}
			else
			{
				return null;
			}
		}

		public T GetSurfaceBgStyle<T>() where T : ModSurfaceBgStyle
		{
			return (T)GetSurfaceBgStyle(typeof(T).Name);
		}

		/// <summary>
		/// Returns the Slot of the surface background style corresponding to the given name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public int GetSurfaceBgStyleSlot(string name)
		{
			ModSurfaceBgStyle style = GetSurfaceBgStyle(name);
			return style == null ? -1 : style.Slot;
		}

		public int GetSurfaceBgStyleSlot<T>() where T : ModSurfaceBgStyle
		{
			return GetSurfaceBgStyleSlot(typeof(T).Name);
		}

		private void AutoloadSurfaceBgStyle(Type type)
		{
			ModSurfaceBgStyle surfaceBgStyle = (ModSurfaceBgStyle)Activator.CreateInstance(type);
			surfaceBgStyle.mod = this;
			string name = type.Name;
			if (surfaceBgStyle.Autoload(ref name))
			{
				AddSurfaceBgStyle(name, surfaceBgStyle);
			}
		}

		/// <summary>
		/// Adds the given global background style with the given name to this mod.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="globalBgStyle">The global bg style.</param>
		public void AddGlobalBgStyle(string name, GlobalBgStyle globalBgStyle)
		{
			globalBgStyle.mod = this;
			globalBgStyle.Name = name;
			globalBgStyles[name] = globalBgStyle;
			GlobalBgStyleLoader.globalBgStyles.Add(globalBgStyle);
		}

		/// <summary>
		/// Returns the global background style corresponding to the given name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public GlobalBgStyle GetGlobalBgStyle(string name)
		{
			if (globalBgStyles.ContainsKey(name))
			{
				return globalBgStyles[name];
			}
			else
			{
				return null;
			}
		}

		public T GetGlobalBgStyle<T>() where T : GlobalBgStyle
		{
			return (T)GetGlobalBgStyle(typeof(T).Name);
		}


		private void AutoloadGlobalBgStyle(Type type)
		{
			GlobalBgStyle globalBgStyle = (GlobalBgStyle)Activator.CreateInstance(type);
			globalBgStyle.mod = this;
			string name = type.Name;
			if (globalBgStyle.Autoload(ref name))
			{
				AddGlobalBgStyle(name, globalBgStyle);
			}
		}

		/// <summary>
		/// Adds the given water style to the game with the given name, texture path, and block texture path.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="waterStyle">The water style.</param>
		/// <param name="texture">The texture.</param>
		/// <param name="blockTexture">The block texture.</param>
		public void AddWaterStyle(string name, ModWaterStyle waterStyle, string texture, string blockTexture)
		{
			int style = WaterStyleLoader.ReserveStyle();
			waterStyle.mod = this;
			waterStyle.Name = name;
			waterStyle.Type = style;
			waterStyle.texture = texture;
			waterStyle.blockTexture = blockTexture;
			waterStyles[name] = waterStyle;
			WaterStyleLoader.waterStyles.Add(waterStyle);
		}

		/// <summary>
		/// Returns the water style with the given name from this mod.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public ModWaterStyle GetWaterStyle(string name)
		{
			if (waterStyles.ContainsKey(name))
			{
				return waterStyles[name];
			}
			else
			{
				return null;
			}
		}

		public T GetWaterStyle<T>() where T : ModWaterStyle
		{
			return (T)GetWaterStyle(typeof(T).Name);
		}

		private void AutoloadWaterStyle(Type type)
		{
			ModWaterStyle waterStyle = (ModWaterStyle)Activator.CreateInstance(type);
			waterStyle.mod = this;
			string name = type.Name;
			string texture = (type.Namespace + "." + type.Name).Replace('.', '/');
			string blockTexture = texture + "_Block";
			if (waterStyle.Autoload(ref name, ref texture, ref blockTexture))
			{
				AddWaterStyle(name, waterStyle, texture, blockTexture);
			}
		}

		/// <summary>
		/// Adds the given waterfall style to the game with the given name and texture path.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="waterfallStyle">The waterfall style.</param>
		/// <param name="texture">The texture.</param>
		public void AddWaterfallStyle(string name, ModWaterfallStyle waterfallStyle, string texture)
		{
			int slot = WaterfallStyleLoader.ReserveStyle();
			waterfallStyle.mod = this;
			waterfallStyle.Name = name;
			waterfallStyle.Type = slot;
			waterfallStyle.texture = texture;
			waterfallStyles[name] = waterfallStyle;
			WaterfallStyleLoader.waterfallStyles.Add(waterfallStyle);
		}

		/// <summary>
		/// Returns the waterfall style with the given name from this mod.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public ModWaterfallStyle GetWaterfallStyle(string name)
		{
			if (waterfallStyles.ContainsKey(name))
			{
				return waterfallStyles[name];
			}
			else
			{
				return null;
			}
		}

		public T GetWaterfallStyle<T>() where T : ModWaterfallStyle
		{
			return (T)GetWaterfallStyle(typeof(T).Name);
		}

		/// <summary>
		/// Returns the waterfall style corresponding to the given name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public int GetWaterfallStyleSlot(string name)
		{
			ModWaterfallStyle style = GetWaterfallStyle(name);
			return style == null ? -1 : style.Type;
		}

		public int GetWaterfallStyleSlot<T>() where T : ModWaterfallStyle
		{
			return GetWaterfallStyleSlot(typeof(T).Name);
		}

		private void AutoloadWaterfallStyle(Type type)
		{
			ModWaterfallStyle waterfallStyle = (ModWaterfallStyle)Activator.CreateInstance(type);
			waterfallStyle.mod = this;
			string name = type.Name;
			string texture = (type.Namespace + "." + type.Name).Replace('.', '/');
			if (waterfallStyle.Autoload(ref name, ref texture))
			{
				AddWaterfallStyle(name, waterfallStyle, texture);
			}
		}

		/// <summary>
		/// Adds the given texture to the game as a custom gore, with the given custom gore behavior. If no custom gore behavior is provided, the custom gore will have the default vanilla behavior.
		/// </summary>
		/// <param name="texture">The texture.</param>
		/// <param name="modGore">The mod gore.</param>
		public void AddGore(string texture, ModGore modGore = null)
		{
			int id = ModGore.ReserveGoreID();
			ModGore.gores[texture] = id;
			if (modGore != null)
			{
				ModGore.modGores[id] = modGore;
			}
		}

		/// <summary>
		/// Shorthand for calling ModGore.GetGoreSlot(this.Name + '/' + name).
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public int GetGoreSlot(string name)
		{
			return ModGore.GetGoreSlot(Name + '/' + name);
		}

		/// <summary>
		/// Same as the other GetGoreSlot, but assumes that the class name and internal name are the same.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public int GetGoreSlot<T>() where T : ModGore
		{
			return GetGoreSlot(typeof(T).Name);
		}

		private void AutoloadGores(IList<Type> modGores)
		{
			var modGoreNames = modGores.ToDictionary(t => t.Namespace + "." + t.Name);
			foreach (var texture in textures.Keys.Where(t => t.StartsWith("Gores/")))
			{
				ModGore modGore = null;
				Type t;
				if (modGoreNames.TryGetValue(Name + "." + texture.Replace('/', '.'), out t))
					modGore = (ModGore)Activator.CreateInstance(t);

				AddGore(Name + '/' + texture, modGore);
			}
		}

		/// <summary>
		/// Adds the given sound file to the game as the given type of sound and with the given custom sound playing. If no ModSound instance is provided, the custom sound will play in a similar manner as the default vanilla ones.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="soundPath">The sound path.</param>
		/// <param name="modSound">The mod sound.</param>
		public void AddSound(SoundType type, string soundPath, ModSound modSound = null)
		{
			int id = SoundLoader.ReserveSoundID(type);
			SoundLoader.sounds[type][soundPath] = id;
			if (modSound != null)
			{
				SoundLoader.modSounds[type][id] = modSound;
				modSound.sound = ModLoader.GetSound(soundPath);
			}
		}

		/// <summary>
		/// Shorthand for calling SoundLoader.GetSoundSlot(type, this.Name + '/' + name).
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public int GetSoundSlot(SoundType type, string name)
		{
			return SoundLoader.GetSoundSlot(type, Name + '/' + name);
		}

		/// <summary>
		/// Shorthand for calling SoundLoader.GetLegacySoundSlot(type, this.Name + '/' + name).
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public LegacySoundStyle GetLegacySoundSlot(SoundType type, string name)
		{
			return SoundLoader.GetLegacySoundSlot(type, Name + '/' + name);
		}

		private void AutoloadSounds(IList<Type> modSounds)
		{
			var modSoundNames = modSounds.ToDictionary(t => t.Namespace + "." + t.Name);
			foreach (var sound in sounds.Keys.Where(t => t.StartsWith("Sounds/")))
			{
				string substring = sound.Substring("Sounds/".Length);
				SoundType soundType = SoundType.Custom;
				if (substring.StartsWith("Item/"))
				{
					soundType = SoundType.Item;
				}
				else if (substring.StartsWith("NPCHit/"))
				{
					soundType = SoundType.NPCHit;
				}
				else if (substring.StartsWith("NPCKilled/"))
				{
					soundType = SoundType.NPCKilled;
				}
				else if (substring.StartsWith("Music/"))
				{
					soundType = SoundType.Music;
				}
				ModSound modSound = null;
				Type t;
				if (modSoundNames.TryGetValue((Name + '/' + sound).Replace('/', '.'), out t))
					modSound = (ModSound)Activator.CreateInstance(t);

				AddSound(soundType, Name + '/' + sound, modSound);
			}
		}

		/// <summary>
		/// Adds a texture to the list of background textures and assigns it a background texture slot.
		/// </summary>
		/// <param name="texture">The texture.</param>
		public void AddBackgroundTexture(string texture)
		{
			int slot = BackgroundTextureLoader.ReserveBackgroundSlot();
			BackgroundTextureLoader.backgrounds[texture] = slot;
			ModLoader.GetTexture(texture);
		}

		/// <summary>
		/// Gets the texture slot corresponding to the specified texture name. Shorthand for calling BackgroundTextureLoader.GetBackgroundSlot(this.Name + '/' + name).
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public int GetBackgroundSlot(string name)
		{
			return BackgroundTextureLoader.GetBackgroundSlot(Name + '/' + name);
		}

		private void AutoloadBackgrounds()
		{
			foreach (string texture in textures.Keys.Where(t => t.StartsWith("Backgrounds/")))
			{
				AddBackgroundTexture(Name + '/' + texture);
			}
		}

		/// <summary>
		/// Manually add a Global Recipe during Load
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="globalRecipe">The global recipe.</param>
		public void AddGlobalRecipe(string name, GlobalRecipe globalRecipe)
		{
			globalRecipe.Name = name;
			globalRecipes[name] = globalRecipe;
			globalRecipe.mod = this;
			RecipeHooks.Add(globalRecipe);
		}

		private void AutoloadGlobalRecipe(Type type)
		{
			GlobalRecipe globalRecipe = (GlobalRecipe)Activator.CreateInstance(type);
			globalRecipe.mod = this;
			string name = type.Name;
			if (globalRecipe.Autoload(ref name))
			{
				AddGlobalRecipe(name, globalRecipe);
			}
		}

		private void AutoloadCommand(Type type)
		{
			var mc = (ModCommand)Activator.CreateInstance(type);
			mc.mod = this;
			var name = type.Name;
			if (mc.Autoload(ref name))
				AddCommand(name, mc);
		}

		/// <summary>
		/// Gets the global recipe corresponding to the specified name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public GlobalRecipe GetGlobalRecipe(string name)
		{
			if (globalRecipes.ContainsKey(name))
			{
				return globalRecipes[name];
			}
			else
			{
				return null;
			}
		}

		public T GetGlobalRecipe<T>() where T : GlobalRecipe
		{
			return (T)GetGlobalRecipe(typeof(T).Name);
		}

		/// <summary>
		/// Allows you to tie a music ID, and item ID, and a tile ID together to form a music box. When music with the given ID is playing, equipped music boxes have a chance to change their ID to the given item type. When an item with the given item type is equipped, it will play the music that has musicSlot as its ID. When a tile with the given type and Y-frame is nearby, if its X-frame is >= 36, it will play the music that has musicSlot as its ID.
		/// </summary>
		/// <param name="musicSlot">The music slot.</param>
		/// <param name="itemType">Type of the item.</param>
		/// <param name="tileType">Type of the tile.</param>
		/// <param name="tileFrameY">The tile frame y.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// Cannot assign music box to vanilla music ID " + musicSlot
		/// or
		/// Music ID " + musicSlot + " does not exist
		/// or
		/// Cannot assign music box to vanilla item ID " + itemType
		/// or
		/// Item ID " + itemType + " does not exist
		/// or
		/// Cannot assign music box to vanilla tile ID " + tileType
		/// or
		/// Tile ID " + tileType + " does not exist
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// Music ID " + musicSlot + " has already been assigned a music box
		/// or
		/// Item ID " + itemType + " has already been assigned a music
		/// or
		/// or
		/// Y-frame must be divisible by 36
		/// </exception>
		public void AddMusicBox(int musicSlot, int itemType, int tileType, int tileFrameY = 0)
		{
			if (musicSlot < Main.maxMusic)
			{
				throw new ArgumentOutOfRangeException("Cannot assign music box to vanilla music ID " + musicSlot);
			}
			if (musicSlot >= SoundLoader.SoundCount(SoundType.Music))
			{
				throw new ArgumentOutOfRangeException("Music ID " + musicSlot + " does not exist");
			}
			if (itemType < ItemID.Count)
			{
				throw new ArgumentOutOfRangeException("Cannot assign music box to vanilla item ID " + itemType);
			}
			if (ItemLoader.GetItem(itemType) == null)
			{
				throw new ArgumentOutOfRangeException("Item ID " + itemType + " does not exist");
			}
			if (tileType < TileID.Count)
			{
				throw new ArgumentOutOfRangeException("Cannot assign music box to vanilla tile ID " + tileType);
			}
			if (TileLoader.GetTile(tileType) == null)
			{
				throw new ArgumentOutOfRangeException("Tile ID " + tileType + " does not exist");
			}
			if (SoundLoader.musicToItem.ContainsKey(musicSlot))
			{
				throw new ArgumentException("Music ID " + musicSlot + " has already been assigned a music box");
			}
			if (SoundLoader.itemToMusic.ContainsKey(itemType))
			{
				throw new ArgumentException("Item ID " + itemType + " has already been assigned a music");
			}
			if (!SoundLoader.tileToMusic.ContainsKey(tileType))
			{
				SoundLoader.tileToMusic[tileType] = new Dictionary<int, int>();
			}
			if (SoundLoader.tileToMusic[tileType].ContainsKey(tileFrameY))
			{
				string message = "Y-frame " + tileFrameY + " of tile type " + tileType + " has already been assigned a music";
				throw new ArgumentException(message);
			}
			if (tileFrameY % 36 != 0)
			{
				throw new ArgumentException("Y-frame must be divisible by 36");
			}
			SoundLoader.musicToItem[musicSlot] = itemType;
			SoundLoader.itemToMusic[itemType] = musicSlot;
			SoundLoader.tileToMusic[tileType][tileFrameY] = musicSlot;
		}

		/// <summary>
		/// Registers a hotkey with a name and defaultKey. Use the returned ModHotKey to detect when buttons are pressed. Do this in a ModPlayer.ProcessTriggers.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="defaultKey">The default key.</param>
		/// <returns></returns>
		public ModHotKey RegisterHotKey(string name, string defaultKey)
		{
			return ModLoader.RegisterHotKey(this, name, defaultKey);
		}

		/// <summary>
		/// Creates a ModTranslation object that you can use in AddTranslation.
		/// </summary>
		/// <param name="key">The key for the ModTranslation. The full key will be Mods.ModName.key</param>
		public ModTranslation CreateTranslation(string key)
		{
			key = string.Format("Mods.{0}.{1}", Name, key);
			return new ModTranslation(key);
		}

		/// <summary>
		/// Adds a ModTranslation to the game so that you can use Language.GetText to get a LocalizedText.
		/// </summary>
		public void AddTranslation(ModTranslation translation)
		{
			translations[translation.Key] = translation;
		}

		internal void SetupContent()
		{
			foreach (ModItem item in items.Values)
			{
				Main.itemTexture[item.item.type] = ModLoader.GetTexture(item.texture);
				EquipLoader.SetSlot(item.item);
				item.SetStaticDefaults();
				ItemLoader.SetupItemInfo(item.item);
				item.SetDefaults();
				DrawAnimation animation = item.GetAnimation();
				if (animation != null)
				{
					Main.RegisterItemAnimation(item.item.type, animation);
					ItemLoader.animations.Add(item.item.type);
				}
				if (item.flameTexture.Length > 0)
				{
					Main.itemFlameTexture[item.item.type] = ModLoader.GetTexture(item.flameTexture);
				}
			}
			foreach (ModDust dust in dusts.Values)
			{
				dust.SetDefaults();
			}
			foreach (ModTile tile in tiles.Values)
			{
				Main.tileTexture[tile.Type] = ModLoader.GetTexture(tile.texture);
				TileLoader.SetDefaults(tile);
				if (!string.IsNullOrEmpty(tile.chest))
				{
					TileID.Sets.BasicChest[tile.Type] = true;
				}
			}
			foreach (GlobalTile globalTile in globalTiles.Values)
			{
				globalTile.SetDefaults();
			}
			foreach (ModWall wall in walls.Values)
			{
				Main.wallTexture[wall.Type] = ModLoader.GetTexture(wall.texture);
				wall.SetDefaults();
			}
			foreach (GlobalWall globalWall in globalWalls.Values)
			{
				globalWall.SetDefaults();
			}
			foreach (ModProjectile projectile in projectiles.Values)
			{
				Main.projectileTexture[projectile.projectile.type] = ModLoader.GetTexture(projectile.texture);
				Main.projFrames[projectile.projectile.type] = 1;
				projectile.SetStaticDefaults();
				ProjectileLoader.SetupProjectileInfo(projectile.projectile);
				projectile.SetDefaults();
				if (projectile.projectile.hostile)
				{
					Main.projHostile[projectile.projectile.type] = true;
				}
				if (projectile.projectile.aiStyle == 7)
				{
					Main.projHook[projectile.projectile.type] = true;
				}
			}
			foreach (ModNPC npc in npcs.Values)
			{
				Main.npcTexture[npc.npc.type] = ModLoader.GetTexture(npc.texture);
				npc.SetStaticDefaults();
				NPCLoader.SetupNPCInfo(npc.npc);
				npc.SetDefaults();
				if (npc.banner != 0 && npc.bannerItem != 0)
				{
					NPCLoader.bannerToItem[npc.banner] = npc.bannerItem;
				}
				if (npc.npc.lifeMax > 32767 || npc.npc.boss)
				{
					Main.npcLifeBytes[npc.npc.type] = 4;
				}
				else if (npc.npc.lifeMax > 127)
				{
					Main.npcLifeBytes[npc.npc.type] = 2;
				}
				else
				{
					Main.npcLifeBytes[npc.npc.type] = 1;
				}
				int altTextureCount = NPCID.Sets.ExtraTextureCount[npc.npc.type];
				Main.npcAltTextures[npc.npc.type] = new Texture2D[altTextureCount + 1];
				if (altTextureCount > 0)
				{
					Main.npcAltTextures[npc.npc.type][0] = Main.npcTexture[npc.npc.type];
				}
				for (int k = 1; k <= altTextureCount; k++)
				{
					Main.npcAltTextures[npc.npc.type][k] = ModLoader.GetTexture(npc.altTextures[k - 1]);
				}
			}
			foreach (ModMountData modMountData in mountDatas.Values)
			{
				var mountData = modMountData.mountData;
				mountData.modMountData = modMountData;
				MountLoader.SetupMount(mountData);
				Mount.mounts[modMountData.Type] = mountData;
			}
			foreach (ModBuff buff in buffs.Values)
			{
				Main.buffTexture[buff.Type] = ModLoader.GetTexture(buff.texture);
				buff.SetDefaults();
			}
			foreach (ModWaterStyle waterStyle in waterStyles.Values)
			{
				LiquidRenderer.Instance._liquidTextures[waterStyle.Type] = ModLoader.GetTexture(waterStyle.texture);
				Main.liquidTexture[waterStyle.Type] = ModLoader.GetTexture(waterStyle.blockTexture);
			}
			foreach (ModWaterfallStyle waterfallStyle in waterfallStyles.Values)
			{
				Main.instance.waterfallManager.waterfallTexture[waterfallStyle.Type]
					= ModLoader.GetTexture(waterfallStyle.texture);
			}
		}

		internal void UnloadContent()
		{
			Unload();
			recipes.Clear();
			items.Clear();
			globalItems.Clear();
			equipTextures.Clear();
			dusts.Clear();
			tiles.Clear();
			globalTiles.Clear();
			tileEntities.Clear();
			walls.Clear();
			globalWalls.Clear();
			projectiles.Clear();
			globalProjectiles.Clear();
			npcs.Clear();
			globalNPCs.Clear();
			buffs.Clear();
			globalBuffs.Clear();
			worlds.Clear();
			ugBgStyles.Clear();
			surfaceBgStyles.Clear();
			globalBgStyles.Clear();
			waterStyles.Clear();
			waterfallStyles.Clear();
			globalRecipes.Clear();
			translations.Clear();
		}

		/// <summary>
		/// Shorthand for calling ModLoader.GetFileBytes(this.FileName(name)). Note that file extensions are used here.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public byte[] GetFileBytes(string name)
		{
			return File?.GetFile(name);
		}

		/// <summary>
		/// Shorthand for calling ModLoader.FileExists(this.FileName(name)). Note that file extensions are used here.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public bool FileExists(string name)
		{
			return File != null && File.HasFile(name);
		}

		/// <summary>
		/// Shorthand for calling ModLoader.GetTexture(this.FileName(name)).
		/// </summary>
		/// <exception cref="Terraria.ModLoader.Exceptions.MissingResourceException"></exception>
		public Texture2D GetTexture(string name)
		{
			Texture2D t;
			if (!textures.TryGetValue(name, out t))
				throw new MissingResourceException(name);

			return t;
		}

		/// <summary>
		/// Shorthand for calling ModLoader.TextureExists(this.FileName(name)).
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public bool TextureExists(string name)
		{
			return textures.ContainsKey(name);
		}

		/// <summary>
		/// Shorthand for calling ModLoader.AddTexture(this.FileName(name), texture).
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="texture">The texture.</param>
		/// <exception cref="Terraria.ModLoader.Exceptions.ModNameException">Texture already exist: " + name</exception>
		public void AddTexture(string name, Texture2D texture)
		{
			if (TextureExists(name))
				throw new ModNameException("Texture already exist: " + name);

			textures[name] = texture;
		}

		/// <summary>
		/// Shorthand for calling ModLoader.GetSound(this.FileName(name)).
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		/// <exception cref="Terraria.ModLoader.Exceptions.MissingResourceException"></exception>
		public SoundEffect GetSound(string name)
		{
			SoundEffect sound;
			if (!sounds.TryGetValue(name, out sound))
				throw new MissingResourceException(name);

			return sound;
		}

		/// <summary>
		/// Shorthand for calling ModLoader.SoundExists(this.FileName(name)).
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public bool SoundExists(string name)
		{
			return sounds.ContainsKey(name);
		}

		/// <summary>
		/// Gets a SpriteFont loaded from the specified path.
		/// </summary>
		/// <exception cref="Terraria.ModLoader.Exceptions.MissingResourceException"></exception>
		public SpriteFont GetFont(string name)
		{
			SpriteFont font;
			if (!fonts.TryGetValue(name, out font))
				throw new MissingResourceException(name);

			return font;
		}

		/// <summary>
		/// Used to check if a custom SpriteFont exists
		/// </summary>
		public bool FontExists(string name)
		{
			return fonts.ContainsKey(name);
		}

		/// <summary>
		/// Gets an Effect loaded from the specified path.
		/// </summary>
		/// <exception cref="Terraria.ModLoader.Exceptions.MissingResourceException"></exception>
		public Effect GetEffect(string name)
		{
			Effect effect;
			if (!effects.TryGetValue(name, out effect))
				throw new MissingResourceException(name);

			return effect;
		}

		/// <summary>
		/// Used to check if a custom Effect exists
		/// </summary>
		public bool EffectExists(string name)
		{
			return effects.ContainsKey(name);
		}

		/// <summary>
		/// Used for weak inter-mod communication. This allows you to interact with other mods without having to reference their types or namespaces, provided that they have implemented this method.
		/// </summary>
		public virtual object Call(params object[] args)
		{
			return null;
		}

		/// <summary>
		/// Creates a ModPacket object that you can write to and then send between servers and clients.
		/// </summary>
		/// <param name="capacity">The capacity.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception">Cannot get packet for " + Name + " because it does not exist on the other side</exception>
		public ModPacket GetPacket(int capacity = 256)
		{
			if (netID < 0)
				throw new Exception("Cannot get packet for " + Name + " because it does not exist on the other side");

			var p = new ModPacket(MessageID.ModPacket, capacity + 5);
			p.Write(netID);
			return p;
		}
	}
}
