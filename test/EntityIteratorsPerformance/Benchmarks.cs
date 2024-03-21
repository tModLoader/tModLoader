using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.ID;

namespace EntityIteratorsPerformance;

[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Benchmark")]
[SimpleJob, GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory), CategoriesColumn]
public class Benchmarks
{
	public readonly ref struct ActiveEntityIterator<T> where T : Entity
	{
		private readonly ReadOnlySpan<T> span;

		public ActiveEntityIterator(ReadOnlySpan<T> span) => this.span = span;

		public readonly Enumerator GetEnumerator() => new(span.GetEnumerator());

		public ref struct Enumerator
		{
			private ReadOnlySpan<T>.Enumerator enumerator;

			public Enumerator(ReadOnlySpan<T>.Enumerator enumerator) => this.enumerator = enumerator;

			public readonly T Current => enumerator.Current;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool MoveNext()
			{
				while (enumerator.MoveNext()) {
					if (enumerator.Current.active)
						return true;
				}

				return false;
			}
		}
	}

	public readonly ref struct ActiveEntityIteratorUnsafe<T> where T : Entity
	{
		private readonly ReadOnlySpan<T> span;

		public ActiveEntityIteratorUnsafe(ReadOnlySpan<T> span) => this.span = span;

		public readonly Enumerator GetEnumerator() => new(span);

		public ref struct Enumerator
		{
			private ref T begin;
			private ref T end;

			public Enumerator(ReadOnlySpan<T> span)
			{
				begin = ref MemoryMarshal.GetReference(span);
				end = ref Unsafe.Add(ref begin, span.Length);
			}

			public T Current { get; private set; }

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool MoveNext()
			{
				while (Unsafe.IsAddressLessThan(ref begin, ref end)) {
					Current = begin;
					begin = ref Unsafe.Add(ref begin, 1);

					if (Current.active) 
						return true;
				}

				return false;
			}
		}
	}


	private static ReadOnlySpan<NPC> SpanNPCs => Main.npc.AsSpan(0, Main.maxNPCs);
	private static ReadOnlySpan<Projectile> SpanProjectiles => Main.projectile.AsSpan(0, Main.maxProjectiles);
	private static ActiveEntityIterator<NPC> ActiveNPCs => new(SpanNPCs);
	private static ActiveEntityIterator<Projectile> ActiveProjectiles => new(SpanProjectiles);
	private static ActiveEntityIteratorUnsafe<NPC> ActiveNPCsUnsafe => new(SpanNPCs);
	private static ActiveEntityIteratorUnsafe<Projectile> ActiveProjectilesUnsafe => new(SpanProjectiles);

	public NPC NPC;
	public int NPCType;

	[GlobalSetup]
	public void Setup()
	{
		Terraria.Program.SavePath = Path.GetTempPath();

		var rand = new Random(12345);

		for (int i = 0; i < Main.npc.Length; i++) {
			Main.npc[i] = new NPC {
				type = i/2+1,
				active = rand.Next(2) == 1,
				position = new(rand.NextSingle() * 1000 - 500, rand.NextSingle() * 1000 - 500),
				Size = new(50, 50)
			};
		}

		for (int i = 0; i < Main.projectile.Length; i++) {
			Main.projectile[i] = new Projectile {
				type = i/2+1,
				active = rand.Next(2) == 1,
				position = new(rand.NextSingle() * 1000 - 500, rand.NextSingle() * 1000 - 500),
				Size = new(50, 50)
			};
		}

		NPC = Main.npc.Skip(100).First(n => n.active);
		NPCType = NPC.type;

		int activeNpcCount = 0; foreach (var npc in ActiveNPCs) activeNpcCount++;
		Debug.WriteLine($"Active NPC Count {activeNpcCount}");
		int activeProjCount = 0; foreach (var proj in ActiveProjectiles) activeProjCount++;
		Debug.WriteLine($"Active Projectile Count {activeProjCount}");
		int collisions = NPCProjectileCollisions_Iterator();
		Debug.WriteLine($"{collisions}/{activeNpcCount*activeProjCount} Collisions");
		int activeNpcsOfType = 0; foreach (var npc in ActiveNPCs) if (npc.type == NPCType) activeNpcsOfType++;
		Debug.WriteLine($"{activeNpcsOfType} Active NPCs of Type {NPCType}");

		// Active NPC Count 97
		// Active Projectile Count 491
		// 459/47627 Collisions
		// 2 Active NPCs of Type 51
	}

#pragma warning disable IDE0060 // Remove unused parameter
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void NoInline(NPC npc) { }

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void NoInline(Projectile proj) { }
#pragma warning restore IDE0060

	[Benchmark(Baseline = true), BenchmarkCategory("Simple")]
	public void Simple_ForLoop()
	{
		for (int i = 0; i < Main.maxNPCs; i++) {
			var npc = Main.npc[i];

			if (npc.active) {
				NoInline(npc);
			}
		}
	}

	[Benchmark, BenchmarkCategory("Simple")]
	public void Simple_SpanLoop()
	{
		foreach (var npc in SpanNPCs) {
			if (npc.active) {
				NoInline(npc);
			}
		}
	}

	[Benchmark, BenchmarkCategory("Simple")]
	public void Simple_Iterator()
	{
		foreach (var npc in ActiveNPCs) {
			NoInline(npc);
		}
	}

	[Benchmark, BenchmarkCategory("Simple")]
	public void Simple_IteratorUnsafe()
	{
		foreach (var npc in ActiveNPCsUnsafe) {
			NoInline(npc);
		}
	}

	[Benchmark(Baseline = true), BenchmarkCategory("SimpleType")]
	public void SimpleType_ForLoop()
	{
		for (int i = 0; i < Main.maxNPCs; i++) {
			var npc = Main.npc[i];

			if (npc.active && npc.type == NPCType) {
				NoInline(npc);
			}
		}
	}

	[Benchmark, BenchmarkCategory("SimpleType")]
	public void SimpleType_SpanLoop()
	{
		foreach (var npc in SpanNPCs) {
			if (npc.active && npc.type == NPCType) {
				NoInline(npc);
			}
		}
	}

	[Benchmark, BenchmarkCategory("SimpleType")]
	public void SimpleType_Iterator()
	{
		foreach (var npc in ActiveNPCs) {
			if (npc.type == NPCType) {
				NoInline(npc);
			}
		}
	}

	[Benchmark, BenchmarkCategory("SimpleType")]
	public void SimpleType_IteratorUnsafe()
	{
		foreach (var npc in ActiveNPCsUnsafe) {
			if (npc.type == NPCType) {
				NoInline(npc);
			}
		}
	}

	[Benchmark(Baseline = true), BenchmarkCategory("DistanceSq")]
	public NPC DistanceSq_ForLoop()
	{
		NPC closest = null;
		float minSqDist = float.MinValue;
		for (int i = 0; i < Main.maxNPCs; i++) {
			var npc = Main.npc[i];

			if (npc.active && NPC.whoAmI != i && NPC.DistanceSQ(npc.Center) is float distSq && distSq < minSqDist) {
				minSqDist = distSq;
				closest = npc;
			}
		}

		return closest;
	}

	[Benchmark, BenchmarkCategory("DistanceSq")]
	public NPC DistanceSq_SpanLoop()
	{
		NPC closest = null;
		float minSqDist = float.MinValue;
		foreach (var npc in SpanNPCs) {
			if (npc.active && NPC != npc && NPC.DistanceSQ(npc.Center) is float distSq && distSq < minSqDist) {
				minSqDist = distSq;
				closest = npc;
			}
		}

		return closest;
	}

	[Benchmark, BenchmarkCategory("DistanceSq")]
	public NPC DistanceSq_Iterator()
	{
		NPC closest = null;
		float minSqDist = float.MinValue;
		foreach (var npc in ActiveNPCs) {
			if (NPC != npc && NPC.DistanceSQ(npc.Center) is float distSq && distSq < minSqDist) {
				minSqDist = distSq;
				closest = npc;
			}
		}

		return closest;
	}

	[Benchmark, BenchmarkCategory("DistanceSq")]
	public NPC DistanceSq_IteratorUnsafe()
	{
		NPC closest = null;
		float minSqDist = float.MinValue;
		foreach (var npc in ActiveNPCsUnsafe) {
			if (NPC != npc && NPC.DistanceSQ(npc.Center) is float distSq && distSq < minSqDist) {
				minSqDist = distSq;
				closest = npc;
			}
		}

		return closest;
	}

	[Benchmark(Baseline = true), BenchmarkCategory("Collision")]
	public int NPCProjectileCollisions_ForLoop()
	{
		int hits = 0;
		for (int i = 0; i < Main.maxNPCs; i++) {
			var npc = Main.npc[i];

			if (npc.active) {
				for (int j = 0; j < Main.maxProjectiles; j++) {
					var proj = Main.projectile[j];

					if (proj.active && npc.Hitbox.Intersects(proj.Hitbox)) {
						NoInline(proj);
						hits++;
					}
				}
			}
		}
		return hits;
	}

	[Benchmark, BenchmarkCategory("Collision")]
	public int NPCProjectileCollisions_SpanLoop()
	{
		int hits = 0;
		foreach (var npc in SpanNPCs) {
			if (npc.active) {
				foreach (var proj in SpanProjectiles) {
					if (proj.active && npc.Hitbox.Intersects(proj.Hitbox)) {
						NoInline(proj);
						hits++;
					}
				}
			}
		}
		return hits;
	}

	[Benchmark, BenchmarkCategory("Collision")]
	public int NPCProjectileCollisions_Iterator()
	{
		int hits = 0;
		foreach (var npc in ActiveNPCs) {
			foreach (var proj in ActiveProjectiles) {
				if (npc.Hitbox.Intersects(proj.Hitbox)) {
					NoInline(proj);
					hits++;
				}
			}
		}
		return hits;
	}

	[Benchmark, BenchmarkCategory("Collision")]
	public int NPCProjectileCollisions_IteratorUnsafe()
	{
		int hits = 0;
		foreach (var npc in ActiveNPCsUnsafe) {
			foreach (var proj in ActiveProjectilesUnsafe) {
				if (npc.Hitbox.Intersects(proj.Hitbox)) {
					NoInline(proj);
					hits++;
				}
			}
		}
		return hits;
	}
}
