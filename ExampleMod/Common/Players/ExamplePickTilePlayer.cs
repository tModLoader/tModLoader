using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	public class ExamplePickTilePlayer : ModPlayer
	{
		public override void ModifyPickTile(IEntitySource source, int x, int y, ref int pick) {
			base.ModifyPickTile(source, x, y, ref pick);
			//if item is copperpickaxe then modify pick to 999
			//So he could dig up everything
			if(source is EntitySource_ItemUse itemSource) {
				if (itemSource.Item.type == ItemID.CopperPickaxe) {
					pick = 999;
				}
			}

			if(source is EntitySource_Mount mountSource) {
				if(mountSource.MountId == MountID.Drill) {
					pick = 1; // modify drill pick to 1
				}
			}
		}
	}
}
