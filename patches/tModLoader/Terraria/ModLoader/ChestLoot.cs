using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Enums;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.Localization;

namespace Terraria.ModLoader
{
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
			public static readonly Condition DownedPlantera = new(NetworkText.Empty, () => NPC.downedPlantBoss);
			public static readonly Condition DownedGolem = new(NetworkText.Empty, () => NPC.downedGolemBoss);
			public static readonly Condition DownedCultist = new(NetworkText.Empty, () => NPC.downedAncientCultist);
			public static readonly Condition DownedClown = new(NetworkText.Empty, () => NPC.downedClown);
			public static readonly Condition DownedPirates = new(NetworkText.Empty, () => NPC.downedPirates);
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
			private bool hide;

			public bool Hidden { private get => hide; set {
					if (value)
						hide = true;
				}
			}

			public Entry(Item item, params ICondition[] condition) {
				hide = false;
				this.item = item;
				conditions = condition.ToList();
			}

			public void AddCondition(ICondition condition) {
				ArgumentNullException.ThrowIfNull(condition, nameof(condition));
				conditions.Add(condition);
			}

			public bool IsAvailable() {
				if (hide)
					return false;

				foreach (ICondition condition in conditions) {
					if (!condition.IsAvailable()) {
						return false;
					}
				}
				return true;
			}
		}

		private static readonly Item emptyInstance = new();

		private readonly List<Entry> items;

		public IReadOnlyList<Entry> Items => items;

		public Entry this[int item] => items.Find(x => x.item.type.Equals(item));

		public Entry this[Index index] => items[index];

		public ChestLoot() {
			items = new();
		}

		public bool Add(int item, params ICondition[] condition) {
			return Add(ContentSamples.ItemsByType[item], condition);
		}

		public bool Add(Item item, params ICondition[] condition) {
			items.Add(new(item, condition));
			return true;
		}

		private bool PutAt(int index, int item, params ICondition[] condition) {
			return PutAt(index, ContentSamples.ItemsByType[item], condition);
		}

		private bool PutAt(int index, Item item, params ICondition[] condition) {
			items.Insert(index, new(item, condition));
			return true;
		}

		public bool InsertBefore(int destination, int item, params ICondition[] condition) {
			return PutAt(destination, item, condition);
		}

		public bool InsertBefore(int destination, Item item, params ICondition[] condition) {
			return PutAt(destination, item, condition);
		}

		public bool InsertAfter(int destination, int item, params ICondition[] condition) {
			return PutAt(destination + 1, item, condition);
		}

		public bool InsertAfter(int destination, Item item, params ICondition[] condition) {
			return PutAt(destination + 1, item, condition);
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

			foreach (Entry group in items) {
				if (group.IsAvailable()) {
					array.Add(group.item);
				}
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

		public void Register(ModNPC npc) {
		}
	}
}
