using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using ReLogic.Content;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.Graphics;
using Terraria.Graphics.Light;
using Terraria.IO;
using Terraria.WorldBuilding;

foreach (var f in Directory.GetFiles(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "tModLoader*.*"))
	File.Delete(f);
foreach (var f in Directory.GetFiles(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "FNA*.*"))
	File.Delete(f);
foreach (var f in Directory.GetFiles(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "ReLogic*.*"))
	File.Delete(f);

AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
{
	var lib = Directory.GetFiles(Directory.GetCurrentDirectory(), new AssemblyName(args.Name).Name + ".dll", SearchOption.AllDirectories).Single();
	return AssemblyLoadContext.Default.LoadFromAssemblyPath(lib);
};

var asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(Directory.GetCurrentDirectory(), "tModLoader.dll"));
var hookMethod = asm.GetType("Terraria.Main").GetMethod("DedServ_PostModLoad", BindingFlags.Instance | BindingFlags.NonPublic);
new ILHook(hookMethod, il =>
{
    new ILCursor(il).EmitDelegate<Action>(ServerLoaded);
});

ApplyHooks();
asm.GetType("MonoLaunch").GetMethod("Main", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new[] { new [] { "-server" } });


void ApplyHooks()
{
	new ILHook(typeof(SpriteBatch).GetConstructors().Single(), il => new ILCursor(il).Emit(OpCodes.Ret));
	new ILHook(typeof(SpriteBatch).GetMethod("PrepRenderState", BindingFlags.NonPublic | BindingFlags.Instance), il => new ILCursor(il).Emit(OpCodes.Ret));
	new ILHook(typeof(SpriteBatch).GetMethod("PushSprite", BindingFlags.NonPublic | BindingFlags.Instance), il => new ILCursor(il).Emit(OpCodes.Ret));

	new ILHook(typeof(TileBatch).GetConstructors().Single(), il => new ILCursor(il).Emit(OpCodes.Ret));
	new ILHook(typeof(TileBatch).GetMethod("InternalDraw", BindingFlags.NonPublic | BindingFlags.Instance), il => new ILCursor(il).Emit(OpCodes.Ret));
	new ILHook(typeof(TileBatch).GetMethod("Finalize", BindingFlags.NonPublic | BindingFlags.Instance), il => new ILCursor(il).Emit(OpCodes.Ret));

	new Hook(typeof(Texture2D).GetConstructors().First(), new Action<Action<Texture2D, GraphicsDevice, int, int>, Texture2D, GraphicsDevice, int, int>((orig, self, gd, w, h) => {
		typeof(Texture2D).GetProperty("Width").GetSetMethod(true).Invoke(self, new object[] { w });
		typeof(Texture2D).GetProperty("Height").GetSetMethod(true).Invoke(self, new object[] { h });
	}));

	new ILHook(typeof(TileDrawing).GetConstructors().Single(), il =>
	{
		var c = new ILCursor(il);
		c.GotoNext(insn => insn.MatchLdcI4(9000));
		c.Remove();
		c.Emit(OpCodes.Ldc_I4, 50000);
	});
}

void ServerLoaded()
{
	Console.Clear();
	Main.worldName = "1";
	Main.ActiveWorldFileData = new WorldFileData(Path.Combine(Main.WorldPath, Main.worldName + ".wld"), false);
	GenWorld();
	SaveWorld();
	LoadWorld();
	DrawWorld();
	//ExportLightmap();

	List<TimeSpan> gen = new();
	List<TimeSpan> save = new();
	List<TimeSpan> load = new();
	List<TimeSpan> draw = new();
	List<TimeSpan> light = new();


	foreach (var seed in new[] { "1", "2", "3" })
	{
		Main.worldName = seed;
		Main.ActiveWorldFileData = new WorldFileData(Path.Combine(Main.WorldPath, Main.worldName + ".wld"), false);
		gen.Add(GenWorld());
		save.Add(SaveWorld());
		load.Add(LoadWorld());
		DrawWorld(); // reduces profiling jitter, sometimes a DrawWorld call takes a long time, maybe the JIT is a bit slower to warmup?
		draw.Add(DrawWorld());
		//light.Add(ExportLightmap());
	}

	Console.Clear();
	Console.WriteLine(Result(nameof(GenWorld), gen));
	Console.WriteLine(Result(nameof(SaveWorld), save));
	Console.WriteLine(Result(nameof(LoadWorld), load));
	Console.WriteLine(Result(nameof(DrawWorld), draw));
	//Console.WriteLine(Result(nameof(ExportLightmap), light));

	while (true)
	{
		Console.ReadLine();
	}
}

string Result(string name, List<TimeSpan> times)
{
	return $"{name}: \t{string.Join('\t', times.Select(t => $"{(long)t.TotalSeconds}.{t.Milliseconds / 100:0}s"))}";
}

TimeSpan ExportLightmap()
{
	int w = 180, h = 120;
	var lightMap = new LightMap();
	lightMap.SetSize(w, h);

	var sw = new Stopwatch();
	sw.Start();
	for (int x = 5; x + w < Main.maxTilesX - 5; x += w)
	{
		for (int y = 5; y < Main.maxTilesY - 5; y += h)
		{
			new TileLightScanner().ExportTo(new Rectangle(x, y, w, h), lightMap);
		}
	}
	return sw.Elapsed;
}

TimeSpan DrawWorld()
{
	Asset<Texture2D>.DefaultValue = new Texture2D(null, 16, 16);
	InitAssets(TextureAssets.Wall);
	InitAssets(TextureAssets.Tile);
	InitAssets(TextureAssets.Flames);
	InitAssets(TextureAssets.GlowMask);
	InitAssets(TextureAssets.Liquid);
	TextureAssets.WallOutline = Asset<Texture2D>.Empty;
	TextureAssets.ShroomCap = Asset<Texture2D>.Empty;
	TextureAssets.SunAltar = Asset<Texture2D>.Empty;
	TextureAssets.SunOrb = Asset<Texture2D>.Empty;

	Main.instance.TilePaintSystem = new TilePaintSystemV2();
	Main.instance.TilesRenderer = new TileDrawing(Main.instance.TilePaintSystem);
	Main.instance.WallsRenderer = new WallDrawing(Main.instance.TilePaintSystem);
	Lighting.Mode = LightMode.Color;
	Main.offScreenRange = 10000*16;
	Main.GameViewMatrix = new SpriteViewMatrix(null);
	Main.GameViewMatrix.SetViewportOverride(new Viewport(0, 0, 1, 1));

	Main.spriteBatch = new SpriteBatch(null);
	Main.spriteBatch.Begin();
	Main.tileBatch = new TileBatch(null);

	TimeLogger.Initialize();
	Main.sectionManager = new WorldSections(Main.maxTilesX / 200, Main.maxTilesY / 150);

	var sw = new Stopwatch();
	sw.Start();
	typeof(Main).GetMethod("DrawWalls", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Main.instance, null);
	typeof(Main).GetMethod("DrawTiles", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Main.instance, new object[] { false, false, true, -1 });
	typeof(Main).GetMethod("DrawTiles", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Main.instance, new object[] { true, false, true, -1 });
	return sw.Elapsed;
}

void InitAssets<T>(Asset<T>[] assets) where T : class
{
    for (int i = 0; i < assets.Length; i++) {
		assets[i] = Asset<T>.Empty;
    }
}

TimeSpan SaveWorld()
{
	var sw = new Stopwatch();
	sw.Start();
	WorldFile.SaveWorld(false);
	return sw.Elapsed;
}

TimeSpan LoadWorld()
{
	var sw = new Stopwatch();
	sw.Start();
	WorldFile.LoadWorld(false);
	return sw.Elapsed;
}

TimeSpan GenWorld()
{
	Main.maxTilesX = 8400;
	Main.maxTilesY = 2400;
	Main.ActiveWorldFileData.SetSeed(Main.worldName);
	Main.menuMode = 10;

	var sw = new Stopwatch();
	sw.Start();

	GenerationProgress generationProgress = new GenerationProgress();
	Task task = WorldGen.CreateNewWorld(generationProgress);
	while (!task.IsCompleted)
	{
		Console.Write($"\r{generationProgress.TotalProgress:0.0%}");
	}
	task.Wait();

	return sw.Elapsed;
}