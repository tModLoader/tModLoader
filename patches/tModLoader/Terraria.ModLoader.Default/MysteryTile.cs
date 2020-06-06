namespace Terraria.ModLoader.Default
{
	public class MysteryTile : ModTile
	{
		public override void SetDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileFrameImportant[Type] = true;
		}

		public override void MouseOver(int i, int j)
		{
			var tile = Main.tile[i, j];
			if(tile != null && tile.type == Type) {
				var frame = new MysteryTileFrame(tile.frameX, tile.frameY);
				var infos = ModContent.GetInstance<MysteryTilesWorld>().infos;
				int frameID = frame.FrameID;
				if (frameID >= 0 && frameID < infos.Count) { // This only works in SP
					var info = infos[frameID];

					if (info != null) {
						Main.LocalPlayer.showItemIcon = true;
						Main.LocalPlayer.showItemIcon2 = -1;
						Main.LocalPlayer.showItemIconText = $"{info.modName}: {info.name}";
					}
				}
			}
		}
	}
}
