using BenchmarkDotNet.Attributes;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ID;

namespace EntityIteratorsPerformance;

[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Benchmark")]
public class Benchmarks
{
	private static ReadOnlySpan<NPC> SpanActiveNPCs => Main.npc.AsSpan(0, Main.maxNPCs);
	private static ReadOnlySpan<Projectile> SpanActiveProjs => Main.projectile.AsSpan(0, Main.maxProjectiles);

	public NPC NPC;
	public int NPCType;
	public float DistanceSq;

	[GlobalSetup]
	public void Setup()
	{
		Terraria.Program.SavePath = Path.GetTempPath();

		var rand = new Random(12345);

		Main.npc = new NPC[Main.maxNPCs + 1];
		for (int i = 0; i < Main.npc.Length; i++) {
			Main.npc[i] = new NPC {
				type = rand.Next(0, NPCID.Count),
				active = rand.Next(2) == 1
			};
		}

		Main.projectile = new Projectile[Main.maxProjectiles + 1];
		for (int i = 0; i < Main.projectile.Length; i++) {
			Main.projectile[i] = new Projectile {
				type = rand.Next(0, ProjectileID.Count),
				active = rand.Next(2) == 1
			};
		}

		NPC = new NPC {
			type = rand.Next(0, NPCID.Count),
			whoAmI = rand.Next(0, Main.maxNPCs),
			active = true
		};
		NPCType = NPC.type;

		DistanceSq = (float)rand.NextDouble() * 500f;
		DistanceSq *= DistanceSq;
	}

	[GlobalCleanup]
	public void Cleanup()
	{
		Array.Clear(Main.projectile);
		Main.projectile = null;

		Array.Clear(Main.npc);
		Main.npc = null;
	}

#pragma warning disable IDE0060 // Remove unused parameter
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void NoInline(NPC npc) { }

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void NoInline(Projectile proj) { }
#pragma warning restore IDE0060

	[Benchmark]
	public void Simple_ForLoop()
	{
		for (int i = 0; i < Main.maxNPCs; i++) {
			var npc = Main.npc[i];

			if (npc.active) {
				NoInline(npc);
			}
		}
	}

	[Benchmark]
	public void Simple_SpanLoop()
	{
		foreach (var npc in SpanActiveNPCs) {
			if (npc.active) {
				NoInline(npc);
			}
		}
	}

	[Benchmark]
	public void SimpleType_ForLoop()
	{
		for (int i = 0; i < Main.maxNPCs; i++) {
			var npc = Main.npc[i];

			if (npc.active && npc.type == NPCType) {
				NoInline(npc);
			}
		}
	}

	[Benchmark]
	public void SimpleType_SpanLoop()
	{
		foreach (var npc in SpanActiveNPCs) {
			if (npc.active && npc.type == NPCType) {
				NoInline(npc);
			}
		}
	}

	[Benchmark]
	public void DistanceSq_ForLoop()
	{
		for (int i = 0; i < Main.maxNPCs; i++) {
			var npc = Main.npc[i];

			if (npc.active && NPC.whoAmI != i && NPC.DistanceSQ(npc.Center) <= DistanceSq) {
				NoInline(npc);
			}
		}
	}

	[Benchmark]
	public void DistanceSq_SpanLoop()
	{
		foreach (var npc in SpanActiveNPCs) {
			if (npc.active && NPC.whoAmI != npc.whoAmI && NPC.DistanceSQ(npc.Center) <= DistanceSq) {
				NoInline(npc);
			}
		}
	}

	[Benchmark]
	public void NPCProjectileCollisions_ForLoop()
	{
		for (int i = 0; i < Main.maxNPCs; i++) {
			var npc = Main.npc[i];

			if (npc.active) {
				NoInline(npc);

				for (int j = 0; j < Main.maxProjectiles; j++) {
					var proj = Main.projectile[j];

					if (proj.active && npc.Hitbox.Intersects(proj.Hitbox)) {
						NoInline(proj);
					}
				}
			}
		}
	}

	[Benchmark]
	public void NPCProjectileCollisions_SpanLoop()
	{
		foreach (var npc in SpanActiveNPCs) {
			if (npc.active) {
				NoInline(npc);

				foreach (var proj in SpanActiveProjs) {
					if (proj.active && npc.Hitbox.Intersects(proj.Hitbox)) {
						NoInline(proj);
					}
				}
			}
		}
	}
}
