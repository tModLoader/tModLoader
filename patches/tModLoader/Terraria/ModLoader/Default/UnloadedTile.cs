namespace Terraria.ModLoader.Default
{
	[Autoload(false)] // need two named versions
	public class UnloadedTile : ModTile
	{
		public override string Name{get;}
		internal bool IsSolid;

		public override string Texture => "ModLoader/UnloadedTile";

		public UnloadedTile(string name = null,bool isSolid = true) {
			Name = name ?? base.Name;
			this.IsSolid = isSolid;
		}

		public override void SetDefaults() {
			Main.tileSolid[Type] = IsSolid;
			Main.tileFrameImportant[Type] = true;
		}

		public override void MouseOver(int i, int j)
		{
			var tile = Main.tile[i, j];
			if(tile != null && tile.type == Type) {
				var infoID = new UnloadedPosIndexing(i, j).PosID;
				var infos = ModContent.GetInstance<UnloadedTilesWorld>().tileInfos;
				if (infoID >= 0 && infoID < infos.Count) { // This only works in SP
					var info = infos[infoID];
					if (info != null) {
						Main.LocalPlayer.cursorItemIconEnabled = true;
						Main.LocalPlayer.cursorItemIconID = -1;
						Main.LocalPlayer.cursorItemIconText = $"{info.modName}: {info.name}";
					}
				}
			}
		}
	}
}
