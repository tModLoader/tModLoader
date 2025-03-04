using ExampleMod.Content.Biomes;
using ExampleMod.Content.Items.Tools;
using ExampleMod.Content.NPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	// This class showcases things you can do with fishing
	public class ExampleFishingPlayer : ModPlayer
	{
		public static LocalizedText WrongText { get; private set; }
		public bool hasExampleCrateBuff;

		public override void SetStaticDefaults() {
			WrongText = Mod.GetLocalization($"{nameof(ExampleFishingPlayer)}.Wrong");
		}

		public override void ResetEffects() {
			hasExampleCrateBuff = false;
		}

		public override void ModifyFishingAttempt(ref FishingAttempt attempt) {
			// If the player has the Example Crate buff (given by Example Crate Potion), 10% additional chance that the catch will be a crate
			// The "tier" of the crate depends on the rarity, which we don't modify here, see the comments in CatchFish for details
			if (hasExampleCrateBuff && !attempt.crate) {
				if (Main.rand.Next(100) < 10) {
					attempt.crate = true;
				}
			}
		}

		public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) {
			bool inWater = !attempt.inLava && !attempt.inHoney;
			bool inExampleSurfaceBiome = Player.InModBiome<ExampleSurfaceBiome>();
			if (attempt.playerFishingConditions.PoleItemType == ModContent.ItemType<ExampleFishingRod>() && inWater && inExampleSurfaceBiome) {
				// In this example, we will fish up an Example Person from the water in Example Surface Biome,
				// as long as there isn't one in the world yet
				// NOTE: if a fishing rod has multiple bobbers, then each one can spawn the NPC
				int npc = ModContent.NPCType<ExamplePerson>();
				if (!NPC.AnyNPCs(npc)) {
					// Make sure itemDrop = -1 when summoning an NPC, as otherwise terraria will only spawn the item
					npcSpawn = npc;
					itemDrop = -1;

					// Also, to make it cooler, we will make a special sonar message for when it shows up
					sonar.Text = WrongText.Value;
					sonar.Color = Color.LimeGreen;
					sonar.Velocity = Vector2.Zero;
					sonar.DurationInFrames = 300;

					// And that text shows up on the player's head, not on the bobber location.
					sonarPosition = new Vector2(Player.position.X, Player.position.Y - 64);

					return; // This is important so your code after this that rolls items will not run
				}
			}

			if (inWater && inExampleSurfaceBiome && attempt.crate) {
				// If the game rolls a crate, we want to give ours to the player if he is in Example Surface Biome

				// We don't want to replace golden/titanium crates (the highest tier crates), as they take highest priority in crate catches
				// Their drop conditions are "veryrare" or "legendary"
				// (After that come biome crates ("rare"), then iron/mythril ("uncommon"), then wood/pearl (none of the previous))
				// Let's replace biome crates 50% of the time (player could be in multiple (modded) biomes, we should respect that)
				if (!attempt.veryrare && !attempt.legendary && attempt.rare && Main.rand.NextBool()) {
					itemDrop = ModContent.ItemType<Content.Items.Consumables.ExampleFishingCrate>();
					return; // This is important so your code after this that rolls items will not run
				}
			}

			// Here we will set the catch conditions for our ExampleQuestFish
			int exampleQuestFish = ModContent.ItemType<Content.Items.ExampleQuestFish>(); // We'll store the type as a variable, since we'll be referencing it several times
			// First we check if today's quest matches our quest fish
			if (attempt.questFish == exampleQuestFish) {
				// Our ExampleQuestFish states that it can only be caught whilst upside-down, so we'll have to check the gravity
				// Normal gravity is positive, whilst reversed gravity is negative
				// Finally, most vanilla quest fish only appear on an uncommon roll, so we'll do the same
				if (Player.gravDir < 0f && attempt.uncommon) {
					itemDrop = exampleQuestFish;
					return; // While there is no more code that could roll a fish after this, we might add some in the future so it's best to return here
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
