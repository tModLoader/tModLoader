using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Enums;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.Localization;

namespace Terraria.ModLoader;

// TODO: AddPylon method at some point?
public class ChestLoot {
	public interface ICondition {
		string Description { get; }

		bool IsAvailable();
	}

	public sealed class Condition : ICondition {
		#region Common conditions
		public static readonly Condition Christmas = new(NetworkText.FromLiteral("During Christmas"), () => Main.xMas);
		public static readonly Condition Halloween = new(NetworkText.FromLiteral("During Halloween"), () => Main.halloween);
		public static readonly Condition Hardmode = new(NetworkText.FromLiteral("In Hardmode"), () => Main.hardMode);
		public static readonly Condition PreHardmode = new(NetworkText.FromLiteral("Before Hardmode"), () => !Main.hardMode);
		public static readonly Condition TimeDay = new(NetworkText.FromLiteral("During day"), () => Main.dayTime);
		public static readonly Condition TimeNight = new(NetworkText.FromLiteral("During night"), () => !Main.dayTime);
		public static readonly Condition NotBloodMoon = new(NetworkText.FromLiteral("During normal night"), () => !Main.bloodMoon);
		public static readonly Condition BloodMoon = new(NetworkText.FromLiteral("During blood moon"), () => Main.bloodMoon);
		public static readonly Condition Eclipse = new(NetworkText.FromLiteral("During solar eclipse"), () => Main.eclipse);
		public static readonly Condition NotEclipse = new(NetworkText.FromLiteral("During normal day"), () => !Main.eclipse);
		public static readonly Condition RemixWorld = new(NetworkText.FromLiteral("In Remix world"), () => Main.remixWorld);
		public static readonly Condition TenthAnniversary = new(NetworkText.FromLiteral("In Tenth Anniversary world"), () => Main.tenthAnniversaryWorld);
		public static readonly Condition BirthdayPartyIsUp = new(NetworkText.FromLiteral("During birthday party"), () => BirthdayParty.PartyIsUp);
		public static readonly Condition NightLanternsUp = new(NetworkText.FromLiteral("While night lanterns are up"), () => LanternNight.LanternsUp);
		public static readonly Condition IsMoonFull = new(NetworkText.FromLiteral("During full moon"), () => Main.GetMoonPhase() == MoonPhase.Full);
		public static readonly Condition IsMoonWaningGibbous = new(NetworkText.FromLiteral("During waning gibbous moon"), () => Main.GetMoonPhase() == MoonPhase.ThreeQuartersAtLeft);
		public static readonly Condition IsMoonThirdQuarter = new(NetworkText.FromLiteral("During third quarter moon"), () => Main.GetMoonPhase() == MoonPhase.HalfAtLeft);
		public static readonly Condition IsMoonWaningCrescent = new(NetworkText.FromLiteral("During waning crescent moon"), () => Main.GetMoonPhase() == MoonPhase.QuarterAtLeft);
		public static readonly Condition IsMoonNew = new(NetworkText.FromLiteral("During new moon"), () => Main.GetMoonPhase() == MoonPhase.Empty);
		public static readonly Condition IsMoonWaxingCrescent = new(NetworkText.FromLiteral("During waxing crescent moon"), () => Main.GetMoonPhase() == MoonPhase.QuarterAtRight);
		public static readonly Condition IsMoonFirstQuarter = new(NetworkText.FromLiteral("During first quarter moon"), () => Main.GetMoonPhase() == MoonPhase.HalfAtRight);
		public static readonly Condition IsMoonWaxingGibbous = new(NetworkText.FromLiteral("During waxing gibbous moon"), () => Main.GetMoonPhase() == MoonPhase.ThreeQuartersAtRight);
		public static readonly Condition HappyWindyDay = new(NetworkText.FromLiteral("During windy day"), () => Main.IsItAHappyWindyDay);
		public static readonly Condition InSnowBiome = new(NetworkText.FromLiteral("In Snow"), () => Main.LocalPlayer.ZoneSnow);
		public static readonly Condition InJungleBiome = new(NetworkText.FromLiteral("In Jungle"), () => Main.LocalPlayer.ZoneJungle);
		public static readonly Condition InCorruptBiome = new(NetworkText.FromLiteral("In Corruption"), () => Main.LocalPlayer.ZoneCorrupt);
		public static readonly Condition InCrimsonBiome = new(NetworkText.FromLiteral("In Crimson"), () => Main.LocalPlayer.ZoneCrimson);
		public static readonly Condition InHallowBiome = new(NetworkText.FromLiteral("In Hallow"), () => Main.LocalPlayer.ZoneHallow);
		public static readonly Condition InDesertBiome = new(NetworkText.FromLiteral("In Desert"), () => Main.LocalPlayer.ZoneDesert);
		public static readonly Condition InGraveyard = new(NetworkText.FromLiteral("In Graveyard"), () => Main.LocalPlayer.ZoneGraveyard);
		public static readonly Condition InGlowshroomBiome = new(NetworkText.FromLiteral("In Glowing Mushroom"), () => Main.LocalPlayer.ZoneGlowshroom);
		public static readonly Condition InBeachBiome = new(NetworkText.FromLiteral("In Ocean"), () => Main.LocalPlayer.ZoneBeach);
		public static readonly Condition InDungeonBiome = new(NetworkText.FromLiteral("In Dungeon"), () => Main.LocalPlayer.ZoneDungeon);
		public static readonly Condition NotInSnowBiome = new(NetworkText.FromLiteral("Not in Snow"), () => !Main.LocalPlayer.ZoneSnow);
		public static readonly Condition NotInJungleBiome = new(NetworkText.FromLiteral("Not in Jungle"), () => !Main.LocalPlayer.ZoneJungle);
		public static readonly Condition NotInCorruptBiome = new(NetworkText.FromLiteral("Not in Corruption"), () => !Main.LocalPlayer.ZoneCorrupt);
		public static readonly Condition NotInCrimsonBiome = new(NetworkText.FromLiteral("Not in Crimson"), () => !Main.LocalPlayer.ZoneCrimson);
		public static readonly Condition NotInHallowBiome = new(NetworkText.FromLiteral("Not in Hallow"), () => !Main.LocalPlayer.ZoneHallow);
		public static readonly Condition NotInDesertBiome = new(NetworkText.FromLiteral("Not in Desert"), () => !Main.LocalPlayer.ZoneDesert);
		public static readonly Condition NotInGraveyard = new(NetworkText.FromLiteral("Not in Graveyard"), () => !Main.LocalPlayer.ZoneGraveyard);
		public static readonly Condition NotInGlowshroomBiome = new(NetworkText.FromLiteral("Not in Glowing Mushroom"), () => !Main.LocalPlayer.ZoneGlowshroom);
		public static readonly Condition NotInBeachBiome = new(NetworkText.FromLiteral("Not in Ocean"), () => !Main.LocalPlayer.ZoneBeach);
		public static readonly Condition NotInDungeonBiome = new(NetworkText.FromLiteral("Not in Dungeon"), () => !Main.LocalPlayer.ZoneDungeon);
		public static readonly Condition CorruptionWorld = new(NetworkText.FromLiteral("In Corruption world"), () => !WorldGen.crimson);
		public static readonly Condition CrimsonWorld = new(NetworkText.FromLiteral("In Crimson world"), () => WorldGen.crimson);
		public static readonly Condition DownedKingSlime = new(NetworkText.FromLiteral("King Slime is slain"), () => NPC.downedSlimeKing);
		public static readonly Condition DownedEyeOfCthulhu = new(NetworkText.FromLiteral("Eye of Cthulhu is slain"), () => NPC.downedBoss1);
		public static readonly Condition DownedEowOrBoc = new(NetworkText.FromLiteral("Boss of Evil is slain"), () => NPC.downedBoss2);
		public static readonly Condition DownedEaterOfWorlds = new(NetworkText.FromLiteral("Eater of Worlds is slain"), () => NPC.downedBoss2 && !WorldGen.crimson);
		public static readonly Condition DownedBrainOfCthulhu = new(NetworkText.FromLiteral("Brain of Cthulhu is slain"), () => NPC.downedBoss2 && WorldGen.crimson);
		public static readonly Condition DownedQueenBee = new(NetworkText.FromLiteral("Queen Bee is slain"), () => NPC.downedQueenBee);
		public static readonly Condition DownedSkeletron = new(NetworkText.FromLiteral("Skeletron is slain"), () => NPC.downedBoss3);
		public static readonly Condition DownedMechBossAny = new(NetworkText.FromLiteral("Any Mechanical Boss is slain"), () => NPC.downedMechBossAny);
		public static readonly Condition DownedTwins = new(NetworkText.FromLiteral("The Twins are slain"), () => NPC.downedMechBoss2);
		public static readonly Condition DownedDestroyer = new(NetworkText.FromLiteral("The Destroyer is slain"), () => NPC.downedMechBoss1);
		public static readonly Condition DownedSkeletronPrime = new(NetworkText.FromLiteral("Skeletron Prime is slain"), () => NPC.downedMechBoss3);
		public static readonly Condition DownedPlantera = new(NetworkText.FromLiteral("Plantera is slain"), () => NPC.downedPlantBoss);
		public static readonly Condition DownedGolem = new(NetworkText.FromLiteral("Golem is slain"), () => NPC.downedGolemBoss);
		public static readonly Condition DownedCultist = new(NetworkText.FromLiteral("Lunatic Cultist is slain"), () => NPC.downedAncientCultist);
		public static readonly Condition DownedMoonLord = new(NetworkText.FromLiteral("Moon Lord is slain"), () => NPC.downedMoonlord);
		public static readonly Condition DownedClown = new(NetworkText.FromLiteral("Clown is slain"), () => NPC.downedClown);
		public static readonly Condition DownedPirates = new(NetworkText.FromLiteral("Pirates are defeated"), () => NPC.downedPirates);
		public static readonly Condition DownedMartians = new(NetworkText.FromLiteral("Martians are defeated"), () => NPC.downedMartians);
		public static readonly Condition DownedFrost = new(NetworkText.FromLiteral("Frost Legion is defeated"), () => NPC.downedFrost);
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
	private string name;

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

	public void RegisterShop(int npcId, string name = "Shop") {
		this.name = name;
		Main.TMLLootDB.RegisterNpcShop(npcId, (ChestLoot)MemberwiseClone(), name);
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

	public ChestLoot AddRange(params Entry[] entries) {
		items.AddRange(entries);
		return this;
	}

	public ChestLoot Add(Entry entry) {
		items.Add(entry);
		return this;
	}

	public ChestLoot Add(int item, params ICondition[] condition) {
		return Add(ContentSamples.ItemsByType[item], condition);
	}

	public ChestLoot Add(Item item, params ICondition[] condition) {
		return Add(new Entry(item, condition));
	}

	private ChestLoot PutAt(int destination, Item item, bool after, params ICondition[] condition) {
		putCandidates.Add(new(destination, after));
		putCandidates2.Add(new(item, condition));
		return this;
	}

	private ChestLoot PutAt(int targetItem, int item, bool after, params ICondition[] condition) {
		return PutAt(targetItem, ContentSamples.ItemsByType[item], after, condition);
	}

	public ChestLoot InsertBefore(int targetItem, int item, params ICondition[] condition) {
		return PutAt(targetItem, item, false, condition);
	}

	public ChestLoot InsertBefore(int targetItem, Item item, params ICondition[] condition) {
		return PutAt(targetItem, item, false, condition);
	}

	public ChestLoot InsertAfter(int targetItem, int item, params ICondition[] condition) {
		return PutAt(targetItem + 1, item, true, condition);
	}

	public ChestLoot InsertAfter(int targetItem, Item item, params ICondition[] condition) {
		return PutAt(targetItem + 1, item, true, condition);
	}

	public ChestLoot Hide(int item) {
		Entry entry = this[item];
		entry.Hidden = true;
		return this;
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
		if (array.Count > 40) {
			Main.NewText("Not all items could fit in the shop!", 255, 0, 0);
			Logging.tML.Warn($"Unable to fit all item in the shop {name}");
			slots = 40;
		}
		if (array.Count < 40) {
			array.AddRange(Enumerable.Repeat(emptyInstance, 40 - array.Count));
		}
		array = array.Take(40).ToList();
		if (lastSlotEmpty)
			array[^1] = emptyInstance;

		return array.ToArray();
	}
}