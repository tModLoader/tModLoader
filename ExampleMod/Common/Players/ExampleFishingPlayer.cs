using ExampleMod.Content.Biomes;
using ExampleMod.Content.Items.Tools;
using ExampleMod.Content.NPCs;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	public class ExampleFishingPlayer: ModPlayer
	{
		bool fishingWithLadybug = false;

		public override void ResetEffects() {
			fishingWithLadybug = false;
		}

		public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) {
			/*In this example, we will fish up an Example person from the water in Example Surface Biome, as long as there isn't one in the world yet*/
			if (attempt.playerFishingConditions.PoleItemType == ModContent.ItemType<ExampleFishingRod>() && !attempt.inLava && !attempt.inHoney &&
				Player.InModBiome(ModContent.GetInstance<ExampleSurfaceBiome>())){
				int npc = ModContent.NPCType<ExamplePerson>();
				if (!NPC.AnyNPCs(ModContent.NPCType<ExamplePerson>())) {
					//Make sure itemDrop = -1 when summoning an NPC, as otherwise terraria will only spawn the item
					npcSpawn = npc;
					itemDrop = -1;

					/*Also, to make it cooler, we will make a special sonar message for when it shows up*/
					sonar.Text = "Something's wrong...";
					sonar.Color = Color.LimeGreen;
					sonar.Velocity = Vector2.Zero;
					sonar.DurationInFrames = 300;

					//And that text shows up on the player's head, not on the bobber location.
					sonarPosition = new Vector2(Player.position.X, Player.position.Y - 64);

					return;
				}
			}
			/*In this example, we make sure that we got a Ladybug as bait, and later on use that to determine what we catch*/
			if(attempt.playerFishingConditions.BaitItemType == ItemID.LadyBug) {
				fishingWithLadybug = true;
			}
		}

		public override bool? WillConsumeBait(Item bait) {
			if (bait.type == ItemID.LadyBug) {
				fishingWithLadybug = true;
			}
			//The golden fishing rod will never consume a ladybug
			if((fishingWithLadybug || Player.GetFishingConditions().BaitItemType == ItemID.GoldLadyBug) && Player.GetFishingConditions().Pole.type == ItemID.GoldenFishingRod) {
				return false;
			}
			return base.WillConsumeBait(bait);
		}

		//If fishing with ladybug, we will receive multiple "fish" per bobber. Does not apply to quest fish
		public override void ModifyCaughtFish(Item fish) {
			if (fishingWithLadybug && fish.rare != ItemRarityID.Quest) {
				fish.stack += Main.rand.Next(1, 4);
			}
			return;
		}

	}
}
