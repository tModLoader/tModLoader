using ExampleMod.Common.Players;
using ExampleMod.Content.Items.Consumables;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Humanizer.In;

namespace ExampleMod.Content.Items
{
	// This class showcases a "pickup". Also known as a power-up.
	// Pickup refers to items that don't enter then inventory when picked up, but rather have some other effect when obtained.
	// Pickups usually provide resources to the player, such as hearts providing life or stars providing mana. Nebula armor boosters is another example.
	// This example drops from enemies when Example Resource is low, similar to how hearts and stars only drop if the player is lacking health or mana.
	public class ExampleResourcePickup : ModItem
	{
		public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatShouldNotBeInInventory[Type] = true;
			ItemID.Sets.IgnoresEncumberingStone[Type] = true;
			ItemID.Sets.IsAPickup[Type] = true;
			ItemID.Sets.ItemSpawnDecaySpeed[Type] = 4;
		}

		public override void SetDefaults() {
			Item.height = 12;
			Item.width = 12;
		}

		public override bool OnPickup(Player player) {
			ExampleResourcePlayer exampleResourcePlayer = player.GetModPlayer<ExampleResourcePlayer>();
			exampleResourcePlayer.exampleResourceCurrent = Math.Clamp(exampleResourcePlayer.exampleResourceCurrent + 50, 0, exampleResourcePlayer.exampleResourceMax2);

			SoundEngine.PlaySound(SoundID.Grab, player.Center);

			CombatText.NewText(player.getRect(), ExampleResourcePlayer.HealExampleResource, 50);

			if (Main.netMode == NetmodeID.MultiplayerClient) {
				ExampleResourcePlayer.SendExampleResourceEffectMessage();

				ModPacket packet = Mod.GetPacket();
				packet.Write((byte)ExampleMod.MessageType.ExampleResourceEffect);
				packet.Write((byte)Player.whoAmI);
				packet.Write((byte)exampleLifeFruits);
				packet.Write((byte)exampleManaCrystals);
				packet.Send(toWho, fromWho);

			//	NetMessage.SendData(43, -1, -1, null, whoAmI, manaAmount);
			}

			// We return false to prevent the item from going into the players inventory.
			return false;
		}

		public override bool CanPickup(Player player) {
			return base.CanPickup(player); // not needed, ItemID.Sets.IsAPickup
		}

		public override void GrabRange(Player player, ref int grabRange) {
			// GrabRange can be used to implement effects similar to Heartreach potion or Celestial Magnet. 
			grabRange += 250;
		}

		public override bool GrabStyle(Player player) {
			// necessary example? Faster?
			return base.GrabStyle(player);
		}
	}

	public class ExampleResourcePickupGlobalNPC : GlobalNPC {
		public override void OnKill(NPC npc) {
			// explain why here instead of drop rules.

			// This code closely mimics NPC.NPCLoot_DropCommonLifeAndMana

			Player closestPlayer = Main.player[Player.FindClosest(npc.position, npc.width, npc.height)];
			ExampleResourcePlayer exampleResourcePlayer = closestPlayer.GetModPlayer<ExampleResourcePlayer>();

			// MotherSlime, CorruptSlime, and Slimer do not count as dying for the purposes of resource drops because they spawn other enemies when they die.
			if (npc.type != NPCID.MotherSlime && npc.type != NPCID.CorruptSlime && npc.type != NPCID.Slimer && closestPlayer.RollLuck(6) == 0 && npc.lifeMax > 1 && npc.damage > 0 && Main.rand.NextBool(2) && exampleResourcePlayer.exampleResourceCurrent < exampleResourcePlayer.exampleResourceMax2) {
				Item.NewItem(npc.GetSource_Loot(), npc.getRect(), ModContent.ItemType<ExampleResourcePickup>());
			}
		}
	}
}
