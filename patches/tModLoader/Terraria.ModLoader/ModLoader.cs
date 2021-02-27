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

namespace Terraria.ModLoader
{
	/// <summary>
	/// This serves as the central class which loads mods. It contains many static fields and methods related to mods and their contents.
	/// </summary>
	public static class ModLoader
	{
		public static readonly Version version = new Version(0, 11, 8, 1);
		// Stores the most recent version of tModLoader launched. Can be used for migration.
		public static Version LastLaunchedTModLoaderVersion;
		// public static bool ShowWhatsNew;
		public static bool ShowFirstLaunchWelcomeMessage;

		public static readonly string branchName = "";
		// beta > 0 cannot publish to mod browser
		public static readonly int beta = 0;

		// SteamApps.GetCurrentBetaName(out string betaName, 100) ? betaName :
		public static readonly string versionedName = $"tModLoader v{version}" +
													  (branchName.Length == 0 ? "" : $" {branchName}") +
													  (beta == 0 ? "" : $" Beta {beta}");

		public static readonly string versionTag = $"v{version}" +
													(branchName.Length == 0 ? "" : $"-{branchName.ToLower()}") +
													(beta == 0 ? "" : $"-beta{beta}");

		[Obsolete("Use Platform.IsWindows")]
		public static readonly bool windows = Platform.IsWindows;
		[Obsolete("Use Platform.IsLinux")]
		public static readonly bool linux = Platform.IsLinux;
		[Obsolete("Use Platform.IsOSX")]
		public static readonly bool mac = Platform.IsOSX;

		[Obsolete("Use CompressedPlatformRepresentation instead")]
		public static readonly string compressedPlatformRepresentation = Platform.IsWindows ? "w" : (Platform.IsLinux ? "l" : "m");

		public static string CompressedPlatformRepresentation => (Platform.IsWindows ? "w" : (Platform.IsLinux ? "l" : "m")) + (InstallVerifier.IsGoG ? "g" : "s") + (FrameworkVersion.Framework == Framework.NetFramework ? "n" : (FrameworkVersion.Framework == Framework.Mono ? "o" : "u"));

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

		internal static bool skipLoad;

		internal static Action OnSuccessfulLoad;

		public static Mod[] Mods { get; private set; } = new Mod[0];

		/// <summary>
		/// Gets the instance of the Mod with the specified name.
		/// </summary>
		public static Mod GetMod(string name)
		{
			modsByName.TryGetValue(name, out Mod m);
			return m;
		}

		public static Mod GetMod(int index) => index >= 0 && index < Mods.Length ? Mods[index] : null;

		[Obsolete("Use ModLoader.Mods", true)]
		public static Mod[] LoadedMods => Mods;

		[Obsolete("Use ModLoader.Mods.Length", true)]
		public static int ModCount => Mods.Length;

		[Obsolete("Use ModLoader.Mods.Select(m => m.Name)", true)]
		public static string[] GetLoadedMods() => Mods.Reverse().Select(m => m.Name).ToArray();

		internal static void EngineInit()
		{
			DotNet45Check();
			FileAssociationSupport.UpdateFileAssociation();
			GLCallLocker.Init();
			HiDefGraphicsIssues.Init();
			MonoModHooks.Initialize();
			ZipExtractFix.Init();
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
					if (mod != null && mod.tModLoaderVersion != version)
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

		private static void DotNet45Check()
		{
			if (FrameworkVersion.Framework != Framework.NetFramework || FrameworkVersion.Version >= new Version(4, 5))
				return;

			var msg = Language.GetTextValue("tModLoader.LoadErrorDotNet45Required");
#if CLIENT
			Interface.MessageBoxShow(msg);
			Process.Start("https://dotnet.microsoft.com/download/dotnet-framework");
#else
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(msg);
			Console.ResetColor();
			Console.WriteLine("Press any key to exit...");
			Console.ReadKey();
#endif
			Environment.Exit(-1);
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
				// have to move unload logic to a separate method so the stack frame is cleared. Otherwise unloading can capture mod instances in local variables, even with memory barriers (thanks compiler weirdness)
				do_Unload();
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

		private static void do_Unload()
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
			GC.Collect();
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
			return f.VerifySignature(mod.hash, mod.signature);
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
			Main.Configuration.Put("LastLaunchedTModLoaderVersion", version.ToString());
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
			LastLaunchedTModLoaderVersion = new Version(Main.Configuration.Get("LastLaunchedTModLoaderVersion", "0.0"));
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
		/*
		 * Forwarder, deprecated, methods
		 * These are methods used likely by many modders, which may need some time to adjust to changes
		 */
		[Obsolete("ModLoader.GetFileBytes is deprecated since v0.11, use ModContent.GetFileBytes instead.", true)]
		public static byte[] GetFileBytes(string name) => ModContent.GetFileBytes(name);

		[Obsolete("ModLoader.FileExists is deprecated since v0.11, use ModContent.FileExists instead.", true)]
		public static bool FileExists(string name) => ModContent.FileExists(name);

		[Obsolete("ModLoader.GetTexture is deprecated since v0.11, use ModContent.GetTexture instead.", true)]
		public static Texture2D GetTexture(string name) => ModContent.GetTexture(name);

		[Obsolete("ModLoader.TextureExists is deprecated since v0.11, use ModContent.TextureExists instead.", true)]
		public static bool TextureExists(string name) => ModContent.TextureExists(name);

		[Obsolete("ModLoader.GetSound is deprecated since v0.11, use ModContent.GetSound instead.", true)]
		public static SoundEffect GetSound(string name) => ModContent.GetSound(name);

		[Obsolete("ModLoader.SoundExists is deprecated since v0.1, use ModContent.SoundExists instead.", true)]
		public static bool SoundExists(string name) => ModContent.SoundExists(name);

		[Obsolete("ModLoader.GetMusic is deprecated since v0.11, use ModContent.GetMusic instead.", true)]
		public static Music GetMusic(string name) => ModContent.GetMusic(name);

		[Obsolete("ModLoader.MusicExists is deprecated since v0.11, use ModContent.MusicExists instead.", true)]
		public static bool MusicExists(string name) => ModContent.MusicExists(name);
	}
}
