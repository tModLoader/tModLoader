using MonoMod.Cil;
using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.WorldBuilding;
using Terraria.IO;
using Terraria.ModLoader;
using System.ComponentModel;
using Terraria.ID;
using Terraria.GameContent.ItemDropRules;
using System.Collections;

namespace Terraria;

public partial class WorldGen
{
	internal static void ClearGenerationPasses()
	{
		_generator?._passes.Clear();
	}

	internal static Dictionary<string, GenPass> _vanillaGenPasses = new();
	public static IReadOnlyDictionary<string, GenPass> VanillaGenPasses => _vanillaGenPasses;

	public static void ModifyPass(PassLegacy pass, ILContext.Manipulator callback)
	{
		MonoModHooks.Modify(pass._method.Method, callback);
	}

	// The self reference has to be object, because the actual type is a compiler generated closure class
	// The self reference isn't useful anyway, since the closure doesn't capture any method locals or an enclosing class instance
	// We might think to omit the self parameter from mod delegates, and register a wrapper which propogates self via a closure, but then MonoModHooks will attribute the hook to tModLoader rather than the original mod.
	public delegate void GenPassDetour(orig_GenPassDetour orig, object self, GenerationProgress progress, GameConfiguration configuration);
	public delegate void orig_GenPassDetour(object self, GenerationProgress progress, GameConfiguration configuration);

	public static void DetourPass(PassLegacy pass, GenPassDetour hookDelegate)
	{
		MonoModHooks.Add(pass._method.Method, hookDelegate);
	}
	/// <summary>
	/// Attempts to place a chest and fill it with typical loot according to the style (<paramref name="Style"/>) and depth. Without any parameters, a regular, gold, or locked shadow chest will be created, depending on the depth. You can pass in an item type (<paramref name="contain"/>) and the first item in the chest will be that item. Unlike <see cref="PlaceChest(int, int, ushort, bool, int)"/>, the resulting chest will be placed with the bottom right corner at the given coordinates (<paramref name="i"/>, <paramref name="j"/>). In addition, if the given <paramref name="j"/> coordinate isn't suitable, AddBuriedChest will search down from the given coordinate to find the first solid tile it encounters and attempt to place there. This method returns true if a chest was successfully placed, but be aware that the chest might not be exactly at the coordinates you provide. This makes further adjusting the chest contents directly difficult.
	/// <para/> A video guide can be found on the <see href="https://github.com/tModLoader/tModLoader/wiki/World-Generation#terrariaworldgen-public-static-void-tilerunnerint-i-int-j-double-strength-int-steps-int-type-bool-addtile--false-float-speedx--0f-float-speedy--0f-bool-noychange--false-bool-override--true">World Generation wiki page</see>. It shows an example of running the method with the default parameters <c>WorldGen.AddBuriedChest(x, y);</c>. Notice how the chest style changes according to depth and how the chest is placed on the floor directly below the provided coordinates if possible.
	/// </summary>
	/// <param name="i"></param>
	/// <param name="j"></param>
	/// <param name="pool"></param>
	/// <param name="notNearOtherChests"></param>
	/// <param name="Style"></param>
	/// <param name="trySlope"></param>
	/// <param name="chestTileType"></param>
	/// <param name="forceContain"></param>
	/// <returns></returns>
	public static bool AddBuriedChest(int i, int j, string pool, bool notNearOtherChests = false, int Style = -1, bool trySlope = false, ushort chestTileType = 0, int forceContain = ItemID.None)
	{
		if (chestTileType == 0)
			chestTileType = 21;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		bool flag6 = false;
		bool flag7 = false;
		bool flag8 = false;
		bool flag9 = false;
		bool flag10 = false;
		bool canBeReplacedByAngelStatue = false;
		int num = 15;
		if (tenthAnniversaryWorldGen)
			num *= 3;

		for (int floorTileY = j; floorTileY < Main.maxTilesY - 10; floorTileY++) {
			int num2 = -1;
			int num3 = -1;
			if (Main.tile[i, (int)floorTileY].shimmer())
				return false;

			if (trySlope && Main.tile[i, (int)floorTileY].active() && Main.tileSolid[Main.tile[i, (int)floorTileY].type] && !Main.tileSolidTop[Main.tile[i, (int)floorTileY].type]) {
				if (Style == 17) {
					int num4 = 30;
					for (int l = i - num4; l <= i + num4; l++) {
						for (int m = floorTileY - num4; m <= floorTileY + num4; m++) {
							if (!InWorld(l, m, 5))
								return false;

							if (Main.tile[l, m].active() && (Main.tile[l, m].type == 21 || Main.tile[l, m].type == 467))
								return false;
						}
					}
				}

				if (Main.tile[i - 1, (int)floorTileY].topSlope()) {
					num2 = Main.tile[i - 1, (int)floorTileY].slope();
					Main.tile[i - 1, (int)floorTileY].slope(0);
				}

				if (Main.tile[i, (int)floorTileY].topSlope()) {
					num3 = Main.tile[i, (int)floorTileY].slope();
					Main.tile[i, (int)floorTileY].slope(0);
				}
			}

			if (remixWorldGen && (double)i > (double)Main.maxTilesX * 0.37 && (double)i < (double)Main.maxTilesX * 0.63 && floorTileY > Main.maxTilesY - 250)
				return false;

			int num5 = 2;
			for (int n = i - num5; n <= i + num5; n++) {
				for (int num6 = floorTileY - num5; num6 <= floorTileY + num5; num6++) {
					if (Main.tile[n, num6].active() && (TileID.Sets.Boulders[Main.tile[n, num6].type] || Main.tile[n, num6].type == 26 || Main.tile[n, num6].type == 237))
						return false;
				}
			}

			if (!SolidTile(i, (int)floorTileY))
				continue;

			int floorTileType = Main.tile[i, floorTileY].type;
			bool isHellChest = false;
			int style = 0;
			bool flag12 = (double)floorTileY >= Main.worldSurface + 25.0;
			if (remixWorldGen)
				flag12 = floorTileY < Main.maxTilesY - 400;

			if (Style >= 0) {
				style = Style;
			}
			else if (flag12 || !string.IsNullOrEmpty(pool)) {
				style = 1;
			}


			if ((chestTileType == 467 && style == 10) || (string.IsNullOrEmpty(pool) && floorTileY <= Main.maxTilesY - 205 && IsUndergroundDesert(i, floorTileY))) {
				flag2 = true;
				style = 10;
				chestTileType = 467;
				pool = (floorTileY <= (GenVars.desertHiveHigh * 3 + GenVars.desertHiveLow * 4) / 7) ? ChestLootLoader.LootPoolNames.SandstoneHigh : ChestLootLoader.LootPoolNames.SandstoneLow;
				canBeReplacedByAngelStatue = true;
			}

			if (chestTileType == 21 && (style == 11 || (string.IsNullOrEmpty(pool) && floorTileY >= Main.worldSurface + 25.0 && floorTileY <= Main.maxTilesY - 205 && floorTileType is TileID.SnowBlock or TileID.IceBlock or TileID.BreakableIce))) {
				style = 11;
				pool = ChestLootLoader.LootPoolNames.Frozen;

				canBeReplacedByAngelStatue = true;
			}
			if (chestTileType == 21 && (Style == 10 || contain == 211 || contain == 212 || contain == 213 || contain == 753)) {
				flag3 = true;
				style = 10;
				canBeReplacedByAngelStatue = true;
			}

			if (chestTileType == 21 && floorTileY > Main.maxTilesY - 205 && string.IsNullOrEmpty(pool)) {
				flag7 = true;
				contain = GenVars.hellChestItem[GenVars.hellChest];
				style = 4;
				canBeReplacedByAngelStatue = true;
			}

			if (chestTileType == 21 && style == 17 && string.IsNullOrEmpty(pool)) {
				pool = ChestLootLoader.LootPoolNames.WaterAnywhere;
				canBeReplacedByAngelStatue = true;
			}

			if (chestTileType == 21 && style == 12) {
				flag5 = true;
				canBeReplacedByAngelStatue = true;
			}

			if (chestTileType == 21 && style == 32) {
				flag6 = true;
				canBeReplacedByAngelStatue = true;
			}

			if (chestTileType == 21 && style != 0 && IsDungeon(i, floorTileY))
				flag8 = true;
			if (chestTileType == 21 && style != 0 && (contain == 848 || contain == 857 || contain == 934))
				flag9 = true;
			if (chestTileType == 21 && (style == 13 || contain == 159 || contain == 65 || contain == 158 || contain == 2219)) {
				if (remixWorldGen && !getGoodWorldGen) {
					if (crimson) {
						style = 43;
					}
					else {
						chestTileType = 467;
						style = 3;
					}
				}
			}

			if (noTrapsWorldGen && style == 1 && chestTileType == 21 && (!remixWorldGen || genRand.Next(3) == 0)) {
				style = 4;
				chestTileType = TileID.Containers2;
			}
			int chestIndex = PlaceChest(i - 1, floorTileY - 1, chestTileType, notNearOtherChests, style);
			if (chestIndex >= 0) {
				switch (pool) {
					case ChestLootLoader.LootPoolNames.Shadow:
						GenVars.hellChest++;
						break;
					case ChestLootLoader.LootPoolNames.Jungle:
						GenVars.JungleItemCount++;
						break;
					case ChestLootLoader.LootPoolNames.WaterAnywhere:
						GenVars.WaterItemCount++;
						break;
				}

				Chest chest = Main.chest[chestIndex];
				bool replaceFirstSuccessWithAngelStatue = getGoodWorldGen && canBeReplacedByAngelStatue && genRand.NextBool(num);
				if (replaceFirstSuccessWithAngelStatue && forceContain != ItemID.None) {
					forceContain = ItemID.AngelStatue;
					replaceFirstSuccessWithAngelStatue = false;
				}
				chest.item[0].SetDefaults(forceContain);
				DropAttemptInfo dropInfo = new() {
					chest = chest,
					IsExpertMode = Main.expertMode,
					IsMasterMode = Main.masterMode,
					rng = genRand,
				};
				ulong openSlots = 0x0;
				for (int scanSlotIndex = 0; scanSlotIndex < chest.item.Length; scanSlotIndex++) {
					if (chest.item[scanSlotIndex].IsAir)
						openSlots |= 0x1ul << scanSlotIndex;
				}
				foreach (IItemDropRule rule in ChestLootLoader.GetLootPool(pool)) {
					ItemDropAttemptResult result = ItemDropResolver.ResolveRule(rule, dropInfo);
					if (replaceFirstSuccessWithAngelStatue && result.State == ItemDropAttemptResultState.Success) {
						bool firstDrop = true;
						for (int scanSlotIndex = 0; scanSlotIndex < chest.item.Length; scanSlotIndex++) {
							if (!chest.item[scanSlotIndex].IsAir && (openSlots & (0x1ul << scanSlotIndex)) != 0) {
								if (firstDrop) {
									chest.item[scanSlotIndex].SetDefaults(ItemID.AngelStatue);
									firstDrop = false;
								}
								else {
									chest.item[scanSlotIndex].TurnToAir();
								}
							}
						}
						replaceFirstSuccessWithAngelStatue = false;
					}
				}
				return true;
			}

			if (trySlope) {
				if (num2 > -1)
					Main.tile[i - 1, floorTileY].slope((byte)num2);

				if (num3 > -1)
					Main.tile[i, floorTileY].slope((byte)num3);
			}

			return false;
		}

		return false;
	}
}
