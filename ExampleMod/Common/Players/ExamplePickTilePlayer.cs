using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	public class ExamplePickTilePlayer : ModPlayer
	{
		public override void ModifyPickTile(Item item, int x, int y, ref int pick) {
			base.ModifyPickTile(item, x, y, ref pick);
			//if item is copperpickaxe then modify pick to 999
			//So he could dig up everything
			if (item.type == ItemID.CopperPickaxe) {
				pick = 999;
			}
		}
	}
}
