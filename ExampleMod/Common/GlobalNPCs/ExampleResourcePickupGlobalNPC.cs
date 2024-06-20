using ExampleMod.Common.Players;
using ExampleMod.Content.Items;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalNPCs
{
	public class ExampleResourcePickupGlobalNPC : GlobalNPC
	{
		public override void OnKill(NPC npc) {
			// Here we handle the ExampleResourcePickup drops in a similar manner to Heart and Star drops.
			// This code closely mimics the NPC.NPCLoot_DropCommonLifeAndMana method for consistency.

			Player closestPlayer = Main.player[Player.FindClosest(npc.position, npc.width, npc.height)];
			ExampleResourcePlayer exampleResourcePlayer = closestPlayer.GetModPlayer<ExampleResourcePlayer>();

			// These conditions match the Heart and Star drop logic, but can be adjusted based on the intended rarity of the resource.
			if (!NPCID.Sets.NeverDropsResourcePickups[npc.type] && closestPlayer.RollLuck(6) == 0 && npc.lifeMax > 1 && npc.damage > 0 && Main.rand.NextBool(2) && exampleResourcePlayer.exampleResourceCurrent < exampleResourcePlayer.exampleResourceMax2) {
				Item.NewItem(npc.GetSource_Loot(), npc.getRect(), ModContent.ItemType<ExampleResourcePickup>());
			}
		}
	}
}
