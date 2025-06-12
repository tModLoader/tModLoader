using ExampleMod.Content.Items;
using ExampleMod.Content.NPCs;
using ExampleMod.Content.Projectiles;
using ExampleMod.Content.Tiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalTiles
{
	public class GlobalShakeTrees : GlobalTile
	{
		// With this hook, we can determine the item drop when a tree is shaken.
		// See ExampleSourceDependentItemTweaks for another example for tree shaking.
		public override bool ShakeTree(int x, int y, TreeTypes treeType) {
			// Normal forest trees have a 5% chance to drop an Example Item.
			if (treeType == TreeTypes.Forest && WorldGen.genRand.NextBool(20)) {
				Item.NewItem(WorldGen.GetItemSource_FromTreeShake(x, y), x * 16, y * 16, 16, 16, ModContent.ItemType<ExampleItem>());
				// We return true to indicate that the primary item has dropped and prevent the game from attempting to drop other items.
				return true;
			}

			// Glowing Mushroom trees have 10% chance to drop between 3 and 10 Mushroom Torches.
			if (treeType == TreeTypes.Mushroom && WorldGen.genRand.NextBool(10)) {
				Item.NewItem(WorldGen.GetItemSource_FromTreeShake(x, y), x * 16, y * 16, 16, 16, ItemID.MushroomTorch, WorldGen.genRand.Next(3, 11));
				return true;
			}

			// Snow (Boreal) trees have a 5% chance to spawn an Example Paper Airplane projectile that flies left or right.
			// The owner of the projectile is set to Main.myPlayer which means the server owns the projectile in multiplayer.
			if (treeType == TreeTypes.Snow && WorldGen.genRand.NextBool(20)) {
				Projectile.NewProjectile(new EntitySource_ShakeTree(x, y), x * 16, y * 16, Main.rand.Next(-16, 16), 0f, ModContent.ProjectileType<ExamplePaperAirplaneProjectile>(), Damage: 4, KnockBack: 1f, Owner: Main.myPlayer);
				return true;
			}

			// Jungle (Mahogany) trees have a 14% chance to spawn a Giant Flying Fox if located on the surface and in Hardmode.
			// y == 0 is the top of the world, so y < Main.worldSurface is the area from the surface height to the top of the world.
			if (treeType == TreeTypes.Jungle && WorldGen.genRand.NextBool(7) && y < Main.worldSurface && Main.hardMode) {
				NPC.NewNPC(WorldGen.GetNPCSource_ShakeTree(x, y), x * 16, y * 16, NPCID.GiantFlyingFox);
				return true;
			}

			// Modded trees will be tree type Custom by default.
			// In this example, there is a 50% chance for a modded tree to drop a Party Zombie at night.
			if (treeType == TreeTypes.Custom && WorldGen.genRand.NextBool(2) && !Main.dayTime) {
				NPC.NewNPC(WorldGen.GetNPCSource_ShakeTree(x, y), x * 16, y * 16, ModContent.NPCType<PartyZombie>());
				return true;
			}

			// In this example, we want to target Example Trees specifically. We don't want other modded trees such as Example Palm Tree.
			if (treeType == TreeTypes.Custom) {
				WorldGen.GetTreeBottom(x, y, out int baseX, out int baseY); // Finds the block that the tree is planted on.
				// If the block the tree is planted on is an Example Block, we know we have found an Example Tree.
				if (Main.tile[baseX, baseY].TileType == ModContent.TileType<ExampleBlock>() && WorldGen.genRand.NextBool(2)) {
					Item.NewItem(WorldGen.GetItemSource_FromTreeShake(x, y), x * 16, y * 16, 16, 16, ItemID.Coconut);
					return true;
				}
			}

			// To make things happen when your modded tree is shook, override ModTree.Shake() instead. See ExampleTree.
			return false;
		}

		// With PreShakeTree we can spawn bonus items or prevent existing item drop options.
		// These bonus drops will not replace the primary item drop choice.
		public override void PreShakeTree(int x, int y, TreeTypes treeType) {
			// In this example, there is 20% chance for any tree to shoot a hostile arrow downwards on No Traps and Get Fixed Boi worlds.
			if (WorldGen.genRand.NextBool(5) && Main.noTrapsWorld) {
				Projectile.NewProjectile(new EntitySource_ShakeTree(x, y), x * 16, y * 16, Main.rand.Next(-100, 101) * 0.002f, 8f, ProjectileID.WoodenArrowHostile, Damage: 10, KnockBack: 0f, Owner: Main.myPlayer);
			}

			// This prevents acorns from dropping from normal forest trees during the night
			if (treeType == TreeTypes.Forest && !Main.dayTime) {
				NPCLoader.blockLoot.Add(ItemID.Acorn);
			}
		}
	}
}
