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
	internal class ExampleBucket2 : ModItem
	{

		public override void SetDefaults() {
			Item.width = 24;
			Item.height = 22;
			Item.maxStack = 9999;
			Item.useStyle = 1;
			Item.useTime = 10;
			Item.useAnimation = 15;
			Item.consumable = true;
			Item.autoReuse = true;
			Item.useTurn = true;
		}

		public override bool? UseItem(Player player) {
			Tile liquid = Main.tile[Player.tileTargetX, Player.tileTargetY];

			int liquidType = ModContent.GetInstance<ExampleLiquid2>().Type;

			if (liquid.LiquidAmount == 0 || liquid.LiquidType == liquidType) {

				liquid.LiquidType = liquidType;
				liquid.LiquidAmount = 255;

				WorldGen.SquareTileFrame(Player.tileTargetX, Player.tileTargetY, true);

			}

			return true;
		}
	}
}
