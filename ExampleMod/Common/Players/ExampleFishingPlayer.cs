﻿using ExampleMod.Content.Biomes;
using ExampleMod.Content.Items.Tools;
using ExampleMod.Content.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	public class ExampleFishingPlayer : ModPlayer
	{
		public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) {
			// In this example, we will fish up an Example Person from the water in Example Surface Biome,
			// as long as there isn't one in the world yet
			// NOTE: if a fishing rod has multiple bobbers, then each one can spawn the NPC
			if (attempt.playerFishingConditions.PoleItemType == ModContent.ItemType<ExampleFishingRod>() && !attempt.inLava && !attempt.inHoney &&
					Player.InModBiome(ModContent.GetInstance<ExampleSurfaceBiome>())) {

				int npc = ModContent.NPCType<ExamplePerson>();
				if (!NPC.AnyNPCs(npc)) {
					// Make sure itemDrop = -1 when summoning an NPC, as otherwise terraria will only spawn the item
					npcSpawn = npc;
					itemDrop = -1;

					// Also, to make it cooler, we will make a special sonar message for when it shows up
					sonar.Text = "Something's wrong...";
					sonar.Color = Color.LimeGreen;
					sonar.Velocity = Vector2.Zero;
					sonar.DurationInFrames = 300;

					// And that text shows up on the player's head, not on the bobber location.
					sonarPosition = new Vector2(Player.position.X, Player.position.Y - 64);
				}
			}
		}

		public override bool? CanConsumeBait(Item bait) {
			// Player.GetFishingConditions() returns you the best fishing pole Item, type and power, the best bait Item, type and Power, and the total fishing level, including modded values
			// These are the same Pole and Bait the game considers when calculating the obtained fish.
			// during CanConsumeBait, Player.GetFishingConditions() == attempt.playerFishingConditions from CatchFish.
			PlayerFishingConditions conditions = Player.GetFishingConditions();

			// The golden fishing rod will never consume a ladybug
			if ((bait.type == ItemID.LadyBug || bait.type == ItemID.GoldLadyBug) && conditions.Pole.type == ItemID.GoldenFishingRod) {
				return false;
			}

			return null; // Let the default logic run
		}

		// If fishing with ladybug, we will receive multiple "fish" per bobber. Does not apply to quest fish
		public override void ModifyCaughtFish(Item fish) {
			// In this example, we make sure that we got a Ladybug as bait, and later on use that to determine what we catch
			if (Player.GetFishingConditions().BaitItemType == ItemID.LadyBug && fish.rare != ItemRarityID.Quest) {
				fish.stack += Main.rand.Next(1, 4);
			}
		}
	}
}
