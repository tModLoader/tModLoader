using System.Linq;

namespace Terraria.ModLoader.Default
{
	public class MysteryTile : ModTile
	{
		public override void SetDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileFrameImportant[Type] = true;
		}

		//public override void MouseOver(int i, int j) {
		//	var tile = Main.tile[i, j];
		//	if (tile != null && tile.type == Type) {
		//		var frame = new MysteryTileFrame(tile.frameX, tile.frameY);
		//		var info = ModLoaderMod.Instance.GetModWorld<MysteryTilesWorld>().infos
		//			.FirstOrDefault(x => {
		//				return frame.FrameID == new MysteryTileFrame(x.frameX, x.frameY).FrameID;
		//			});

		//		if (info != null) {
		//			Main.hoverItemName = $"{info.modName}: {info.name}";
		//		}
		//	}
		//}
	}
}
