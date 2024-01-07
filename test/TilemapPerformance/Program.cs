using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using ReLogic.Content;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.Graphics;
using Terraria.Graphics.Light;
using Terraria.IO;
using Terraria.WorldBuilding;

foreach (var f in Directory.GetFiles(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))) {
	if ((f.EndsWith(".pdb") || f.EndsWith(".dll") || f.EndsWith(".xml")) && Path.GetFileNameWithoutExtension(f) != "TilemapPerformance")
		File.Delete(f);
}

AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
{
	var asmName = new AssemblyName(args.Name);
	var dir = Path.Combine("Libraries", asmName.Name);

	var files = Directory.GetFiles(dir, asmName.Name + ".dll", SearchOption.AllDirectories);
	var path = files.Count() == 1
		? files.First() : files.Where(f => f.Contains(RuntimeInformation.RuntimeIdentifier)).Single();

	return AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(path));
};

var asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(Directory.GetCurrentDirectory(), args[0]));
Launch();

void Launch() {
	var types = asm.GetTypes();
	var hookMethod = asm.GetType("Terraria.Main").GetMethod("DedServ_PostModLoad", BindingFlags.Instance | BindingFlags.NonPublic);
	new ILHook(hookMethod, il =>
	{
		new ILCursor(il).EmitDelegate<Action>(ServerLoaded);
		Console.WriteLine("Applied hook!");
	});

	ApplyHooks();
	asm.GetType("Terraria.MonoLaunch").GetMethod("Main", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new[] { new[] { "-server" } });
}

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

void ServerLoaded() {
	Console.WriteLine("ServerLoaded()");

	Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;

	List<(List<TimeSpan> results, string name, Func<TimeSpan> run)> tests = new();
	tests.Add((new(), nameof(GenWorld), GenWorld));
	tests.Add((new(), nameof(SaveWorld), Median(SaveWorld, 5)));
	tests.Add((new(), nameof(LoadWorld), Median(LoadWorld, 5)));
	tests.Add((new(), nameof(DrawWorld), Median(DrawWorld, 5)));

	Console.Clear();
	Main.worldName = "1";
	Main.ActiveWorldFileData = new WorldFileData(Path.Combine(Main.WorldPath, Main.worldName + ".wld"), false);

	// warmup
	for (int i = 0; i < 3; i++) {
		foreach (var test in tests) {
			test.run();
		}
	}


	foreach (var seed in new[] { "3", "2", "1" }) {
		Main.worldName = seed;
		Main.ActiveWorldFileData = new WorldFileData(Path.Combine(Main.WorldPath, Main.worldName + ".wld"), false);

		foreach (var test in tests) {
			test.results.Add(test.run());
		}
	}

	Console.Clear();
	foreach (var test in tests) {
		Console.WriteLine($"{test.name}: \t{string.Join('\t', test.results.Select(t => $"{(long)t.TotalSeconds}.{t.Milliseconds / 100:0}s"))}");
	}

	while (true) {
		Console.ReadLine();
	}
}

Func<TimeSpan> Median(Func<TimeSpan> run, int attempts) =>
	() => {
		var list = Enumerable.Range(0, attempts).Select(_ => run()).OrderBy(t => t).ToList();
		return list[attempts / 2];
	};

TimeSpan ExportLightmap()
{
	int w = 180, h = 120;
	var lightMap = new LightMap();
	lightMap.SetSize(w, h);

	var lightMapOptions = new TileLightScannerOptions() {
		DrawInvisibleWalls = true,
	};

	var sw = new Stopwatch();
	sw.Start();
	for (int x = 5; x + w < Main.maxTilesX - 5; x += w)
	{
		for (int y = 5; y < Main.maxTilesY - 5; y += h)
		{
			new TileLightScanner().ExportTo(new Rectangle(x, y, w, h), lightMap, lightMapOptions);
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
	Main.TileFrameSeed = 0;

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