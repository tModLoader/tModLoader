using Terraria.ID;
namespace Terraria.ModLoader.Default

{
	[Autoload(false)] // need two named versions
	public class UnloadedDresser : ModTile
	{
		public override string Name{ get; }

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
				left--;
			while (Main.tile[i, j - 1].type == Type)
				top--;

			if (tile != null && tile.type == Type) {
				UnloadedTilesSystem modSystem = ModContent.GetInstance<UnloadedTilesSystem>();
				UnloadedPosIndexing posIndex = new UnloadedPosIndexing(i, j);
				int infoID = posIndex.FloorGetValue(modSystem.chestInfoMap);
				var infos = modSystem.chestInfos;
				if (infoID < infos.Count) { // This only works in SP
					var info = infos[infoID];
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
