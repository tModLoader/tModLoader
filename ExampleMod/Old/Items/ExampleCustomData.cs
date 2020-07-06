using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ExampleMod.Items
{
	// This ModItem shows off custom data in a ModItem. 
	// It shows examples of Save, Load, NetSend, and NetRecieve, all of which are necessary to preserve custom data.
	class ExampleCustomData : ModItem
	{
		// By setting CloneNewInstances to true, our countdownTimer value will properly show in tooltips.
		public override bool CloneNewInstances => true;
		// countdownTimer is the custom data we have attached to this ModItem, this value needs to by synced and remembered.
		public int countdownTimer = 600;

		public override string Texture => "Terraria/Item_" + ItemID.StickyBomb; // Using a vanilla sprite for simplicity

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Hot Potato");
			Tooltip.SetDefault("Pass this item back and forth and try not to die. Fun for the whole family.");
		}

		public override void SetDefaults() {
			item.CloneDefaults(ItemID.StickyBomb);
			item.maxStack = 1; // We limit maxStack to 1, since countdownTimer wouldn't be preserved if we allowed it to stack with other stacks.
			item.color = Color.Orange;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			var countdownTooltip = new TooltipLine(mod, "CountdownTimer", $"It says {countdownTimer / 60}s on it...I wonder what that means...");
			countdownTooltip.overrideColor = Color.OrangeRed;
			tooltips.Add(countdownTooltip);
		}

		public override void UpdateInventory(Player player) {
			// This method handles the Hot Potato game functionality.
			if (player.whoAmI == Main.myPlayer) {
				countdownTimer -= 1;
				if (countdownTimer <= 0) {
					// Once the timer reaches 0, a grenade projectile is spawned with 2 ticks left, this is the simplest way to simulate the explosion.
					Projectile projectile = Projectile.NewProjectileDirect(player.Center, Vector2.Zero, ProjectileID.Grenade, 100, 1, Main.myPlayer);
					projectile.timeLeft = 2;
					projectile.netUpdate = true;
					// The item itself is deleted from the players inventory.
					item.TurnToAir();
				}
			}
		}

		// Save and Load preserve the value of countdownTimer when leaving and entering the world.
		public override TagCompound Save() {
			return new TagCompound {
				[nameof(countdownTimer)] = countdownTimer,
			};
		}

		public override void Load(TagCompound tag) {
			countdownTimer = tag.GetInt(nameof(countdownTimer));
		}

		// NetSend and NetRecieve allow the countdownTimer value to be synced correctly even as the item is tossed out into the world and picked up by other players
		public override void NetSend(BinaryWriter writer) {
			writer.Write(countdownTimer);
		}

		public override void NetRecieve(BinaryReader reader) {
			countdownTimer = reader.ReadInt32();
		}

		public override void AddRecipes() {
			var recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.StickyBomb);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
