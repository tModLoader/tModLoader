using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.CSharp;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.Exceptions;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader
{
	public static class ModLoader
	{
		//change Terraria.Main.DrawMenu change drawn version number string to include this
		public static readonly string version = "tModLoader v0.7.1";
		#if WINDOWS
		private const bool windows = true;

#else
		private const bool windows = false;
		#endif
		//change Terraria.Main.SavePath and cloud fields to use "ModLoader" folder
		public static readonly string ModPath = Main.SavePath + Path.DirectorySeparatorChar + "Mods";
		public static readonly string ModSourcePath = Main.SavePath + Path.DirectorySeparatorChar + "Mod Sources";
		public static readonly string DllPath = Main.SavePath + Path.DirectorySeparatorChar + "dllReferences";
		private static readonly string ImagePath = "Content" + Path.DirectorySeparatorChar + "Images";
		private static bool referencesLoaded = false;
		private static bool assemblyResolverAdded = false;
		private static readonly IList<string> buildReferences = new List<string>();
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
		internal static readonly IDictionary<string, Tuple<string, string>> modHotKeys = new Dictionary<string, Tuple<string, string>>();

		private static void LoadReferences()
		{
			if (referencesLoaded)
			{
				return;
			}
			Assembly current = Assembly.GetExecutingAssembly();
			buildReferences.Add(current.Location);
			AssemblyName[] references = current.GetReferencedAssemblies();
			foreach (AssemblyName reference in references)
			{
				buildReferences.Add(Assembly.Load(reference).Location);
			}
			referencesLoaded = true;
		}

		private static void AddAssemblyResolver()
		{
			if (assemblyResolverAdded)
			{
				return;
			}
			AppDomain.CurrentDomain.AssemblyResolve += ResolveDllReference;
			AppDomain.CurrentDomain.AssemblyResolve += ResolveModReference;
			assemblyResolverAdded = true;
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
			if (mod == null)
			{
				return null;
			}
			return mod.code;
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

		private static void do_Load(object threadContext)
		{
			if (!LoadMods())
			{
				Main.menuMode = Interface.errorMessageID;
				return;
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
			if (modFile.HasFile("All"))
			{
				modCode = Assembly.Load(modFile.GetFile("All"));
			}
			else
			{
				modCode = Assembly.Load(modFile.GetFile(windows ? "Windows" : "Other"));
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
								ushort wFormatTag = 1;
								ushort nChannels;
								uint nSamplesPerSec;
								uint nAvgBytesPerSec;
								ushort nBlockAlign;
								ushort wBitsPerSample = 16;
								const int headerSize = 44;
								MemoryStream output = new MemoryStream(headerSize + data.Length);
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
			ThreadPool.QueueUserWorkItem(new WaitCallback(do_BuildAllMods), 1);
		}

		internal static void do_BuildAllMods(object threadContext)
		{
			string[] modFolders = FindModSources();
			int num = 0;
			bool flag = false;
			foreach (string modFolder in modFolders)
			{
				Interface.buildMod.SetProgress(num, modFolders.Length);
				modToBuild = modFolder;
				if (!do_BuildMod(threadContext))
				{
					flag = true;
				}
				num++;
			}
			Main.menuMode = flag ? Interface.errorMessageID : (reloadAfterBuild ? Interface.reloadModsID : 0);
		}

		internal static void BuildMod()
		{
			Interface.buildMod.SetProgress(0, 1);
			ThreadPool.QueueUserWorkItem(new WaitCallback(delegate (object threadContext)
					{
						try
						{
							do_BuildMod(threadContext);
						}
						catch (Exception e)
						{
							ErrorLogger.LogException(e);
						}
					}), 1);
		}

		internal static bool do_BuildMod(object threadContext)
		{
			Interface.buildMod.SetReading();
			BuildProperties properties = BuildProperties.ReadBuildFile(modToBuild);
			if (!CreateModReferenceDlls(properties))
			{
				if (!buildAll)
				{
					Main.menuMode = Interface.errorMessageID;
				}
				return false;
			}
			LoadReferences();
			Interface.buildMod.SetCompiling();
			byte[] windowsData;
			byte[] otherData;
			if (properties.noCompile)
			{
				string modDir = modToBuild + Path.DirectorySeparatorChar;
				if (File.Exists(modDir + "All.dll"))
				{
					windowsData = File.ReadAllBytes(modDir + "All.dll");
					otherData = File.ReadAllBytes(modDir + "All.dll");
				}
				else if (File.Exists(modDir + "Windows.dll") && File.Exists(modDir + "Other.dll"))
				{
					windowsData = File.ReadAllBytes(modDir + "Windows.dll");
					otherData = File.ReadAllBytes(modDir + "Other.dll");
				}
				else
				{
					ErrorLogger.LogDllBuildError(modToBuild);
					if (!buildAll)
					{
						Main.menuMode = Interface.errorMessageID;
					}
					return false;
				}
			}
			else
			{
				windowsData = CompileMod(modToBuild, properties, true);
				otherData = CompileMod(modToBuild, properties, false);
				if (windowsData == null || otherData == null)
				{
					if (!buildAll)
					{
						Main.menuMode = Interface.errorMessageID;
					}
					return false;
				}
			}
			Interface.buildMod.SetBuildText();
			string file = ModPath + Path.DirectorySeparatorChar + Path.GetFileName(modToBuild) + ".tmod";
			byte[] propertiesData = properties.ToBytes();
			TmodFile modFile = new TmodFile(file);
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(memoryStream))
				{
					writer.Write(version);
					writer.Write(propertiesData.Length);
					writer.Write(propertiesData);
					writer.Flush();
					modFile.AddFile("Info", memoryStream.ToArray());
				}
			}
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(memoryStream))
				{
					writer.Write(Path.GetFileName(modToBuild));
					string[] resources = Directory.GetFiles(modToBuild, "*", SearchOption.AllDirectories);
					foreach (string resource in resources)
					{
						if (Path.GetExtension(resource) == ".cs" && !properties.includeSource)
						{
							continue;
						}
						if (Path.GetFileName(resource) == "Thumbs.db")
						{
							continue;
						}
						if (resource.Substring(modToBuild.Length + 1) == "build.txt")
						{
							continue;
						}
						string resourcePath = resource.Replace(ModSourcePath + Path.DirectorySeparatorChar, null);
						resourcePath = resourcePath.Replace(Path.DirectorySeparatorChar, '/');
						byte[] buffer = File.ReadAllBytes(resource);
						writer.Write(resourcePath);
						writer.Write(buffer.Length);
						writer.Write(buffer);
					}
					writer.Write("end");
					writer.Flush();
					modFile.AddFile("Resources", memoryStream.ToArray());
				}
			}
			bool same = windowsData.Length == otherData.Length;
			if (same)
			{
				for (int k = 0; k < windowsData.Length; k++)
				{
					if (windowsData[k] != otherData[k])
					{
						same = false;
						break;
					}
				}
			}
			if (same)
			{
				modFile.AddFile("All", windowsData);
			}
			else
			{
				modFile.AddFile("Windows", windowsData);
				modFile.AddFile("Other", otherData);
			}
			modFile.Save();
			EnableMod(file);
			if (!buildAll)
			{
				Main.menuMode = reloadAfterBuild ? Interface.reloadModsID : 0;
			}
			return true;
		}

		internal static bool CreateModReferenceDlls(BuildProperties properties)
		{
			TmodFile[] refFiles = new TmodFile[properties.modReferences.Length];
			for (int k = 0; k < properties.modReferences.Length; k++)
			{
				string modReference = properties.modReferences[k];
				string filePath = ModLoader.ModPath + Path.DirectorySeparatorChar + modReference + ".tmod";
				TmodFile refFile = new TmodFile(filePath);
				refFile = new TmodFile(filePath);
				refFile.Read();
				if (!refFile.ValidMod())
				{
					ErrorLogger.LogModReferenceError(refFile.Name);
					return false;
				}
				refFiles[k] = refFile;
			}
			for (int k = 0; k < refFiles.Length; k++)
			{
				TmodFile refFile = refFiles[k];
				string modReference = properties.modReferences[k];
				byte[] data1;
				byte[] data2;
				if (refFile.HasFile("All"))
				{
					data1 = refFile.GetFile("All");
					data2 = refFile.GetFile("All");
				}
				else
				{
					data1 = refFile.GetFile("Windows");
					data2 = refFile.GetFile("Other");
				}
				string refFileName = ModLoader.ModSourcePath + Path.DirectorySeparatorChar + modReference;
				File.WriteAllBytes(refFileName + "1.dll", data1);
				File.WriteAllBytes(refFileName + "2.dll", data2);
			}
			return true;
		}

		private static byte[] CompileMod(string modDir, BuildProperties properties, bool forWindows)
		{
			CompilerParameters compileOptions = new CompilerParameters();
			compileOptions.GenerateExecutable = false;
			compileOptions.GenerateInMemory = false;
			string outFile = ModPath + Path.DirectorySeparatorChar + Path.GetFileName(modDir) + ".dll";
			compileOptions.OutputAssembly = outFile;
			bool flag = false;
			foreach (string reference in buildReferences)
			{
				if (forWindows)
				{
					if (reference.IndexOf("FNA.dll") >= 0)
					{
						compileOptions.ReferencedAssemblies.Add("Microsoft.Xna.Framework.dll");
						compileOptions.ReferencedAssemblies.Add("Microsoft.Xna.Framework.Game.dll");
						compileOptions.ReferencedAssemblies.Add("Microsoft.Xna.Framework.Graphics.dll");
						compileOptions.ReferencedAssemblies.Add("Microsoft.Xna.Framework.Xact.dll");
						continue;
					}
					else if (!windows && reference.IndexOf("Terraria.exe") >= 0)
					{
						compileOptions.ReferencedAssemblies.Add("TerrariaWindows.exe");
						continue;
					}
				}
				else
				{
					if (reference.IndexOf("Microsoft.Xna.Framework") >= 0)
					{
						if (!flag)
						{
							compileOptions.ReferencedAssemblies.Add("FNA.dll");
							flag = true;
						}
						continue;
					}
					else if (windows && reference.IndexOf(System.AppDomain.CurrentDomain.FriendlyName) >= 0)
					{
						compileOptions.ReferencedAssemblies.Add("TerrariaMac.exe");
						continue;
					}
				}
				compileOptions.ReferencedAssemblies.Add(reference);
			}
			Directory.CreateDirectory(DllPath);
			foreach (string reference in properties.dllReferences)
			{
				compileOptions.ReferencedAssemblies.Add(DllPath + Path.DirectorySeparatorChar + reference + ".dll");
			}
			foreach (string reference in properties.modReferences)
			{
				string refFile = ModSourcePath + Path.DirectorySeparatorChar + reference;
				string realRefFile = refFile + ".dll";
				refFile += forWindows ? "1.dll" : "2.dll";
				if (File.Exists(realRefFile))
				{
					File.Delete(realRefFile);
				}
				File.Move(refFile, realRefFile);
				compileOptions.ReferencedAssemblies.Add(realRefFile);
			}
			var options = new Dictionary<string, string> { { "CompilerVersion", "v4.0" } };
			CodeDomProvider codeProvider = new CSharpCodeProvider(options);
			CompilerResults results = codeProvider.CompileAssemblyFromFile(compileOptions, Directory.GetFiles(modDir, "*.cs", SearchOption.AllDirectories));
			CompilerErrorCollection errors = results.Errors;
			foreach (string reference in properties.modReferences)
			{
				File.Delete(ModSourcePath + Path.DirectorySeparatorChar + reference + ".dll");
			}
			if (errors.HasErrors)
			{
				ErrorLogger.LogCompileErrors(errors);
				return null;
			}
			byte[] data = File.ReadAllBytes(outFile);
			File.Delete(outFile);
			return data;
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

		public static void RegisterHotKey(string name, string defaultKey)
		{
			modHotKeys[name] = new Tuple<string, string>(defaultKey, defaultKey);
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