using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
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
using System.Windows.Forms;
using Steamworks;
using Terraria.Localization;
using Terraria.ModLoader.Audio;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.Engine;
using Terraria.ModLoader.UI;
using Version = System.Version;
using Terraria.Initializers;
using Terraria.ModLoader.Assets;
using ReLogic.Content;
using ReLogic.Graphics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NETCORE
using System.Runtime.Loader;
#endif

namespace Terraria.ModLoader
{
	/// <summary>
	/// This serves as the central class which loads mods. It contains many static fields and methods related to mods and their contents.
	/// </summary>
	public static class ModLoader
	{
		// Stores the most recent version of tModLoader launched. Can be used for migration.
		public static Version LastLaunchedTModLoaderVersion;
		// public static bool ShowWhatsNew;
		public static bool ShowFirstLaunchWelcomeMessage;

		public static string versionedName => ModCompile.DeveloperMode ? BuildInfo.versionedNameDevFriendly : BuildInfo.versionedName;

#if NETCORE
		public static string CompressedPlatformRepresentation => (Platform.IsWindows ? "w" : (Platform.IsLinux ? "l" : "m")) + (InstallVerifier.IsGoG ? "g" : "s") + "c";
#else
		public static string CompressedPlatformRepresentation => "w" + (InstallVerifier.IsGoG ? "g" : "s") + "n";
#endif

		public static string ModPath => ModOrganizer.modPath;

		private static readonly IDictionary<string, Mod> modsByName = new Dictionary<string, Mod>(StringComparer.OrdinalIgnoreCase);
		private static WeakReference[] weakModReferences = new WeakReference[0];

		internal static readonly string modBrowserPublicKey = "<RSAKeyValue><Modulus>oCZObovrqLjlgTXY/BKy72dRZhoaA6nWRSGuA+aAIzlvtcxkBK5uKev3DZzIj0X51dE/qgRS3OHkcrukqvrdKdsuluu0JmQXCv+m7sDYjPQ0E6rN4nYQhgfRn2kfSvKYWGefp+kqmMF9xoAq666YNGVoERPm3j99vA+6EIwKaeqLB24MrNMO/TIf9ysb0SSxoV8pC/5P/N6ViIOk3adSnrgGbXnFkNQwD0qsgOWDks8jbYyrxUFMc4rFmZ8lZKhikVR+AisQtPGUs3ruVh4EWbiZGM2NOkhOCOM4k1hsdBOyX2gUliD0yjK5tiU3LBqkxoi2t342hWAkNNb4ZxLotw==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
		internal static string modBrowserPassphrase = "";

		private static string steamID64 = "";
		internal static string SteamID64 {
			get => InstallVerifier.IsGoG ? steamID64 : Steamworks.SteamUser.GetSteamID().ToString();
			set => steamID64 = value;
		}

		internal static bool autoReloadAndEnableModsLeavingModBrowser = true;
		internal static bool dontRemindModBrowserUpdateReload;
		internal static bool dontRemindModBrowserDownloadEnable;
		internal static bool removeForcedMinimumZoom;
		internal static bool showMemoryEstimates = true;
		internal static bool notifyNewMainMenuThemes = true;

		internal static bool skipLoad;

		internal static Action OnSuccessfulLoad;

		public static Mod[] Mods { get; private set; } = new Mod[0];

		internal static AssetRepository ManifestAssets { get; set; } //This is used for keeping track of assets that are loaded either from the application's resources, or created directly from a texture.
		internal static AssemblyResourcesContentSource ManifestContentSource { get; set; }

		// Get

		/// <summary> Gets the instance of the Mod with the specified name. This will throw an exception if the mod cannot be found. </summary>
		/// <exception cref="KeyNotFoundException"/>
		public static Mod GetMod(string name) => modsByName[name];

		// TryGet

		/// <summary> Safely attempts to get the instance of the Mod with the specified name. </summary>
		/// <returns> Whether or not the requested instance has been found. </returns>
		public static bool TryGetMod(string name, out Mod result) => modsByName.TryGetValue(name, out result);

		internal static void EngineInit()
		{
			FileAssociationSupport.UpdateFileAssociation();
			MonoModHooks.Initialize();
			ZipExtractFix.Init();
			XnaTitleContainerRelativePathFix.Init();
			LoaderManager.AutoLoad();
		}

		internal static void PrepareAssets()
		{
			if (Main.dedServ) {
				return;
			}

			ManifestContentSource = new AssemblyResourcesContentSource(Assembly.GetExecutingAssembly());
			ManifestAssets = new AssetRepository(AssetInitializer.assetReaderCollection, new[] { ManifestContentSource }) {
				AssetLoadFailHandler = Main.OnceFailedLoadingAnAsset
			};
		}

		internal static void BeginLoad(CancellationToken token) => Task.Run(() => Load(token));

		private static bool isLoading = false;
		private static void Load(CancellationToken token = default)
		{
			try {
				if (isLoading)
					throw new Exception("Load called twice");
				isLoading = true;

				if (!Unload())
					return;

				var modInstances = ModOrganizer.LoadMods(token);

				weakModReferences = modInstances.Select(x => new WeakReference(x)).ToArray();
				modInstances.Insert(0, new ModLoaderMod());
				Mods = modInstances.ToArray();
				foreach (var mod in Mods)
					modsByName[mod.Name] = mod;

				ModContent.Load(token);

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
					var mod = ModOrganizer.FindMods().FirstOrDefault(m => m.Name == responsibleMods[0]); //use First rather than Single, incase of "Two mods with the same name" error message from ModOrganizer (#639)
					if (mod != null && mod.tModLoaderVersion != BuildInfo.tMLVersion)
						msg += "\n" + Language.GetTextValue("tModLoader.LoadErrorVersionMessage", mod.tModLoaderVersion, versionedName);
				}
				if (responsibleMods.Count > 0)
					msg += "\n" + Language.GetTextValue("tModLoader.LoadErrorDisabled");
				else
					msg += "\n" + Language.GetTextValue("tModLoader.LoadErrorCulpritUnknown");

				if (e is ReflectionTypeLoadException reflectionTypeLoadException)
					msg += "\n\n" + string.Join("\n", reflectionTypeLoadException.LoaderExceptions.Select(x => x.Message));

				Logging.tML.Error(msg, e);

				foreach (var mod in responsibleMods)
					DisableMod(mod);

				isLoading = false; // disable loading flag, because server will just instantly retry reload
				DisplayLoadError(msg, e, e.Data.Contains("fatal"), responsibleMods.Count == 0);
			}
			finally {
				isLoading = false;
				OnSuccessfulLoad = null;
				skipLoad = false;
				ModNet.NetReloadActive = false;
			}
		}

		internal static void Reload()
		{
			if (Main.dedServ)
				Load();
			else
				Main.menuMode = Interface.loadModsID;
		}

		private static bool Unload()
		{
			try {
				Mods_Unload();
				WarnModsStillLoaded();
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

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static void Mods_Unload()
		{
			Logging.tML.Info("Unloading mods");
			if (Main.dedServ) {
				Console.WriteLine("Unloading mods...");
			}
			else {
				Interface.loadMods.SetLoadStage("tModLoader.MSUnloading", Mods.Length);
			}

			ModContent.UnloadModContent();
			Mods = new Mod[0];
			modsByName.Clear();
			ModContent.Unload();
			MemoryTracking.Clear();
			Thread.MemoryBarrier();

			AssemblyManager.Unload();
		}

		internal static List<string> badUnloaders = new List<string>();
		private static void WarnModsStillLoaded()
		{
			badUnloaders = weakModReferences.Where(r => r.IsAlive).Select(r => ((Mod)r.Target).Name).ToList();
			foreach (var modName in badUnloaders)
				Logging.tML.WarnFormat("{0} not fully unloaded during unload.", modName);
		}

		private static void DisplayLoadError(string msg, Exception e, bool fatal, bool continueIsRetry = false)
		{
			msg += "\n\n" + (e.Data.Contains("hideStackTrace") ? e.Message : e.ToString());

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
				Interface.errorMessage.Show(msg,
					gotoMenu: fatal ? -1 : Interface.reloadModsID,
					webHelpURL: e.HelpLink,
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

		private static bool _pauseSavingEnabledMods;
		private static bool _needsSavingEnabledMods;
		internal static bool PauseSavingEnabledMods {
			get => _pauseSavingEnabledMods;
			set {
				if (_pauseSavingEnabledMods == value) { return; }
				if (!value && _needsSavingEnabledMods) {
					ModOrganizer.SaveEnabledMods();
					_needsSavingEnabledMods = false;
				}
				_pauseSavingEnabledMods = value;
			}
		}
		/// <summary>A cached list of enabled mods (not necessarily currently loaded or even installed), mirroring the enabled.json file.</summary>
		private static HashSet<string> _enabledMods;
		internal static HashSet<string> EnabledMods => _enabledMods ?? (_enabledMods = ModOrganizer.LoadEnabledMods());

		internal static bool IsEnabled(string modName) => EnabledMods.Contains(modName);
		internal static void EnableMod(string modName) => SetModEnabled(modName, true);
		internal static void DisableMod(string modName) => SetModEnabled(modName, false);
		internal static void SetModEnabled(string modName, bool active)
		{
			if (active) {
				EnabledMods.Add(modName);
				Logging.tML.InfoFormat("Enabling Mod: {0}", modName);
			}
			else {
				EnabledMods.Remove(modName);
				Logging.tML.InfoFormat("Disabling Mod: {0}", modName);
			}
			if (PauseSavingEnabledMods) {
				_needsSavingEnabledMods = true;
			}
			else {
				ModOrganizer.SaveEnabledMods();
			}
		}

		internal static void SaveConfiguration()
		{
			Main.Configuration.Put("ModBrowserPassphrase", modBrowserPassphrase);
			Main.Configuration.Put("SteamID64", steamID64);
			Main.Configuration.Put("DownloadModsFromServers", ModNet.downloadModsFromServers);
			Main.Configuration.Put("OnlyDownloadSignedModsFromServers", ModNet.onlyDownloadSignedMods);
			Main.Configuration.Put("AutomaticallyReloadAndEnableModsLeavingModBrowser", autoReloadAndEnableModsLeavingModBrowser);
			Main.Configuration.Put("DontRemindModBrowserUpdateReload", dontRemindModBrowserUpdateReload);
			Main.Configuration.Put("DontRemindModBrowserDownloadEnable", dontRemindModBrowserDownloadEnable);
			Main.Configuration.Put("RemoveForcedMinimumZoom", removeForcedMinimumZoom);
			Main.Configuration.Put("ShowMemoryEstimates", showMemoryEstimates);
			Main.Configuration.Put("AvoidGithub", UI.ModBrowser.UIModBrowser.AvoidGithub);
			Main.Configuration.Put("AvoidImgur", UI.ModBrowser.UIModBrowser.AvoidImgur);
			Main.Configuration.Put(nameof(UI.ModBrowser.UIModBrowser.EarlyAutoUpdate), UI.ModBrowser.UIModBrowser.EarlyAutoUpdate);
			Main.Configuration.Put("ShowModMenuNotifications", notifyNewMainMenuThemes);
			Main.Configuration.Put("LastSelectedModMenu", MenuLoader.LastSelectedModMenu);
			Main.Configuration.Put("KnownMenuThemes", MenuLoader.KnownMenuSaveString);
			Main.Configuration.Put("BossBarStyle", BossBarLoader.lastSelectedStyle);

			Main.Configuration.Put("LastLaunchedTModLoaderVersion", BuildInfo.tMLVersion.ToString());
		}

		internal static void LoadConfiguration()
		{
			Main.Configuration.Get("ModBrowserPassphrase", ref modBrowserPassphrase);
			Main.Configuration.Get("SteamID64", ref steamID64);
			Main.Configuration.Get("DownloadModsFromServers", ref ModNet.downloadModsFromServers);
			Main.Configuration.Get("OnlyDownloadSignedModsFromServers", ref ModNet.onlyDownloadSignedMods);
			Main.Configuration.Get("AutomaticallyReloadAndEnableModsLeavingModBrowser", ref autoReloadAndEnableModsLeavingModBrowser);
			Main.Configuration.Get("DontRemindModBrowserUpdateReload", ref dontRemindModBrowserUpdateReload);
			Main.Configuration.Get("DontRemindModBrowserDownloadEnable", ref dontRemindModBrowserDownloadEnable);
			Main.Configuration.Get("RemoveForcedMinimumZoom", ref removeForcedMinimumZoom);
			Main.Configuration.Get("ShowMemoryEstimates", ref showMemoryEstimates);
			Main.Configuration.Get("AvoidGithub", ref UI.ModBrowser.UIModBrowser.AvoidGithub);
			Main.Configuration.Get("AvoidImgur", ref UI.ModBrowser.UIModBrowser.AvoidImgur);
			Main.Configuration.Get(nameof(UI.ModBrowser.UIModBrowser.EarlyAutoUpdate), ref UI.ModBrowser.UIModBrowser.EarlyAutoUpdate);
			Main.Configuration.Get("ShowModMenuNotifications", ref notifyNewMainMenuThemes);
			Main.Configuration.Get("LastSelectedModMenu", ref MenuLoader.LastSelectedModMenu);
			Main.Configuration.Get("KnownMenuThemes", ref MenuLoader.KnownMenuSaveString);
			Main.Configuration.Get("BossBarStyle", ref BossBarLoader.lastSelectedStyle);

			LastLaunchedTModLoaderVersion = new Version(Main.Configuration.Get(nameof(LastLaunchedTModLoaderVersion), "0.0"));
		}

		internal static void MigrateSettings()
		{
			if (LastLaunchedTModLoaderVersion < new Version(0, 11, 7, 5))
				showMemoryEstimates = true;
			
			/*
			if (LastLaunchedTModLoaderVersion < version)
				ShowWhatsNew = true;
			*/

			if (LastLaunchedTModLoaderVersion == new Version(0, 0))
				ShowFirstLaunchWelcomeMessage = true;
		}

		/// <summary>
		/// Allows type inference on T and F
		/// </summary>
		internal static void BuildGlobalHook<T, F>(ref F[] list, IList<T> providers, Expression<Func<T, F>> expr)
		{
			list = BuildGlobalHook(providers, expr).Select(expr.Compile()).ToArray();
		}

		internal static T[] BuildGlobalHook<T, F>(IList<T> providers, Expression<Func<T, F>> expr)
		{
			return BuildGlobalHook(providers, Method(expr));
		}

		internal static T[] BuildGlobalHook<T>(IList<T> providers, MethodInfo method)
		{
			if (!method.IsVirtual) throw new ArgumentException("Cannot build hook for non-virtual method " + method);
			var argTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();

			return providers.Where(p => p.GetType().GetMethod(method.Name, argTypes).DeclaringType != typeof(T)).ToArray();
		}

		internal static int[] BuildGlobalHookNew<T>(IList<T> providers, MethodInfo method) {
			if (!method.IsVirtual)
				throw new ArgumentException("Cannot build hook for non-virtual method " + method);

			var argTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
			var list = new List<int>();
			var baseDeclaringType = method.DeclaringType;
			bool isInterface = baseDeclaringType.IsInterface;

			for (int i = 0; i < providers.Count; i++) {
				var currentType = providers[i].GetType();

				if (isInterface) {
					// In case of interfaces, we can skip shenanigans that 'explicit interface method implementations' bring,
					// and just check if the provider implements the interface.
					if (baseDeclaringType.IsAssignableFrom(currentType)) {
						list.Add(i);
					}
				}
				else {
					var currentMethod = currentType.GetMethod(method.Name, argTypes);

					if (currentMethod != null && currentMethod.DeclaringType != baseDeclaringType) {
						list.Add(i);
					}
				}
			}

			return list.ToArray();
		}

		internal static MethodInfo Method<T, F>(Expression<Func<T, F>> expr)
		{
			MethodInfo method;

			try {
				var convert = expr.Body as UnaryExpression;
				var makeDelegate = convert.Operand as MethodCallExpression;
				var methodArg = makeDelegate.Object as ConstantExpression;
				method = methodArg.Value as MethodInfo;
				if (method == null) throw new NullReferenceException();
			}
			catch (Exception e) {
				throw new ArgumentException("Invalid hook expression " + expr, e);
			}

			return method;
		}
	}
}
