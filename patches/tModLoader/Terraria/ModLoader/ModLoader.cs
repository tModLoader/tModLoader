using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using ReLogic.Content;
using ReLogic.OS;
using Terraria.GameContent.Liquid;
using Terraria.Initializers;
using Terraria.Localization;
using Terraria.ModLoader.Assets;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.Engine;
using Terraria.ModLoader.Exceptions;
using Terraria.ModLoader.UI;
using Terraria.Social.Steam;

namespace Terraria.ModLoader;

/// <summary>
/// This serves as the central class which loads mods. It contains many static fields and methods related to mods and their contents.
/// </summary>
public static class ModLoader
{
	// Stores the most recent version of tModLoader launched. Can be used for migration.
	public static Version LastLaunchedTModLoaderVersion;
	// Stores the most recent sha for a launched official preview build. Used for ShowWhatsNew
	public static string LastLaunchedTModLoaderAlphaSha;
	public static bool ShowWhatsNew;
	public static bool PreviewFreezeNotification;
	public static bool DownloadedDependenciesOnStartup;
	public static bool ShowFirstLaunchWelcomeMessage;
	public static bool SeenFirstLaunchModderWelcomeMessage;
	public static bool WarnedFamilyShare;
	public static bool WarnedFamilyShareDontShowAgain;
	public static Version LastPreviewFreezeNotificationSeen;
	public static int LatestNewsTimestamp;

	// Update this name if doing an upgrade
	public static bool BetaUpgradeWelcomed144;

	public static string versionedName => (BuildInfo.Purpose != BuildInfo.BuildPurpose.Stable) ? BuildInfo.versionedNameDevFriendly : BuildInfo.versionedName;

#if NETCORE
	public static string CompressedPlatformRepresentation => (Platform.IsWindows ? "w" : (Platform.IsLinux ? "l" : "m")) + (InstallVerifier.DistributionPlatform == DistributionPlatform.GoG ? "g" : "s") + "c";
#else
	public static string CompressedPlatformRepresentation => "w" + (InstallVerifier.IsGoG ? "g" : "s") + "n";
#endif

	public static string ModPath => ModOrganizer.modPath;

	private static readonly IDictionary<string, Mod> modsByName = new Dictionary<string, Mod>(StringComparer.OrdinalIgnoreCase);

	internal static readonly string modBrowserPublicKey = "<RSAKeyValue><Modulus>oCZObovrqLjlgTXY/BKy72dRZhoaA6nWRSGuA+aAIzlvtcxkBK5uKev3DZzIj0X51dE/qgRS3OHkcrukqvrdKdsuluu0JmQXCv+m7sDYjPQ0E6rN4nYQhgfRn2kfSvKYWGefp+kqmMF9xoAq666YNGVoERPm3j99vA+6EIwKaeqLB24MrNMO/TIf9ysb0SSxoV8pC/5P/N6ViIOk3adSnrgGbXnFkNQwD0qsgOWDks8jbYyrxUFMc4rFmZ8lZKhikVR+AisQtPGUs3ruVh4EWbiZGM2NOkhOCOM4k1hsdBOyX2gUliD0yjK5tiU3LBqkxoi2t342hWAkNNb4ZxLotw==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
	internal static string modBrowserPassphrase = "";

	internal static bool autoReloadAndEnableModsLeavingModBrowser = true; // Currently unimplemented
	internal static bool autoReloadRequiredModsLeavingModsScreen = true;
	internal static bool removeForcedMinimumZoom;
	internal static int attackSpeedScalingTooltipVisibility = 1; // Shown, WhenNonZero, Hidden
	internal static bool notifyNewMainMenuThemes = true;
	internal static bool showNewUpdatedModsInfo = true;
	internal static bool showConfirmationWindowWhenEnableDisableAllMods = true;
	internal static bool skipLoad;
	internal static bool preparingServerSidePublish;
	internal static Action OnSuccessfulLoad;

	internal static bool isLoading;

	public static Mod[] Mods { get; private set; } = new Mod[0];

	internal static AssetRepository ManifestAssets { get; set; } //This is used for keeping track of assets that are loaded either from the application's resources, or created directly from a texture.
	internal static AssemblyResourcesContentSource ManifestContentSource { get; set; }

	/// <summary> Gets the instance of the Mod with the specified name. This will throw an exception if the mod cannot be found so it should only be used for mods known to be enabled, such as a strong mod dependency.
	/// <para/> Use <see cref="TryGetMod(string, out Mod)"/> instead if the mod might not be enabled. </summary>
	/// <exception cref="KeyNotFoundException"/>
	public static Mod GetMod(string name) => modsByName[name];

	/// <summary> Safely attempts to get the instance of the Mod with the specified name. </summary>
	/// <returns> Whether or not the requested instance has been found. </returns>
	public static bool TryGetMod(string name, out Mod result) => modsByName.TryGetValue(name, out result);

	/// <summary> Safely checks whether or not a mod with the specified internal name is currently loaded. </summary>
	/// <returns> Whether or not a mod with the provided internal name has been found. </returns>
	public static bool HasMod(string name) => modsByName.ContainsKey(name);

	internal static void EngineInit()
	{
		FileAssociationSupport.UpdateFileAssociation();
		FolderShortcutSupport.UpdateFolderShortcuts();
		MonoModHooks.Initialize();
		FNAFixes.Init();
		LoaderManager.AutoLoad();
	}

	internal static void PrepareAssets()
	{
		ManifestContentSource = new AssemblyResourcesContentSource(
			Assembly.GetExecutingAssembly(),
			excludedStartingPaths: new[] { "Terraria/ModLoader/Templates/" }
		);
		ManifestAssets = new AssetRepository(AssetInitializer.assetReaderCollection, new[] { ManifestContentSource }) {
			AssetLoadFailHandler = Main.OnceFailedLoadingAnAsset
		};
	}

	internal static void BeginLoad(CancellationToken token) => Task.Run(() => Load(token));

	private static void Load(CancellationToken token = default)
	{
		if (isLoading)
			throw new Exception("Load called twice");
		isLoading = true;

		if (!Unload())
			return;

		var availableMods = ModOrganizer.FindMods(logDuplicates: true);
		try {
			var sw = Stopwatch.StartNew();

			var modsToLoad = ModOrganizer.SelectAndSortMods(availableMods, token);
			var modInstances = AssemblyManager.InstantiateMods(modsToLoad, token);
			modInstances.Insert(0, new ModLoaderMod());
			Mods = modInstances.ToArray();
			foreach (var mod in Mods)
				modsByName[mod.Name] = mod;

			ModContent.Load(token);

			Logging.tML.Info($"Mod Load Completed in {sw.ElapsedMilliseconds}ms");

			if (preparingServerSidePublish)
				Environment.Exit(0);

			if (OnSuccessfulLoad != null) {
				OnSuccessfulLoad();
			}
			else {
				Main.menuMode = 0;
			}
		}
		catch when (token.IsCancellationRequested) {
			// cancel needs to reload with ModLoaderMod and all others skipped
			skipLoad = true;
			OnSuccessfulLoad += () => Main.menuMode = Interface.modsMenuID;

			isLoading = false;
			Load(); // don't provide a token, loading just ModLoaderMod should be quick
		}
		catch (Exception e) {
			var responsibleMods = new List<string>();
			if (e.Data.Contains("mod"))
				responsibleMods.Add((string)e.Data["mod"]);
			if (e.Data.Contains("mods"))
				responsibleMods.AddRange((IEnumerable<string>)e.Data["mods"]);
			responsibleMods.Remove("ModLoader");

			if (responsibleMods.Count == 0 && AssemblyManager.FirstModInStackTrace(new StackTrace(e), out var stackMod))
				responsibleMods.Add(stackMod);

			var msg = Language.GetTextValue("tModLoader.LoadError", string.Join(", ", responsibleMods));
			if (responsibleMods.Count == 1) {
				var mod = availableMods.FirstOrDefault(m => m.Name == responsibleMods[0]); //use First rather than Single, incase of "Two mods with the same name" error message from ModOrganizer (#639)
				if (mod != null)
					msg += $" v{mod.Version}";
				if (mod != null && mod.tModLoaderVersion.MajorMinorBuild() != BuildInfo.tMLVersion.MajorMinorBuild())
					msg += "\n" + Language.GetTextValue("tModLoader.LoadErrorVersionMessage", mod.tModLoaderVersion, versionedName);
				else if (mod != null)
					// if the mod exists, and the MajorMinorBuild() is identical, then assume it is an error in the Steam install/deployment - Solxan
					SteamedWraps.QueueForceValidateSteamInstall();

				if (e is Exceptions.JITException)
					msg += "\n" + $"The mod will need to be updated to match the current tModLoader version, or may be incompatible with the version of some of your other mods. Click the '{Language.GetTextValue("tModLoader.OpenWebHelp")}' button to learn more.";
			}
			if (responsibleMods.Count > 0)
				msg += "\n" + Language.GetTextValue("tModLoader.LoadErrorDisabled");
			else
				msg += "\n" + Language.GetTextValue("tModLoader.LoadErrorCulpritUnknown");

			if (e is ReflectionTypeLoadException reflectionTypeLoadException)
				msg += "\n\n" + string.Join("\n", reflectionTypeLoadException.LoaderExceptions.Select(x => x.Message));

			if (e.Data.Contains("contentType") && e.Data["contentType"] is Type contentType)
				msg += "\n" + Language.GetTextValue("tModLoader.LoadErrorContentType", contentType.FullName);

			foreach (var mod in responsibleMods) {
				DisableModAndDependents(mod);
			}
			void DisableModAndDependents(string mod)
			{
				DisableMod(mod);

				var dependents = availableMods
					.Where(m => IsEnabled(m.Name) && m.properties.RefNames(includeWeak: false).Any(refName => refName.Equals(mod)))
					.Select(m => m.Name);

				foreach (var dependent in dependents) {
					DisableModAndDependents(dependent);
				}
			}

			Logging.tML.Error(msg, e);

			isLoading = false; // disable loading flag, because server will just instantly retry reload
			DisplayLoadError(msg, e, e.Data.Contains("fatal"), responsibleMods.Count == 0);
		}
		finally {
			isLoading = false;
			OnSuccessfulLoad = null;
			skipLoad = false;
			ModNet.NetReloadActive = false;
			//TODO: FUTURE
			//GOGModUpdateChecker.CheckModUpdates();
		}
	}

	internal static void Reload()
	{
		if (Main.dedServ)
			Load();
		else
			Main.menuMode = Interface.loadModsID;
	}

	internal static bool Unload()
	{
		try {
			var weakModRefs = GetWeakModRefs();
			Mods_Unload();
			WarnModsStillLoaded(weakModRefs);
			return true;
		}
		catch (Exception e) {
			var msg = Language.GetTextValue("tModLoader.UnloadError");

			if (e.Data.Contains("mod"))
				msg += "\n" + Language.GetTextValue("tModLoader.DefensiveUnload", e.Data["mod"]);

			Logging.tML.Fatal(msg, e);
			DisplayLoadError(msg, e, true);

			return false;
		}
	}

	internal static bool IsUnloadedModStillAlive(string name) => AssemblyManager.OldLoadContexts().Contains(name);

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static WeakReference<Mod>[] GetWeakModRefs() => Mods.Select(x => new WeakReference<Mod>(x)).ToArray();

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Mods_Unload()
	{
		Interface.loadMods.SetLoadStage("tModLoader.MSUnloading", Mods.Length);

		WorldGen.clearWorld();
		ModContent.UnloadModContent();

		Mods = new Mod[0];
		modsByName.Clear();
		ModContent.Unload();
		MemoryTracking.Clear();
		Thread.MemoryBarrier();
		AssemblyManager.Unload();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void WarnModsStillLoaded(IReadOnlyList<WeakReference<Mod>> weakModRefs)
	{
		foreach (var alcName in AssemblyManager.OldLoadContexts().Distinct()) {
			if (weakModRefs.Any(modRef => modRef.TryGetTarget(out var mod) && mod.Name == alcName)) {
				Logging.tML.WarnFormat($"{alcName} mod class still using memory. Some content references have probably not been cleared. Use a heap dump to figure out why.");
			}
			else {
				Logging.tML.WarnFormat($"{alcName} AssemblyLoadContext still using memory. Some classes are being held by Terraria or another mod. Use a heap dump to figure out why.");
			}
		}
	}

	private static void DisplayLoadError(string msg, Exception e, bool fatal, bool continueIsRetry = false)
	{
		msg += "\n\n" + (e.Data.Contains("hideStackTrace") ? e.Message : e.ToString());

		if (Main.dedServ) {
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(msg);
			Console.ResetColor();

			if (preparingServerSidePublish)
				Environment.Exit(-1);
			else if (fatal) {
				Console.WriteLine("Press any key to exit...");
				Console.ReadKey();
				Environment.Exit(-1);
			}
			else {
				Reload();
			}
		}
		else {
			string HelpLink = e.HelpLink;
			if(HelpLink == null && e is MultipleException multipleException)
				HelpLink = multipleException.InnerExceptions.Where(x => x.HelpLink != null).Select(x => x.HelpLink).FirstOrDefault();
			Interface.errorMessage.Show(msg,
				gotoMenu: fatal ? -1 : Interface.reloadModsID,
				webHelpURL: HelpLink,
				continueIsRetry: continueIsRetry,
				showSkip: !fatal);
		}
	}

	// TODO: This doesn't work on mono for some reason. Investigate.
	public static bool IsSignedBy(TmodFile mod, string xmlPublicKey)
	{
		var f = new RSAPKCS1SignatureDeformatter();
		var v = AsymmetricAlgorithm.Create("RSA");
		f.SetHashAlgorithm("SHA1");
		v.FromXmlString(xmlPublicKey);
		f.SetKey(v);
		return f.VerifySignature(mod.Hash, mod.Signature);
	}

	/// <summary>A cached list of enabled mods (not necessarily currently loaded or even installed), mirroring the enabled.json file.</summary>
	private static HashSet<string> _enabledMods;
	internal static HashSet<string> EnabledMods => _enabledMods ??= ModOrganizer.LoadEnabledMods();

	internal static bool IsEnabled(string modName) => EnabledMods.Contains(modName);
	internal static void EnableMod(string modName) => SetModEnabled(modName, true);
	internal static void DisableMod(string modName) => SetModEnabled(modName, false);
	internal static void SetModEnabled(string modName, bool active)
	{
		if (active == IsEnabled(modName))
			return;

		Logging.tML.Info($"{(active ? "Enabling" : "Disabling")} Mod: {modName}");
		if (active)
			EnabledMods.Add(modName);
		else
			EnabledMods.Remove(modName);

		ModOrganizer.SaveEnabledMods();
	}

	internal static void DisableAllMods()
	{
		Logging.tML.InfoFormat($"Disabling All Mods: {string.Join(", ", EnabledMods)}");
		EnabledMods.Clear();
		ModOrganizer.SaveEnabledMods();
	}

	internal static void SaveConfiguration()
	{
		Main.Configuration.Put("ModBrowserPassphrase", modBrowserPassphrase);
		Main.Configuration.Put("DownloadModsFromServers", ModNet.downloadModsFromServers);
		Main.Configuration.Put("AutomaticallyReloadAndEnableModsLeavingModBrowser", autoReloadAndEnableModsLeavingModBrowser);
		Main.Configuration.Put("AutomaticallyReloadRequiredModsLeavingModsScreen", autoReloadRequiredModsLeavingModsScreen);
		Main.Configuration.Put("RemoveForcedMinimumZoom", removeForcedMinimumZoom);
		Main.Configuration.Put(nameof(attackSpeedScalingTooltipVisibility).ToUpperInvariant(), attackSpeedScalingTooltipVisibility);
		Main.Configuration.Put("AvoidGithub", UI.ModBrowser.UIModBrowser.AvoidGithub);
		Main.Configuration.Put("AvoidImgur", UI.ModBrowser.UIModBrowser.AvoidImgur);
		Main.Configuration.Put(nameof(UI.ModBrowser.UIModBrowser.EarlyAutoUpdate), UI.ModBrowser.UIModBrowser.EarlyAutoUpdate);
		Main.Configuration.Put("ShowModMenuNotifications", notifyNewMainMenuThemes);
		Main.Configuration.Put("ShowNewUpdatedModsInfo", showNewUpdatedModsInfo);
		Main.Configuration.Put("ShowConfirmationWindowWhenEnableDisableAllMods", showConfirmationWindowWhenEnableDisableAllMods);
		Main.Configuration.Put("LastSelectedModMenu", MenuLoader.LastSelectedModMenu);
		Main.Configuration.Put("KnownMenuThemes", MenuLoader.KnownMenuSaveString);
		Main.Configuration.Put("BossBarStyle", BossBarLoader.lastSelectedStyle);
		Main.Configuration.Put("SeenFirstLaunchModderWelcomeMessage", SeenFirstLaunchModderWelcomeMessage);

		Main.Configuration.Put("LastLaunchedTModLoaderVersion", BuildInfo.tMLVersion.ToString());
		Main.Configuration.Put(nameof(BetaUpgradeWelcomed144), BetaUpgradeWelcomed144);
		Main.Configuration.Put(nameof(LastLaunchedTModLoaderAlphaSha), BuildInfo.IsPreview && BuildInfo.CommitSHA != "unknown" ? BuildInfo.CommitSHA : LastLaunchedTModLoaderAlphaSha);
		Main.Configuration.Put(nameof(LastPreviewFreezeNotificationSeen), LastPreviewFreezeNotificationSeen.ToString());
		Main.Configuration.Put(nameof(ModOrganizer.ModPackActive), ModOrganizer.ModPackActive);
		Main.Configuration.Put(nameof(LatestNewsTimestamp), LatestNewsTimestamp);
		Main.Configuration.Put(nameof(WarnedFamilyShareDontShowAgain), WarnedFamilyShareDontShowAgain);
		Main.Configuration.Put(nameof(ModsMenuSortMode), Enum.GetName(typeof(ModsMenuSortMode), Interface.modsMenu.sortMode));

		Main.Configuration.Put("LiquidSlopeFix", LiquidEdgeRenderer.Enabled);
	}

	internal static void LoadConfiguration()
	{
		Main.Configuration.Get("ModBrowserPassphrase", ref modBrowserPassphrase);
		Main.Configuration.Get("DownloadModsFromServers", ref ModNet.downloadModsFromServers);
		Main.Configuration.Get("AutomaticallyReloadAndEnableModsLeavingModBrowser", ref autoReloadAndEnableModsLeavingModBrowser);
		Main.Configuration.Get("AutomaticallyReloadRequiredModsLeavingModsScreen", ref autoReloadRequiredModsLeavingModsScreen);
		Main.Configuration.Get("RemoveForcedMinimumZoom", ref removeForcedMinimumZoom);
		Main.Configuration.Get(nameof(attackSpeedScalingTooltipVisibility).ToUpperInvariant(), ref attackSpeedScalingTooltipVisibility);
		Main.Configuration.Get("AvoidGithub", ref UI.ModBrowser.UIModBrowser.AvoidGithub);
		Main.Configuration.Get("AvoidImgur", ref UI.ModBrowser.UIModBrowser.AvoidImgur);
		Main.Configuration.Get(nameof(UI.ModBrowser.UIModBrowser.EarlyAutoUpdate), ref UI.ModBrowser.UIModBrowser.EarlyAutoUpdate);
		Main.Configuration.Get("ShowModMenuNotifications", ref notifyNewMainMenuThemes);
		Main.Configuration.Get("ShowConfirmationWindowWhenEnableDisableAllMods", ref showConfirmationWindowWhenEnableDisableAllMods);
		Main.Configuration.Get("ShowNewUpdatedModsInfo", ref showNewUpdatedModsInfo);
		Main.Configuration.Get("LastSelectedModMenu", ref MenuLoader.LastSelectedModMenu);
		Main.Configuration.Get("KnownMenuThemes", ref MenuLoader.KnownMenuSaveString);
		Main.Configuration.Get("BossBarStyle", ref BossBarLoader.lastSelectedStyle);
		Main.Configuration.Get("SeenFirstLaunchModderWelcomeMessage", ref SeenFirstLaunchModderWelcomeMessage);
		Main.Configuration.Get(nameof(ModOrganizer.ModPackActive), ref ModOrganizer.ModPackActive);

		LastLaunchedTModLoaderVersion = new Version(Main.Configuration.Get(nameof(LastLaunchedTModLoaderVersion), "0.0"));
		Main.Configuration.Get(nameof(BetaUpgradeWelcomed144), ref BetaUpgradeWelcomed144);
		Main.Configuration.Get(nameof(LastLaunchedTModLoaderAlphaSha), ref LastLaunchedTModLoaderAlphaSha);
		LastPreviewFreezeNotificationSeen = new Version(Main.Configuration.Get(nameof(LastPreviewFreezeNotificationSeen), "0.0"));
		Main.Configuration.Get(nameof(LatestNewsTimestamp), ref LatestNewsTimestamp);
		Main.Configuration.Get(nameof(WarnedFamilyShareDontShowAgain), ref WarnedFamilyShareDontShowAgain);
		if (Enum.TryParse<ModsMenuSortMode>(Main.Configuration.Get(nameof(ModsMenuSortMode), ModsMenuSortMode.RecentlyUpdated.ToString()), out var modsMenuSortMode))
			Interface.modsMenu.sortMode = modsMenuSortMode;

		Main.Configuration.Get("LiquidSlopeFix", ref LiquidEdgeRenderer.Enabled);
	}

	internal static void MigrateSettings()
	{
		// TODO: Stable RecentGitHubCommits.txt is probably not accurate for showing stable users, we could use a summary for the month of changes rather than recent commits.
		if (BuildInfo.IsPreview && LastLaunchedTModLoaderVersion != BuildInfo.tMLVersion) {
			ShowWhatsNew = true;
			// TODO: Start retrieving what's new data from github here.
		}

		if (LastLaunchedTModLoaderVersion == new Version(0, 0))
			ShowFirstLaunchWelcomeMessage = true;
	}

	/// <summary>
	/// Allows type inference on T and F
	/// </summary>
	internal static void BuildGlobalHook<T, F>(ref F[] list, IList<T> providers, Expression<Func<T, F>> expr) where F : Delegate
	{
		var query = expr.ToOverrideQuery();
		list = providers.Where(query.HasOverride).Select(t => (F)query.Binder(t)).ToArray();
	}
}
