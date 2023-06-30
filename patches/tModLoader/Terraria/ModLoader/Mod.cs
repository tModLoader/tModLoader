using log4net;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.Exceptions;
using System.Linq;
using Terraria.ModLoader.Config;
using ReLogic.Content;
using ReLogic.Content.Sources;

namespace Terraria.ModLoader;

/// <summary>
/// Mod is an abstract class that you will override. It serves as a central place from which the mod's contents are stored. It provides methods for you to use or override.
/// </summary>
public partial class Mod
{
	/// <summary>
	/// The TmodFile object created when tModLoader reads this mod.
	/// </summary>
	internal TmodFile File { get; set; }
	/// <summary>
	/// The assembly code this is loaded when tModLoader loads this mod. <br/>
	/// Do NOT call <see cref="Assembly.GetTypes"/> on this as it will error out if the mod uses the <see cref="ExtendsFromModAttribute"/> attribute to inherit from weakly referenced mods. Use <see cref="AssemblyManager.GetLoadableTypes(Assembly)"/> instead.
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

	public List<string> TranslationForMods { get; internal set; }

	/// <summary>
	/// Whether or not this mod will autoload content by default. Autoloading content means you do not need to manually add content through methods.
	/// </summary>
	public bool ContentAutoloadingEnabled { get; init; } = true;
	/// <summary>
	/// Whether or not this mod will automatically add images in the Gores folder as gores to the game, along with any ModGore classes that share names with the images. This means you do not need to manually call Mod.AddGore.
	/// </summary>
	public bool GoreAutoloadingEnabled { get; init; } = true;
	/// <summary>
	/// Whether or not this mod will automatically add music to the game. All supported audio files in a folder or subfolder of a folder named "Music" will be autoloaded as music.
	/// </summary>
	public bool MusicAutoloadingEnabled { get; init; } = true;
	/// <summary>
	/// Whether or not this mod will automatically add images in the Backgrounds folder as background textures to the game. This means you do not need to manually call Mod.AddBackgroundTexture.
	/// </summary>
	public bool BackgroundAutoloadingEnabled { get; init; } = true;


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
	public short NetID => netID;
	public bool IsNetSynced => netID >= 0;

	private IDisposable fileHandle;

	public GameContent.Bestiary.ModSourceBestiaryInfoElement ModSourceBestiaryInfoElement;

	public PreJITFilter PreJITFilter { get; protected set; } = new PreJITFilter();

	internal void AutoloadConfig()
	{
		if (Code == null)
			return;

		// TODO: Attribute to specify ordering of ModConfigs
		foreach (Type type in AssemblyManager.GetLoadableTypes(Code).OrderBy(type => type.FullName)) {
			if (type.IsAbstract) {
				continue;
			}
			if (type.IsSubclassOf(typeof(ModConfig))) {
				var mc = (ModConfig)Activator.CreateInstance(type, true)!;
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
		ModTypeLookup<ModConfig>.Register(mc);
	}

	/// <summary> Call this to manually add a content instance of the specified type (with a parameterless constructor) to the game. </summary>
	/// <returns> true if the instance was successfully added </returns>
	public bool AddContent<T>() where T : ILoadable, new() => AddContent(new T());

	/// <summary> Call this to manually add the given content instance to the game. </summary>
	/// <param name="instance"> The content instance to add </param>
	/// <returns> true if the instance was successfully added </returns>
	public bool AddContent(ILoadable instance)
	{
		if (!loading)
			throw new Exception(Language.GetTextValue("tModLoader.LoadErrorNotLoading"));


		if (!instance.IsLoadingEnabled(this))
			return false;

		instance.Load(this);
		content.Add(instance);
		ContentInstance.Register(instance);
		return true;
	}

	/// <summary>
	/// Returns all registered content instances that are added by this mod.
	/// <br/>This only includes the 'template' instance for each piece of content, not all the clones/new instances which get added to Items/Players/NPCs etc. as the game is played
	/// </summary>
	public IEnumerable<ILoadable> GetContent() => content;

	/// <summary>
	/// Returns all registered content instances that derive from the provided type that are added by this mod.
	/// <br/>This only includes the 'template' instance for each piece of content, not all the clones/new instances which get added to Items/Players/NPCs etc. as the game is played
	/// </summary>
	public IEnumerable<T> GetContent<T>() where T : ILoadable => content.OfType<T>();

	/// <summary> Attempts to find the template instance from this mod with the specified name (not the clone/new instance which gets added to Items/Players/NPCs etc. as the game is played). Caching the result is recommended.<para/>This will throw exceptions on failure. </summary>
	/// <exception cref="KeyNotFoundException"/>
	public T Find<T>(string name) where T : IModType => ModContent.Find<T>(Name, name);

	/// <summary> Safely attempts to find the template instance from this mod with the specified name (not the clone/new instance which gets added to Items/Players/NPCs etc. as the game is played). Caching the result is recommended. </summary>
	/// <returns> Whether or not the requested instance has been found. </returns>
	public bool TryFind<T>(string name, out T value) where T : IModType => ModContent.TryFind(Name, name, out value);

	/// <summary>
	/// Creates a localization key following the pattern of "Mods.{ModName}.{suffix}". Use this with <see cref="Language.GetOrRegister(string, Func{string})"/> to retrieve a <see cref="LocalizedText"/> for custom localization keys. Custom localization keys need to be registered during the mod loading process to appear automtaically in the localization files.
	/// </summary>
	/// <param name="suffix"></param>
	/// <returns></returns>
	public string GetLocalizationKey(string suffix) => $"Mods.{Name}.{suffix}";

	/// <summary>
	/// Assigns a head texture to the given town NPC type.
	/// </summary>
	/// <param name="npcType">Type of the NPC.</param>
	/// <param name="texture">The texture.</param>
	/// <returns>The boss head texture slot</returns>
	/// <exception cref="MissingResourceException"></exception>
	public int AddNPCHeadTexture(int npcType, string texture)
	{
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
		return slot;
	}

	/// <summary>
	/// Assigns a head texture that can be used by NPCs on the map.
	/// </summary>
	/// <param name="texture">The texture.</param>
	/// <param name="npcType">An optional npc id for NPCID.Sets.BossHeadTextures</param>
	/// <returns>The boss head texture slot</returns>
	public int AddBossHeadTexture(string texture, int npcType = -1)
	{
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
	/// Retrieves the names of every file packaged into this mod.
	/// Note that this includes extensions, and for images the extension will always be <c>.rawimg</c>.
	/// </summary>
	/// <returns></returns>
	public List<string> GetFileNames() => File?.GetFileNames();

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
	/// <param name="newFileStream"></param>
	/// <returns></returns>
	public Stream GetFileStream(string name, bool newFileStream = false) => File?.GetStream(name, newFileStream);

	public bool FileExists(string name) => File != null && File.HasFile(name);

	public bool HasAsset(string assetName) => RootContentSource.HasAsset(assetName);

	public bool RequestAssetIfExists<T>(string assetName, out Asset<T> asset) where T : class
	{
		if (!HasAsset(assetName)) {
			asset = default;
			return false;
		}

		asset = Assets.Request<T>(assetName);
		return true;
	}

	/// <summary>
	/// Used for weak inter-mod communication. This allows you to interact with other mods without having to reference their types or namespaces, provided that they have implemented this method.<br/>
	/// The <see href="https://github.com/tModLoader/tModLoader/wiki/Expert-Cross-Mod-Content">Expert Cross Mod Content Guide</see> explains how to use this hook to implement and utilize cross-mod capabilities.
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

	[Obsolete("Use Recipe.Create", true)]
	public Recipe CreateRecipe(int result, int amount = 1) => Recipe.Create(result, amount);

	[Obsolete("Use Recipe.Clone", true)]
	public Recipe CloneRecipe(Recipe recipe) => recipe.Clone();
}
