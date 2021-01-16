using Terraria.ID;
namespace Terraria.ModLoader.Default

{
	[Autoload(false)] // need two named versions
	public class UnloadedDresser : ModTile
	{
		public override string Name{get;}
		
		internal bool IsSemi;

		public override string Texture => "ModLoader/UnloadedDresser";

		public UnloadedDresser(string name = null) {
			Name = name ?? base.Name;
		}

		public override void SetDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileTable[Type] = true;
			Main.tileSolidTop[Type] = true;
			TileID.Sets.BasicDresser[Type]= true;
		}

		public override void MouseOver(int i, int j)
		{
			Player player = Main.LocalPlayer;
			Tile tile = Main.tile[i, j];
			int left = i;
			int top = j;
			//TODO: Work on a better way to find the top left tile of a dresser
			while (Main.tile[i - 1, j].type == Type)
				i--;
			while (Main.tile[i, j - 1].type == Type)
				j--;

			if (tile != null && tile.type == Type) {
				int PosID = top * Main.maxTilesX + left;
				ModContent.GetInstance<UnloadedTilesWorld>().chestInfoMap.TryGetValue(PosID, out int frameID);
				var infos = ModContent.GetInstance<UnloadedTilesWorld>().chestInfos;
				if (frameID >= 0 && frameID < infos.Count) { // This only works in SP
					var info = infos[frameID];
					if (info != null) {
						player.cursorItemIconEnabled = true;
						player.cursorItemIconID = -1;
						player.cursorItemIconText = $"{info.modName}: {info.name}";
					}
				}
				if (Main.tile[left, top].frameX / 36 == 1) {
					player.cursorItemIconID = Terraria.ID.ItemID.BoneKey;
				}
			}

			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
		}
	}
}
