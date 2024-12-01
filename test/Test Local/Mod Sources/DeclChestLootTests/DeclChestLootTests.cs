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

namespace DeclChestLootTests {
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
		public List<(string name, Dictionary<int, float> inaccuracies)> dropCountInaccuracies = [];
		public delegate bool orig_AddBuriedChest(int i, int j, string pool, bool notNearOtherChests = false, int Style = -1, bool trySlope = false, ushort chestTileType = 0, int forceContain = ItemID.None);
		public delegate bool hook_AddBuriedChest(orig_AddBuriedChest orig, int i, int j, string pool, bool notNearOtherChests = false, int Style = -1, bool trySlope = false, ushort chestTileType = 0, int forceContain = ItemID.None);
		public bool _AddBuriedChest(orig_AddBuriedChest orig, int i, int j, string pool, bool notNearOtherChests = false, int Style = -1, bool trySlope = false, ushort chestTileType = 0, int forceContain = ItemID.None) {
			bool result = orig(i, j, pool, notNearOtherChests, Style, trySlope, chestTileType, forceContain);
			if (!result) return false;
			if (Main.chest.IndexInRange(lastChest)) {
				int x = Main.chest[lastChest].x;
				int y = Main.chest[lastChest].y;
				Chest.DestroyChestDirect(x, y, lastChest);
				KillTile(x, y);
			}
			Dictionary<int, int> dropCountsVanilla = [];
			Dictionary<int, int> dropCountsNew = [];
			Dictionary<int, float> dropCountInaccuracy = [];
			int tries = 1;
			do {
				dropCountsVanilla = CalculateDropCounts(null, i, j, pool, notNearOtherChests, Style, trySlope, chestTileType, forceContain);
				dropCountsNew = CalculateDropCounts(orig, i, j, pool, notNearOtherChests, Style, trySlope, chestTileType, forceContain);
				dropCountInaccuracy.Clear();
				foreach (int item in dropCountsVanilla.Keys.Union(dropCountsNew.Keys)) {
					dropCountsVanilla.TryGetValue(item, out int dropCountVanilla);
					dropCountsNew.TryGetValue(item, out int dropCountNew);
					float inaccuracy = dropCountVanilla == 0 ? float.PositiveInfinity : (dropCountNew / (float)dropCountVanilla);
					float unsignedInaccuracy = inaccuracy;
					if (unsignedInaccuracy < 1) {
						if (dropCountNew == 0) {
							inaccuracy = float.NegativeInfinity;
							unsignedInaccuracy = float.PositiveInfinity;
						} else {
							unsignedInaccuracy = 1f / unsignedInaccuracy;
						}

					}
					inaccuracy -= 1;
					if (unsignedInaccuracy - 1 > 0.05f) {
						dropCountInaccuracy.Add(item, inaccuracy);
					}
				}
			} while (dropCountInaccuracy.Count > 0 && --tries > 0);
			if (dropCountInaccuracy.Count > 0)
				dropCountInaccuracies.Add((pool, dropCountInaccuracy));
			orig(i, j, pool, notNearOtherChests, Style, trySlope, chestTileType, forceContain);
			return dropCountsVanilla.Count + dropCountsNew.Count > 0;
		}
		Dictionary<int, int> CalculateDropCounts(orig_AddBuriedChest orig, int i, int j, string pool, bool notNearOtherChests = false, int Style = -1, bool trySlope = false, ushort chestTileType = 0, int forceContain = ItemID.None) {
			const float try_count = 1000;
			Dictionary<int, int> dropCounts = [];
			if (orig is null) {
				for (int index = 0; index < try_count; index++) {
					switch (pool) {
						case ChestLootLoader.LootPoolNames.Jungle:
						forceContain = GetNextJungleChestItem();
						GenVars.JungleItemCount--;
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
						if (genRand.NextBool(tenthAnniversaryWorldGen ? 7 : 10)) {
							forceContain = 863;
						} else {
							switch (i % 4) {
								case 1:
								forceContain = 186;
								break;
								case 2:
								forceContain = 4404;
								break;
								case 3:
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
						switch (genRand.Next(4)) {
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

						for (int index2 = 0; index2 < Main.chest[lastChest].item.Length; index2++) {
							Item item = Main.chest[lastChest].item[index2];
							if (!item.IsAir) {
								dropCounts.TryGetValue(item.type, out int currentCount);
								dropCounts[item.type] = currentCount + item.stack;
							}
						}
						int x = Main.chest[lastChest].x;
						int y = Main.chest[lastChest].y;
						Chest.DestroyChestDirect(x, y, lastChest);
						KillTile(x, y);
					} else {
						continue;
					}
				}
			} else {
				for (int index = 0; index < try_count; index++) {
					if (orig(i, j, pool, notNearOtherChests, Style, trySlope, chestTileType, forceContain)) {
						if (!Main.chest.IndexInRange(lastChest))
							continue;

						switch (pool) {
							case ChestLootLoader.LootPoolNames.Shadow:
							GenVars.hellChest--;
							break;
							case ChestLootLoader.LootPoolNames.Jungle:
							GenVars.JungleItemCount--;
							break;
						}

						for (int index2 = 0; index2 < Main.chest[lastChest].item.Length; index2++) {
							Item item = Main.chest[lastChest].item[index2];
							if (!item.IsAir) {
								dropCounts.TryGetValue(item.type, out int currentCount);
								dropCounts[item.type] = currentCount + item.stack;
							}
						}
						int x = Main.chest[lastChest].x;
						int y = Main.chest[lastChest].y;
						Chest.DestroyChestDirect(x, y, lastChest);
						KillTile(x, y);
					} else {
						continue;
					}
				}
			}
			return dropCounts;
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
			List<(string name, Dictionary<int, float> inaccuracies)> inaccuracies = ModContent.GetInstance<DeclChestLootTests>().dropCountInaccuracies;
			bool exists = inaccuracies.IndexInRange(type);
			Item item = ContentSamples.ItemsByType[exists ? inaccuracies[type].inaccuracies.Keys.First() : ItemID.Waldo];
			UIMethods.DrawColoredItemSlot(spriteBatch, ref item, position, TextureAssets.InventoryBack13.Value, hovering ? ItemSourceHelperConfig.Instance.HoveredItemSlotColor : ItemSourceHelperConfig.Instance.ItemSlotColor);
			if (hovering && exists) UICommon.TooltipMouseText(inaccuracies[type].name ?? "Unspecified (null)");
		}
		public override IEnumerable<LootSource> FillSourceList() {
			for (int i = 0; i < 1000; i++) {
				yield return new(this, i);
			}
		}
		public override List<DropRateInfo> GetDrops(int type) {
			List<(string name, Dictionary<int, float> inaccuracies)> inaccuracies = ModContent.GetInstance<DeclChestLootTests>().dropCountInaccuracies;
			if (!inaccuracies.IndexInRange(type)) return [];
			List<DropRateInfo> drops = new(inaccuracies[type].inaccuracies.Count);
			foreach (KeyValuePair<int, float> inaccuracy in inaccuracies[type].inaccuracies.OrderByDescending(kvp => kvp.Value)) {
				drops.Add(new() {
					itemId = inaccuracy.Key,
					dropRate = inaccuracy.Value,
				});
			}
			return drops;
		}
		public override Dictionary<string, string> GetSearchData(int type) => [];
		public override bool DoubleClick(int type) {
			if (Main.mouseRight) {
				//ItemSourceHelper.Instance.BrowserWindow.SetTab<ItemBrowserWindow>(true).ScrollToItem(type);
				return true;
			}
			return false;
		}
	}
}
