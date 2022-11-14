using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Terraria.Utilities;

namespace Terraria.ModLoader
{
	public sealed class LuckRandom : UnifiedRandom {
		public float Luck { get; }

		public LuckRandom() : this(0f) {
		}

		public LuckRandom(float luck) {
			Luck = luck;
		}

		public override int Next(int maxValue) {
			if (Luck > 0f && Main.rand.NextFloat() < Luck)
				return Main.rand.Next(Main.rand.Next(maxValue / 2, maxValue));

			if (Luck < 0f && Main.rand.NextFloat() < 0f - Luck)
				return Main.rand.Next(Main.rand.Next(maxValue, maxValue * 2));

			return Main.rand.Next(maxValue);
		}

		public override int Next(int minValue, int maxValue) {
			if (Luck > 0f && Main.rand.NextFloat() < Luck)
				return Main.rand.Next(Main.rand.Next(minValue / 2, maxValue));

			if (Luck < 0f && Main.rand.NextFloat() < 0f - Luck)
				return Main.rand.Next(Main.rand.Next(minValue, maxValue * 2));

			return Main.rand.Next(minValue, maxValue);
		}

		public override double NextDouble() {
			if (Luck > 0f && Main.rand.NextDouble() < Luck)
				return Main.rand.NextFloat(Main.rand.NextFloat(0.5f));

			if (Luck < 0f && Main.rand.NextDouble() < 0 - Luck)
				return Main.rand.NextFloat(Main.rand.NextFloat(2f));

			return Main.rand.NextDouble();
		}
	}

	[DebuggerDisplay("Count = {Pools.Count}")]
	public sealed class LootTable : IEnumerable<LootTablePool> {
		public List<LootTablePool> Pools { get; private set; } = new List<LootTablePool>();
		private Func<UnifiedRandom> _rand;
		public UnifiedRandom Rand => GetRand();

		public LootTable(Func<UnifiedRandom> rand = null) {
			_rand = rand;
		}

		public void Add(LootTablePool pool) {
			Pools.Add(pool);
		}

		public void CalculateWeight() {
			double sum = 0;
			foreach (LootTablePool pool in Pools) {
				foreach (PoolEntry entry in pool.Entries) {
					sum += entry.Weight;
				}

				foreach (PoolEntry e in pool.Entries) {
					e.TotalWeight = sum;
				}
				sum = 0.0;
			}
		}

		public UnifiedRandom GetRand() {
			return _rand?.Invoke() ?? Main.rand;
		}

		public void SetRand(Func<UnifiedRandom> rand) {
			_rand = rand;
		}

		public List<PoolItem> Roll(int? dropAtleastXItems = null, bool ignoreDuplicates = true, bool shouldCalculateWeight = true) {
			if (shouldCalculateWeight)
				CalculateWeight();

			int a = 0;
			List<PoolItem> winners = new List<PoolItem>();
			if (dropAtleastXItems is null) {
				foreach (LootTablePool pool in Pools) {
					winners.AddRange(pool.Roll(GetRand, ref a));
				}
				return winners;
			}

			while (true) {
				for (int i = 0; i < Pools.Count; i++) {
					List<PoolItem> e2 = Pools[i].Roll(GetRand, ref a);
					if (ignoreDuplicates) {
						IReadOnlyList<PoolItem> e = e2;
						foreach (PoolItem r in e.Where(r => winners.Any(y => y.Item.Equals(r.Item))).ToArray()) {
							e2.Remove(r);
						}
					}

					winners.AddRange(e2);
					if (winners.Count >= dropAtleastXItems.Value + a)
						return winners;
				}
			}
		}

		public Enumerator GetEnumerator()
			=> new Enumerator(this);

		IEnumerator<LootTablePool> IEnumerable<LootTablePool>.GetEnumerator()
			=> new Enumerator(this);

		IEnumerator IEnumerable.GetEnumerator()
			=> new Enumerator(this);

		public struct Enumerator : IEnumerator<LootTablePool>, IEnumerator
		{
			private readonly LootTable _table;
			private int _index;
			private LootTablePool _current;

			internal Enumerator(LootTable table) {
				_table = table;
				_index = 0;
				_current = default;
			}

			public void Dispose() {
			}

			public bool MoveNext() {
				LootTable localList = _table;

				if ((uint)_index < (uint)localList.Pools.Count) {
					_current = localList.Pools[_index];
					_index++;
					return true;
				}
				return MoveNextRare();
			}

			private bool MoveNextRare() {
				_index = _table.Pools.Count + 1;
				_current = default;
				return false;
			}

			public LootTablePool Current => _current!;

			object IEnumerator.Current {
				get {
					if (_index == 0 || _index == _table.Pools.Count + 1) {
						throw new InvalidOperationException();
					}
					return Current;
				}
			}

			void IEnumerator.Reset() {
				_index = 0;
				_current = default;
			}
		}
	}
	[DebuggerDisplay("Rolls = {MinRolls + 1}-{MaxRolls}, Count = {Entries.Count}")]
	public sealed class LootTablePool : IEnumerable<PoolEntry> {
		public List<PoolEntry> Entries { get; private set; } = new List<PoolEntry>();

		public int MinRolls { get; }
		public int MaxRolls { get; }
		public int BonusRolls { get; }

		public LootTablePool(int minRolls = 0, int maxRolls = 1, int bounsRolls = 0) {
			MinRolls = minRolls;
			MaxRolls = maxRolls;
			BonusRolls = bounsRolls;
		}

		public List<PoolItem> Roll(Func<UnifiedRandom> rand, ref int a) {
			Dictionary<int, List<PoolItem>> winners = new Dictionary<int, List<PoolItem>>();
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
			return new List<PoolItem>(winners.Values.SelectMany(x => x));
		}

		public void Add(PoolEntry entry) {
			Entries.Add(entry);
		}

		public Enumerator GetEnumerator()
			=> new Enumerator(this);

		IEnumerator<PoolEntry> IEnumerable<PoolEntry>.GetEnumerator()
			=> new Enumerator(this);

		IEnumerator IEnumerable.GetEnumerator()
			=> new Enumerator(this);

		public struct Enumerator : IEnumerator<PoolEntry>, IEnumerator
		{
			private readonly LootTablePool _table;
			private int _index;
			private PoolEntry _current;

			internal Enumerator(LootTablePool table) {
				_table = table;
				_index = 0;
				_current = default;
			}

			public void Dispose() {
			}

			public bool MoveNext() {
				LootTablePool localList = _table;

				if ((uint)_index < (uint)localList.Entries.Count) {
					_current = localList.Entries[_index];
					_index++;
					return true;
				}
				return MoveNextRare();
			}

			private bool MoveNextRare() {
				_index = _table.Entries.Count + 1;
				_current = default;
				return false;
			}

			public PoolEntry Current => _current!;

			object IEnumerator.Current {
				get {
					if (_index == 0 || _index == _table.Entries.Count + 1) {
						throw new InvalidOperationException();
					}
					return Current;
				}
			}

			void IEnumerator.Reset() {
				_index = 0;
				_current = default;
			}
		}
	}
	[DebuggerDisplay("Item = {Item}, Count = {MinCount + 1}-{MaxCount}, Weight = {Weight}")]
	public sealed class PoolEntry {
		public event Func<bool?> OnRoll;

		public List<PoolEntry> ChainedEntries { get; } = new List<PoolEntry>();

		public int MinCount { get; }
		public int MaxCount { get; }

		public int Item { get; }
		public double Weight { get; }

		internal double TotalWeight { get; set; }

		public PoolEntry(int item, double weight = 1.0, int minCount = 0, int maxCount = 1, Func<bool?> onRoll = null) {
			Item = item;
			Weight = weight;
			MinCount = minCount;
			MaxCount = maxCount;
			OnRoll = onRoll;
			TotalWeight = weight;
		}

		public List<PoolItem> Roll(Func<UnifiedRandom> rand, bool first, ref int a) {
			List<PoolItem> entries = new List<PoolItem>();
			bool? flag = OnRoll?.Invoke();
			double percent = 1.0 / (TotalWeight / Weight);

			if (flag == false || flag == null && rand().NextDouble() > percent)
				return entries;

			entries.Add(new PoolItem(Item, rand().Next(MinCount, MaxCount) + 1));
			if (!first) {
				a++;
			}
			foreach (PoolEntry entry in ChainedEntries) {
				List<PoolItem> list = entry.Roll(rand, false, ref a);
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
	[DebuggerDisplay("Item = {Item}, Count = {Count}")]
	public readonly record struct PoolItem(int Item, int Count) {
	}
}
