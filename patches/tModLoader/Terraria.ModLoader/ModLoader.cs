using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.CSharp;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.Default;
using Microsoft.Xna.Framework.Audio;

namespace Terraria.ModLoader
{
	public static class ModLoader
	{
		//change Terraria.Main.DrawMenu change drawn version number string to include this
		public static readonly string version = "tModLoader v0.5";
		//change Terraria.Main.SavePath to use "ModLoader" folder
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
		private static readonly IList<string> loadedMods = new List<string>();
		internal static readonly IDictionary<string, Mod> mods = new Dictionary<string, Mod>();
		private static readonly IDictionary<string, byte[]> files = new Dictionary<string, byte[]>();
		private static readonly IDictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
		private static readonly IDictionary<string, SoundEffect> sounds = new Dictionary<string, SoundEffect>();

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
					ErrorLogger.LogLoadingError(mod.file, e);
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
				}
				catch (Exception e)
				{
					DisableMod(mod.file);
					ErrorLogger.LogLoadingError(mod.file, e);
					Main.menuMode = Interface.errorMessageID;
					return;
				}
				num++;
			}
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
				ErrorLogger.LogLoadingError("recipes", e);
				Main.menuMode = Interface.errorMessageID;
				return;
			}
			Main.menuMode = 0;
		}

		private static void ResizeArrays(bool unloading = false)
		{
			ItemLoader.ResizeArrays();
			EquipLoader.ResizeAndFillArrays();
			Main.InitializeItemAnimations();
			TileLoader.ResizeArrays(unloading);
			WallLoader.ResizeArrays(unloading);
			ProjectileLoader.ResizeArrays();
			NPCLoader.ResizeArrays();
			ModGore.ResizeAndFillArrays();
			NPCHeadLoader.ResizeAndFillArrays();
			SoundLoader.ResizeAndFillArrays();
			MountLoader.ResizeArrays();
		}

		internal static string[] FindMods()
		{
			Directory.CreateDirectory(ModPath);
			return Directory.GetFiles(ModPath, "*.tmod", SearchOption.TopDirectoryOnly);
		}

		private static bool LoadMods()
		{
			Interface.loadMods.SetProgressFinding();
			string[] modFiles = FindMods();
			List<string> enabledMods = new List<string>();
			foreach (string modFile in modFiles)
			{
				if (IsEnabled(modFile))
				{
					enabledMods.Add(modFile);
				}
			}
			IDictionary<string, BuildProperties> properties = new Dictionary<string, BuildProperties>();
			List<string> modsToLoad = new List<string>();
			foreach (string modFile in enabledMods)
			{
				properties[modFile] = LoadBuildProperties(modFile);
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
					foreach (string reference in properties[modsToLoad[k]].modReferences)
					{
						if (!ModLoaded(ModPath + Path.DirectorySeparatorChar + reference + ".tmod"))
						{
							canLoad = false;
							break;
						}
					}
					if (canLoad)
					{
						Interface.loadMods.SetProgressReading(Path.GetFileNameWithoutExtension(modsToLoad[k]), num, enabledMods.Count);
						try
						{
							LoadMod(modsToLoad[k], properties[modsToLoad[k]]);
						}
						catch (Exception e)
						{
							DisableMod(modsToLoad[k]);
							ErrorLogger.LogLoadingError(modsToLoad[k], e);
							return false;
						}
						loadedMods.Add(modsToLoad[k]);
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
				foreach (string modFile in modsToLoad)
				{
					DisableMod(modFile);
				}
				ErrorLogger.LogMissingLoadReference(modsToLoad);
				return false;
			}
			return true;
		}

		private static void LoadMod(string modFile, BuildProperties properties)
		{
			AddAssemblyResolver();
			Assembly modCode;
			string rootDirectory;
			using (FileStream fileStream = File.OpenRead(modFile))
			{
				using (BinaryReader reader = new BinaryReader(fileStream))
				{
					fileStream.Seek(reader.ReadInt32(), SeekOrigin.Current);
					modCode = Assembly.Load(reader.ReadBytes(reader.ReadInt32()));
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
					mod.file = modFile;
					mod.code = modCode;
					mod.Init();
					if (mods.ContainsKey(mod.Name))
					{
						throw new Exception("Two mods share the internal name " + mod.Name);
					}
					if (rootDirectory != mod.Name)
					{
						throw new Exception("Mod name " + mod.Name + " does not match source directory name " + rootDirectory);
					}
					mods[mod.Name] = mod;
				}
			}
		}

		internal static BuildProperties LoadBuildProperties(string modFile)
		{
			BuildProperties properties = new BuildProperties();
			byte[] data;
			using (FileStream fileStream = File.OpenRead(modFile))
			{
				using (BinaryReader reader = new BinaryReader(fileStream))
				{
					data = reader.ReadBytes(reader.ReadInt32());
				}
			}
			if (data.Length == 0)
			{
				return properties;
			}
			using (MemoryStream memoryStream = new MemoryStream(data))
			{
				using (BinaryReader reader = new BinaryReader(memoryStream))
				{
					for (string tag = reader.ReadString(); tag.Length > 0; tag = reader.ReadString())
					{
						if (tag == "dllReferences")
						{
							List<string> dllReferences = new List<string>();
							for (string reference = reader.ReadString(); reference.Length > 0; reference = reader.ReadString())
							{
								dllReferences.Add(reference);
							}
							properties.dllReferences = dllReferences.ToArray();
						}
						if (tag == "modReferences")
						{
							List<string> modReferences = new List<string>();
							for (string reference = reader.ReadString(); reference.Length > 0; reference = reader.ReadString())
							{
								modReferences.Add(reference);
							}
							properties.modReferences = modReferences.ToArray();
						}
						if (tag == "author")
						{
							properties.author = reader.ReadString();
						}
						if (tag == "version")
						{
							properties.version = reader.ReadString();
						}
						if (tag == "displayName")
						{
							properties.displayName = reader.ReadString();
						}
					}
				}
			}
			return properties;
		}

		internal static void Unload()
		{
			foreach (Mod mod in mods.Values)
			{
				mod.Unload();
			}
			loadedMods.Clear();
			ItemLoader.Unload();
			EquipLoader.Unload();
			TileLoader.Unload();
			WallLoader.Unload();
			ProjectileLoader.Unload();
			NPCLoader.Unload();
			ModGore.Unload();
			NPCHeadLoader.Unload();
			SoundLoader.Unload();
			textures.Clear();
			sounds.Clear();
			mods.Clear();
			MountLoader.Unload();
			ResizeArrays(true);
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
			ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(object threadContext)
					{
						do_BuildMod(threadContext);
					}), 1);
		}

		internal static bool do_BuildMod(object threadContext)
		{
			Interface.buildMod.SetReading();
			BuildProperties properties = ReadBuildProperties(modToBuild);
			if (properties == null)
			{
				if (!buildAll)
				{
					Main.menuMode = Interface.errorMessageID;
				}
				return false;
			}
			LoadReferences();
			Interface.buildMod.SetCompiling();
			if (!CompileMod(modToBuild, properties))
			{
				if (!buildAll)
				{
					Main.menuMode = Interface.errorMessageID;
				}
				return false;
			}
			Interface.buildMod.SetBuildText();
			byte[] propertiesData = PropertiesToBytes(properties);
			string file = ModPath + Path.DirectorySeparatorChar + Path.GetFileName(modToBuild) + ".tmod";
			byte[] buffer = File.ReadAllBytes(file);
			using (FileStream fileStream = File.Create(file))
			{
				using (BinaryWriter writer = new BinaryWriter(fileStream))
				{
					writer.Write(propertiesData.Length);
					writer.Write(propertiesData);
					writer.Write(buffer.Length);
					writer.Write(buffer);
					writer.Write(Path.GetFileName(modToBuild));
					string[] resources = Directory.GetFiles(modToBuild, "*", SearchOption.AllDirectories);
					foreach (string resource in resources)
					{
						string resourcePath = resource.Replace(ModSourcePath + Path.DirectorySeparatorChar, null);
						resourcePath = resourcePath.Replace(Path.DirectorySeparatorChar, '/');
						buffer = File.ReadAllBytes(resource);
						writer.Write(resourcePath);
						writer.Write(buffer.Length);
						writer.Write(buffer);
					}
					writer.Write("end");
				}
			}
			EnableMod(file);
			if (!buildAll)
			{
				Main.menuMode = reloadAfterBuild ? Interface.reloadModsID : 0;
			}
			return true;
		}

		private static BuildProperties ReadBuildProperties(string modDir)
		{
			string propertiesFile = modDir + Path.DirectorySeparatorChar + "build.txt";
			BuildProperties properties = new BuildProperties();
			if (!File.Exists(propertiesFile))
			{
				return properties;
			}
			string[] lines = File.ReadAllLines(propertiesFile);
			foreach (string line in lines)
			{
				if (line.Length == 0)
				{
					continue;
				}
				int split = line.IndexOf('=');
				string property = line.Substring(0, split).Trim();
				string value = line.Substring(split + 1).Trim();
				if (value.Length == 0)
				{
					continue;
				}
				switch (property)
				{
					case "dllReferences":
						string[] dllReferences = value.Split(',');
						for (int k = 0; k < dllReferences.Length; k++)
						{
							string dllReference = dllReferences[k].Trim();
							if (dllReference.Length > 0)
							{
								dllReferences[k] = dllReference;
							}
						}
						properties.dllReferences = dllReferences;
						break;
					case "modReferences":
						string[] modReferences = value.Split(',');
						for (int k = 0; k < modReferences.Length; k++)
						{
							string modReference = modReferences[k].Trim();
							if (modReference.Length > 0)
							{
								modReferences[k] = modReference;
							}
						}
						properties.modReferences = modReferences;
						break;
					case "author":
						properties.author = value;
						break;
					case "version":
						properties.version = value;
						break;
					case "displayName":
						properties.displayName = value;
						break;
				}
			}
			foreach (string modReference in properties.modReferences)
			{
				string path = ModPath + Path.DirectorySeparatorChar + modReference + ".tmod";
				if (!File.Exists(path))
				{
					ErrorLogger.LogModReferenceError(modReference);
					return null;
				}
				byte[] data;
				using (FileStream fileStream = File.OpenRead(path))
				{
					using (BinaryReader reader = new BinaryReader(fileStream))
					{
						fileStream.Seek(reader.ReadInt32(), SeekOrigin.Current);
						data = reader.ReadBytes(reader.ReadInt32());
					}
				}
				using (FileStream writeStream = File.Create(ModSourcePath + Path.DirectorySeparatorChar + modReference + ".dll"))
				{
					using (BinaryWriter writer = new BinaryWriter(writeStream))
					{
						writer.Write(data);
					}
				}
			}
			return properties;
		}

		private static byte[] PropertiesToBytes(BuildProperties properties)
		{
			byte[] data;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(memoryStream))
				{
					if (properties.dllReferences.Length > 0)
					{
						writer.Write("dllReferences");
						foreach (string reference in properties.dllReferences)
						{
							writer.Write(reference);
						}
						writer.Write("");
					}
					if (properties.modReferences.Length > 0)
					{
						writer.Write("modReferences");
						foreach (string reference in properties.modReferences)
						{
							writer.Write(reference);
						}
						writer.Write("");
					}
					if (properties.author.Length > 0)
					{
						writer.Write("author");
						writer.Write(properties.author);
					}
					if (properties.version.Length > 0)
					{
						writer.Write("version");
						writer.Write(properties.version);
					}
					if (properties.displayName.Length > 0)
					{
						writer.Write("displayName");
						writer.Write(properties.displayName);
					}
					writer.Write("");
					writer.Flush();
					data = memoryStream.ToArray();
				}
			}
			return data;
		}

		private static bool CompileMod(string modDir, BuildProperties properties)
		{
			CompilerParameters compileOptions = new CompilerParameters();
			compileOptions.GenerateExecutable = false;
			compileOptions.GenerateInMemory = false;
			compileOptions.OutputAssembly = ModPath + Path.DirectorySeparatorChar + Path.GetFileName(modDir) + ".tmod";
			foreach (string reference in buildReferences)
			{
				compileOptions.ReferencedAssemblies.Add(reference);
			}
			Directory.CreateDirectory(DllPath);
			foreach (string reference in properties.dllReferences)
			{
				compileOptions.ReferencedAssemblies.Add(DllPath + Path.DirectorySeparatorChar + reference + ".dll");
			}
			foreach (string reference in properties.modReferences)
			{
				compileOptions.ReferencedAssemblies.Add(ModSourcePath + Path.DirectorySeparatorChar + reference + ".dll");
			}
			CodeDomProvider codeProvider = new CSharpCodeProvider();
			CompilerResults results = codeProvider.CompileAssemblyFromFile(compileOptions, Directory.GetFiles(modDir, "*.cs", SearchOption.AllDirectories));
			CompilerErrorCollection errors = results.Errors;
			foreach (string reference in properties.modReferences)
			{
				File.Delete(ModSourcePath + Path.DirectorySeparatorChar + reference + ".dll");
			}
			if (errors.HasErrors)
			{
				ErrorLogger.LogCompileErrors(errors);
				return false;
			}
			return true;
		}

		public static byte[] GetFileBytes(string name)
		{
			if (!FileExists(name))
			{
				throw new ArgumentException("Missing file " + name);
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
				throw new ArgumentException("Missing texture " + name);
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
				throw new ArgumentException("Texture already exist: " + name);
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
				throw new ArgumentException("Missing sound " + name);
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