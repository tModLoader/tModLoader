using log4net;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Audio;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.Exceptions;
using System.Linq;
using Terraria.ModLoader.Config;
using ReLogic.Content;
using ReLogic.Content.Sources;
using ReLogic.Graphics;

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
		internal TmodFile File { get; set; }
		/// <summary>
		/// The assembly code this is loaded when tModLoader loads this mod.
		/// </summary>
		public Assembly Code { get; internal set; }
		/// <summary>
		/// A logger with this mod's name for easy logging.
		/// </summary>
		public ILog Logger { get; internal set; }

		/// <summary>
		/// Stores the name of the mod. This name serves as the mod's identification, and also helps with saving everything your mod adds. By default this returns the name of the folder that contains all your code and stuff.
		/// </summary>
		public virtual string Name => File.Name;
		/// <summary>
		/// The version of tModLoader that was being used when this mod was built.
		/// </summary>
		public Version TModLoaderVersion { get; internal set; }
		/// <summary>
		/// This version number of this mod.
		/// </summary>
		public virtual Version Version => File.Version;

		public ModProperties Properties { get; protected set; } = ModProperties.AutoLoadAll;
		/// <summary>
		/// The ModSide that controls how this mod is synced between client and server.
		/// </summary>
		public ModSide Side { get; internal set; }
		/// <summary>
		/// The display name of this mod in the Mods menu.
		/// </summary>
		public string DisplayName { get; internal set; }

		public AssetRepository Assets { get; private set; }

		public IContentSource RootContentSource { get; private set; }

		internal short netID = -1;
		public bool IsNetSynced => netID >= 0;

		private IDisposable fileHandle;

		public GameContent.Bestiary.ModSourceBestiaryInfoElement ModSourceBestiaryInfoElement;

		internal void AutoloadConfig()
		{
			if (Code == null)
				return;

			// TODO: Attribute to specify ordering of ModConfigs
			foreach (Type type in Code.GetTypes().OrderBy(type => type.FullName))
			{
				if (type.IsAbstract)
				{
					continue;
				}
				if (type.IsSubclassOf(typeof(ModConfig)))
				{
					var mc = (ModConfig)Activator.CreateInstance(type);
					// Skip loading ClientSide on Main.dedServ?
					if (mc.Mode == ConfigScope.ServerSide && (Side == ModSide.Client || Side == ModSide.NoSync)) // Client and NoSync mods can't have ServerSide ModConfigs. Server can, but won't be synced.
						throw new Exception($"The ModConfig {mc.Name} can't be loaded because the config is ServerSide but this Mods ModSide isn't Both or Server");
					if (mc.Mode == ConfigScope.ClientSide && Side == ModSide.Server) // Doesn't make sense. 
						throw new Exception($"The ModConfig {mc.Name} can't be loaded because the config is ClientSide but this Mods ModSide is Server");
					mc.Mod = this;
					var name = type.Name;
					if (mc.Autoload(ref name))
						AddConfig(name, mc);
				}
			}
		}

		public void AddConfig(string name, ModConfig mc)
		{
			mc.Name = name;
			mc.Mod = this;

			ConfigManager.Add(mc);
			ContentInstance.Register(mc);
		}

		public void AddContent<T>() where T:ILoadable, new() => AddContent(new T());

		public void AddContent(ILoadable instance){
			if (!loading)
				throw new Exception(Language.GetTextValue("tModLoader.LoadErrorNotLoading"));
			instance.Load(this);
			content.Add(instance);
			ContentInstance.Register(instance);
		}

		public IEnumerable<ILoadable> GetContent() => content;

		public IEnumerable<T> GetContent<T>() where T : ILoadable => content.OfType<T>();

		/// <summary>
		/// Adds an equipment texture of the specified type, internal name, and associated item to your mod. 
		/// (The item parameter may be null if you don't want to associate an item with the texture.) 
		/// You can then get the ID for your texture by calling EquipLoader.GetEquipTexture, and using the EquipTexture's Slot property. 
		/// If the EquipType is EquipType.Body, make sure that you also provide an armTexture and a femaleTexture. 
		/// Returns the ID / slot that is assigned to the equipment texture.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="type">The type.</param>
		/// <param name="name">The name.</param>
		/// <param name="texture">The texture.</param>
		/// <param name="armTexture">The arm texture (for body slots).</param>
		/// <param name="femaleTexture">The female texture (for body slots), if missing the regular body texture is used.</param>
		/// <returns></returns>
		public int AddEquipTexture(ModItem item, EquipType type, string texture) {
			return AddEquipTexture(new EquipTexture(), item, type, texture);
		}

		/// <summary>
		/// Adds an equipment texture of the specified type, internal name, and associated item to your mod. 
		/// This method is different from the other AddEquipTexture in that you can specify the class of the equipment texture, thus allowing you to override EquipmentTexture's hooks. 
		/// All other parameters are the same as the other AddEquipTexture.
		/// </summary>
		/// <param name="equipTexture">The equip texture.</param>
		/// <param name="item">The item.</param>
		/// <param name="type">The type.</param>
		/// <param name="name">The name.</param>
		/// <param name="texture">The texture.</param>
		/// <param name="armTexture">The arm texture (for body slots).</param>
		/// <param name="femaleTexture">The female texture (for body slots), if missing the regular body texture is used.</param>
		/// <returns></returns>
		public int AddEquipTexture(EquipTexture equipTexture, ModItem item, EquipType type, string texture) {
			if (!loading)
				throw new Exception("AddEquipTexture can only be called from Mod.Load or Mod.Autoload");

			ModContent.Request<Texture2D>(texture); //ensure texture exists

			equipTexture.Texture = texture;
			equipTexture.Mod = this;
			equipTexture.Name = item.Name;
			equipTexture.Type = type;
			equipTexture.Item = item;
			int slot = equipTexture.Slot = EquipLoader.ReserveEquipID(type);

			EquipLoader.equipTextures[type][slot] = equipTexture;
			equipTextures[Tuple.Create(item.Name, type)] = equipTexture;

			if (type == EquipType.Body) {
				if (!ModContent.HasAsset(item.FemaleTexture)) {
					EquipLoader.femaleTextures[slot] = texture;
				}
				else {
					EquipLoader.femaleTextures[slot] = item.FemaleTexture;
				}
				ModContent.Request<Texture2D>(item.ArmTexture); //ensure texture exists
				EquipLoader.armTextures[slot] = item.ArmTexture;
			}

			if (!EquipLoader.idToSlot.TryGetValue(item.Type, out IDictionary<EquipType, int> slots))
				EquipLoader.idToSlot[item.Type] = slots = new Dictionary<EquipType, int>();

			slots[type] = slot;
			if (type == EquipType.Head || type == EquipType.Body || type == EquipType.Legs)
				EquipLoader.slotToId[type][slot] = item.Type;

			return slot;
		}

		/// <summary>
		/// Gets the EquipTexture instance corresponding to the name and EquipType. Returns null if no EquipTexture with the given name and EquipType is found.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		public EquipTexture GetEquipTexture(string name, EquipType type) =>
			equipTextures.TryGetValue(Tuple.Create(name, type), out var texture) ? texture : null;

		/// <summary>
		/// Gets the slot/ID of the equipment texture corresponding to the given name. Returns -1 if no EquipTexture with the given name is found.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="type"></param>
		/// <returns></returns>
		public int GetEquipSlot(string name, EquipType type) => GetEquipTexture(name, type)?.Slot ?? -1;

		/// <summary>
		/// Same as GetEquipSlot, except returns the number as an sbyte (signed byte) for your convenience.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="type"></param>
		/// <returns></returns>
		public sbyte GetAccessorySlot(string name, EquipType type) => (sbyte)GetEquipSlot(name, type);

		/// <summary> Attempts to find the content instance from this mod with the specified name. Caching the result is recommended.<para/>This will throw exceptions on failure. </summary>
		/// <exception cref="KeyNotFoundException"/>
		public T Find<T>(string name) where T : IModType => ModContent.Find<T>(Name, name);

		/// <summary> Safely attempts to find the content instance from this mod with the specified name. Caching the result is recommended. </summary>
		/// <returns> Whether or not the requested instance has been found. </returns>
		public bool TryFind<T>(string name, out T value) where T : IModType => ModContent.TryFind(Name, name, out value);

		/// <summary>
		/// Assigns a head texture to the given town NPC type.
		/// </summary>
		/// <param name="npcType">Type of the NPC.</param>
		/// <param name="texture">The texture.</param>
		/// <returns>The boss head txture slot</returns>
		/// <exception cref="MissingResourceException"></exception>
		public int AddNPCHeadTexture(int npcType, string texture) {
			if (!loading)
				throw new Exception("AddNPCHeadTexture can only be called from Mod.Load or Mod.Autoload");

			int slot = NPCHeadLoader.ReserveHeadSlot();

			NPCHeadLoader.heads[texture] = slot;
			
			if (!Main.dedServ) {
				ModContent.Request<Texture2D>(texture);
			}
			/*else if (Main.dedServ && !(ModLoader.FileExists(texture + ".png") || ModLoader.FileExists(texture + ".rawimg")))
			{
				throw new MissingResourceException(texture);
			}*/

			NPCHeadLoader.npcToHead[npcType] = slot;
			NPCHeadLoader.headToNPC[slot] = npcType;
			return slot;
		}

		/// <summary>
		/// Assigns a head texture that can be used by NPCs on the map.
		/// </summary>
		/// <param name="texture">The texture.</param>
		/// <param name="npcType">An optional npc id for NPCID.Sets.BossHeadTextures</param>
		/// <returns>The boss head txture slot</returns>
		public int AddBossHeadTexture(string texture, int npcType = -1) {
			if (!loading)
				throw new Exception("AddBossHeadTexture can only be called from Mod.Load or Mod.Autoload");

			int slot = NPCHeadLoader.ReserveBossHeadSlot(texture);
			NPCHeadLoader.bossHeads[texture] = slot;
			ModContent.Request<Texture2D>(texture);
			if (npcType >= 0) {
				NPCHeadLoader.npcToBossHead[npcType] = slot;
			}
			return slot;
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
		public void AddMusicBox(int musicSlot, int itemType, int tileType, int tileFrameY = 0) {
			throw new NotImplementedException();

			if (!loading)
				throw new Exception("AddMusicBox can only be called from Mod.Load or Mod.Autoload");

			if (Main.audioSystem == null)
				return;

			if (musicSlot < Main.maxMusic) {
				throw new ArgumentOutOfRangeException("Cannot assign music box to vanilla music ID " + musicSlot);
			}
			/*if (musicSlot >= SoundLoader.SoundCount(SoundType.Music)) {
				throw new ArgumentOutOfRangeException("Music ID " + musicSlot + " does not exist");
			}*/
			if (itemType < ItemID.Count) {
				throw new ArgumentOutOfRangeException("Cannot assign music box to vanilla item ID " + itemType);
			}
			if (ItemLoader.GetItem(itemType) == null) {
				throw new ArgumentOutOfRangeException("Item ID " + itemType + " does not exist");
			}
			if (tileType < TileID.Count) {
				throw new ArgumentOutOfRangeException("Cannot assign music box to vanilla tile ID " + tileType);
			}
			if (TileLoader.GetTile(tileType) == null) {
				throw new ArgumentOutOfRangeException("Tile ID " + tileType + " does not exist");
			}
			if (SoundLoader.musicToItem.ContainsKey(musicSlot)) {
				throw new ArgumentException("Music ID " + musicSlot + " has already been assigned a music box");
			}
			if (SoundLoader.itemToMusic.ContainsKey(itemType)) {
				throw new ArgumentException("Item ID " + itemType + " has already been assigned a music");
			}
			if (!SoundLoader.tileToMusic.ContainsKey(tileType)) {
				SoundLoader.tileToMusic[tileType] = new Dictionary<int, int>();
			}
			if (SoundLoader.tileToMusic[tileType].ContainsKey(tileFrameY)) {
				string message = "Y-frame " + tileFrameY + " of tile type " + tileType + " has already been assigned a music";
				throw new ArgumentException(message);
			}
			if (tileFrameY % 36 != 0) {
				throw new ArgumentException("Y-frame must be divisible by 36");
			}
			SoundLoader.musicToItem[musicSlot] = itemType;
			SoundLoader.itemToMusic[itemType] = musicSlot;
			SoundLoader.tileToMusic[tileType][tileFrameY] = musicSlot;
		}

		/// <summary>
		/// Retrieve contents of files within the tmod file
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public byte[] GetFileBytes(string name) => File?.GetBytes(name);

		/// <summary>
		/// Retrieve contents of files within the tmod file
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public Stream GetFileStream(string name, bool newFileStream = false) => File?.GetStream(name, newFileStream);

		public bool FileExists(string name) => File != null && File.HasFile(name);

		public bool HasAsset(string assetName) => RootContentSource.HasAsset(assetName);

		public bool RequestAssetIfExists<T>(string assetName, out Asset<T> asset) where T : class {
			if (!HasAsset(assetName)) {
				asset = default;
				return false;
			}

			asset = Assets.Request<T>(assetName);
			return true;
		}

		/// <summary>
		/// Shorthand for calling ModContent.GetMusic(this.FileName(name)).
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		/// <exception cref="MissingResourceException"></exception>
		public Music GetMusic(string name) {
			if (!musics.TryGetValue(name, out var music))
				throw new MissingResourceException(name);

			return music;
		}

		/// <summary>
		/// Shorthand for calling ModLoader.MusicExists(this.FileName(name)).
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public bool MusicExists(string name) => musics.ContainsKey(name);

		/// <summary>
		/// Used for weak inter-mod communication. This allows you to interact with other mods without having to reference their types or namespaces, provided that they have implemented this method.
		/// </summary>
		public virtual object Call(params object[] args) {
			return null;
		}

		/// <summary>
		/// Creates a ModPacket object that you can write to and then send between servers and clients.
		/// </summary>
		/// <param name="capacity">The capacity.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception">Cannot get packet for " + Name + " because it does not exist on the other side</exception>
		public ModPacket GetPacket(int capacity = 256) {
			if (netID < 0)
				throw new Exception("Cannot get packet for " + Name + " because it does not exist on the other side");

			var p = new ModPacket(MessageID.ModPacket, capacity + 5);
			if (ModNet.NetModCount < 256)
				p.Write((byte)netID);
			else
				p.Write(netID);

			p.netID = netID;
			return p;
		}

		public ModConfig GetConfig(string name)
		{
			if (ConfigManager.Configs.TryGetValue(this, out List<ModConfig> configs)) {
				return configs.Single(x => x.Name == name);
			}
			return null;
		}

		public Recipe CreateRecipe(int result, int amount = 1) => Recipe.Create(this, result, amount);
	}
}
