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

	HookStorage.Store(new ILHook(hookMethod, il => {
		new ILCursor(il).EmitDelegate<Action>(ServerLoaded);
	}));

	ApplyHooks();

	var tempModsFile = Path.GetTempFileName() + ".json";
	File.WriteAllText(tempModsFile, "[]");

	asm.GetType("Terraria.MonoLaunch").GetMethod("Main", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, [new[] { "-server", "-modpack", tempModsFile }]);
}

void ApplyHooks()
{
	HookStorage.Store(new ILHook(typeof(SpriteBatch).GetConstructors().Single(), il => new ILCursor(il).Emit(OpCodes.Ret)));
	HookStorage.Store(new ILHook(typeof(SpriteBatch).GetMethod("PrepRenderState", BindingFlags.NonPublic | BindingFlags.Instance), il => new ILCursor(il).Emit(OpCodes.Ret)));
	HookStorage.Store(new ILHook(typeof(SpriteBatch).GetMethod("PushSprite", BindingFlags.NonPublic | BindingFlags.Instance), il => new ILCursor(il).Emit(OpCodes.Ret)));

	HookStorage.Store(new ILHook(typeof(TileBatch).GetConstructors().Single(), il => new ILCursor(il).Emit(OpCodes.Ret)));
	HookStorage.Store(new ILHook(typeof(TileBatch).GetMethod("InternalDraw", BindingFlags.NonPublic | BindingFlags.Instance), il => new ILCursor(il).Emit(OpCodes.Ret)));
	HookStorage.Store(new ILHook(typeof(TileBatch).GetMethod("Finalize", BindingFlags.NonPublic | BindingFlags.Instance), il => new ILCursor(il).Emit(OpCodes.Ret)));

	HookStorage.Store(new Hook(typeof(Texture2D).GetConstructors().First(), new Action<Action<Texture2D, GraphicsDevice, int, int>, Texture2D, GraphicsDevice, int, int>((orig, self, gd, w, h) => {
		typeof(Texture2D).GetProperty("Width").GetSetMethod(true).Invoke(self, new object[] { w });
		typeof(Texture2D).GetProperty("Height").GetSetMethod(true).Invoke(self, new object[] { h });
	})));

	HookStorage.Store(new ILHook(typeof(TileDrawing).GetConstructors().Single(), il =>
	{
		var c = new ILCursor(il);
		c.GotoNext(insn => insn.MatchLdcI4(9000));
		c.Remove();
		c.Emit(OpCodes.Ldc_I4, 50000);
	}));

	HookStorage.Store(new Hook(typeof(TilePaintSystemV2).GetMethod("TryGetTileAndRequestIfNotReady", BindingFlags.Public | BindingFlags.Instance), new Func<Func<object, int, int, int, Texture2D>, object, int, int, int, Texture2D>((orig, self, tt, ts, pc) => {
		return Asset<Texture2D>.DefaultValue;
	})));
}

void ServerLoaded() {
	Console.WriteLine("ServerLoaded()");

	Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;

	List<(List<TestResult> results, string name, Func<TestResult> run)> tests =
	[
		(new(), nameof(GenWorld), GenWorld),
		(new(), nameof(SaveWorld), Median(SaveWorld, 5)),
		(new(), nameof(LoadWorld), Median(LoadWorld, 5)),
		(new(), nameof(DrawWorld), Median(DrawWorld, 5)),
	];

	var seeds = new[] { "1", "2", "3" };
	void RunTests()
	{
		foreach (var seed in seeds) {
			Main.worldName = seed;
			Main.ActiveWorldFileData = new WorldFileData(Path.Combine(Main.WorldPath, Main.worldName + ".wld"), false);

			foreach (var test in tests) {
				test.results.Add(test.run());
			}
		}
	}

	Console.Clear();

	// warmup
	RunTests();
	foreach (var test in tests)
		test.results.Clear();
	
	RunTests();

	Console.Clear();
	Console.WriteLine($"Seed:    \t{string.Join("", seeds.Select(s => $"{s,-15}"))}");
	foreach (var (results, name, _) in tests) {
		Console.WriteLine($"{name}:\t{string.Join("", results.Select(r => $"{r,-15}"))}");
	}

	while (true) {
		Console.ReadLine();
	}
}

Func<TestResult> Median(Func<TestResult> run, int attempts) =>
	() => {
		var list = Enumerable.Range(0, attempts).Select(_ => run()).OrderBy(t => t.Total).ToList();
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

TestResult DrawWorld()
{
	void InitAssets<T>(Asset<T>[] assets) where T : class {
		foreach (ref var a in assets.AsSpan())
			a = Asset<T>.Empty;
	}

	Asset<Texture2D>.DefaultValue = new Texture2D(null, 16, 16);
	InitAssets(TextureAssets.Wall);
	InitAssets(TextureAssets.Tile);
	InitAssets(TextureAssets.Flames);
	InitAssets(TextureAssets.GlowMask);
	InitAssets(TextureAssets.Liquid);
	InitAssets(TextureAssets.LiquidSlope);
	TextureAssets.WallOutline = Asset<Texture2D>.Empty;
	TextureAssets.ShroomCap = Asset<Texture2D>.Empty;
	TextureAssets.SunAltar = Asset<Texture2D>.Empty;
	TextureAssets.SunOrb = Asset<Texture2D>.Empty;

	Main.instance.TilePaintSystem = new TilePaintSystemV2();
	Main.instance.TilesRenderer = new TileDrawing(Main.instance.TilePaintSystem);
	Main.instance.WallsRenderer = new WallDrawing(Main.instance.TilePaintSystem);
	Lighting.Mode = LightMode.Color;

	Main.GameViewMatrix = new SpriteViewMatrix(null);
	Main.GameViewMatrix.SetViewportOverride(new Viewport(0, 0, 1, 1));

	Main.spriteBatch = new SpriteBatch(null);
	Main.spriteBatch.Begin();
	Main.tileBatch = new TileBatch(null);

	TimeLogger.Initialize();
	Main.sectionManager = new WorldSections(Main.maxTilesX / 200, Main.maxTilesY / 150);
	Main.TileFrameSeed = 0;

	int stepSize = 1000;
	Main.offScreenRange = stepSize/2 * 16;
	Main.screenWidth = Main.screenHeight = 160;

	var timer = new TestTimer();
	for (int x = stepSize / 2; x < Main.maxTilesX; x += stepSize)
		for (int y = stepSize / 2; y < Main.maxTilesY; y += stepSize) {
			Main.screenPosition = new Vector2(x, y) * 16;
			typeof(Main).GetMethod("DrawWalls", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Main.instance, null);
			Main.instance.TilesRenderer.PreDrawTiles(false, false, true);
			typeof(Main).GetMethod("DrawTiles", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Main.instance, new object[] { false, false, true, -1 });
			Main.instance.TilesRenderer.PreDrawTiles(true, false, true);
			typeof(Main).GetMethod("DrawTiles", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Main.instance, new object[] { true, false, true, -1 });
		}
	return timer.Result;
}

TestResult SaveWorld()
{
	var timer = new TestTimer();
	WorldFile.SaveWorld(false);
	return timer.Result;
}

TestResult LoadWorld()
{
	var timer = new TestTimer();
	WorldFile.LoadWorld(false);
	return timer.Result;
}

TestResult GenWorld()
{
	Main.maxTilesX = 8400;
	Main.maxTilesY = 2400;
	Main.ActiveWorldFileData.SetSeed(Main.worldName);
	Main.menuMode = 10;

	var timer = new TestTimer();

	GenerationProgress generationProgress = new GenerationProgress();
	Task task = WorldGen.CreateNewWorld(generationProgress);
	while (!task.IsCompleted)
	{
		Console.Write($"\r{generationProgress.TotalProgress:0.0%}");
	}
	task.Wait();

	return timer.Result;
}

class HookStorage
{
	public static List<object> storage = new List<object>();

	public static void Store(object hook) => storage.Add(hook);
}

readonly record struct TestResult(TimeSpan Total, TimeSpan GCTime)
{
	public override string ToString() => $"{(long)Total.TotalSeconds}.{Total.Milliseconds / 100:0}s";// GC: {(int)(GCTime/Total*100)}%";
}

class TestTimer
{
	private readonly Stopwatch sw;
	private readonly TimeSpan gc;

	public TestTimer()
	{
		GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive);
		sw = Stopwatch.StartNew();
		gc = GC.GetTotalPauseDuration();
	}

	public TestResult Result => new(sw.Elapsed, GC.GetTotalPauseDuration() - gc);
}