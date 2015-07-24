using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Microsoft.CSharp;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;

namespace Terraria.ModLoader {
public static class ModLoader
{
    //change Terraria.Main.SavePath to use "ModLoader" folder
    public static readonly string ModPath = Main.SavePath + Path.DirectorySeparatorChar + "Mods";
    public static readonly string ModSourcePath = Main.SavePath + Path.DirectorySeparatorChar + "Mod Sources";
    private static bool referencesLoaded = false;
    private static readonly IList<string> buildReferences = new List<string>();
    internal const int earliestRelease = 149;
    internal static string modToBuild;
    internal static bool reloadAfterBuild = false;
    internal static bool buildAll = false;
    private static readonly IList<string> loadedMods = new List<string>();
    internal static readonly IDictionary<string, Mod> mods = new Dictionary<string, Mod>();
    private static readonly IDictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

    private static void LoadReferences()
    {
        if(referencesLoaded)
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

    internal static bool ModLoaded(string name)
    {
        return loadedMods.Contains(name);
    }

    public static Mod GetMod(string name)
    {
        if(mods.ContainsKey(name))
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
        if(!LoadMods())
        {
            Main.menuMode = Interface.errorMessageID;
            return;
        }
        int num = 0;
        foreach(Mod mod in mods.Values)
        {
            Interface.loadMods.SetProgressInit(mod.Name, num, mods.Count);
            try
            {
                mod.Load();
                mod.Autoload();
            }
            catch(Exception e)
            {
                DisableMod(mod.file);
                ErrorLogger.LogLoadingError(mod.file, e);
                Main.menuMode = Interface.errorMessageID;
                return;
            }
            num++;
        }
        Interface.loadMods.SetProgressSetup(0f);
        ItemLoader.ResizeArrays();
        EquipLoader.ResizeAndFillArrays();
        Main.InitializeItemAnimations();
        num = 0;
        foreach(Mod mod in mods.Values)
        {
            Interface.loadMods.SetProgressLoad(mod.Name, num, mods.Count);
            try
            {
                mod.SetupContent();
            }
            catch(Exception e)
            {
                DisableMod(mod.file);
                ErrorLogger.LogLoadingError(mod.file, e);
                Main.menuMode = Interface.errorMessageID;
                return;
            }
            num++;
        }
        Interface.loadMods.SetProgressRecipes();
        Recipe.numRecipes = 0;
        try
        {
            Recipe.SetupRecipes();
        }
        catch(Exception e)
        {
            ErrorLogger.LogLoadingError("recipes", e);
            Main.menuMode = Interface.errorMessageID;
            return;
        }
        Main.menuMode = 0;
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
        foreach(string modFile in modFiles)
        {
            if (IsEnabled(modFile))
            {
                enabledMods.Add(modFile);
            }
        }
        int num = 0;
        foreach(string modFile in enabledMods)
        {
            Interface.loadMods.SetProgressReading(Path.GetFileNameWithoutExtension(modFile), num, enabledMods.Count);
            try
            {
                LoadMod(modFile);
            }
            catch(Exception e)
            {
                DisableMod(modFile);
                ErrorLogger.LogLoadingError(modFile, e);
                return false;
            }
            loadedMods.Add(modFile);
            num++;
        }
        return true;
    }

    private static void LoadMod(string modFile)
    {
        Assembly modCode;
        using(FileStream fileStream = File.OpenRead(modFile))
        {
            using(BinaryReader reader = new BinaryReader(fileStream))
            {
                modCode = Assembly.Load(reader.ReadBytes(reader.ReadInt32()));
                for(string texturePath = reader.ReadString(); texturePath != "end"; texturePath = reader.ReadString())
                {
                    byte[] imageData = reader.ReadBytes(reader.ReadInt32());
                    using(MemoryStream buffer = new MemoryStream(imageData))
                    {
                        textures[texturePath] = Texture2D.FromStream(Main.instance.GraphicsDevice, buffer);
                    }
                }
            }
        }
        Type[] classes = modCode.GetTypes();
        foreach(Type type in classes)
        {
            if(type.IsSubclassOf(typeof(Mod)))
            {
                Mod mod = (Mod)Activator.CreateInstance(type);
                mod.file = modFile;
                mod.code = modCode;
                mod.Init();
                mods[mod.Name] = mod;
            }
        }
    }

    internal static void Unload()
    {
        foreach(Mod mod in mods.Values)
        {
            mod.Unload();
        }
        loadedMods.Clear();
        ItemLoader.Unload();
        EquipLoader.Unload();
        textures.Clear();
        mods.Clear();
    }

    internal static void Reload()
    {
        Unload();
        Main.menuMode = Interface.loadModsID;
    }

    internal static bool IsEnabled(string modFile)
    {
        string enablePath = Path.ChangeExtension(modFile, "enabled");
        return !File.Exists(enablePath) || File.ReadAllText(enablePath) != "false";
    }

    internal static void SetModActive(string modFile, bool active)
    {
        string path = Path.ChangeExtension(modFile, "enabled");
        using(StreamWriter writer = File.CreateText(path))
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
        return Directory.GetDirectories(ModSourcePath, "*", SearchOption.TopDirectoryOnly);
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
        foreach(string modFolder in modFolders)
        {
            Interface.buildMod.SetProgress(num, modFolders.Length);
            modToBuild = modFolder;
            if(!do_BuildMod(threadContext))
            {
                flag = true;
            }
            num++;
        }
        Main.menuMode = flag ? Interface.errorMessageID : ( reloadAfterBuild ? Interface.loadModsID : 0);
    }

    internal static void BuildMod()
    {
        Interface.buildMod.SetProgress(0, 1);
        ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(object threadContext) { do_BuildMod(threadContext); }), 1);
    }

    internal static bool do_BuildMod(object threadContext)
    {
        LoadReferences();
        Interface.buildMod.SetCompiling();
        if (!CompileMod(modToBuild))
        {
            if (!buildAll)
            {
                Main.menuMode = Interface.errorMessageID;
            }
            return false;
        }
        Interface.buildMod.SetBuildText();
        string file = ModPath + Path.DirectorySeparatorChar + Path.GetFileName(modToBuild) + ".tmod";
        byte[] buffer = File.ReadAllBytes(file);
        using(FileStream fileStream = File.Create(file))
        {
            using(BinaryWriter writer = new BinaryWriter(fileStream))
            {
                writer.Write(buffer.Length);
                writer.Write(buffer);
                string[] images = Directory.GetFiles(modToBuild, "*.png", SearchOption.AllDirectories);
                foreach (string image in images)
                {
                    string texturePath = image.Replace(ModSourcePath + Path.DirectorySeparatorChar, null);
                    texturePath = Path.ChangeExtension(texturePath.Replace(Path.DirectorySeparatorChar, '/'), null);
                    buffer = File.ReadAllBytes(image);
                    writer.Write(texturePath);
                    writer.Write(buffer.Length);
                    writer.Write(buffer);
                }
                writer.Write("end");
            }
        }
        EnableMod(file);
        if (!buildAll)
        {
            Main.menuMode = reloadAfterBuild ? Interface.loadModsID : 0;
        }
        return true;
    }

    private static bool CompileMod(string modDir)
    {
        CompilerParameters compileOptions = new CompilerParameters();
        compileOptions.GenerateExecutable = false;
        compileOptions.GenerateInMemory = false;
        compileOptions.OutputAssembly = ModPath + Path.DirectorySeparatorChar + Path.GetFileName(modDir) + ".tmod";
        foreach(string reference in buildReferences)
        {
            compileOptions.ReferencedAssemblies.Add(reference);
        }

        CodeDomProvider codeProvider = new CSharpCodeProvider();
        CompilerResults results = codeProvider.CompileAssemblyFromFile(compileOptions, Directory.GetFiles(modDir, "*.cs", SearchOption.AllDirectories));
        CompilerErrorCollection errors = results.Errors;
        if(errors.HasErrors)
        {
            ErrorLogger.LogCompileErrors(errors);
            return false;
        }
        return true;
    }

    public static Texture2D GetTexture(string name)
    {
        return textures[name];
    }

    //place near end of Terraria.Recipe.SetupRecipes before material checks
    internal static void AddRecipes()
    {
        foreach(Mod mod in mods.Values)
        {
            try
            {
                mod.AddRecipes();
            }
            catch
            {
                DisableMod(mod.file);
                throw;
            }
        }
    }
}}