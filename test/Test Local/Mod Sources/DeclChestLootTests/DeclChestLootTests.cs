using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.WorldBuilding;
using static Terraria.WorldGen;
using ItemSourceHelper.Default;
using ItemSourceHelper.Core;
using ItemSourceHelper;
using Microsoft.Xna.Framework;
using System.Collections;
using static DeclChestLootTests.DeclChestLootTests;

namespace DeclChestLootTests; 
// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
public class DeclChestLootTests : Mod {
	public override void Load() {
		MonoModHooks.Add(
			typeof(WorldGen).GetMethod(nameof(WorldGen.AddBuriedChest), typeof(orig_AddBuriedChest).GetMethod(nameof(orig_AddBuriedChest.Invoke)).GetParameters().Select(p => p.ParameterType).ToArray()),
			_AddBuriedChest
		);
		On_WorldGen.PlaceChest += On_WorldGen_PlaceChest;
	}

	private int On_WorldGen_PlaceChest(On_WorldGen.orig_PlaceChest orig, int x, int y, ushort type, bool notNearOtherChests, int style) {
		return lastChest = orig(x, y, type, notNearOtherChests, style);
	}

	public int lastChest = -1;
	public List<(string name, (Dictionary<int, DropCountAccuracyData> inaccuracies, Point pos) data)> dropCountInaccuracies = [];
	public delegate bool orig_AddBuriedChest(int i, int j, string pool, bool notNearOtherChests = false, int Style = -1, bool trySlope = false, ushort chestTileType = 0, int forceContain = ItemID.None);
	public delegate bool hook_AddBuriedChest(orig_AddBuriedChest orig, int i, int j, string pool, bool notNearOtherChests = false, int Style = -1, bool trySlope = false, ushort chestTileType = 0, int forceContain = ItemID.None);
	static int _WaterItemCount = 0;
	public bool _AddBuriedChest(orig_AddBuriedChest orig, int i, int j, string pool, bool notNearOtherChests = false, int Style = -1, bool trySlope = false, ushort chestTileType = 0, int forceContain = ItemID.None) {
		_WaterItemCount = GenVars.WaterItemCount;
		bool result = orig(i, j, pool, notNearOtherChests, Style, trySlope, chestTileType, forceContain);
		if (!result) return false;
		int x = 0;
		int y = 0;
		if (Main.chest.IndexInRange(lastChest)) {
			x = Main.chest[lastChest].x;
			y = Main.chest[lastChest].y;
			Chest.DestroyChestDirect(x, y, lastChest);
			KillTile(x, y);
		}
		Dictionary<int, DropCountAccuracyData> dropCountsVanilla = [];
		Dictionary<int, DropCountAccuracyData> dropCountsNew = [];
		Dictionary<int, DropCountAccuracyData> dropCountInaccuracy = [];
		static void Combine(ref Dictionary<int, DropCountAccuracyData> container, Dictionary<int, DropCountAccuracyData> addition) {
			foreach (KeyValuePair<int, DropCountAccuracyData> entry in addition) {
				if (container.TryGetValue(entry.Key, out DropCountAccuracyData existing)) {
					container[entry.Key] = existing.Add(entry.Value);
				} else {
					container.Add(entry.Key, entry.Value);
				}
			}
		}
		int tries = 10;
		do {
			Combine(ref dropCountsVanilla, CalculateDropCounts(null, i, j, pool, notNearOtherChests, Style, trySlope, chestTileType, forceContain));
			Combine(ref dropCountsNew, CalculateDropCounts(orig, i, j, pool, notNearOtherChests, Style, trySlope, chestTileType, forceContain));
			dropCountInaccuracy.Clear();
			foreach (int item in dropCountsVanilla.Keys.Union(dropCountsNew.Keys)) {
				dropCountsVanilla.TryGetValue(item, out DropCountAccuracyData dropCountVanilla);
				dropCountsNew.TryGetValue(item, out DropCountAccuracyData dropCountNew);
				float inaccuracy = dropCountVanilla.Count == 0 ? float.PositiveInfinity : (dropCountNew.Count / (float)dropCountVanilla.Count);
				float unsignedInaccuracy = inaccuracy;
				if (unsignedInaccuracy < 1) {
					if (dropCountNew.Count == 0) {
						inaccuracy = float.NegativeInfinity;
						unsignedInaccuracy = float.PositiveInfinity;
					} else {
						unsignedInaccuracy = 1f / unsignedInaccuracy;
					}

				}
				inaccuracy -= 1;
				if (unsignedInaccuracy - 1 > 0.20f || Math.Abs(dropCountNew.Min - dropCountVanilla.Min) > 2 || Math.Abs(dropCountNew.Max - dropCountVanilla.Max) > 2) {
					int _inaccuracy = (int)(inaccuracy * 1000);
					switch (inaccuracy) {
						case float.NegativeInfinity:
						_inaccuracy = int.MinValue;
						break;
						case float.PositiveInfinity:
						_inaccuracy = int.MaxValue;
						break;
					}
					DropCountAccuracyData inaccuractyData = new(1000, _inaccuracy, dropCountNew.Min - dropCountVanilla.Min, dropCountNew.Max - dropCountVanilla.Max);
					dropCountInaccuracy.Add(item, inaccuractyData);
				}
			}
		} while (dropCountInaccuracy.Count > 0 && --tries > 0);
		if (dropCountInaccuracy.Count > 0) {
			string name = pool ?? "Unspecified";
			dropCountInaccuracies.Add((name, (dropCountInaccuracy, new(x, y))));
			dropCountInaccuracies.Add((name + " (Original)", (dropCountsVanilla, new(x, y))));
			dropCountInaccuracies.Add((name + " (New)", (dropCountsNew, new(x, y))));
		}
		orig(i, j, pool, notNearOtherChests, Style, trySlope, chestTileType, forceContain);
		return dropCountsVanilla.Count + dropCountsNew.Count > 0;
	}
	Dictionary<int, DropCountAccuracyData> CalculateDropCounts(orig_AddBuriedChest orig, int i, int j, string pool, bool notNearOtherChests = false, int Style = -1, bool trySlope = false, ushort chestTileType = 0, int forceContain = ItemID.None) {
		const float try_count = 1000;
		Dictionary<int, DropCountAccuracyData> dropCounts = [];
		if (orig is null) {
			for (int index = 0; index < try_count; index++) {
				switch (pool) {
					case ChestLootLoader.LootPoolNames.Jungle:
					forceContain = GetNextJungleChestItem();
					GenVars.JungleItemCount--;
					break;
					case ChestLootLoader.LootPoolNames.JungleTree:
					if (genRand.NextBool(4)) {
						forceContain = 0;
					} else {
						forceContain = GetNextJungleChestItem();
						GenVars.JungleItemCount--;
					}
					break;
					case ChestLootLoader.LootPoolNames.Web:
					forceContain = ItemID.WebSlinger;
					break;
					case ChestLootLoader.LootPoolNames.WaterOceanCave:
					forceContain = genRand.NextFromList(new short[5] {
						863,
						186,
						277,
						187,
						4404
					});
					break;
					case ChestLootLoader.LootPoolNames.WaterAnywhere:
					GenVars.WaterItemCount = _WaterItemCount;
					if (genRand.NextBool(tenthAnniversaryWorldGen ? 7 : 10)) {
						forceContain = 863;
					} else {
						switch ((GenVars.WaterItemCount + 1) % 4) {
							case 0:
							forceContain = 186;
							break;
							case 1:
							forceContain = 4404;
							break;
							case 2:
							forceContain = 277;
							break;
							default:
							forceContain = 187;
							break;
						}
					}
					break;
					case ChestLootLoader.LootPoolNames.PyramidGold:
					forceContain = genRand.Next(3);
					if (forceContain == 0)
						forceContain = genRand.Next(3);

					if (Main.tenthAnniversaryWorld && forceContain == 0)
						forceContain = 1;

					switch (forceContain) {
						case 0:
						forceContain = 848;
						break;
						case 1:
						forceContain = 857;
						break;
						case 2:
						forceContain = 934;
						break;
					}
					break;
					case ChestLootLoader.LootPoolNames.LivingWood:
					forceContain = genRand.NextBool(3) ? 4281 : 832;
					break;
					case ChestLootLoader.LootPoolNames.Lihzahrd:
					forceContain = ItemID.LihzahrdPowerCell;
					break;
					case ChestLootLoader.LootPoolNames.FloatingIsland:
					switch (GenVars.skyIslandHouseCount < 4 ? GenVars.skyIslandHouseCount : genRand.Next(4)) {
						case 0:
						forceContain = 159;
						break;
						case 1:
						forceContain = 65;
						break;
						case 2:
						forceContain = 158;
						break;
						case 3:
						forceContain = 2219;
						break;
					}
					Style = getGoodWorldGen ? 2 : 13;
					break;
				}
				if (AddBuriedChest(i, j, forceContain, notNearOtherChests, Style, trySlope, chestTileType)) {
					if (!Main.chest.IndexInRange(lastChest))
						continue;

					int x = Main.chest[lastChest].x;
					int y = Main.chest[lastChest].y;
					for (int index2 = 0; index2 < Main.chest[lastChest].item.Length; index2++) {
						Item item = Main.chest[lastChest].item[index2];
						if (!item.IsAir) {
							if (!dropCounts.TryGetValue(item.type, out DropCountAccuracyData currentCount)) {
								currentCount = currentCount with { TotalCount = 1000, Max = int.MinValue, Min = int.MaxValue };
							}
							dropCounts[item.type] = currentCount.Add(item.stack);
						}
					}
					Chest.DestroyChestDirect(x, y, lastChest);
					KillTile(x, y);
				} else {
					continue;
				}
			}
		} else {
			for (int index = 0; index < try_count; index++) {
				switch (pool) {
					case ChestLootLoader.LootPoolNames.WaterAnywhere:
					GenVars.WaterItemCount = _WaterItemCount;
					break;
				}
				if (orig(i, j, pool, notNearOtherChests, Style, trySlope, chestTileType, forceContain)) {
					if (!Main.chest.IndexInRange(lastChest))
						continue;

					switch (pool) {
						case ChestLootLoader.LootPoolNames.Shadow:
						GenVars.hellChest--;
						break;
						case ChestLootLoader.LootPoolNames.JungleTree or ChestLootLoader.LootPoolNames.Jungle:
						GenVars.JungleItemCount--;
						break;
					}

					int x = Main.chest[lastChest].x;
					int y = Main.chest[lastChest].y;
					for (int index2 = 0; index2 < Main.chest[lastChest].item.Length; index2++) {
						Item item = Main.chest[lastChest].item[index2];
						if (!item.IsAir) {
							if (!dropCounts.TryGetValue(item.type, out DropCountAccuracyData currentCount)) {
								currentCount = currentCount with { TotalCount = 1000, Max = int.MinValue, Min = int.MaxValue };
							}
							dropCounts[item.type] = currentCount.Add(item.stack);
						}
					}
					Chest.DestroyChestDirect(x, y, lastChest);
					KillTile(x, y);
				} else {
					continue;
				}
			}
		}
		return dropCounts;
	}
	public readonly record struct DropCountAccuracyData(int TotalCount, int Count, int Min, int Max) {
		public readonly DropCountAccuracyData Add(int stack) => new(TotalCount, Count + 1, Math.Min(stack, Min), Math.Max(stack, Max));
		public readonly DropCountAccuracyData Add(DropCountAccuracyData other) => new(TotalCount + other.TotalCount, Count + other.Count, Math.Min(other.Min, Min), Math.Max(other.Max, Max));
	}
}
public class DeclChestLootTestsSystem : ModSystem {
	public override void PreWorldGen() {
		ModContent.GetInstance<DeclChestLootTests>().dropCountInaccuracies.Clear();
	}
}
public class InaccuracyLootSourceType : LootSourceType {
	public override string Texture => "Terraria/Images/Item_" + ItemID.Chest;
	public override void DrawSource(SpriteBatch spriteBatch, int type, Vector2 position, bool hovering) {
		List<(string name, (Dictionary<int, DropCountAccuracyData> inaccuracies, Point pos) data)> inaccuracies = ModContent.GetInstance<DeclChestLootTests>().dropCountInaccuracies;
		bool exists = inaccuracies.IndexInRange(type);
		int iconType = ItemID.Waldo;
		if (exists) {
			if (inaccuracies[type].name.EndsWith(')')) {
				iconType = inaccuracies[type].data.inaccuracies.Keys.First();
			} else {
				bool first = true;
				foreach (int inaccuracy in inaccuracies[type].data.inaccuracies.Keys) {
					if (ContentSamples.ItemCreativeSortingId.TryGetValue(inaccuracy, out var grouping) && grouping.Group == ContentSamples.CreativeHelper.ItemGroup.Dye) {
						iconType = inaccuracy;
						break;
					}
					if (first) {
						iconType = inaccuracy;
						first = false;
					}
				}

			}
		}
		Item item = ContentSamples.ItemsByType[iconType];
		UIMethods.DrawColoredItemSlot(spriteBatch, ref item, position, TextureAssets.InventoryBack13.Value, hovering ? ItemSourceHelperConfig.Instance.HoveredItemSlotColor : ItemSourceHelperConfig.Instance.ItemSlotColor);
		if (hovering && exists) UICommon.TooltipMouseText(inaccuracies[type].name ?? "Unspecified (null)");
	}
	public override IEnumerable<LootSource> FillSourceList() {
		for (int i = 0; i < 1000; i++) {
			yield return new(this, i);
		}
	}
	public override List<DropRateInfo> GetDrops(int type) {
		List<(string name, (Dictionary<int, DropCountAccuracyData> inaccuracies, Point pos) data)> inaccuracies = ModContent.GetInstance<DeclChestLootTests>().dropCountInaccuracies;
		if (!inaccuracies.IndexInRange(type)) return [];
		(string name, (Dictionary<int, DropCountAccuracyData> inaccuracies, Point pos) data) = inaccuracies[type];
		List<DropRateInfo> drops = new(data.inaccuracies.Count);
		foreach (KeyValuePair<int, DropCountAccuracyData> inaccuracy in data.inaccuracies.OrderDescending(new LootOrderer(name.EndsWith(')')))) {
			float dropRate = inaccuracy.Value.Count / (float)inaccuracy.Value.TotalCount;
			if (inaccuracy.Value.Count == int.MaxValue) {
				dropRate = float.PositiveInfinity;
			} else if(inaccuracy.Value.Count == int.MinValue) {
				dropRate = float.NegativeInfinity;
			}
			drops.Add(new() {
				itemId = inaccuracy.Key,
				dropRate = dropRate,
				stackMin = inaccuracy.Value.Min,
				stackMax = inaccuracy.Value.Max
			});
		}
		return drops;
	}
	private class LootOrderer(bool isComparison) : IComparer<KeyValuePair<int, DropCountAccuracyData>> {
		public int Compare(KeyValuePair<int, DropCountAccuracyData> x, KeyValuePair<int, DropCountAccuracyData> y) {
			ContentSamples.CreativeHelper.ItemGroupAndOrderInGroup item1 = ContentSamples.ItemCreativeSortingId[x.Key];
			ContentSamples.CreativeHelper.ItemGroupAndOrderInGroup item2 = ContentSamples.ItemCreativeSortingId[y.Key];
			int num;
			if (isComparison && (item1.Group == ContentSamples.CreativeHelper.ItemGroup.Dye || item2.Group == ContentSamples.CreativeHelper.ItemGroup.Dye)) {
				num = (item1.Group == ContentSamples.CreativeHelper.ItemGroup.Dye).ToInt().CompareTo((item2.Group == ContentSamples.CreativeHelper.ItemGroup.Dye).ToInt());
			} else {
				num = item1.Group.CompareTo(item2.Group);
			}
			if (num == 0)
				num = item1.OrderInGroup.CompareTo(item2.OrderInGroup);

			return num;
		}
	}
	public override Dictionary<string, string> GetSearchData(int type) => [];
	public override bool DoubleClick(int type) {
		Main.LocalPlayer.Teleport(ModContent.GetInstance<DeclChestLootTests>().dropCountInaccuracies[type].data.pos.ToWorldCoordinates(8, -8));
		return false;
	}
}
