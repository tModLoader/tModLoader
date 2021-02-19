using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;


namespace Terraria.ModLoader.Default
{
	[Autoload(false)] // need multiple named versions
	public class UnloadedTile : ModTile {
		public override string Name { get; }

		internal bool isSolid;
		internal bool isSemi;
		internal bool isChest;
		internal bool isDresser;

		public override string Texture => "ModLoader/UnloadedTile";

		public UnloadedTile(string name = null, bool isSolid = true, bool isSemi = false, bool isChest = false, bool isDresser = false) {
			Name = name ?? base.Name;

			this.isSolid = isSolid;
			this.isSemi = isSemi;
			this.isChest = isChest;
			this.isDresser = isDresser;
		}

		public override void SetDefaults() {
			//common
			Main.tileFrameImportant[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;

			Main.tileSolid[Type] = isSolid && !isChest;
			Main.tileNoAttach[Type] = (!isSolid || isSemi || isChest || isDresser);
			Main.tileTable[Type] = isSemi || isDresser;
			Main.tileSolidTop[Type] = isSemi || isDresser;

			TileID.Sets.Platforms[Type] = isSemi;
			TileID.Sets.BasicChest[Type] = isChest;
			TileID.Sets.BasicDresser[Type] = isDresser;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1); // Disables hammering
			TileObjectData.addTile(Type);

			//unique 
			if (isSemi) {
				AdjTiles = new int[] { TileID.Platforms };
			}
			
			if (isChest) {
				
				Main.tileSpelunker[Type] = isChest;
				Main.tileContainer[Type] = isChest;
				Main.tileShine2[Type] = isChest;
				Main.tileShine[Type] = 1200;
				Main.tileOreFinderPriority[Type] = 500;

				//TileID.Sets.HasOutlines[Type] = isChest;

				AdjTiles = new int[] { TileID.Containers };

				ContainerName.SetDefault("UnloadedChest");

				ModTranslation name = CreateMapEntryName();
				name.SetDefault("Unloaded Chest");
				AddMapEntry(new Color(200, 200, 200), name, MapChestName);

				name = CreateMapEntryName(Name + "_Locked");
				name.SetDefault("Locked Unloaded Chest");
				AddMapEntry(new Color(0, 141, 63), name, MapChestName);
			}
			
		}

		public override void MouseOver(int i, int j) {
			DisplayInfos(i, j);
		}

		public override void MouseOverFar(int i, int j) {
			MouseOver(i, j);

			DisplayInfos(i, j);
		}

		private void DisplayInfos(int i, int j) {
			var tile = Main.tile[i, j];

			if (tile == null || tile.type != Type) {
				return;
			}

			Player player = Main.LocalPlayer; 
			var modSystem = ModContent.GetInstance<UnloadedTilesSystem>();
			var posIndex = new UnloadedPosIndexing(i, j);

			//NOTE: Onwards only works in singleplayer, as the lists aren't synced afaik.
			int infoID = posIndex.FloorGetValue(TileIO.tileInfoMap);
			var infos = modSystem.tileInfos;

			if (infoID < infos.Count) {
				var info = infos[infoID];

				if (info != null) {
					player.cursorItemIconEnabled = true;
					player.cursorItemIconID = -1;
					player.cursorItemIconText = $"{info.modName}: {info.name}";
				}
			}
		}

		public static string MapChestName(string name, int i, int j) {
			int left = i;
			int top = j;
			Tile tile = Main.tile[i, j];

			if (tile.frameX % 36 != 0) {
				left--;
			}

			if (tile.frameY != 0) {
				top--;
			}

			int chest = Chest.FindChest(left, top);
			if (chest < 0) {
				return Language.GetTextValue("LegacyChestType.0");
			}

			if (Main.chest[chest].name == "") {
				return name;
			}

			return name + ": " + Main.chest[chest].name;
		}
	}
}
