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
				UnloadedTilesWorld modWorld = ModContent.GetInstance<UnloadedTilesWorld>();
				int posID = new UnloadedPosIndexing(i, j).PosID;
				modWorld.tileInfoMap.TryGetValue(posID, out int infoID);
				if (infoID >= 0) { // This only works in SP
					var info = modWorld.tileInfos[infoID];
					if (info != null) {
						Main.LocalPlayer.cursorItemIconEnabled = true;
						Main.LocalPlayer.cursorItemIconID = -1;
						Main.LocalPlayer.cursorItemIconText = $"{info.modName}: {info.name}";
					}
					else {
						Main.LocalPlayer.cursorItemIconEnabled = true;
						Main.LocalPlayer.cursorItemIconID = -1;
						Main.LocalPlayer.cursorItemIconText = $" info not at ID {infoID} Error";
					}
				}
			}
		}
	}
}
