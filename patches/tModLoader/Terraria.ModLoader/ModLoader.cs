using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using Terraria.Localization;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.Exceptions;
using Terraria.ModLoader.IO;
using System.Security.Cryptography;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Terraria.ModLoader.Audio;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This serves as the central class which loads mods. It contains many static fields and methods related to mods and their contents.
	/// </summary>
	public static class ModLoader
	{
		//change Terraria.Main.DrawMenu change drawn version number string to include this
		/// <summary>The name and version number of tModLoader.</summary>
		public static readonly Version version = new Version(0, 10, 1, 5);
		// Marks this release as a beta release, preventing publishing and marking all built mods as unpublishable.
#if !BETA
		public static readonly string versionedName = "tModLoader v" + version;
		public static readonly bool beta = false;
#else
		public static readonly string versionedName = "tModLoader v" + version + " - BetaNameHere Beta 1";
		public static readonly bool beta = true;
#endif
#if WINDOWS
		public static readonly bool windows = true;
#else
		public static readonly bool windows = false;
#endif
#if LINUX
		public static readonly bool linux = true;
#else
		public static readonly bool linux = false;
#endif
#if MAC
		public static readonly bool mac = true;
#else
		public static readonly bool mac = false;
#endif
#if GOG
		public static readonly bool gog = true;
#else
		public static readonly bool gog = false;
#endif
		public static readonly string compressedPlatformRepresentation = (windows ? "w" : (linux ? "l" : "m")) + (gog ? "g" : "s");
		//change Terraria.Main.SavePath and cloud fields to use "ModLoader" folder
		/// <summary>The file path in which mods are stored.</summary>
		public static string ModPath => modPath;
		internal static string modPath = Main.SavePath + Path.DirectorySeparatorChar + "Mods";
		/// <summary>The file path in which mod sources are stored. Mod sources are the code and images that developers work with.</summary>
		public static readonly string ModSourcePath = Main.SavePath + Path.DirectorySeparatorChar + "Mod Sources";
		internal const int earliestRelease = 149;
		
		private static Mod[] mods = new Mod[0];
		private static readonly IDictionary<string, Mod> modsByName = new Dictionary<string, Mod>(StringComparer.OrdinalIgnoreCase);
		private static WeakReference[] weakModReferences = new WeakReference[0];

		internal static readonly string modBrowserPublicKey = "<RSAKeyValue><Modulus>oCZObovrqLjlgTXY/BKy72dRZhoaA6nWRSGuA+aAIzlvtcxkBK5uKev3DZzIj0X51dE/qgRS3OHkcrukqvrdKdsuluu0JmQXCv+m7sDYjPQ0E6rN4nYQhgfRn2kfSvKYWGefp+kqmMF9xoAq666YNGVoERPm3j99vA+6EIwKaeqLB24MrNMO/TIf9ysb0SSxoV8pC/5P/N6ViIOk3adSnrgGbXnFkNQwD0qsgOWDks8jbYyrxUFMc4rFmZ8lZKhikVR+AisQtPGUs3ruVh4EWbiZGM2NOkhOCOM4k1hsdBOyX2gUliD0yjK5tiU3LBqkxoi2t342hWAkNNb4ZxLotw==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
		internal static string modBrowserPassphrase = "";

		private static string steamID64 = "";
		internal static string SteamID64 {
			get => gog ? steamID64 : Steamworks.SteamUser.GetSteamID().ToString();
			set => steamID64 = value;
		}

		internal static bool isModder;
		internal static bool alwaysLogExceptions;
		internal static bool dontRemindModBrowserUpdateReload;
		internal static bool dontRemindModBrowserDownloadEnable;
		internal static byte musicStreamMode;
		internal static bool removeForcedMinimumZoom;
		internal static bool allowGreaterResolutions;

		internal static string modToBuild;
		internal static bool reloadAfterBuild = false;
		internal static bool buildAll = false;

		internal static Action OnSuccessfulLoad;
		
		public static Mod[] Mods => mods;

		/// <summary>
		/// Gets the instance of the Mod with the specified name.
		/// </summary>
		public static Mod GetMod(string name)
		{
			modsByName.TryGetValue(name, out Mod m);
			return m;
		}

		public static Mod GetMod(int index) => index >= 0 && index < mods.Length ? mods[index] : null;
		
		[Obsolete("Use ModLoader.Mods", true)]
		public static Mod[] LoadedMods => mods;

		[Obsolete("Use ModLoader.Mods.Length", true)]
		public static int ModCount => mods.Length;
		
		[Obsolete("Use Modloader.Mods.Select(m => m.Name)", true)]
		public static string[] GetLoadedMods() => Mods.Reverse().Select(m => m.Name).ToArray();

		internal static void BeginLoad() => ThreadPool.QueueUserWorkItem(_ => Load());

		internal static void Load()
		{
			try
			{
				var modInstances = ModOrganizer.LoadMods();
				if (modInstances == null)
					return;
			
				weakModReferences = modInstances.Select(x => new WeakReference(x)).ToArray();
				modInstances.Insert(0, new ModLoaderMod());
				mods = modInstances.ToArray();
				foreach (var mod in mods)
					modsByName[mod.Name] = mod;

				if (!ModContent.Load())
					return;

				if (OnSuccessfulLoad != null)
					OnSuccessfulLoad();
				else
					Main.menuMode = 0;
			}
			finally
			{
				OnSuccessfulLoad = null;
			}
		}

		internal static void Unload()
		{
			foreach (var mod in mods.Reverse())
				mod.UnloadContent();
			
			mods = new Mod[0];
			modsByName.Clear();

			ModContent.Unload();

			GC.Collect();
			if (isModder) {
				foreach (var mod in weakModReferences.Where(r => r.IsAlive).Select(r => (Mod)r.Target))
					ErrorLogger.Log(mod.Name + " not fully unloaded during unload.");
			}

			if (!Main.dedServ && Main.netMode != 1) //disable vanilla client compatiblity restrictions when reloading on a client
				ModNet.AllowVanillaClients = false;
		}

		internal static void Reload()
		{
			Unload();
			Main.menuMode = Interface.loadModsID;
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

		/// <summary>A cached list of enabled mods (not necessarily currently loaded or even installed), mirroring the enabled.json file.</summary>
		private static HashSet<string> _enabledMods;
		internal static HashSet<string> EnabledMods => _enabledMods ?? (_enabledMods = ModOrganizer.LoadEnabledMods());

		internal static bool IsEnabled(string modName) => EnabledMods.Contains(modName);
		internal static void EnableMod(string modName) => SetModEnabled(modName, true);
		internal static void DisableMod(string modName) => SetModEnabled(modName, false);
		internal static void SetModEnabled(string modName, bool active)
		{
			if (active)
				EnabledMods.Add(modName);
			else
				EnabledMods.Remove(modName);

			ModOrganizer.SaveEnabledMods();
		}

		internal static string[] FindModSources()
		{
			Directory.CreateDirectory(ModSourcePath);
			return Directory.GetDirectories(ModSourcePath, "*", SearchOption.TopDirectoryOnly).Where(dir => new DirectoryInfo(dir).Name != ".vs").ToArray();
		}

		internal static void BuildAllMods()
		{
			ThreadPool.QueueUserWorkItem(_ =>
				{
					PostBuildMenu(ModCompile.BuildAll(FindModSources(), Interface.buildMod));
				});
		}

		internal static void BuildMod()
		{
			Interface.buildMod.SetProgress(0, 1);
			ThreadPool.QueueUserWorkItem(_ => {
				try
				{
					PostBuildMenu(ModCompile.Build(modToBuild, Interface.buildMod));
				}
				catch (Exception e)
				{
					ErrorLogger.LogException(e);
				}
			});
		}

		private static void PostBuildMenu(bool success)
		{
			Main.menuMode = success ? (reloadAfterBuild ? Interface.reloadModsID : 0) : Interface.errorMessageID;
		}

		internal static void SaveConfiguration()
		{
			Main.Configuration.Put("ModBrowserPassphrase", modBrowserPassphrase);
			Main.Configuration.Put("SteamID64", steamID64);
			Main.Configuration.Put("DownloadModsFromServers", ModNet.downloadModsFromServers);
			Main.Configuration.Put("OnlyDownloadSignedModsFromServers", ModNet.onlyDownloadSignedMods);
			Main.Configuration.Put("DontRemindModBrowserUpdateReload", dontRemindModBrowserUpdateReload);
			Main.Configuration.Put("DontRemindModBrowserDownloadEnable", dontRemindModBrowserDownloadEnable);
			Main.Configuration.Put("MusicStreamMode", musicStreamMode);
			Main.Configuration.Put("AlwaysLogExceptions", alwaysLogExceptions);
			Main.Configuration.Put("RemoveForcedMinimumZoom", removeForcedMinimumZoom);
			Main.Configuration.Put("AllowGreaterResolutions", allowGreaterResolutions);
		}

		internal static void LoadConfiguration()
		{
			Main.Configuration.Get("ModBrowserPassphrase", ref modBrowserPassphrase);
			Main.Configuration.Get("SteamID64", ref steamID64);
			Main.Configuration.Get("DownloadModsFromServers", ref ModNet.downloadModsFromServers);
			Main.Configuration.Get("OnlyDownloadSignedModsFromServers", ref ModNet.onlyDownloadSignedMods);
			Main.Configuration.Get("DontRemindModBrowserUpdateReload", ref dontRemindModBrowserUpdateReload);
			Main.Configuration.Get("DontRemindModBrowserDownloadEnable", ref dontRemindModBrowserDownloadEnable);
			Main.Configuration.Get("MusicStreamMode", ref musicStreamMode);
			Main.Configuration.Get("AlwaysLogExceptions", ref alwaysLogExceptions);
			Main.Configuration.Get("RemoveForcedMinimumZoom", ref removeForcedMinimumZoom);
			Main.Configuration.Get("AllowGreaterResolutions", ref removeForcedMinimumZoom);
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
			try
			{
				var convert = expr.Body as UnaryExpression;
				var makeDelegate = convert.Operand as MethodCallExpression;
				var methodArg = makeDelegate.Arguments[2] as ConstantExpression;
				method = methodArg.Value as MethodInfo;
				if (method == null) throw new NullReferenceException();
			}
			catch (Exception e)
			{
				throw new ArgumentException("Invalid hook expression " + expr, e);
			}
			return method;
		}
		/*
		 * Forwarder, deprecated, methods
		 * These are methods used likely by many modders, which may need some time to adjust to changes
		 */
		[Obsolete("ModLoader.GetFileBytes is deprecated since v0.10.1.4, use ModContent.GetFileBytes instead.", true)]
		public static byte[] GetFileBytes(string name) => ModContent.GetFileBytes(name);
		
		[Obsolete("ModLoader.FileExists is deprecated since v0.10.1.4, use ModContent.FileExists instead.", true)]
		public static bool FileExists(string name) => ModContent.FileExists(name);

		[Obsolete("ModContent.GetTexture is deprecated since v0.10.1.4, use ModContent.GetTexture instead.", true)]
		public static Texture2D GetTexture(string name) => ModContent.GetTexture(name);

		[Obsolete("ModLoader.TextureExists is deprecated since v0.10.1.4, use ModContent.TextureExists instead.", true)]
		public static bool TextureExists(string name) => ModContent.TextureExists(name);

		[Obsolete("ModContent.GetSound is deprecated since v0.10.1.4, use ModContent.GetSound instead.", true)]
		public static SoundEffect GetSound(string name) => ModContent.GetSound(name);

		[Obsolete("ModLoader.SoundExists is deprecated since v0.10.1.4, use ModContent.SoundExists instead.", true)]
		public static bool SoundExists(string name) => ModContent.SoundExists(name);

		[Obsolete("ModContent.GetMusic is deprecated since v0.10.1.4, use ModContent.GetMusic instead.", true)]
		public static Music GetMusic(string name) => ModContent.GetMusic(name);

		[Obsolete("ModLoader.MusicExists is deprecated since v0.10.1.4, use ModContent.MusicExists instead.", true)]
		public static bool MusicExists(string name) => ModContent.MusicExists(name);
	}
}
