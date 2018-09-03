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

		private static readonly Stack<string> loadOrder = new Stack<string>();
		private static WeakReference[] loadedModsWeakReferences = new WeakReference[0];
		private static Mod[] loadedMods = new Mod[0];
		internal static readonly IDictionary<string, Mod> mods = new Dictionary<string, Mod>(StringComparer.OrdinalIgnoreCase);

		internal static readonly string modBrowserPublicKey = "<RSAKeyValue><Modulus>oCZObovrqLjlgTXY/BKy72dRZhoaA6nWRSGuA+aAIzlvtcxkBK5uKev3DZzIj0X51dE/qgRS3OHkcrukqvrdKdsuluu0JmQXCv+m7sDYjPQ0E6rN4nYQhgfRn2kfSvKYWGefp+kqmMF9xoAq666YNGVoERPm3j99vA+6EIwKaeqLB24MrNMO/TIf9ysb0SSxoV8pC/5P/N6ViIOk3adSnrgGbXnFkNQwD0qsgOWDks8jbYyrxUFMc4rFmZ8lZKhikVR+AisQtPGUs3ruVh4EWbiZGM2NOkhOCOM4k1hsdBOyX2gUliD0yjK5tiU3LBqkxoi2t342hWAkNNb4ZxLotw==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
		internal static string modBrowserPassphrase = "";

		internal static bool isModder;
		internal static bool alwaysLogExceptions;
		internal static bool dontRemindModBrowserUpdateReload;
		internal static bool dontRemindModBrowserDownloadEnable;
		internal static byte musicStreamMode;
		internal static bool removeForcedMinimumZoom;
		internal static bool allowGreaterResolutions;
		internal static string commandLineModPack = "";
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
			set
			{
				steamID64 = value;
			}
		}

		internal static string modToBuild;
		internal static bool reloadAfterBuild = false;
		internal static bool buildAll = false;

		internal static Action PostLoad;

		public static int ModCount => loadedMods.Length;

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

		public static Mod[] LoadedMods => loadedMods;
		public static Mod[] Mods => loadedMods;

		/// <summary>
		/// Returns an array containing the names of all loaded mods. The array entries will be in the reverse order in which the mods were loaded.
		/// </summary>
		public static string[] GetLoadedMods()
		{
			return loadOrder.ToArray();
		}

		internal static void Load()
		{
			ThreadPool.QueueUserWorkItem(do_Load, 1);
		}

		internal static void do_Load(object threadContext)
		{
			if (!LoadMods())
			{
				Main.menuMode = Interface.errorMessageID;
				return;
			}

			ModContent.Load();

			if (PostLoad != null)
			{
				PostLoad();
				PostLoad = null;
			}
			else
			{
				Main.menuMode = 0;
			}
		}

		internal static Dictionary<string, LocalMod> modsDirCache = new Dictionary<string, LocalMod>();
		internal static LocalMod[] FindMods()
		{
			Directory.CreateDirectory(ModPath);
			var mods = new List<LocalMod>();

			foreach (string fileName in Directory.GetFiles(ModPath, "*.tmod", SearchOption.TopDirectoryOnly))
			{
				if (Path.GetFileName(fileName) == "temporaryDownload.tmod")
					continue;
				var lastModified = File.GetLastWriteTime(fileName);
				if (!modsDirCache.TryGetValue(fileName, out var mod) || mod.lastModified != lastModified)
				{
					var modFile = new TmodFile(fileName);
					try
					{
						modFile.Read(TmodFile.LoadedState.Info);
					}
					catch (Exception e) //this will probably spam, given the number of calls to FindMods
					{
						// TODO: Reflect these skipped Mods in the UI somehow.
						//ErrorLogger.LogException(e, Language.GetTextValue("tModLoader.LoadErrorErrorReadingModFile", modFile.path));
						continue;
					}

					mod = new LocalMod(modFile) { lastModified = lastModified };
					modsDirCache[fileName] = mod;
				}
				mods.Add(mod);
			}
			return mods.OrderBy(x => x.Name, StringComparer.InvariantCulture).ToArray();
		}

		private static bool LoadMods()
		{
			//load all referenced assemblies before mods for compiling
			ModCompile.LoadReferences();

			if (!CommandLineModPackOverride())
				return false;

			Interface.loadMods.SetProgressFinding();
			var modsToLoad = FindMods().Where(mod => IsEnabled(mod.Name) && LoadSide(mod.properties.side)).ToList();

			// Press shift while starting up tModLoader or while trapped in a reload cycle to skip loading all mods.
			if (Main.oldKeyState.PressingShift())
				modsToLoad.Clear();

			if (!VerifyNames(modsToLoad))
				return false;

			try
			{
				EnsureDependenciesExist(modsToLoad, false);
				EnsureTargetVersionsMet(modsToLoad);
				modsToLoad = Sort(modsToLoad);
			}
			catch (ModSortingException e)
			{
				foreach (var mod in e.errored)
					mod.Enabled = false;

				ErrorLogger.LogDependencyError(e.Message);
				return false;
			}

			var modInstances = AssemblyManager.InstantiateMods(modsToLoad);
			if (modInstances == null)
				return false;

			modInstances.Insert(0, new ModLoaderMod());
			loadedMods = modInstances.ToArray();
			loadedModsWeakReferences = loadedMods.Skip(1).Select(x => new WeakReference(x)).ToArray();
			foreach (var mod in modInstances)
			{
				loadOrder.Push(mod.Name);
				mods[mod.Name] = mod;
			}

			return true;
		}

		private static bool CommandLineModPackOverride()
		{
			if (commandLineModPack == "")
				return true;

			if (!commandLineModPack.EndsWith(".json"))
				commandLineModPack += ".json";

			string filePath = Path.Combine(UI.UIModPacks.ModListSaveDirectory, commandLineModPack);

			try
			{
				Directory.CreateDirectory(UI.UIModPacks.ModListSaveDirectory);

				Console.WriteLine(Language.GetTextValue("tModLoader.LoadingSpecifiedModPack", commandLineModPack) + "\n");
				var modSet = JsonConvert.DeserializeObject<HashSet<string>>(File.ReadAllText(filePath));
				foreach (var mod in FindMods())
				{
					SetModEnabled(mod.Name, modSet.Contains(mod.Name));
				}

				return true;
			}
			catch (Exception e)
			{
				string err;
				if (e is FileNotFoundException)
					err = Language.GetTextValue("tModLoader.ModPackDoesNotExist", filePath) + "\n";
				else
					err = Language.GetTextValue("tModLoader.ModPackDoesNotExist", commandLineModPack, e.Message) + "\n";

				if (Main.dedServ)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine(err);
					Console.ResetColor();
				}
				else
				{
					Interface.errorMessage.SetMessage(err);
				}

				return false;
			}
			finally
			{
				commandLineModPack = "";
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

		private static bool VerifyNames(List<LocalMod> mods)
		{
			var names = new HashSet<string>();
			foreach (var mod in mods)
			{
				try
				{
					if (mod.Name.Length == 0)
						throw new ModNameException(Language.GetTextValue("tModLoader.BuildErrorModNameEmpty"));

					if (mod.Name.Equals("Terraria", StringComparison.InvariantCultureIgnoreCase))
						throw new ModNameException(Language.GetTextValue("tModLoader.BuildErrorModNamedTerraria"));

					if (names.Contains(mod.Name))
						throw new ModNameException(Language.GetTextValue("tModLoader.BuildErrorTwoModsSameName", mod.Name));

					if (mod.Name.IndexOf('.') >= 0)
						throw new ModNameException(Language.GetTextValue("tModLoader.BuildErrorModNameHasPeriod"));

					names.Add(mod.Name);
				}
				catch (Exception e)
				{
					mod.Enabled = false;
					ErrorLogger.LogLoadingError(mod.Name, mod.modFile.tModLoaderVersion, e);
					return false;
				}
			}

			return true;
		}

		internal static void EnsureDependenciesExist(ICollection<LocalMod> mods, bool includeWeak)
		{
			var nameMap = mods.ToDictionary(mod => mod.Name);
			var errored = new HashSet<LocalMod>();
			var errorLog = new StringBuilder();

			foreach (var mod in mods)
				foreach (var depName in mod.properties.RefNames(includeWeak))
					if (!nameMap.ContainsKey(depName))
					{
						errored.Add(mod);
						errorLog.AppendLine(Language.GetTextValue("tModLoader.LoadErrorDependencyMissing", depName, mod));
					}

			if (errored.Count > 0)
				throw new ModSortingException(errored, errorLog.ToString());
		}

		internal static void EnsureTargetVersionsMet(ICollection<LocalMod> mods)
		{
			var nameMap = mods.ToDictionary(mod => mod.Name);
			var errored = new HashSet<LocalMod>();
			var errorLog = new StringBuilder();

			foreach (var mod in mods)
				foreach (var dep in mod.properties.Refs(true))
					if (nameMap.TryGetValue(dep.mod, out var inst) && inst.properties.version < dep.target)
					{
						errored.Add(mod);
						errorLog.AppendLine(Language.GetTextValue("tModLoader.LoadErrorDependencyVersionTooLow", mod, dep.target, dep.mod, inst.properties.version));
					}

			if (errored.Count > 0)
				throw new ModSortingException(errored, errorLog.ToString());
		}

		internal static void EnsureSyncedDependencyStability(TopoSort<LocalMod> synced, TopoSort<LocalMod> full)
		{
			var errored = new HashSet<LocalMod>();
			var errorLog = new StringBuilder();

			foreach (var mod in synced.list)
			{
				var chains = new List<List<LocalMod>>();
				//define recursive chain finding method
				Action<LocalMod, Stack<LocalMod>> FindChains = null;
				FindChains = (search, stack) =>
				{
					stack.Push(search);

					if (search.properties.side == ModSide.Both && stack.Count > 1)
					{
						if (stack.Count > 2)//direct Both -> Both references are ignored
							chains.Add(stack.Reverse().ToList());
					}
					else
					{//recursively build the chain, all entries in stack should be unsynced
						foreach (var dep in full.Dependencies(search))
							FindChains(dep, stack);
					}

					stack.Pop();
				};
				FindChains(mod, new Stack<LocalMod>());

				if (chains.Count == 0)
					continue;

				var syncedDependencies = synced.AllDependencies(mod);
				foreach (var chain in chains)
					if (!syncedDependencies.Contains(chain.Last()))
					{
						errored.Add(mod);
						errorLog.AppendLine(mod + " indirectly depends on " + chain.Last() + " via " + string.Join(" -> ", chain));
					}
			}

			if (errored.Count > 0)
			{
				errorLog.AppendLine("Some of these mods may not exist on both client and server. Add a direct sort entries or weak references.");
				throw new ModSortingException(errored, errorLog.ToString());
			}
		}

		private static TopoSort<LocalMod> BuildSort(ICollection<LocalMod> mods)
		{
			var nameMap = mods.ToDictionary(mod => mod.Name);
			return new TopoSort<LocalMod>(mods,
				mod => mod.properties.sortAfter.Where(nameMap.ContainsKey).Select(name => nameMap[name]),
				mod => mod.properties.sortBefore.Where(nameMap.ContainsKey).Select(name => nameMap[name]));
		}

		internal static List<LocalMod> Sort(ICollection<LocalMod> mods)
		{
			var preSorted = mods.OrderBy(mod => mod.Name).ToList();
			var syncedSort = BuildSort(preSorted.Where(mod => mod.properties.side == ModSide.Both).ToList());
			var fullSort = BuildSort(preSorted);
			EnsureSyncedDependencyStability(syncedSort, fullSort);

			try
			{
				var syncedList = syncedSort.Sort();

				//preserve synced order
				for (int i = 1; i < syncedList.Count; i++)
					fullSort.AddEntry(syncedList[i - 1], syncedList[i]);

				return fullSort.Sort();
			}
			catch (TopoSort<LocalMod>.SortingException e)
			{
				throw new ModSortingException(e.set, e.Message);
			}
		}

		internal static void Unload()
		{
			while (loadOrder.Count > 0)
				GetMod(loadOrder.Pop()).UnloadContent();

			loadOrder.Clear();
			loadedMods = new Mod[0];
			mods.Clear();

			ModContent.Unload();
			GC.Collect();

			if (isModder) {
				foreach (var mod in loadedModsWeakReferences.Where(r => r.IsAlive).Select(r => (Mod)r.Target))
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

		internal static bool LoadSide(ModSide side) => side != (Main.dedServ ? ModSide.Client : ModSide.Server);

		/// <summary>A cached list of enabled mods (not necessarily currently loaded or even installed), mirroring the enabled.json file.</summary>
		private static HashSet<string> _enabledMods;

		internal static HashSet<string> EnabledMods
		{
			get
			{
				if (_enabledMods == null)
				{
					try
					{
						string path = ModPath + Path.DirectorySeparatorChar + "enabled.json";
						_enabledMods = JsonConvert.DeserializeObject<HashSet<string>>(File.ReadAllText(path));
					}
					catch
					{
						_enabledMods = new HashSet<string>();
					}
				}
				return _enabledMods;
			}
		}

		internal static bool IsEnabled(string modName) => EnabledMods.Contains(modName);

		internal static void EnableMod(string modName) => SetModEnabled(modName, true);
		internal static void DisableMod(string modName) => SetModEnabled(modName, false);

		internal static void SetModEnabled(string modName, bool active)
		{
			if (active)
				EnabledMods.Add(modName);
			else
				EnabledMods.Remove(modName);

			//save
			Directory.CreateDirectory(ModPath);
			string path = ModPath + Path.DirectorySeparatorChar + "enabled.json";
			_enabledMods.IntersectWith(ModLoader.FindMods().Select(x => x.Name)); // Clear out mods that no longer exist.
			string json = JsonConvert.SerializeObject(EnabledMods, Formatting.Indented);
			File.WriteAllText(path, json);
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
			Main.Configuration.Put("ModBrowserPassphrase", ModLoader.modBrowserPassphrase);
			Main.Configuration.Put("SteamID64", ModLoader.steamID64);
			Main.Configuration.Put("DownloadModsFromServers", ModNet.downloadModsFromServers);
			Main.Configuration.Put("OnlyDownloadSignedModsFromServers", ModNet.onlyDownloadSignedMods);
			Main.Configuration.Put("DontRemindModBrowserUpdateReload", ModLoader.dontRemindModBrowserUpdateReload);
			Main.Configuration.Put("DontRemindModBrowserDownloadEnable", ModLoader.dontRemindModBrowserDownloadEnable);
			Main.Configuration.Put("MusicStreamMode", ModLoader.musicStreamMode);
			Main.Configuration.Put("AlwaysLogExceptions", ModLoader.alwaysLogExceptions);
			Main.Configuration.Put("RemoveForcedMinimumZoom", ModLoader.removeForcedMinimumZoom);
			Main.Configuration.Put("AllowGreaterResolutions", ModLoader.allowGreaterResolutions);
		}

		internal static void LoadConfiguration()
		{
			Main.Configuration.Get<string>("ModBrowserPassphrase", ref ModLoader.modBrowserPassphrase);
			Main.Configuration.Get<string>("SteamID64", ref ModLoader.steamID64);
			Main.Configuration.Get<bool>("DownloadModsFromServers", ref ModNet.downloadModsFromServers);
			Main.Configuration.Get<bool>("OnlyDownloadSignedModsFromServers", ref ModNet.onlyDownloadSignedMods);
			Main.Configuration.Get<bool>("DontRemindModBrowserUpdateReload", ref ModLoader.dontRemindModBrowserUpdateReload);
			Main.Configuration.Get<bool>("DontRemindModBrowserDownloadEnable", ref ModLoader.dontRemindModBrowserDownloadEnable);
			Main.Configuration.Get<byte>("MusicStreamMode", ref ModLoader.musicStreamMode);
			Main.Configuration.Get<bool>("AlwaysLogExceptions", ref ModLoader.alwaysLogExceptions);
			Main.Configuration.Get<bool>("RemoveForcedMinimumZoom", ref ModLoader.removeForcedMinimumZoom);
			Main.Configuration.Get<bool>("AllowGreaterResolutions", ref ModLoader.removeForcedMinimumZoom);
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
		[Obsolete("ModLoader.GetFileBytes is deprecated since v0.10.1.4, use ModContent.GetFileBytes instead.")]
		public static byte[] GetFileBytes(string name) => ModContent.GetFileBytes(name);
		
		[Obsolete("ModLoader.FileExists is deprecated since v0.10.1.4, use ModContent.FileExists instead.")]
		public static bool FileExists(string name) => ModContent.FileExists(name);
		
		[Obsolete("ModLoader.GetTexture is deprecated since v0.10.1.4, use ModContent.GetTexture instead.")]
		public static Texture2D GetTexture(string name) => ModContent.GetTexture(name);

		[Obsolete("ModLoader.TextureExists is deprecated since v0.10.1.4, use ModContent.TextureExists instead.")]
		public static bool TextureExists(string name) => ModContent.TextureExists(name);
		
		[Obsolete("ModLoader.GetSound is deprecated since v0.10.1.4, use ModContent.GetSound instead.")]
		public static SoundEffect GetSound(string name) => ModContent.GetSound(name);
		
		[Obsolete("ModLoader.SoundExists is deprecated since v0.10.1.4, use ModContent.SoundExists instead.")]
		public static bool SoundExists(string name) => ModContent.SoundExists(name);

		[Obsolete("ModLoader.GetMusic is deprecated since v0.10.1.4, use ModContent.GetMusic instead.")]
		public static Music GetMusic(string name) => ModContent.GetMusic(name);
		
		[Obsolete("ModLoader.MusicExists is deprecated since v0.10.1.4, use ModContent.MusicExists instead.")]
		public static bool MusicExists(string name) => ModContent.MusicExists(name);
		
		[Obsolete("ModLoader.RegisterHotKey is deprecated since v0.10.1.4, use ModContent.RegisterHotKey instead.")]
		public static ModHotKey RegisterHotKey(Mod mod, string name, string defaultKey) => ModContent.RegisterHotKey(mod, name, defaultKey);
	}
}
