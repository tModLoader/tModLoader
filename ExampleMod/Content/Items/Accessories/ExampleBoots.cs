using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Accessories
{
	// This example attempts to showcase most of the common boot accessory effects.
	// Of particular note is a showcase of the correct approaches to various movement speed modifications.
	[AutoloadEquip(EquipType.Shoes)]
	public class ExampleBoots : ModItem
	{
		public static readonly int MoveSpeedBonus = 8;
		public static readonly int LavaImmunityTime = 2;

		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MoveSpeedBonus, LavaImmunityTime);

		public override void SetDefaults() {
			Item.width = 22;
			Item.height = 22;

			Item.accessory = true;
			Item.rare = ItemRarityID.Red;
			Item.value = Item.buyPrice(gold: 1); // Equivalent to Item.buyPrice(0, 1, 0, 0);
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			// These 2 stat changes are equal to the Lightning Boots
			player.moveSpeed += MoveSpeedBonus / 100f; // Modifies the player movement speed bonus.
			player.accRunSpeed = 6.75f; // Sets the players sprint speed in boots.

			// player.maxRunSpeed and player.runAcceleration are usually not set by boots and should not be changed in UpdateAccessory due to the logic order. See ExampleStatBonusAccessoryPlayer.PostUpdateRunSpeeds for an example of adjusting those speed stats.

			// Determines whether the boots count as rocket boots
			// 0 - These are not rocket boots
			// Anything else - These are rocket boots
			player.rocketBoots = 2;

			// Sets which dust and sound to use for the rocket flight
			// 1 - Rocket Boots
			// 2 - Fairy Boots, Spectre Boots, Lightning Boots
			// 3 - Frostspark Boots
			// 4 - Terrraspark Boots
			// 5 - Hellfire Treads
			player.vanityRocketBoots = 2;

			player.waterWalk2 = true; // Allows walking on all liquids without falling into it
			player.waterWalk = true; // Allows walking on water, honey, and shimmer without falling into it
			player.iceSkate = true; // Grant the player improved speed on ice and not breaking thin ice when falling onto it
			player.desertBoots = true; // Grants the player increased movement speed while running on sand
			player.fireWalk = true; // Grants the player immunity from Meteorite and Hellstone tile damage
			player.noFallDmg = true; // Grants the player the Lucky Horseshoe effect of nullifying fall damage
			player.lavaRose = true; // Grants the Lava Rose effect
			player.lavaMax += LavaImmunityTime * 60; // Grants the player 2 additional seconds of lava immunity

			// player.DoBootsEffect(player.DoBootsEffect_PlaceFlowersOnTile); // Spawns flowers when walking on normal or Hallowed grass

			// These effects are visual only. These are replicated in UpdateVanity below so they apply for vanity equipment.
			if (!hideVisual) {
				player.CancelAllBootRunVisualEffects(); // This ensures that boot visual effects don't overlap if multiple are equipped

				// Hellfire Treads sprint dust. For more info on sprint dusts see Player.SpawnFastRunParticles() method in Player.cs
				player.hellfireTreads = true;
				// Other boot run visual effects include: sailDash, coldDash, desertDash, fairyBoots

				if (!player.mount.Active || player.mount.Type != MountID.WallOfFleshGoat) {
					// Spawns flames when walking, like Flame Waker Boots. We also check the Goat Skull mount so the effects don't overlap.
					player.DoBootsEffect(player.DoBootsEffect_PlaceFlamesOnTile);
				}
			}
		}

		public override void UpdateVanity(Player player) {
			// This code is a copy of the visual effects code in UpdateAccessory above
			player.CancelAllBootRunVisualEffects();
			player.vanityRocketBoots = 2;
			player.hellfireTreads = true;
			if (!player.mount.Active || player.mount.Type != MountID.WallOfFleshGoat) {
				player.DoBootsEffect(player.DoBootsEffect_PlaceFlamesOnTile);
			}
		}
	}
}