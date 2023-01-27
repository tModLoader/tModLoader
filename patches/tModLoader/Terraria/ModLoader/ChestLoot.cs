using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Enums;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.Localization;

namespace Terraria.ModLoader;

// TODO: AddPylon method at some point?
public readonly struct ChestLoot {
	public interface ICondition {
		string Description { get; }

		bool IsAvailable();
	}

	public sealed class Condition : ICondition {
		#region Common conditions
		public static readonly Condition Christmas = new(NetworkText.Empty, () => Main.xMas);
		public static readonly Condition Halloween = new(NetworkText.Empty, () => Main.halloween);
		public static readonly Condition Hardmode = new(NetworkText.Empty, () => Main.hardMode);
		public static readonly Condition PreHardmode = new(NetworkText.Empty, () => !Main.hardMode);
		public static readonly Condition TimeDay = new(NetworkText.Empty, () => Main.dayTime);
		public static readonly Condition TimeNight = new(NetworkText.Empty, () => !Main.dayTime);
		public static readonly Condition NotBloodMoon = new(NetworkText.Empty, () => !Main.bloodMoon);
		public static readonly Condition BloodMoon = new(NetworkText.Empty, () => Main.bloodMoon);
		public static readonly Condition Eclipse = new(NetworkText.Empty, () => Main.eclipse);
		public static readonly Condition NotEclipse = new(NetworkText.Empty, () => !Main.eclipse);
		public static readonly Condition RemixWorld = new(NetworkText.Empty, () => Main.remixWorld);
		public static readonly Condition TenthAnniversary = new(NetworkText.Empty, () => Main.tenthAnniversaryWorld);
		public static readonly Condition BirthdayPartyIsUp = new(NetworkText.Empty, () => BirthdayParty.PartyIsUp);
		public static readonly Condition NightLanternsUp = new(NetworkText.Empty, () => LanternNight.LanternsUp);
		public static readonly Condition IsMoonFull = new(NetworkText.Empty, () => Main.GetMoonPhase() == MoonPhase.Full);
		public static readonly Condition IsMoonWaningGibbous = new(NetworkText.Empty, () => Main.GetMoonPhase() == MoonPhase.ThreeQuartersAtLeft);
		public static readonly Condition IsMoonThirdQuarter = new(NetworkText.Empty, () => Main.GetMoonPhase() == MoonPhase.HalfAtLeft);
		public static readonly Condition IsMoonWaningCrescent = new(NetworkText.Empty, () => Main.GetMoonPhase() == MoonPhase.QuarterAtLeft);
		public static readonly Condition IsMoonNew = new(NetworkText.Empty, () => Main.GetMoonPhase() == MoonPhase.Empty);
		public static readonly Condition IsMoonWaxingCrescent = new(NetworkText.Empty, () => Main.GetMoonPhase() == MoonPhase.QuarterAtRight);
		public static readonly Condition IsMoonFirstQuarter = new(NetworkText.Empty, () => Main.GetMoonPhase() == MoonPhase.HalfAtRight);
		public static readonly Condition IsMoonWaxingGibbous = new(NetworkText.Empty, () => Main.GetMoonPhase() == MoonPhase.ThreeQuartersAtRight);
		public static readonly Condition HappyWindyDay = new(NetworkText.Empty, () => Main.IsItAHappyWindyDay);
		public static readonly Condition InSnowBiome = new(NetworkText.Empty, () => Main.LocalPlayer.ZoneSnow);
		public static readonly Condition InJungleBiome = new(NetworkText.Empty, () => Main.LocalPlayer.ZoneJungle);
		public static readonly Condition InCorruptBiome = new(NetworkText.Empty, () => Main.LocalPlayer.ZoneCorrupt);
		public static readonly Condition InCrimsonBiome = new(NetworkText.Empty, () => Main.LocalPlayer.ZoneCrimson);
		public static readonly Condition InHallowBiome = new(NetworkText.Empty, () => Main.LocalPlayer.ZoneHallow);
		public static readonly Condition InDesertBiome = new(NetworkText.Empty, () => Main.LocalPlayer.ZoneDesert);
		public static readonly Condition InGraveyard = new(NetworkText.Empty, () => Main.LocalPlayer.ZoneGraveyard);
		public static readonly Condition InGlowshroomBiome = new(NetworkText.Empty, () => Main.LocalPlayer.ZoneGlowshroom);
		public static readonly Condition InBeachBiome = new(NetworkText.Empty, () => Main.LocalPlayer.ZoneBeach);
		public static readonly Condition InDungeonBiome = new(NetworkText.Empty, () => Main.LocalPlayer.ZoneDungeon);
		public static readonly Condition NotInSnowBiome = new(NetworkText.Empty, () => !Main.LocalPlayer.ZoneSnow);
		public static readonly Condition NotInJungleBiome = new(NetworkText.Empty, () => !Main.LocalPlayer.ZoneJungle);
		public static readonly Condition NotInCorruptBiome = new(NetworkText.Empty, () => !Main.LocalPlayer.ZoneCorrupt);
		public static readonly Condition NotInCrimsonBiome = new(NetworkText.Empty, () => !Main.LocalPlayer.ZoneCrimson);
		public static readonly Condition NotInHallowBiome = new(NetworkText.Empty, () => !Main.LocalPlayer.ZoneHallow);
		public static readonly Condition NotInDesertBiome = new(NetworkText.Empty, () => !Main.LocalPlayer.ZoneDesert);
		public static readonly Condition NotInGraveyard = new(NetworkText.Empty, () => !Main.LocalPlayer.ZoneGraveyard);
		public static readonly Condition NotInGlowshroomBiome = new(NetworkText.Empty, () => !Main.LocalPlayer.ZoneGlowshroom);
		public static readonly Condition NotInBeachBiome = new(NetworkText.Empty, () => !Main.LocalPlayer.ZoneBeach);
		public static readonly Condition NotInDungeonBiome = new(NetworkText.Empty, () => !Main.LocalPlayer.ZoneDungeon);
		public static readonly Condition CorruptionWorld = new(NetworkText.Empty, () => !WorldGen.crimson);
		public static readonly Condition CrimsonWorld = new(NetworkText.Empty, () => WorldGen.crimson);
		public static readonly Condition DownedKingSlime = new(NetworkText.Empty, () => NPC.downedSlimeKing);
		public static readonly Condition DownedEyeOfCthulhu = new(NetworkText.Empty, () => NPC.downedBoss1);
		public static readonly Condition DownedEowOrBoc = new(NetworkText.Empty, () => NPC.downedBoss2);
		public static readonly Condition DownedEaterOfWorlds = new(NetworkText.Empty, () => NPC.downedBoss2 && !WorldGen.crimson);
		public static readonly Condition DownedBrainOfCthulhu = new(NetworkText.Empty, () => NPC.downedBoss2 && WorldGen.crimson);
		public static readonly Condition DownedQueenBee = new(NetworkText.Empty, () => NPC.downedQueenBee);
		public static readonly Condition DownedSkeletron = new(NetworkText.Empty, () => NPC.downedBoss3);
		public static readonly Condition DownedMechBossAny = new(NetworkText.Empty, () => NPC.downedMechBossAny);
		public static readonly Condition DownedTwins = new(NetworkText.Empty, () => NPC.downedMechBoss2);
		public static readonly Condition DownedDestroyer = new(NetworkText.Empty, () => NPC.downedMechBoss1);
		public static readonly Condition DownedSkeletronPrime = new(NetworkText.Empty, () => NPC.downedMechBoss3);
		public static readonly Condition DownedPlantera = new(NetworkText.Empty, () => NPC.downedPlantBoss);
		public static readonly Condition DownedGolem = new(NetworkText.Empty, () => NPC.downedGolemBoss);
		public static readonly Condition DownedCultist = new(NetworkText.Empty, () => NPC.downedAncientCultist);
		public static readonly Condition DownedMoonLord = new(NetworkText.Empty, () => NPC.downedMoonlord);
		public static readonly Condition DownedClown = new(NetworkText.Empty, () => NPC.downedClown);
		public static readonly Condition DownedPirates = new(NetworkText.Empty, () => NPC.downedPirates);
		public static readonly Condition DownedMartians = new(NetworkText.Empty, () => NPC.downedMartians);
		public static readonly Condition DownedFrost = new(NetworkText.Empty, () => NPC.downedFrost);
		#endregion

		private readonly NetworkText DescriptionText;
		private readonly Func<bool> Predicate;

		public string Description => DescriptionText.ToString();

		public Condition(NetworkText description, Func<bool> predicate) {
			DescriptionText = description ?? throw new ArgumentNullException(nameof(description));
			Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
		}

		public bool IsAvailable() => Predicate();
	}

	public struct Entry {
		public readonly Item item;
		private readonly List<ICondition> conditions;
		private readonly bool askingNicelyToNotAdd = false;
		private bool hide;

		public Dictionary<bool, List<Entry>> ChainedEntries;

		public bool Hidden { private get => hide; set => hide = true; }

		public Entry(params ICondition[] condition) : this(emptyInstance, condition) {
			askingNicelyToNotAdd = true;
		}

		public Entry(int item, params ICondition[] condition) : this(ContentSamples.ItemsByType[item], condition) { }

		public Entry(Item item, params ICondition[] condition) {
			hide = false;
			this.item = item;
			conditions = condition.ToList();

			ChainedEntries = new()
			{
				{ false, new() },
				{ true, new() }
			};
		}

		public Entry OnSuccess(int itemId, params ICondition[] condition) {
			return OnSuccess(new Entry(ContentSamples.ItemsByType[itemId], condition));
		}

		public Entry OnSuccess(Entry entry) {
			ChainedEntries[true].Add(entry);
			return this;
		}

		public Entry OnFail(int itemId, params ICondition[] condition) {
			return OnFail(new Entry(ContentSamples.ItemsByType[itemId], condition));
		}

		public Entry OnFail(Entry entry) {
			ChainedEntries[false].Add(entry);
			return this;
		}

		public void AddCondition(ICondition condition) {
			ArgumentNullException.ThrowIfNull(condition, nameof(condition));
			conditions.Add(condition);
		}

		public bool IsAvailable() {
			foreach (ICondition condition in conditions) {
				if (!condition.IsAvailable()) {
					return false;
				}
			}
			return true;
		}

		public void TryAdd(List<Item> items) {
			if (hide)
				return;

			if (IsAvailable()) {
				if (askingNicelyToNotAdd) {
					items.Add(item);
				}
				foreach (var entry in ChainedEntries[true]) {
					entry.TryAdd(items);
				}
			}
			else {
				foreach (var entry in ChainedEntries[false]) {
					entry.TryAdd(items);
				}
			}
		}
	}

	private static readonly Item emptyInstance = new();
	private static readonly Item defaultInstance = default;

	private readonly List<Entry> items;

	private readonly List<(int nextTo, bool after)> putCandidates;
	private readonly List<Entry> putCandidates2; // list that contains all entries those going to get from putCandidates

	public IReadOnlyList<Entry> Items {
		get {
			List<Entry> entries = items;
			return entries;
		}
	}

	public Entry this[int item] {
		get {
			int index = items.FindIndex(x => x.item.type.Equals(item));
			bool hasInNormal = index != -1;
			if (hasInNormal)
				return items[index];

			index = putCandidates2.FindIndex(x => x.item.type.Equals(item));
			return putCandidates2[index];
		}
	}

	public Entry this[Index index] {
		get {
			var ind2 = items.ElementAtOrDefault(index);
			bool hasInNormal = ind2.item != defaultInstance;
			if (hasInNormal)
				return ind2;

			return putCandidates2.ElementAtOrDefault(index);
		}
	}

	public ChestLoot() {
		items = new();
		putCandidates = new();
		putCandidates2 = new();
	}

	private void AddCandidates(IList<Entry> entries) {
		IReadOnlyList<(int nextTo, bool after)> candidates = putCandidates;
		candidates = candidates.Reverse().ToList();

		List<(int nextTo, bool after)> unsuccessedCandidates = new();

		for (int i = 0; i < 2; i++) { // run twice, cause mods might want to add to chained items
			var a = candidates;
			if (i == 1)
				a = unsuccessedCandidates;

			foreach (var (nextTo, after) in a) {
				int index = items.FindIndex(x => x.item.type.Equals(nextTo));
				if (index != -1) {
					entries.Insert(index + after.ToInt(), putCandidates2[index]);
				}
				else {
					unsuccessedCandidates.Add((nextTo, after));
				}
			}
		}
	}

	public void AddRange(params Entry[] entries) {
		foreach (var e in entries) {
			Add(e);
		}
	}

	public void Add(Entry entry) {
		items.Add(entry);
	}

	public bool Add(int item, params ICondition[] condition) {
		return Add(ContentSamples.ItemsByType[item], condition);
	}

	public bool Add(Item item, params ICondition[] condition) {
		Add(new Entry(item, condition));
		return true;
	}

	private bool PutAt(int destination, Item item, bool after, params ICondition[] condition) {
		putCandidates.Add(new(destination, after));
		putCandidates2.Add(new(item, condition));
		return true;
	}

	private bool PutAt(int destination, int item, bool after, params ICondition[] condition) {
		return PutAt(destination, ContentSamples.ItemsByType[item], after, condition);
	}

	public bool InsertBefore(int destination, int item, params ICondition[] condition) {
		return PutAt(destination, item, false, condition);
	}

	public bool InsertBefore(int destination, Item item, params ICondition[] condition) {
		return PutAt(destination, item, false, condition);
	}

	public bool InsertAfter(int destination, int item, params ICondition[] condition) {
		return PutAt(destination + 1, item, true, condition);
	}

	public bool InsertAfter(int destination, Item item, params ICondition[] condition) {
		return PutAt(destination + 1, item, true, condition);
	}

	public bool Hide(int item) {
		Entry entry = this[item];
		entry.Hidden = true;
		return true;
	}

	public Item[] Build(bool lastSlotEmpty = true) {
		return Build(out _, lastSlotEmpty);
	}

	public Item[] Build(out int slots, bool lastSlotEmpty = true) {
		List<Item> array = new();
		List<Entry> oldEntries = items; // incase current instance still gets used after building for some reason.

		AddCandidates(oldEntries);
		foreach (Entry group in oldEntries) {
			group.TryAdd(array);
		}

		slots = array.Count;
		if (array.Count < 40) {
			array.AddRange(Enumerable.Repeat(emptyInstance, 40 - array.Count));
		}
		array = array.Take(40).ToList();
		if (lastSlotEmpty)
			array[^1] = emptyInstance;

		return array.ToArray();
	}
}