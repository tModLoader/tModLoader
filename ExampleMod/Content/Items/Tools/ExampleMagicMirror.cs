using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Tools
{
	// Magic Mirror is one of the only vanilla items that does its action somewhere other than the start of its animation, which is why we use code in UseStyle NOT UseItem.
	// It may prove a useful guide for ModItems with similar behaviors.
	internal class ExampleMagicMirror : ExampleItem
	{
		public override string Texture => $"Terraria/Images/Item_{ItemID.IceMirror}"; // Copies the texture for the Ice Mirror, make your own texture if need be.

		public override void SetStaticDefaults() {
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1; // Amount of this item needed to research and become available in Journey mode's duplication menu. Amount used based upon vanilla Magic Mirror's amount needed.
		}

		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.IceMirror); // Copies the defaults from the Ice Mirror.
			Item.color = Color.Violet; // Sets the item color
		}

		// UseStyle is called each frame that the item is being actively used.
		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			// Each frame, make some dust
			if (Main.rand.NextBool()) {
				Dust.NewDust(player.position, player.width, player.height, 15, 0f, 0f, 150, Color.White, 1.1f); // Makes dust from the player's position and copies the hitbox of which the dust may spawn. Change these arguments if needed.
			}

			// This sets up the itemTime correctly.
			if (player.itemTime == 0) {
				player.ApplyItemTime(Item);
			}
			else if (player.itemTime == player.itemTimeMax / 2) {
				// This code runs once halfway through the useTime of the Item. You'll notice with magic mirrors you are still holding the item for a little bit after you've teleported.

				// Make dust 70 times for a cool effect.
				for (int d = 0; d < 70; d++) {
					Dust.NewDust(player.position, player.width, player.height, 15, player.velocity.X * 0.5f, player.velocity.Y * 0.5f, 150, default, 1.5f);
				}

				// This code releases all grappling hooks and kills/despawns them.
				player.grappling[0] = -1;
				player.grapCount = 0;

				for (int p = 0; p < 1000; p++) {
					if (Main.projectile[p].active && Main.projectile[p].owner == player.whoAmI && Main.projectile[p].aiStyle == 7) {
						Main.projectile[p].Kill();
					}
				}

				// The actual method that moves the player back to bed/spawn.
				player.Spawn(PlayerSpawnContext.RecallFromItem);

				// Make dust 70 times for a cool effect. This dust is the dust at the destination.
				for (int d = 0; d < 70; d++) {
					Dust.NewDust(player.position, player.width, player.height, 15, 0f, 0f, 150, default, 1.5f);
				}
			}
		}

		private Color[] itemNameCycleColors = { new Color(254, 105, 47), new Color(190, 30, 209), new Color(34, 221, 151), new Color(0, 106, 185) };

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			// This code shows using Color.Lerp,  Main.GameUpdateCount, and the modulo operator (%) to do a neat effect cycling between 4 custom colors.
			foreach (TooltipLine line2 in tooltips) {
				if (line2.mod == "Terraria" && line2.Name == "ItemName") {
					float fade = (Main.GameUpdateCount % 60) / 60f;
					int index = (int)((Main.GameUpdateCount / 60) % 4);
					line2.overrideColor = Color.Lerp(itemNameCycleColors[index], itemNameCycleColors[(index + 1) % 4], fade);
				}
			}
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}
