using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using ExampleMod.Content.Liquids;

namespace ExampleMod.Content.Items
{
	internal class ExampleBucket : ModItem
	{

		public override void SetDefaults() {
			Item.width = 24;
			Item.height = 22;
			Item.maxStack = 99;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 100;
			Item.useAnimation = 1;
			Item.consumable = true;
		}

		public override bool? UseItem(Player player) {
			Tile liquid = Main.tile[Player.tileTargetX, Player.tileTargetY];

			int liquidType = ModContent.GetInstance<ExempleLiquid>().Type;

			if (liquid.LiquidAmount == 0 || liquid.LiquidType == liquidType) {

				//Item newItem = Main.item[Item.NewItem(player.position, item.type)];
				//ModBucket newBucket = newItem.modItem as ModBucket;

				liquid.LiquidType = liquidType;
				liquid.LiquidAmount = 255;

				WorldGen.SquareTileFrame(Player.tileTargetX, Player.tileTargetY, true);

			}

			return true;
		}
	}
}
