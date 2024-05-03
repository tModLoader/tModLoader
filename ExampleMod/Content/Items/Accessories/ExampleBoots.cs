using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Accessories
{
	[AutoloadEquip(EquipType.Shoes)]
	public class ExampleBoots : ModItem
	{
		public override void SetDefaults() {
			Item.width = 22;
			Item.height = 22;

			Item.accessory = true;
			Item.rare = ItemRarityID.Red;
			Item.value = Item.buyPrice(gold: 1); // Equivalent to Item.buyPrice(0, 1, 0, 0);
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.runAcceleration += 0.08f; // Modifies player run acceelration
			player.moveSpeed += 0.1f; // Modifies the player movement speed bonus.

			// Modifies the players max horizontal speed while in air or simply without boots.
			// Be aware that if maxRunSpeed * moveSpeed would be greater than accRunSpeed,
			// Then maxRunSpeed will be used in all movement calculations
			player.maxRunSpeed = 4f;
			player.accRunSpeed = 6f; // Modifies the players maximum run speed in boots.

			// Determines wether the boots count as rocket boots
			// 0 - These are not rocket boots
			// Anything else - These are rocket boots
			player.rocketBoots = 1;
			player.rocketTimeMax = 60; // Max flight time with this boots. Divide by 6 to get time in seconds.

			// Sets which dust and sound to use for the rocket flight
			// 1 - Hermes Boots
			// 2 - Fairy Boots, Spectre Boots, Lightning Boots
			// 3 - Frostspark Boots
			// 4 - Terrraspark Boots
			// 5 - Hellfire Treads
			player.vanityRocketBoots = 5;

			player.waterWalk2 = true; // Allows walking on lava without falling into it
			player.waterWalk = true; // Allows walking on water and honey without falling into it
			player.iceSkate = true; // Grant the player improved speed on ice and not breaking thin ice when falling onto it
			player.desertBoots = true; // Grants the player increased movement speed while running on sand
			player.fireWalk = true; // Grants the player immunity from Meteorite and Hellstone tile damage
			player.noFallDmg = true; // Grants the player the Lucky Horseshoe effect of nullyfying fall damage
			player.lavaRose = true; // Grants the Lava Rose effect
			player.lavaMax = 120; // Grants the player the specified amount of ticks of lava immunity

			player.hellfireTreads = true; // Hellfire Treads sprint dust. For more info on sprint dusts see Player.SpawnFastRunParticles() method in Player.cs
			player.DoBootsEffect(player.DoBootsEffect_PlaceFlamesOnTile); // Spawns flames when walking
			//player.DoBootsEffect(player.DoBootsEffect_PlaceFlowersOnTile); // Spawns flowers when walking on normal or Hallowed grass
		}

		public override void UpdateVanity(Player player) {
			// This makes the chosen dust apply to rocket boots equipped in the normal accessory slot,
			// if Example Boots are in the vanity slot
			player.vanityRocketBoots = 5;
		}
		
	}
}