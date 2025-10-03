using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Terraria.DataStructures;
using Terraria.GameContent.UI;
using Terraria.GameContent.UI.States;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.Engine;
using Terraria.ModLoader.Exceptions;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.ModLoader.Utilities;
using Terraria.GameContent.Creative;
using Terraria.Graphics.Effects;
using Terraria.GameContent.Skies;
using Terraria.GameContent;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Terraria.GameContent.Prefixes;
using Terraria.Achievements;

namespace Terraria.ModLoader;

/// <summary>
/// Manages content added by mods.
/// Liaisons between mod content and Terraria's arrays and oversees the Loader classes.
/// </summary>
public static class ModContent
{
	/// <summary> Returns the template instance of the provided content type (not the clone/new instance which gets added to Items/Players/NPCs etc. as the game is played). </summary>
	public static T GetInstance<T>() where T : class
		=> ContentInstance<T>.Instance;

	/// <summary>
	/// Returns all registered content instances that derive from the provided type and that are added by all currently loaded mods.
	/// <br/>This only includes the 'template' instance for each piece of content, not all the clones/new instances which get added to Items/Players/NPCs etc. as the game is played
	/// </summary>
	public static IEnumerable<T> GetContent<T>() where T : ILoadable
		=> ContentCache.GetContentForAllMods<T>();

	/// <summary> Attempts to find the template instance with the specified full name (not the clone/new instance which gets added to Items/Players/NPCs etc. as the game is played). Caching the result is recommended.<para/>This will throw exceptions on failure. </summary>
	/// <exception cref="KeyNotFoundException"/>
	public static T Find<T>(string fullname) where T : IModType => ModTypeLookup<T>.Get(fullname);
	/// <summary> Attempts to find the template instance with the specified name and mod name (not the clone/new instance which gets added to Items/Players/NPCs etc. as the game is played). Caching the result is recommended.<para/>This will throw exceptions on failure. </summary>
	/// <exception cref="KeyNotFoundException"/>
	public static T Find<T>(string modName, string name) where T : IModType => ModTypeLookup<T>.Get(modName, name);

	/// <summary> Safely attempts to find the template instance with the specified full name (not the clone/new instance which gets added to Items/Players/NPCs etc. as the game is played). Caching the result is recommended. </summary>
	/// <returns> Whether or not the requested instance has been found. </returns>
	public static bool TryFind<T>(string fullname, out T value) where T : IModType => ModTypeLookup<T>.TryGetValue(fullname, out value);
	/// <summary> Safely attempts to find the template instance with the specified name and mod name (not the clone/new instance which gets added to Items/Players/NPCs etc. as the game is played). Caching the result is recommended. </summary>
	/// <returns> Whether or not the requested instance has been found. </returns>
	public static bool TryFind<T>(string modName, string name, out T value) where T : IModType => ModTypeLookup<T>.TryGetValue(modName, name, out value);

	private static readonly char[] nameSplitters = new char[] { '/', ' ', ':' };
	public static void SplitName(string name, out string domain, out string subName)
	{
		int slash = name.IndexOfAny(nameSplitters); // slash is the canonical splitter, but we'll accept space and colon for backwards compatibility, just in case
		if (slash < 0)
			throw new MissingResourceException(Language.GetTextValue("tModLoader.LoadErrorMissingModQualifier", name));

		domain = name.Substring(0, slash);
		subName = name.Substring(slash + 1);
	}

	/// <summary>
	/// Retrieves the contents of a file packaged within the .tmod file as a byte array. Should be used mainly for non-<see cref="Asset{T}"/> files. The <paramref name="name"/> should be in the format of "ModFolder/OtherFolders/FileNameWithExtension". Throws an ArgumentException if the mod does not exist. Returns null if the file does not exist.
	/// <para/> A typical usage of this might be to load a text file containing structured data included within your mod. Make sure the txt file is UTF8 encoded and use the following to retrieve file's text contents: <c>string pointsFileContents = Encoding.UTF8.GetString(ModContent.GetFileBytes("MyMod/data/points.txt"));</c>
	/// </summary>
	/// <exception cref="MissingResourceException"></exception>
	public static byte[] GetFileBytes(string name)
	{
		SplitName(name, out string modName, out string subName);

		if (!ModLoader.TryGetMod(modName, out var mod))
			throw new MissingResourceException(Language.GetTextValue("tModLoader.LoadErrorModNotFoundDuringAsset", modName, name));

		return mod.GetFileBytes(subName);
	}

	/// <summary>
	/// Returns whether or not a file with the specified name exists. Note that this includes file extension, the folder path, and must start with the mod name at the start of the path: "ModFolder/OtherFolders/FileNameWithExtension"
	/// </summary>
	public static bool FileExists(string name)
	{
		if (!name.Contains('/'))
			return false;

		SplitName(name, out string modName, out string subName);

		return ModLoader.TryGetMod(modName, out var mod) && mod.FileExists(subName);
	}

	/// <summary>
	/// Gets the asset with the specified name. Throws an Exception if the asset does not exist.
	/// <para/>
	/// Modders may wish to use <c>Mod.Assets.Request</c> where the mod name prefix may be omitted for convenience.
	/// <para/>
	/// <inheritdoc cref="IAssetRepository.Request{T}(string, AssetRequestMode)"/>
	/// </summary>
	/// <param name="name">The path to the asset without extension, including the mod name (or Terraria) for vanilla assets. Eg "ModName/Folder/FileNameWithoutExtension"</param>
	/// <param name="mode">The desired timing for when the asset actually loads. Use ImmediateLoad if you need correct dimensions immediately, such as with UI initialization</param>
	public static Asset<T> Request<T>(string name, AssetRequestMode mode = AssetRequestMode.AsyncLoad) where T : class
	{
		SplitName(name, out string modName, out string subName);

		// Initialize Main.Assets on server in case it hasn't been initialized. This prevents later crashes when checking Terraria assets
		if (Main.dedServ && Main.Assets == null)
			Main.Assets = new AssetRepository(null);

		if (modName == "Terraria")
			return Main.Assets.Request<T>(subName, mode);

		if (!ModLoader.TryGetMod(modName, out var mod))
			throw new MissingResourceException(Language.GetTextValue("tModLoader.LoadErrorModNotFoundDuringAsset", modName, name));

		return mod.Assets.Request<T>(subName, mode);
	}

	/// <summary>
	/// Returns whether or not a asset with the specified name exists.
	/// Includes the mod name prefix like Request
	/// </summary>
	public static bool HasAsset(string name)
	{
		if (Main.dedServ || string.IsNullOrWhiteSpace(name) || !name.Contains('/'))
			return false;

		SplitName(name, out string modName, out string subName);

		if (modName == "Terraria")
			return Main.AssetSourceController.StaticSource.HasAsset(subName);

		return ModLoader.TryGetMod(modName, out var mod) && mod.RootContentSource.HasAsset(subName);
	}

	public static bool RequestIfExists<T>(string name, out Asset<T> asset, AssetRequestMode mode = AssetRequestMode.AsyncLoad) where T : class
	{
		if (!HasAsset(name)) {
			asset = default;
			return false;
		}

		asset = Request<T>(name, mode);
		return true;
	}

	/// <inheritdoc cref="NPCLoader.GetNPC"/>
	public static ModNPC GetModNPC(int type) => NPCLoader.GetNPC(type);

	/// <inheritdoc cref="NPCHeadLoader.GetBossHeadSlot"/>
	public static int GetModBossHeadSlot(string texture) => NPCHeadLoader.GetBossHeadSlot(texture);

	/// <inheritdoc cref="NPCHeadLoader.GetHeadSlot"/>
	public static int GetModHeadSlot(string texture) => NPCHeadLoader.GetHeadSlot(texture);

	/// <inheritdoc cref="ItemLoader.GetItem"/>
	public static ModItem GetModItem(int type) => ItemLoader.GetItem(type);

	/// <inheritdoc cref="DustLoader.GetDust"/>
	public static ModDust GetModDust(int type) => DustLoader.GetDust(type);

	/// <inheritdoc cref="ProjectileLoader.GetProjectile"/>
	public static ModProjectile GetModProjectile(int type) => ProjectileLoader.GetProjectile(type);

	/// <inheritdoc cref="BuffLoader.GetBuff"/>
	public static ModBuff GetModBuff(int type) => BuffLoader.GetBuff(type);

	/// <inheritdoc cref="EquipLoader.GetEquipTexture(EquipType, int)"/>
	public static EquipTexture GetEquipTexture(EquipType type, int slot) => EquipLoader.GetEquipTexture(type, slot);

	/// <inheritdoc cref="MountLoader.GetMount"/>
	public static ModMount GetModMount(int type) => MountLoader.GetMount(type);

	/// <inheritdoc cref="TileLoader.GetTile"/>
	public static ModTile GetModTile(int type) => TileLoader.GetTile(type);

	/// <inheritdoc cref="WallLoader.GetWall"/>
	public static ModWall GetModWall(int type) => WallLoader.GetWall(type);

	/// <summary>
	/// Returns the ModWaterStyle with the given ID.
	/// </summary>
	public static ModWaterStyle GetModWaterStyle(int style) => LoaderManager.Get<WaterStylesLoader>().Get(style);

	/// <summary>
	/// Returns the ModWaterfallStyle with the given ID.
	/// </summary>
	public static ModWaterfallStyle GetModWaterfallStyle(int style) => LoaderManager.Get<WaterFallStylesLoader>().Get(style);

	/// <inheritdoc cref="BackgroundTextureLoader.GetBackgroundSlot(string)"/>
	public static int GetModBackgroundSlot(string texture) => BackgroundTextureLoader.GetBackgroundSlot(texture);

	/// <summary>
	/// Returns the ModSurfaceBackgroundStyle object with the given ID.
	/// </summary>
	public static ModSurfaceBackgroundStyle GetModSurfaceBackgroundStyle(int style) => LoaderManager.Get<SurfaceBackgroundStylesLoader>().Get(style);

	/// <summary>
	/// Returns the ModUndergroundBackgroundStyle object with the given ID.
	/// </summary>
	public static ModUndergroundBackgroundStyle GetModUndergroundBackgroundStyle(int style) => LoaderManager.Get<UndergroundBackgroundStylesLoader>().Get(style);

	/// <summary>
	/// Get the id (type) of a ModGore by class. Assumes one instance per class.
	/// </summary>
	public static int GoreType<T>() where T : ModGore => GetInstance<T>()?.Type ?? 0;

	/// <summary>
	/// Get the id (type) of a ModCloud by class. Assumes one instance per class.
	/// </summary>
	public static int CloudType<T>() where T : ModCloud => GetInstance<T>()?.Type ?? 0;

	/// <summary>
	/// Get the id (type) of a ModItem by class. Assumes one instance per class.
	/// </summary>
	public static int ItemType<T>() where T : ModItem => GetInstance<T>()?.Type ?? 0;

	/// <summary>
	/// Get the id (type) of a ModPrefix by class. Assumes one instance per class.
	/// </summary>
	public static int PrefixType<T>() where T : ModPrefix => GetInstance<T>()?.Type ?? 0;

	/// <summary>
	/// Get the id (type) of a ModRarity by class. Assumes one instance per class.
	/// </summary>
	public static int RarityType<T>() where T : ModRarity => GetInstance<T>()?.Type ?? 0;

	/// <summary>
	/// Get the id (type) of a ModDust by class. Assumes one instance per class.
	/// </summary>
	public static int DustType<T>() where T : ModDust => GetInstance<T>()?.Type ?? 0;

	/// <summary>
	/// Get the id (type) of a ModTile by class. Assumes one instance per class.
	/// </summary>
	public static int TileType<T>() where T : ModTile => GetInstance<T>()?.Type ?? 0;

	/// <summary>
	/// Get the id (type) of a ModPylon by class. Assumes one instance per class.
	/// If nothing is found, returns 0, or the "Forest Pylon" type.
	/// </summary>
	public static TeleportPylonType PylonType<T>() where T : ModPylon => GetInstance<T>()?.PylonType ?? 0;

	/// <summary>
	/// Get the id (type) of a ModTileEntity by class. Assumes one instance per class.
	/// </summary>
	public static int TileEntityType<T>() where T : ModTileEntity => GetInstance<T>()?.Type ?? 0;

	/// <summary>
	/// Get the id (type) of a ModWall by class. Assumes one instance per class.
	/// </summary>
	public static int WallType<T>() where T : ModWall => GetInstance<T>()?.Type ?? 0;

	/// <summary>
	/// Get the id (type) of a ModProjectile by class. Assumes one instance per class.
	/// </summary>
	public static int ProjectileType<T>() where T : ModProjectile => GetInstance<T>()?.Type ?? 0;

	/// <summary>
	/// Get the id (type) of a ModNPC by class. Assumes one instance per class.
	/// </summary>
	public static int NPCType<T>() where T : ModNPC => GetInstance<T>()?.Type ?? 0;

	/// <summary>
	/// Get the id (type) of a ModBuff by class. Assumes one instance per class.
	/// </summary>
	public static int BuffType<T>() where T : ModBuff => GetInstance<T>()?.Type ?? 0;

	/// <summary>
	/// Get the id (type) of a ModMount by class. Assumes one instance per class.
	/// </summary>
	public static int MountType<T>() where T : ModMount => GetInstance<T>()?.Type ?? 0;

	/// <summary>
	/// Get the id (type) of a ModEmoteBubble by class. Assumes one instance per class.
	/// </summary>
	public static int EmoteBubbleType<T>() where T : ModEmoteBubble => GetInstance<T>()?.Type ?? 0;

	private record struct ScopedCleanup(Action Dispose) : IDisposable
	{
		void IDisposable.Dispose() => Dispose();
	}

	internal static void Load(CancellationToken token)
	{
		using var parallelCts = new CancellationTokenSource();
		using var cancelOnExit = new ScopedCleanup(parallelCts.Cancel);
		var jitTask = JITModsAsync(parallelCts.Token);

		CacheVanillaState();

		Interface.loadMods.SetLoadStage("tModLoader.MSLoading", ModLoader.Mods.Length);
		LoadModContent(token, mod => {
			ContentInstance.Register(mod);
			mod.loading = true;
			mod.AutoloadConfig();
			mod.PrepareAssets();
			mod.Autoload();
			mod.Load();
			SystemLoader.OnModLoad(mod);
			SystemLoader.EnsureResizeArraysAttributeStaticCtorsRun(mod);
			mod.loading = false;
		});

		ContentCache.contentLoadingFinished = true;

		jitTask.GetAwaiter().GetResult();

		Interface.loadMods.SetLoadStage("tModLoader.MSResizing");
		ResizeArrays();
		RecipeGroupHelper.CreateRecipeGroupLookups();

		Main.ResourceSetsManager.AddModdedDisplaySets();
		Main.ResourceSetsManager.SetActiveFromOriginalConfigKey();


		Interface.loadMods.SetLoadStage("tModLoader.MSSetupContent", ModLoader.Mods.Length);
		LanguageManager.Instance.ReloadLanguage(resetValuesToKeysFirst: false); // Don't reset values to keys in case any new translations were registered during Load. All mod translations were wiped in Unload anyway
		LoadModContent(token, mod => {
			mod.SetupContent();
		});

		ContentSamples.Initialize();
		TileLoader.PostSetupContent();
		BuffLoader.PostSetupContent();
		BiomeConversionLoader.PostSetupContent();

		Interface.loadMods.SetLoadStage("tModLoader.MSPostSetupContent", ModLoader.Mods.Length);
		LoadModContent(token, mod => {
			mod.PostSetupContent();
			SystemLoader.PostSetupContent(mod);
			mod.TransferAllAssets();
		});

		MemoryTracking.Finish();

		if (Main.dedServ)
			ModNet.AssignNetIDs();

		ModNet.SetModNetDiagnosticsUI(ModLoader.Mods);

		Main.player[255] = new Player();

		BuffLoader.FinishSetup();
		ItemLoader.FinishSetup();
		NPCLoader.FinishSetup();
		PrefixLoader.FinishSetup();
		ProjectileLoader.FinishSetup();
		PylonLoader.FinishSetup();
		TileLoader.FinishSetup();
		WallLoader.FinishSetup();
		EmoteBubbleLoader.FinishSetup();
		AchievementManager.FinishSetup();

		MapLoader.FinishSetup();
		PlantLoader.FinishSetup();
		RarityLoader.FinishSetup();
		Config.ConfigManager.FinishSetup();

		SystemLoader.ModifyGameTipVisibility(Main.gameTips.allTips);

		PlayerInput.reinitialize = true;
		SetupBestiary();
		NPCShopDatabase.Initialize();
		SetupRecipes(token);
		NPCShopDatabase.FinishSetup();
		ContentSamples.RebuildItemCreativeSortingIDsAfterRecipesAreSetUp();
		ItemSorting.SetupWhiteLists();
		LocalizationLoader.FinishSetup();

		MenuLoader.GotoSavedModMenu();
		BossBarLoader.GotoSavedStyle();

		ModOrganizer.SaveLastLaunchedMods();
	}

	internal static void RunEarlyClassConstructors()
	{
		// The server (or client with Main.SkipAssemblyLoad) doesn't naturally init these, and then the constructors get run twice in ResizeArrays
		RuntimeHelpers.RunClassConstructor(typeof(AmmoID.Sets).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(DustID.Sets).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(MountID.Sets).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(NPCHeadID.Sets).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(HairID.Sets).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(PrefixID.Sets).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(PrefixLegacy.ItemSets).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(ArmorIDs.Head.Sets).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(ArmorIDs.Body.Sets).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(ArmorIDs.Legs.Sets).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(ArmorIDs.HandOn.Sets).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(ArmorIDs.HandOff.Sets).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(ArmorIDs.Back.Sets).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(ArmorIDs.Front.Sets).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(ArmorIDs.Shoe.Sets).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(ArmorIDs.Waist.Sets).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(ArmorIDs.Wing.Sets).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(ArmorIDs.Face.Sets).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(ArmorIDs.Beard.Sets).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(ArmorIDs.Balloon.Sets).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(DamageClass.Sets).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(MusicID.Sets).TypeHandle);
	}

	private static async Task JITModsAsync(CancellationToken token)
	{
		var sw = Stopwatch.StartNew();
		foreach (var mod in ModLoader.Mods)
			if (mod.Code != Assembly.GetExecutingAssembly())
				await AssemblyManager.JITModAsync(mod, token).ConfigureAwait(false);

		Logging.tML.Info($"JITModsAsync completed in {sw.ElapsedMilliseconds}ms");
	}

	private static void CacheVanillaState()
	{
		EffectsTracker.CacheVanillaState();
		DamageClassLoader.RegisterDefaultClasses();
		ExtraJumpLoader.RegisterDefaultJumps();
		InfoDisplayLoader.RegisterDefaultDisplays();
		BuilderToggleLoader.RegisterDefaultToggles();
	}

	private static void LoadModContent(CancellationToken token, Action<Mod> loadAction)
	{
		MemoryTracking.Checkpoint();
		int num = 0;
		foreach (var mod in ModLoader.Mods) {
			using var _2 = new ModContent.TrackCurrentlyLoadingMod(mod.Name);
			token.ThrowIfCancellationRequested();
			Interface.loadMods.SetCurrentMod(num++, mod);
			try {
				using var _ = new AssetWaitTracker(mod);

				loadAction(mod);
			}
			catch (Exception e) {
				e.Data["mod"] = mod.Name;
				throw;
			}
			finally {
				MemoryTracking.Update(mod.Name);
			}
		}
	}

	private static void SetupBestiary()
	{
		//Bestiary DB
		var bestiaryDatabase = new BestiaryDatabase();
		new BestiaryDatabaseNPCsPopulator().Populate(bestiaryDatabase);
		Main.BestiaryDB = bestiaryDatabase;
		ContentSamples.RebuildBestiarySortingIDsByBestiaryDatabaseContents(bestiaryDatabase);

		//Drops DB
		var itemDropDatabase = new ItemDropDatabase();
		itemDropDatabase.Populate();
		Main.ItemDropsDB = itemDropDatabase;

		//Update the bestiary DB with the drops DB.
		bestiaryDatabase.Merge(Main.ItemDropsDB);

		//Etc

		if (!Main.dedServ)
			Main.BestiaryUI = new UIBestiaryTest(Main.BestiaryDB);

		Main.ItemDropSolver = new ItemDropResolver(itemDropDatabase);
		Main.BestiaryTracker = new BestiaryUnlocksTracker();
	}

	private static void SetupRecipes(CancellationToken token)
	{
		Interface.loadMods.SetLoadStage("tModLoader.MSAddingRecipes");
		for (int k = 0; k < Recipe.maxRecipes; k++) {
			token.ThrowIfCancellationRequested();
			Main.recipe[k] = new Recipe();
		}

		Recipe.numRecipes = 0;
		RecipeGroupHelper.ResetRecipeGroups();
		RecipeLoader.setupRecipes = true;
		Recipe.SetupRecipes();
		RecipeLoader.setupRecipes = false;
		ContentSamples.FixItemsAfterRecipesAreAdded();
		RecipeLoader.PostSetupRecipes();
	}

	internal static void UnloadModContent()
	{
		MenuLoader.Unload(); //do this early, so modded menus won't be active when unloaded
		CloudLoader.Unload();

		int i = 0;
		foreach (var mod in ModLoader.Mods.Reverse()) {
			Interface.loadMods.SetCurrentMod(i++, mod);

			try {
				MonoModHooks.RemoveAll(mod);
				mod.Close();
				mod.UnloadContent();
			}
			catch (Exception e) {
				e.Data["mod"] = mod.Name;
				throw;
			}
		}
	}

	//TODO: Unhardcode ALL of this.
	internal static void Unload()
	{
		MonoModHooks.Clear();
		TypeCaching.Clear();
		ContentCache.Unload();
		ItemLoader.Unload();
		EquipLoader.Unload();
		PrefixLoader.Unload();
		DustLoader.Unload();
		TileLoader.Unload();
		PylonLoader.Unload();
		WallLoader.Unload();
		ProjectileLoader.Unload();

		NPCLoader.Unload();
		NPCHeadLoader.Unload();

		BossBarLoader.Unload();
		PlayerLoader.Unload();
		BuffLoader.Unload();
		MountLoader.Unload();
		RarityLoader.Unload();
		DamageClassLoader.Unload();
		InfoDisplayLoader.Unload();
		BuilderToggleLoader.Unload();
		ExtraJumpLoader.Unload();
		GoreLoader.Unload();
		PlantLoader.UnloadPlants();
		HairLoader.Unload();
		EmoteBubbleLoader.Unload();
		BiomeConversionLoader.Unload();
		AchievementManager.Unload();

		ResourceOverlayLoader.Unload();
		ResourceDisplaySetLoader.Unload();

		LoaderManager.Unload();

		GlobalBackgroundStyleLoader.Unload();
		PlayerDrawLayerLoader.Unload();
		MapLayerLoader.Unload();
		SystemLoader.Unload();
		ResizeArrays(true);
		for (int k = 0; k < Recipe.maxRecipes; k++) {
			Main.recipe[k] = new Recipe();
		}
		Recipe.numRecipes = 0;
		RecipeGroupHelper.ResetRecipeGroups();
		Recipe.SetupRecipes();
		TileEntity.manager.Reset();
		MapLoader.UnloadModMap();
		ItemSorting.SetupWhiteLists();
		RecipeLoader.Unload();
		CommandLoader.Unload();
		TagSerializer.Reload();
		ModNet.Unload();
		Config.ConfigManager.Unload();
		CustomCurrencyManager.Initialize();
		EffectsTracker.RemoveModEffects();
		ItemTrader.ChlorophyteExtractinator = ItemTrader.CreateChlorophyteExtractinator();
		Main.gameTips.Reset();

		// ItemID.Search = IdDictionary.Create<ItemID, short>();
		// NPCID.Search = IdDictionary.Create<NPCID, short>();
		// ProjectileID.Search = IdDictionary.Create<ProjectileID, short>();
		// TileID.Search = IdDictionary.Create<TileID, ushort>();
		// WallID.Search = IdDictionary.Create<WallID, ushort>();
		// BuffID.Search = IdDictionary.Create<BuffID, int>();

		CreativeItemSacrificesCatalog.Instance.Initialize();
		ContentSamples.CreativeResearchItemPersistentIdOverride.Clear();
		ContentSamples.Initialize();
		SetupBestiary();

		LocalizationLoader.Unload();

		CleanupModReferences();
	}

	//TODO: Unhardcode ALL of this.
	private static void ResizeArrays(bool unloading = false)
	{
		SetFactory.ResizeArrays(unloading);
		DamageClassLoader.ResizeArrays();
		ExtraJumpLoader.ResizeArrays();
		ItemLoader.ResizeArrays(unloading);
		EquipLoader.ResizeAndFillArrays();
		PrefixLoader.ResizeArrays();
		DustLoader.ResizeArrays();
		TileLoader.ResizeArrays(unloading);
		WallLoader.ResizeArrays(unloading);
		ProjectileLoader.ResizeArrays(unloading);
		NPCLoader.ResizeArrays(unloading);
		NPCHeadLoader.ResizeAndFillArrays();
		MountLoader.ResizeArrays();
		BuffLoader.ResizeArrays();
		PlayerLoader.ResizeArrays();
		PlayerDrawLayerLoader.ResizeArrays();
		MapLayerLoader.ResizeArrays();
		HairLoader.ResizeArrays();
		EmoteBubbleLoader.ResizeArrays();
		BuilderToggleLoader.ResizeArrays();
		BiomeConversionLoader.ResizeArrays();
		SystemLoader.ResizeArrays(unloading);

		if (!Main.dedServ) {
			GlobalBackgroundStyleLoader.ResizeAndFillArrays(unloading);
			GoreLoader.ResizeAndFillArrays();
			CloudLoader.ResizeAndFillArrays(unloading);
		}

		LoaderManager.ResizeArrays();

		// TML: Due to Segments.PlayerSegment._player being initialized way before any mods are loaded, calling methods on this player (which vanilla does) will crash since no ModPlayers are set up for it, so reinitialize it
		if (!Main.dedServ)
			SkyManager.Instance["CreditsRoll"] = new CreditsRollSky();
	}

	/// <summary>
	/// Several arrays and other fields hold references to various classes from mods, we need to clean them up to give properly coded mods a chance to be completely free of references
	/// so that they can be collected by the garbage collection. For most things eventually they will be replaced during gameplay, but we want the old instance completely gone quickly.
	/// </summary>
	internal static void CleanupModReferences()
	{
		// Clear references to ModPlayer instances
		for (int i = 0; i < Main.player.Length; i++) {
			Main.player[i] = new Player();
			// player.whoAmI is only set for active players
		}

		Main.dresserInterfaceDummy = null;
		Main.clientPlayer = new Player();
		Main.ActivePlayerFileData = new Terraria.IO.PlayerFileData();
		Main._characterSelectMenu._playerList?.Clear();
		Main.PlayerList.Clear();

		if (ItemSlot.singleSlotArray[0] != null) {
			ItemSlot.singleSlotArray[0] = new Item();
		}

		WorldGen.ClearGenerationPasses(); // Clean up modded generation passes
	}

	public static Stream OpenRead(string assetName, bool newFileStream = false)
	{
		if (!assetName.StartsWith("tmod:"))
			return File.OpenRead(assetName);

		SplitName(assetName.Substring(5).Replace('\\', '/'), out var modName, out var entryPath);
		return ModLoader.GetMod(modName).GetFileStream(entryPath, newFileStream);
	}

	internal static void TransferCompletedAssets()
	{
		if (!ModLoader.isLoading) {
			DoTransferCompletedAssets();
			return;
		}

		// During mod loading, spin wait for assets to transfer. Note that SpinWait.SpinUntil uses a low resolution timer (~15ms on windows) so it may spin for up to that long.
		// If any assets are queued we will continue to spend main thread time to transfer assets. We accept some frame stutter to hopefully sync up with repeated ImmediateLoad calls and get better throughput
		var sw = Stopwatch.StartNew();
		while (sw.ElapsedMilliseconds < 15 && SpinWait.SpinUntil(DoTransferCompletedAssets, millisecondsTimeout: 1)) { }
	}

	private static bool DoTransferCompletedAssets()
	{
		bool transferredAnything = false;
		foreach (var mod in ModLoader.Mods) {
			if (mod.Assets is AssetRepository assetRepo && !assetRepo.IsDisposed)
				transferredAnything |= assetRepo.TransferCompletedAssets();
		}

		return false;
	}

	private class AssetWaitTracker : IDisposable
	{
		public static readonly TimeSpan MinReportThreshold = TimeSpan.FromMilliseconds(10);

		private readonly Mod mod;
		private TimeSpan total;

		public AssetWaitTracker(Mod mod)
		{
			this.mod = mod;
			AssetRepository.OnBlockingLoadCompleted += AddWaitTime;
		}

		private void AddWaitTime(TimeSpan t) => total += t;

		public void Dispose()
		{
			AssetRepository.OnBlockingLoadCompleted -= AddWaitTime;
			if (total > MinReportThreshold) {
				Logging.tML.Warn(
					$"{mod.Name} spent {(int)total.TotalMilliseconds}ms blocking on asset loading. " +
					$"Avoid using {nameof(AssetRequestMode)}.{nameof(AssetRequestMode.ImmediateLoad)} during mod loading where possible");
			}
		}
	}

	[ThreadStatic]
	private static string currentMod = null;
	internal static string CurrentlyLoadingMod => currentMod ?? "Unknown";

	public ref struct TrackCurrentlyLoadingMod
	{
		private string prev;
		public TrackCurrentlyLoadingMod(string mod)
		{
			prev = currentMod;
			currentMod = mod;
		}
		public void Dispose() => currentMod = prev;
	}
}
