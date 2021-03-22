using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items.Accessories
{
	public class ExampleHermesBoots : ModItem
	{
		public override string Texture => "Terraria/Item_" + ItemID.HermesBoots;

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Example Hermes Boots");
			Tooltip.SetDefault("The wearer can run super fast");
		}

		public override void SetDefaults() {
			item.width = 28; 
			item.height = 24;
			item.accessory = true; // Makes this item an accessory.
			item.color = Color.Orange; // Tints the item's sprite Orange
			item.rare = ItemRarityID.Blue;
			item.value = Item.sellPrice(gold: 1); // Sets the item sell price to one gold coin.
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.accRunSpeed = 6f; // The player's maximum run speed with accessories
			player.moveSpeed += 0.05f; // The acceleration multiplier of the player's movement speed
		}
	}
}
