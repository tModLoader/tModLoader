using ReLogic.OS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.Engine;
using Terraria.ModLoader.UI;
using Terraria.Initializers;
using Terraria.ModLoader.Assets;
using ReLogic.Content;
using System.Runtime.CompilerServices;
using Terraria.Social.Steam;
using Terraria.ModLoader.Exceptions;
using System.Text;
using ReLogic.Localization.IME.WinImm32;
using Terraria.Social.Base;
using System.IO;

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

	internal static bool autoReloadAndEnableModsLeavingModBrowser = true;
	internal static bool autoReloadRequiredModsLeavingModsScreen = true;
	internal static bool removeForcedMinimumZoom;
	internal static int attackSpeedScalingTooltipVisibility = 1; // Shown, WhenNonZero, Hidden
	internal static bool notifyNewMainMenuThemes = true;
	internal static bool showNewUpdatedModsInfo = true;
	internal static bool skipLoad;
	internal static Action OnSuccessfulLoad;

	internal static bool isLoading;

	public static Mod[] Mods { get; private set; } = new Mod[0];

	internal static AssetRepository ManifestAssets { get; set; } //This is used for keeping track of assets that are loaded either from the application's resources, or created directly from a texture.
	internal static AssemblyResourcesContentSource ManifestContentSource { get; set; }

	/// <summary> Gets the instance of the Mod with the specified name. This will throw an exception if the mod cannot be found. </summary>
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
			var responsibleMods = GetResponsibleModsFromException(e);

			var msg = CalculateLoadExceptionMessage(e, responsibleMods, availableMods);

			DisableErroringMods(responsibleMods, availableMods);
			
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

	private static List<string> GetResponsibleModsFromException(Exception e)
	{
		var responsibleMods = new List<string>();
		if (e.Data.Contains("mod"))
			responsibleMods.Add((string)e.Data["mod"]);
		if (e.Data.Contains("mods"))
			responsibleMods.AddRange((IEnumerable<string>)e.Data["mods"]);
		responsibleMods.Remove("ModLoader");

		if (responsibleMods.Count == 0 && AssemblyManager.FirstModInStackTrace(new StackTrace(e), out var stackMod))
			responsibleMods.Add(stackMod);

		return responsibleMods;
	}

	private static string CalculateLoadExceptionMessage(Exception exception, List<string> responsibleMods, LocalMod[] availableMods)
	{
		var sb = new StringBuilder();
		sb.AppendLine("Mod(s) Failed To Load");

		List<LocalMod> erroringMods = new List<LocalMod>();

		// Player First Impression Section
		if (responsibleMods.Count == 0) {
			sb.AppendLine(Language.GetTextValue("tModLoader.LoadErrorCulpritUnknown"));
		}
		else {
			erroringMods = availableMods.Where(mod => responsibleMods.Contains(mod.Name)).ToList();

			foreach (var item in erroringMods) {
				// For whatever reason \t doesn't work here? - Solxan, June 7 2024
				sb.AppendLine($"   {item.DisplayName} v{item.Version}, built for tML v{item.tModLoaderVersion}");
			}

			sb.AppendLine("\n" + Language.GetTextValue("tModLoader.LoadErrorDisabled"));
		}

		sb.AppendLine("-----------------------------------------------");

		// Player Possible Fixes Section
		sb.AppendLine("Possible load issue causes are:");

		(bool relevant, string desc)[] commonIssues = {
			(false, "A dependency mod has updated and this mod is out of date with the dependency"),
			(false, $"You attempted to load a Stable mod on {BuildInfo.Purpose} tModLoader"),
			(false, "You attempted to load a 1.3/1.4.3 Mod on 1.4.4"),
			(false, "You are using a different tML version than the 'Frozen' modpack"),
		};

		foreach (var item in erroringMods) {
			commonIssues[0].relevant |= item.properties.modReferences.Length > 0;
			commonIssues[1].relevant |= !BuildInfo.IsStable && item.tModLoaderVersion.MajorMinor() != BuildInfo.tMLVersion.MajorMinor();
			commonIssues[2].relevant |= SocialBrowserModule.GetBrowserVersionNumber(item.tModLoaderVersion) != SocialBrowserModule.GetBrowserVersionNumber(BuildInfo.tMLVersion);

			if (item.location == ModLocation.Modpack) {
				var ss = File.ReadLines(Path.Combine(ModOrganizer.ModPackActive, "Mods", "tmlversion.txt")).FirstOrDefault();
				commonIssues[3].relevant |= item.location == ModLocation.Modpack && new Version(ss).MajorMinor() != BuildInfo.tMLVersion.MajorMinor();
			}
			
		}

		foreach (var item in commonIssues.Where(item => item.relevant)) {
			sb.AppendLine($"   {item.desc}");
		}

		sb.AppendLine("-----------------------------------------------");

		// Getting Real Technical Section

		sb.AppendLine($"For Support, Include Files at \"Open Logs\" and errors below");

		if (exception is Exceptions.JITException)
			sb.AppendLine($"The mod will need to be updated to match the current tModLoader version, or may be incompatible with the version of some of your other mods. Click the '{Language.GetTextValue("tModLoader.OpenWebHelp")}' button to learn more.");

		if (exception is ReflectionTypeLoadException reflectionTypeLoadException)
			sb.AppendLine("\n" + string.Join("\n", reflectionTypeLoadException.LoaderExceptions.Select(x => x.Message)));

		if (exception.Data.Contains("contentType") && exception.Data["contentType"] is Type contentType)
			sb.AppendLine(Language.GetTextValue("tModLoader.LoadErrorContentType", contentType.FullName));

		return sb.ToString();
	} 

	private static void DisableErroringMods(List<string> responsibleMods, LocalMod[] availableMods)
	{
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
		// These being first ensure that even if the 'hideStackTrace' is 'SET' (ie hide stack) that the trace still shows in the log.
		if (fatal)
			Logging.tML.Fatal(msg, e);
		else
			Logging.tML.Error(msg, e);

		// tML uses hideStackTrace internally on errors where the stack trace would be all tML, and just detract from the message. - CB
		msg += "\n" + (e.Data.Contains("hideStackTrace") ? e.Message : e.ToString());

		if (Main.dedServ) {
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(msg);
			Console.ResetColor();

			if (fatal) {
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
