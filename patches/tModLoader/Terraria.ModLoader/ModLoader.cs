using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Localization;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.Exceptions;
using Terraria.ModLoader.IO;
using Terraria.UI;
using System.Security.Cryptography;
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
		public static readonly Version version = new Version(0, 10, 1, 4);
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

		internal static readonly string ImagePath = "Content" + Path.DirectorySeparatorChar + "Images";
		internal const int earliestRelease = 149;
		internal static string modToBuild;
		internal static bool reloadAfterBuild = false;
		internal static bool buildAll = false;

		// TODO cb suggested loadOrder may not be needed as mods are already in load order
		private static readonly Stack<string> loadOrder = new Stack<string>();
		internal static Mod[] loadedMods = new Mod[0];
		internal static readonly IDictionary<string, Mod> mods = new Dictionary<string, Mod>(StringComparer.OrdinalIgnoreCase);
		public static int ModCount => loadedMods.Length;
		
		internal static readonly string modBrowserPublicKey = "<RSAKeyValue><Modulus>oCZObovrqLjlgTXY/BKy72dRZhoaA6nWRSGuA+aAIzlvtcxkBK5uKev3DZzIj0X51dE/qgRS3OHkcrukqvrdKdsuluu0JmQXCv+m7sDYjPQ0E6rN4nYQhgfRn2kfSvKYWGefp+kqmMF9xoAq666YNGVoERPm3j99vA+6EIwKaeqLB24MrNMO/TIf9ysb0SSxoV8pC/5P/N6ViIOk3adSnrgGbXnFkNQwD0qsgOWDks8jbYyrxUFMc4rFmZ8lZKhikVR+AisQtPGUs3ruVh4EWbiZGM2NOkhOCOM4k1hsdBOyX2gUliD0yjK5tiU3LBqkxoi2t342hWAkNNb4ZxLotw==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
		internal static string modBrowserPassphrase = "";
		internal static bool isModder;
		internal static bool alwaysLogExceptions;
		internal static bool dontRemindModBrowserUpdateReload;
		internal static bool dontRemindModBrowserDownloadEnable;
		internal static byte musicStreamMode;
		internal static bool removeForcedMinimumZoom;
		internal static bool allowGreaterResolutions;
		private static string steamID64 = "";

		internal static string SteamID64
		{
			get
			{
#if GOG
				return steamID64;
#else
				return Steamworks.SteamUser.GetSteamID().ToString();
#endif
			}
			set { steamID64 = value; }
		}

		/// <summary>
		/// Gets the instance of the Mod with the specified name.
		/// </summary>
		public static Mod GetMod(string name)
		{
			mods.TryGetValue(name, out Mod m);
			return m;
		}

		public static Mod GetMod(int index)
		{
			return index >= 0 && index < loadedMods.Length ? loadedMods[index] : null;
		}

		public static Mod[] LoadedMods => (Mod[])loadedMods.Clone();
		public static string[] LoadedModsNames => (string[]) loadedMods.Select(x => x.Name).ToArray().Clone();

		internal static void UnloadMods()
		{
			while (loadOrder.Count > 0)
				GetMod(loadOrder.Pop()).UnloadContent();

			loadOrder.Clear();
			loadedMods = new Mod[0];
		}
		
		internal static void LoadAsync()
		{
			ThreadPool.QueueUserWorkItem(new WaitCallback(LoadMods), 1);
		}

		internal static void LoadMods(object threadContext)
		{
			// TODO: how/when is threadContext used? See Interface.cs line 373

			// Attempt loading mod instances from the organiser
			var instances = ModOrganiser.LoadInstances();
			if (instances == null)
			{
				// No instances found
				Main.menuMode = Interface.errorMessageID;
				return;
			}
			
			// Instances found, update loadedMods/loadOrder/mods arrays and load the content
			loadedMods = instances.ToArray();
			ModOrganiser.loadedModsWeakReferences = loadedMods.Skip(1).Select(x => new WeakReference(x)).ToArray();
			foreach (var mod in instances)
			{
				loadOrder.Push(mod.Name);
				mods[mod.Name] = mod;
			}
			ModContent.LoadContent(new Dictionary<string, Mod>(mods));
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

		internal static void Reload()
		{
			ModContent.Unload();
			Main.menuMode = Interface.loadModsID;
		}

		internal static string[] FindModSources()
		{
			Directory.CreateDirectory(ModSourcePath);
			return Directory.GetDirectories(ModSourcePath, "*", SearchOption.TopDirectoryOnly).Where(dir => new DirectoryInfo(dir).Name != ".vs").ToArray();
		}

		internal static void BuildAllMods()
		{
			ThreadPool.QueueUserWorkItem(_ => { PostBuildMenu(ModCompile.BuildAll(FindModSources(), Interface.buildMod)); });
		}

		internal static void BuildMod()
		{
			Interface.buildMod.SetProgress(0, 1);
			ThreadPool.QueueUserWorkItem(_ =>
			{
				try
				{
					PostBuildMenu(ModCompile.Build(modToBuild, Interface.buildMod));
				}
				catch (Exception e)
				{
					ErrorLogger.LogException(e);
				}
			}, 1);
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
			Main.Configuration.Get<string>("ModBrowserPassphrase", ref modBrowserPassphrase);
			Main.Configuration.Get<string>("SteamID64", ref steamID64);
			Main.Configuration.Get<bool>("DownloadModsFromServers", ref ModNet.downloadModsFromServers);
			Main.Configuration.Get<bool>("OnlyDownloadSignedModsFromServers", ref ModNet.onlyDownloadSignedMods);
			Main.Configuration.Get<bool>("DontRemindModBrowserUpdateReload", ref dontRemindModBrowserUpdateReload);
			Main.Configuration.Get<bool>("DontRemindModBrowserDownloadEnable", ref dontRemindModBrowserDownloadEnable);
			Main.Configuration.Get<byte>("MusicStreamMode", ref musicStreamMode);
			Main.Configuration.Get<bool>("AlwaysLogExceptions", ref alwaysLogExceptions);
			Main.Configuration.Get<bool>("RemoveForcedMinimumZoom", ref removeForcedMinimumZoom);
			Main.Configuration.Get<bool>("AllowGreaterResolutions", ref removeForcedMinimumZoom);
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
			return providers.Where(p => p.GetType().GetMethod(method.Name, argTypes)?.DeclaringType != typeof(T)).ToArray();
		}

		internal static MethodInfo Method<T, F>(Expression<Func<T, F>> expr)
		{
			MethodInfo method;
			try
			{
				var convert = expr.Body as UnaryExpression;
				var makeDelegate = convert?.Operand as MethodCallExpression;
				var methodArg = makeDelegate?.Arguments[2] as ConstantExpression;
				method = methodArg?.Value as MethodInfo;
				if (method == null) throw new NullReferenceException();
			}
			catch (Exception e)
			{
				throw new ArgumentException("Invalid hook expression " + expr, e);
			}

			return method;
		}
	}
}