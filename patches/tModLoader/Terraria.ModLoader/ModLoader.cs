using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.Exceptions;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader
{
	public static class ModLoader
	{
		//change Terraria.Main.DrawMenu change drawn version number string to include this
		public static readonly string version = "tModLoader v0.7.1.2";
#if WINDOWS
		public const bool windows = true;

#else
        public const bool windows = false;
		#endif
		//change Terraria.Main.SavePath and cloud fields to use "ModLoader" folder
		public static readonly string ModPath = Main.SavePath + Path.DirectorySeparatorChar + "Mods";
		public static readonly string ModSourcePath = Main.SavePath + Path.DirectorySeparatorChar + "Mod Sources";
		public static readonly string DllPath = Main.SavePath + Path.DirectorySeparatorChar + "dllReferences";
		private static readonly string ImagePath = "Content" + Path.DirectorySeparatorChar + "Images";
		private static bool assemblyResolverAdded = false;
		internal const int earliestRelease = 149;
		internal static string modToBuild;
		internal static bool reloadAfterBuild = false;
		internal static bool buildAll = false;
		internal static int numLoads = 0;
		private static readonly IList<string> loadedMods = new List<string>();
		internal static readonly IDictionary<string, Mod> mods = new Dictionary<string, Mod>();
		private static readonly Stack<string> loadOrder = new Stack<string>();
		private static readonly IDictionary<string, byte[]> files = new Dictionary<string, byte[]>();
		private static readonly IDictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
		private static readonly IDictionary<string, SoundEffect> sounds = new Dictionary<string, SoundEffect>();
		internal static readonly IDictionary<string, Tuple<Mod, string, string>> modHotKeys = new Dictionary<string, Tuple<Mod, string, string>>();

		private static void AddAssemblyResolver()
		{
			if (assemblyResolverAdded)
			{
				return;
			}
			AppDomain.CurrentDomain.AssemblyResolve += ResolveTerrariaReference;
			AppDomain.CurrentDomain.AssemblyResolve += ResolveDllReference;
			AppDomain.CurrentDomain.AssemblyResolve += ResolveModReference;
			assemblyResolverAdded = true;
		}

		private static Assembly ResolveTerrariaReference(object sender, ResolveEventArgs args)
		{
			string name = args.Name;
			if (name.IndexOf(',') >= 0)
			{
				name = name.Substring(0, name.IndexOf(','));
			}
			if (name.StartsWith("Terraria"))
			{
				return Assembly.GetExecutingAssembly();
			}
			return null;
		}

		private static Assembly ResolveDllReference(object sender, ResolveEventArgs args)
		{
			Directory.CreateDirectory(DllPath);
			string name = args.Name;
			if (name.IndexOf(',') >= 0)
			{
				name = name.Substring(0, name.IndexOf(','));
			}
			try
			{
				return Assembly.LoadFrom(DllPath + Path.DirectorySeparatorChar + name + ".dll");
			}
			catch
			{
				return null;
			}
		}

		private static Assembly ResolveModReference(object sender, ResolveEventArgs args)
		{
			string name = args.Name;
			if (name.IndexOf(',') >= 0)
			{
				name = name.Substring(0, name.IndexOf(','));
			}
			Mod mod = GetMod(name);
			return mod?.code;
		}

		internal static bool ModLoaded(string name)
		{
			return loadedMods.Contains(name);
		}

		public static Mod GetMod(string name)
		{
			if (mods.ContainsKey(name))
			{
				return mods[name];
			}
			return null;
		}

		public static string[] GetLoadedMods()
		{
			return loadOrder.ToArray();
		}

		internal static void Load()
		{
			ThreadPool.QueueUserWorkItem(new WaitCallback(do_Load), 1);
		}

		internal static void do_Load(object threadContext)
		{
			if (!LoadMods())
			{
				Main.menuMode = Interface.errorMessageID;
				return;
			}
			if (Main.dedServ)
			{
				Console.WriteLine("Adding mod content...");
			}
			int num = 0;
			foreach (Mod mod in mods.Values)
			{
				Interface.loadMods.SetProgressInit(mod.Name, num, mods.Count);
				try
				{
					mod.Autoload();
					mod.Load();
				}
				catch (Exception e)
				{
					DisableMod(mod.file);
					ErrorLogger.LogLoadingError(mod.file, mod.buildVersion, e);
					Main.menuMode = Interface.errorMessageID;
					return;
				}
				num++;
			}
			Interface.loadMods.SetProgressSetup(0f);
			ResizeArrays();
			num = 0;
			foreach (Mod mod in mods.Values)
			{
				Interface.loadMods.SetProgressLoad(mod.Name, num, mods.Count);
				try
				{
					mod.SetupContent();
					mod.PostSetupContent();
				}
				catch (Exception e)
				{
					DisableMod(mod.file);
					ErrorLogger.LogLoadingError(mod.file, mod.buildVersion, e);
					Main.menuMode = Interface.errorMessageID;
					return;
				}
				num++;
			}
			MapLoader.SetupModMap();
			Interface.loadMods.SetProgressRecipes();
			for (int k = 0; k < Recipe.maxRecipes; k++)
			{
				Main.recipe[k] = new Recipe();
			}
			Recipe.numRecipes = 0;
			try
			{
				CraftGroup.ResetVanillaGroups();
				AddCraftGroups();
				Recipe.SetupRecipes();
			}
			catch (Exception e)
			{
				ErrorLogger.LogLoadingError("recipes", version, e);
				Main.menuMode = Interface.errorMessageID;
				return;
			}
			Main.menuMode = 0;
			numLoads++;
		}

		private static void ResizeArrays(bool unloading = false)
		{
			ItemLoader.ResizeArrays();
			EquipLoader.ResizeAndFillArrays();
			Main.InitializeItemAnimations();
			ModDust.ResizeArrays();
			TileLoader.ResizeArrays(unloading);
			WallLoader.ResizeArrays(unloading);
			ProjectileLoader.ResizeArrays();
			NPCLoader.ResizeArrays();
			NPCHeadLoader.ResizeAndFillArrays();
			ModGore.ResizeAndFillArrays();
			SoundLoader.ResizeAndFillArrays();
			MountLoader.ResizeArrays();
			BuffLoader.ResizeArrays();
		}

		internal static TmodFile[] FindMods()
		{
			Directory.CreateDirectory(ModPath);
			string[] fileNames = Directory.GetFiles(ModPath, "*.tmod", SearchOption.TopDirectoryOnly);
			IList<TmodFile> files = new List<TmodFile>();
			foreach (string fileName in fileNames)
			{
				TmodFile file = new TmodFile(fileName);
				file.Read();
				if (file.ValidMod())
				{
					files.Add(file);
				}
			}
			return files.ToArray();
		}

		private static bool LoadMods()
		{
			//load all referenced assemblies before mods for compiling
			ModCompile.LoadReferences();
			Interface.loadMods.SetProgressFinding();
			TmodFile[] modFiles = FindMods();
			List<TmodFile> enabledMods = new List<TmodFile>();
			foreach (TmodFile modFile in modFiles)
			{
				if (IsEnabled(modFile.Name))
				{
					enabledMods.Add(modFile);
				}
			}
			IDictionary<string, BuildProperties> properties = new Dictionary<string, BuildProperties>();
			List<TmodFile> modsToLoad = new List<TmodFile>();
			foreach (TmodFile modFile in enabledMods)
			{
				properties[modFile.Name] = BuildProperties.ReadModFile(modFile);
				modsToLoad.Add(modFile);
			}
			int previousCount = 0;
			int num = 0;
			Mod defaultMod = new ModLoaderMod();
			defaultMod.Init();
			mods[defaultMod.Name] = defaultMod;
			loadedMods.Add(defaultMod.Name);
			while (modsToLoad.Count > 0 && modsToLoad.Count != previousCount)
			{
				previousCount = modsToLoad.Count;
				int k = 0;
				while (k < modsToLoad.Count)
				{
					bool canLoad = true;
					foreach (string reference in properties[modsToLoad[k].Name].modReferences)
					{
						if (!ModLoaded(ModPath + Path.DirectorySeparatorChar + reference + ".tmod"))
						{
							canLoad = false;
							break;
						}
					}
					if (canLoad)
					{
						Interface.loadMods.SetProgressCompatibility(Path.GetFileNameWithoutExtension(modsToLoad[k].Name), num, enabledMods.Count);
						try
						{
							LoadMod(modsToLoad[k], properties[modsToLoad[k].Name]);
						}
						catch (Exception e)
						{
							DisableMod(modsToLoad[k].Name);
							ErrorLogger.LogLoadingError(modsToLoad[k].Name, properties[modsToLoad[k].Name].modBuildVersion, e);
							return false;
						}
						loadedMods.Add(modsToLoad[k].Name);
						num++;
						modsToLoad.RemoveAt(k);
					}
					else
					{
						k++;
					}
				}
			}
			if (modsToLoad.Count > 0)
			{
				foreach (TmodFile modFile in modsToLoad)
				{
					DisableMod(modFile.Name);
				}
				ErrorLogger.LogMissingLoadReference(modsToLoad);
				return false;
			}
			return true;
		}

		private static void LoadMod(TmodFile modFile, BuildProperties properties)
		{
			AddAssemblyResolver();
			string fileName = Path.GetFileNameWithoutExtension(modFile.Name);
			Interface.loadMods.SetProgressReading(fileName, 0, 2);
			Assembly modCode;
			string rootDirectory;
			var dllFileName = modFile.HasFile("All") ? "All" : windows ? "Windows" : "Other";
			if (properties.includePDB && modFile.HasFile(dllFileName + ".pdb"))
			{
				modCode = Assembly.Load(modFile.GetFile(dllFileName), modFile.GetFile(dllFileName + ".pdb"));
			}
			else
			{
				modCode = Assembly.Load(modFile.GetFile(dllFileName));
			}
			Interface.loadMods.SetProgressReading(fileName, 1, 2);
			using (MemoryStream memoryStream = new MemoryStream(modFile.GetFile("Resources")))
			{
				using (BinaryReader reader = new BinaryReader(memoryStream))
				{
					rootDirectory = reader.ReadString();
					for (string path = reader.ReadString(); path != "end"; path = reader.ReadString())
					{
						byte[] data = reader.ReadBytes(reader.ReadInt32());
						files[path] = data;
						if (Main.dedServ)
						{
							continue;
						}
						string extension = Path.GetExtension(path);
						switch (extension)
						{
							case ".png":
								string texturePath = Path.ChangeExtension(path, null);
								using (MemoryStream buffer = new MemoryStream(data))
								{
									textures[texturePath] = Texture2D.FromStream(Main.instance.GraphicsDevice, buffer);
								}
								break;
							case ".wav":
								string soundPath = Path.ChangeExtension(path, null);
								using (MemoryStream buffer = new MemoryStream(data))
								{
									sounds[soundPath] = SoundEffect.FromStream(buffer);
								}
								break;
							case ".mp3":
								string mp3Path = Path.ChangeExtension(path, null);
								string wavCacheFilename = mp3Path.Replace('/', '_') + "_" + properties.version + ".wav";
								WAVCacheIO.DeleteIfOlder(modFile.Name, wavCacheFilename);
								if (WAVCacheIO.WAVCacheAvailable(wavCacheFilename))
								{
									sounds[mp3Path] = SoundEffect.FromStream(WAVCacheIO.GetWavStream(wavCacheFilename));
									break;
								}
								ushort wFormatTag = 1;
								ushort nChannels;
								uint nSamplesPerSec;
								uint nAvgBytesPerSec;
								ushort nBlockAlign;
								ushort wBitsPerSample = 16;
								const int headerSize = 44;
								using (MemoryStream output = new MemoryStream())
								using (MemoryStream yourMp3FileStream = new MemoryStream(data))
								{
									using (MP3Sharp.MP3Stream input = new MP3Sharp.MP3Stream(yourMp3FileStream))
									{
										using (BinaryWriter writer = new BinaryWriter(output, Encoding.UTF8))
										{
											output.Position = headerSize;
											input.CopyTo(output);
											UInt32 wavDataLength = (UInt32)output.Length - headerSize;
											output.Position = 0;
											nChannels = (ushort)input.ChannelCount;
											nSamplesPerSec = (uint)input.Frequency;
											nBlockAlign = (ushort)(nChannels * (wBitsPerSample / 8));
											nAvgBytesPerSec = (uint)(nSamplesPerSec * nChannels * (wBitsPerSample / 8));
											//write the header
											writer.Write("RIFF".ToCharArray()); //4
											writer.Write((UInt32)(wavDataLength + 36)); // 4
											writer.Write("WAVE".ToCharArray()); //4
											writer.Write("fmt ".ToCharArray()); //4
											writer.Write(16); //4
											writer.Write(wFormatTag);  //
											writer.Write((ushort)nChannels);
											writer.Write(nSamplesPerSec);
											writer.Write(nAvgBytesPerSec);
											writer.Write(nBlockAlign);
											writer.Write(wBitsPerSample);
											writer.Write("data".ToCharArray());
											writer.Write((UInt32)(wavDataLength));
											output.Position = 0;
											WAVCacheIO.SaveWavStream(output, wavCacheFilename);
											sounds[mp3Path] = SoundEffect.FromStream(output);
										}
									}
								}
								break;
						}
					}
				}
			}
			Type[] classes = modCode.GetTypes();
			foreach (Type type in classes)
			{
				if (type.IsSubclassOf(typeof(Mod)))
				{
					Mod mod = (Mod)Activator.CreateInstance(type);
					mod.file = modFile.Name;
					mod.buildVersion = properties.modBuildVersion;
					mod.code = modCode;
					mod.Init();
					if (mod.Name == "Terraria")
					{
						throw new DuplicateNameException("Mods cannot be named Terraria");
					}
					if (mods.ContainsKey(mod.Name))
					{
						throw new DuplicateNameException("Two mods share the internal name " + mod.Name);
					}
					if (rootDirectory != mod.Name)
					{
						throw new MissingResourceException("Mod name " + mod.Name + " does not match source directory name " + rootDirectory);
					}
					mods[mod.Name] = mod;
					loadOrder.Push(mod.Name);
				}
			}
		}

		internal static void Unload()
		{
			while (loadOrder.Count > 0)
			{
				GetMod(loadOrder.Pop()).UnloadContent();
			}
			loadedMods.Clear();
			ItemLoader.Unload();
			EquipLoader.Unload();
			ModDust.Unload();
			TileLoader.Unload();
			WallLoader.Unload();
			ProjectileLoader.Unload();
			NPCLoader.Unload();
			NPCHeadLoader.Unload();
			PlayerHooks.Unload();
			BuffLoader.Unload();
			MountLoader.Unload();
			ModGore.Unload();
			SoundLoader.Unload();
			textures.Clear();
			sounds.Clear();
			mods.Clear();
			ResizeArrays(true);
			MapLoader.UnloadModMap();
			modHotKeys.Clear();
			WorldHooks.Unload();
		}

		internal static void Reload()
		{
			Unload();
			Main.menuMode = Interface.loadModsID;
		}

		internal static bool IsEnabled(string modFile)
		{
			if (modFile == "ModLoader")
			{
				return true;
			}
			string enablePath = Path.ChangeExtension(modFile, "enabled");
			return !File.Exists(enablePath) || File.ReadAllText(enablePath) != "false";
		}

		internal static void SetModActive(string modFile, bool active)
		{
			if (modFile == "ModLoader")
			{
				return;
			}
			string path = Path.ChangeExtension(modFile, "enabled");
			using (StreamWriter writer = File.CreateText(path))
			{
				writer.Write(active ? "true" : "false");
			}
		}

		internal static void EnableMod(string modFile)
		{
			SetModActive(modFile, true);
		}

		internal static void DisableMod(string modFile)
		{
			SetModActive(modFile, false);
		}

		internal static string[] FindModSources()
		{
			Directory.CreateDirectory(ModSourcePath);
			return Directory.GetDirectories(ModSourcePath, "*", SearchOption.TopDirectoryOnly).Where(dir => dir != ".vs").ToArray();
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

		public static byte[] GetFileBytes(string name)
		{
			if (!FileExists(name))
			{
				throw new MissingResourceException("Missing file " + name);
			}
			return files[name];
		}

		public static bool FileExists(string name)
		{
			return files.ContainsKey(name);
		}

		public static Texture2D GetTexture(string name)
		{
			if (Main.dedServ)
				return null;
			if (!TextureExists(name))
			{
				throw new MissingResourceException("Missing texture " + name);
			}
			if (name.IndexOf("Terraria/") == 0)
			{
				name = name.Substring(9);
				return Main.instance.Content.Load<Texture2D>("Images" + Path.DirectorySeparatorChar + name);
			}
			return textures[name];
		}

		public static bool TextureExists(string name)
		{
			if (name.IndexOf("Terraria/") == 0)
			{
				name = name.Substring(9);
				return File.Exists(ImagePath + Path.DirectorySeparatorChar + name + ".xnb");
			}
			return textures.ContainsKey(name);
		}

		public static void AddTexture(string name, Texture2D texture)
		{
			if (TextureExists(name))
			{
				throw new DuplicateNameException("Texture already exist: " + name);
			}
			textures[name] = texture;
		}

		internal static IList<string> GetTextures(Mod mod)
		{
			IList<string> modTextures = new List<string>();
			foreach (string texture in textures.Keys)
			{
				if (texture.IndexOf(mod.Name + "/") == 0)
				{
					modTextures.Add(texture);
				}
			}
			return modTextures;
		}

		public static SoundEffect GetSound(string name)
		{
			if (Main.dedServ)
				return null;
			if (!SoundExists(name))
			{
				throw new MissingResourceException("Missing sound " + name);
			}
			return sounds[name];
		}

		public static bool SoundExists(string name)
		{
			return sounds.ContainsKey(name);
		}

		internal static IList<string> GetSounds(Mod mod)
		{
			IList<string> modSounds = new List<string>();
			foreach (string sound in sounds.Keys)
			{
				if (sound.IndexOf(mod.Name + "/") == 0)
				{
					modSounds.Add(sound);
				}
			}
			return modSounds;
		}

		public static void RegisterHotKey(Mod mod, string name, string defaultKey)
		{
			string configurationString = mod.Name + "_" + "HotKey" + "_" + name.Replace(' ', '_');
			string keyFromConfigutation = Main.Configuration.Get<string>(configurationString, defaultKey);
			modHotKeys[name] = new Tuple<Mod, string, string>(mod, keyFromConfigutation, defaultKey);
		}
		// example: ExampleMod_HotKey_Random_Buff="P"
		internal static void SaveConfiguration()
		{
			foreach (KeyValuePair<string, Tuple<Mod, string, string>> hotKey in modHotKeys)
			{
				string name = hotKey.Value.Item1.Name + "_" + "HotKey" + "_" + hotKey.Key.Replace(' ', '_');
				Main.Configuration.Put(name, hotKey.Value.Item2);
			}
		}

		private static void AddCraftGroups()
		{
			foreach (Mod mod in mods.Values)
			{
				try
				{
					mod.AddCraftGroups();
				}
				catch
				{
					DisableMod(mod.file);
					throw;
				}
			}
		}
		//place near end of Terraria.Recipe.SetupRecipes before material checks
		internal static void AddRecipes()
		{
			foreach (Mod mod in mods.Values)
			{
				try
				{
					mod.AddRecipes();
					foreach (ModItem item in mod.items.Values)
					{
						item.AddRecipes();
					}
				}
				catch
				{
					DisableMod(mod.file);
					throw;
				}
			}
		}
	}
}