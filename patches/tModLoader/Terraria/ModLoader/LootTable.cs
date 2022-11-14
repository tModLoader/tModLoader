using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Utilities;

namespace Terraria.ModLoader {
	public static class LootTables {
		public static LootTable TravellingMerchant;

		public static void InitializeTravellingMerchantLoot() {
			const int r0 = 6;
			const int r1 = 5;
			const int r2 = 4;
			const int r3 = 3;
			const int r4 = 2;
			const int r5 = 1;

			TravellingMerchant = new() {
				Pools = new() {
					new() {
						Entries = new() {
							new(3309, r4),
							new(3314, r3),
							new(1987, r5),

							new(2270, r4, onRoll: () => Main.hardMode ? null : false),
							new(4760, r4, onRoll: () => Main.hardMode ? null : false),
							new(2278, r4),
							new(2271, r4),

							new(2223, r3, onRoll: () => (Main.hardMode && NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3) ? null : false),
							new(2272, r3),
							new(2276, r3),
							new(2284, r3),
							new(2285, r3),
							new(2286, r3),
							new(2287, r3),
							new(4744, r3),
							new(2296, r3, onRoll: () => NPC.downedBoss3 ? null : false),
							new(3628, r3),
							new(4091, r3, onRoll: () => Main.hardMode ? null : false),
							new(4603, r3),
							new(4604, r3),
							new(5297, r3),
							new(4605, r3),
							new(4550, r3),

							new(2268, r2),
							new(2269, r2, onRoll: () => WorldGen.shadowOrbSmashed ? null : false),
							new(1988, r2),
							new(2275, r2),
							new(2279, r2),
							new(2277, r2),
							new PoolEntry(4555, r0).Then(new(4556, 1)).Then(new(4557, 1)),
							new PoolEntry(4321, r2).Then(new(4322, 1)),
							new PoolEntry(4323, r2).Then(new(4324, 1)).Then(new(4365, 1)),
							new PoolEntry(5390, r2).Then(new(5386, 1)).Then(new(5387, 1)),
							new(4549, r2),
							new(4561, r2),
							new(4774, r2),
							new(5136, r2),
							new(5305, r2),
							new(4562, r2),
							new(4558, r2),
							new(4559, r2),
							new(4563, r2),
							new PoolEntry(4666, r2).Then(new(4664, 1)).Then(new(4665, 1)),
							new(4347, r2, onRoll: () => (!Main.hardMode && (NPC.downedBoss1 || NPC.downedBoss2 || NPC.downedBoss3 || NPC.downedQueenBee)) ? null : false),
							new(4348, r2, onRoll: () => (Main.hardMode && (NPC.downedBoss1 || NPC.downedBoss2 || NPC.downedBoss3 || NPC.downedQueenBee)) ? null : false),
							new(3262, r2, onRoll: () => NPC.downedBoss1 ? null : false),
							new(3284, r2, onRoll: () => NPC.downedMechBossAny ? null : false),

							new(2267, r1),
							new(2214, r1),
							new(2215, r1),
							new(2216, r1),
							new(2217, r1),
							new(3624, r1),
							new(671, r1, onRoll: () => /*Main.remixWorld*/ false ? null : false),
							new(2273, r1, onRoll: () => !/*Main.remixWorld*/false ? null : false),
							new(2274, r1),

							new(2266, r0),
							new(2281, r0),
							new(2282, r0),
							new(2283, r0),
							new(2258, r0),
							new(2242, r0),
							new PoolEntry(2260, r0).Then(new(2261, 1)).Then(new(2262, 1)),
							new(3637, r0),
							new(4420, r0),
							new(3119, r0),
							new(3118, r0),
							new(3099, r0),
						}
					},
					new() {
						Entries = new() {
							new(5121, r3, onRoll: () => !Main.dontStarveWorld ? null : false),
							new(5122, r3, onRoll: () => !Main.dontStarveWorld ? null : false),
							new(5124, r3, onRoll: () => !Main.dontStarveWorld ? null : false),
							new(5123, r3, onRoll: () => !Main.dontStarveWorld ? null : false),

							new(3596, r2, onRoll: () => (Main.hardMode && NPC.downedMoonlord) ? null : false),
							new(2865, r2, onRoll: () => (Main.hardMode && NPC.downedMartians) ? null : false),
							new(2866, r2, onRoll: () => (Main.hardMode && NPC.downedMartians) ? null : false),
							new(2867, r2, onRoll: () => (Main.hardMode && NPC.downedMartians) ? null : false),
							new(3055, r2, onRoll: () => NPC.downedFrost ? null : false),
							new(3056, r2, onRoll: () => NPC.downedFrost ? null : false),
							new(3057, r2, onRoll: () => NPC.downedFrost ? null : false),
							new(3058, r2, onRoll: () => NPC.downedFrost ? null : false),
							new(3059, r2, onRoll: () => NPC.downedFrost ? null : false),
							new(5243, r2, onRoll: () => (Main.hardMode && NPC.downedMoonlord) ? null : false),

							new(5121, r1, onRoll: () => Main.dontStarveWorld ? null : false),
							new(5122, r1, onRoll: () => Main.dontStarveWorld ? null : false),
							new(5124, r1, onRoll: () => Main.dontStarveWorld ? null : false),
							new(5123, r1, onRoll: () => Main.dontStarveWorld ? null : false),
							new(5225, r1),
							new(5229, r1),
							new(5232, r1),
							new(5389, r1),
							new(5233, r1),
							new(5241, r1),
							new(5244, r1),
							new(5242, r1),
						}
					}
				}
			};
		}
	}

	public sealed class LootTable {
		public List<LootTablePool> Pools { get; set; } = new();
		private UnifiedRandom BackendRand { get; }
		public UnifiedRandom GetRand() => BackendRand ?? Main.rand;

		public LootTable(UnifiedRandom rand = null) {
			BackendRand = rand;
		}

		public void CalculateWeight() {
			double sum = 0;
			foreach (var pool in Pools) {
				foreach (var entry in pool.Entries) {
					sum += entry.Weight;
				}
			}
			foreach (var pool in Pools) {
				foreach (var e in pool.Entries) {
					e.TotalWeight = sum;
				}
			}
		}

		public List<PoolItem> Roll(int? dropAtleastXItems = null, bool ignoreDuplicates = true, bool shouldCalculateWeight = true) {
			if (shouldCalculateWeight)
				CalculateWeight();

			int a = 0;
			List<PoolItem> winners = new();
			if (dropAtleastXItems is null)
				goto normal;

			while (true) {
				for (int i = 0; i < Pools.Count; i++) {
					List<PoolItem> e2 = Pools[i].Roll(GetRand, ref a);
					if (ignoreDuplicates) {
						IReadOnlyList<PoolItem> e = e2;
						foreach (var r in e.Where(r => winners.Any(y => y.Item.Equals(r.Item))).ToArray()) {
							e2.Remove(r);
						}
					}

					winners.AddRange(e2);
					if (winners.Count >= dropAtleastXItems.Value + a)
						return winners;
				}
			}

		normal:
			foreach (var pool in Pools) {
				winners.AddRange(pool.Roll(GetRand, ref a));
			}
			return winners;
		}
	}
	public sealed class LootTablePool {
		public List<PoolEntry> Entries = new();

		public int MinRolls { get; }
		public int MaxRolls { get; }
		public int BonusRolls { get; }

		public LootTablePool(int minRolls = 0, int maxRolls = 1, int bounsRolls = 0) {
			MinRolls = minRolls;
			MaxRolls = maxRolls;
			BonusRolls = bounsRolls;
		}

		public List<PoolItem> Roll(Func<UnifiedRandom> rand, ref int a) {
			Dictionary<int, List<PoolItem>> winners = new();
			int maxRolls = MaxRolls + rand().Next(BonusRolls + 1);
			for (int i = MinRolls; i < maxRolls; i++) {
				for (int l = 0; l < Entries.Count; l++) {
					List<PoolItem> s = Entries[l].Roll(rand, true, ref a);
					if (!s.Any()) {
						continue;
					}
					winners[l] = s;
					break;
				}
			}
			return new(winners.Values.SelectMany(x => x));
		}
	}
	public sealed class PoolEntry {
		public event Func<bool?> OnRoll;

		public List<PoolEntry> ChainedEntries { get; } = new();

		public int MinCount { get; }
		public int MaxCount { get; }

		public int Item { get; }
		public double Weight { get; }

		internal double TotalWeight { get; set; } = double.MaxValue;

		public PoolEntry(int item, double weight, int minCount = 0, int maxCount = 1, Func<bool?> onRoll = null) {
			Item = item;
			Weight = weight;
			MinCount = minCount;
			MaxCount = maxCount;
			OnRoll = onRoll;
		}

		public List<PoolItem> Roll(Func<UnifiedRandom> rand, bool first, ref int a) {
			List<PoolItem> entries = new();
			bool? flag = OnRoll?.Invoke();
			double percent = 10000.0 / (TotalWeight / Weight);

			if (flag == false || flag == null && rand().NextDouble() > percent)
				return entries;

			entries.Add(new(Item, rand().Next(MinCount, MaxCount) + 1));
			if (!first) {
				a++;
			}
			foreach (var entry in ChainedEntries) {
				var list = entry.Roll(rand, false, ref a);
				a += list.Count;
				entries.AddRange(list);
			}
			return entries;
		}

		public PoolEntry Then(PoolEntry entry) {
			ChainedEntries.Add(entry);
			return this;
		}
	}
	public readonly record struct PoolItem(int Item, int Count) {
	}
}
