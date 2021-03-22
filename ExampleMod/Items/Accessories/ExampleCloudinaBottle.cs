using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items.Accessories
{
	public class ExampleCloudinaBottle : ModItem
	{
		public override string Texture => "Terraria/Item_" + ItemID.CloudinaBottle;

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Example Cloud in a Bottle");
			Tooltip.SetDefault("Allows the holder to double jump");
		}

		public override void SetDefaults() {
			item.width = 16;
			item.height = 24;
			item.accessory = true; // Makes this item an accessory.
			item.color = Color.Red; // Makes the item's sprite color red.
			item.rare = ItemRarityID.Blue;
			item.value = Item.sellPrice(silver: 50); // Sets the item sell price to 50 silver coins.
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.doubleJumpCloud = true; 
			// Set player.jumpAgainCloud to true and you have infinite double jumps.
			// Set player.doubleJumpSandstorm to true if you want something like the Sandstorm in a Bottle.
		}
	}
}
